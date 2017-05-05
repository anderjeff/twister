﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using Twister.Business.Data;
using Twister.Business.Database;
using Twister.Business.Hardware;
using Twister.Business.Shared;

namespace Twister.Business.Tests
{
    /// <summary>
    ///     Represents the TT-1667 torsion test bench.
    /// </summary>
    public class TestBench : NotifyPropertyChangedBase
    {
        private static readonly string DEFAULT_BENCH_PARAMS = @"G:\Engineering\Programs\Twister 2015\DefaultParams.xml";

        // Singleton pattern.
        private static TestBench _testBench;

        private readonly ServoDrive _acDrive;

        // all tests performed in this session.

        // The bench runs one test at a time.
        private TorqueTest _currentTest;

        // The bench is thread safe
        private readonly object _objLock = new object();

        // Indicates if the TestBench has the ability to tell if the the ServoDrive is in run mode.        
        private bool _runModeDeterminate;

        // The test that has just completed and needs to be saved to the database.
        private TorqueTest _testToSave;

        // The bench has this equipment
        private readonly AnalogInputDevice _torqueCell;

        // private constructor.
        /// <summary>
        ///     A private constructor used to create singleton instance.
        /// </summary>
        private TestBench()
        {
            // specifify the locations that the devices reside on the network.
            _torqueCell = new AnalogInputDevice("128.1.1.30");
            _acDrive = new ServoDrive("128.1.1.35");

            CompletedTests = new List<TorqueTest>();
        }


        /// <summary>
        ///     A single instance of a TestBench.  We only have one bench.
        /// </summary>
        /// <returns></returns>
        public static TestBench Singleton
        {
            get
            {
                if (_testBench == null) _testBench = new TestBench();
                return _testBench;
            }
        }

        public bool IsTesting
        {
            get
            {
                if (CurrentTest == null) return false;
                if (_runModeDeterminate) return TestCurrentlyRunning();
                return false;
            }
        }

        /// <summary>
        ///     The TorqueTest that is currently running on this TestBench.
        /// </summary>
        public TorqueTest CurrentTest
        {
            get { return _currentTest; }
            private set
            {
                if (_currentTest != value)
                {
                    _currentTest = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Tests that have been completed
        /// </summary>
        public List<TorqueTest> CompletedTests { get; set; }

        // events
        public event EventHandler<TorqueTestEventArgs> TestStarted;

        public event EventHandler<TorqueTestEventArgs> TestCompleted;

        /// <summary>
        ///     Indicates if the test bench as completed a test and
        ///     wants to save the test.
        /// </summary>
        /// <returns></returns>
        public bool HasTestToSave()
        {
            return _testToSave != null;
        }

        /// <summary>
        ///     Load default parameters to give the bench a certain basic behavior, before the
        ///     tests alter that behavior.
        /// </summary>
        public void LoadDefaultBenchParameters()
        {
            // load default test settings for the bench, before any test is ran.
            BenchLimits limits = new BenchLimits();
            limits.Load(DEFAULT_BENCH_PARAMS);
        }

        /// <summary>
        ///     Inform the TestBench that you are ready to begin testing.
        /// </summary>
        public void InformReady()
        {
            // -1 means true in the AKD Basic logic I used
            Singleton.LoadTestParameter(ServoDrive.RegisterAddress.SoftwareInitialized, -1);
        }

        /// <summary>
        ///     Inform the TestBench that you are no longer ready to begin testing.
        /// </summary>
        public void InformNotReady()
        {
            // 0 means false in the AKD Basic logic I used.
            Singleton.LoadTestParameter(ServoDrive.RegisterAddress.SoftwareInitialized, 0);
        }

        /// <summary>
        ///     Loads a new test into the test bench.
        /// </summary>
        /// <param name="test">The torque test to run on this test bench.</param>
        public void LoadTest(TorqueTest test)
        {
            // no need to load a null test, setting the _currentTest to null
            // is handled elsewhere in this class.
            if (test != null)
            {
                if (_currentTest != null && _currentTest.InProcess)
                    throw new Exception("Cannot begin a new test before the current test has finished.");
                CurrentTest = test;
                SetTestType(CurrentTest.TestTemplateId);
            }
        }

        /// <summary>
        ///     Tell the AC drive what type of test you want to run.
        /// </summary>
        /// <param name="testId"></param>
        private void SetTestType(int testId)
        {
            _acDrive.StoreParameter(ServoDrive.RegisterAddress.TestType, testId);
        }

        public void BeginCurrentTest()
        {
            if (_currentTest == null) throw new Exception("No test has been defined.  Must create a test first.");
            if (!_currentTest.InProcess)
            {
                _currentTest.StartDate = DateTime.Now;

                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                DateTimeOffset offsetCentralTime = TimeZoneInfo.ConvertTime(new DateTimeOffset(
                    DateTime.UtcNow, new TimeSpan(0, 0, 0)), timeZoneInfo);

                _currentTest.StartTime = offsetCentralTime.TimeOfDay;

                _currentTest.Start();
                OnTestStarted(_currentTest);

                // this starts the test running
                Singleton.TurnOn();
            }
        }

        /// <summary>
        ///     Gets a sample representing the state of the sensors on the TestBench at the
        ///     time of the request.
        /// </summary>
        /// <returns>
        ///     An object of type Sample, that contains the current value from certain sensors.
        /// </returns>
        public Sample GetState()
        {
            /* update the current state of the TestBench.  Updating the state of the TestBench
             * should be done by the TorqueTest, because only the TorqueTest will have a feel 
             * for how often the state needs to be updated.  For example, a fast moving test will 
             * need to be updated more often.
             */
            lock (_objLock)
            {
                _torqueCell.RefreshTorque();
                _acDrive.RefreshPosition();

                /* give the ServoDrive an updated torque value, this needs to go here because 
                 * of the access to _torqueCell, and to _acDrive, which I am trying to 
                 * restrict to inside this class only.
                 */
                LoadTestParameter(ServoDrive.RegisterAddress.TorqueValue, (int) _torqueCell.Torque);
            }

            // now create and return the data point
            Sample sample = new Sample(_torqueCell.Torque, _acDrive.GearboxAngle);
            return sample;
        }

        /// <summary>
        ///     Saves a single sample of data about the test bench.
        /// </summary>
        public void AddToCurrentTestData(Sample sample)
        {
            lock (_objLock)
            {
                if (_currentTest != null) _currentTest.Data.Add(sample);
            }
        }

        /// <summary>
        ///     Gets a copy of all the test data saved for the test that is
        ///     currently running on the test bench.
        /// </summary>
        /// <returns>
        ///     A copy of all the test data for the current test.  If the current
        ///     test is null, this method will return null.
        /// </returns>
        public List<Sample> GetCurrentTestData()
        {
            lock (_objLock)
            {
                if (_currentTest != null)
                {
                    var copyOfData = new List<Sample>();
                    foreach (Sample savedSample in _currentTest.Data)
                        copyOfData.Add(new Sample(
                            savedSample.Torque, savedSample.Angle));
                    return copyOfData;
                }
            }

            return null;
        }

        /// <summary>
        ///     This method must be called on a regular basis to let the TestBench know that you are
        ///     still interested in running the test.  It's a watchdog timer disguised as a method.
        /// </summary>
        public void VerifyAlive()
        {
            /* this value indicates how long the AKD Basic program 
             * will run without communication from this program.
             * A watchdogValue of 50 shuts it down almost instantly.
             */
            var watchdogValue = 100;
            LoadTestParameter(ServoDrive.RegisterAddress.WatchdogValue, watchdogValue);
        }

        /// <summary>
        ///     Stops the test before it has completed.
        /// </summary>
        public void EmergencyStop()
        {
            // stop the test, return the motor shaft to the zero position.
            TurnOff();
        }

        /// <summary>
        ///     This method checks to see that the test is still running.  The test is allowed to run until
        ///     the program that turns the motor (the ADK Basic program running on the _acDrive) updates
        ///     a parameter indicating the test is complete.
        /// </summary>
        /// <returns></returns>
        private bool TestCurrentlyRunning()
        {
            try
            {
                /* This value should be -1 when the test is running, and 
                 * zero when the test stops running.
                 */
                var value = _acDrive.RetrieveParameter(ServoDrive.RegisterAddress.TestInProcess);

                // use my flag, not sure yet if this is important and/or needed.
                if (value == -5000)
                    Debug.WriteLine(
                        "Info _acDrive RetrieveParameter returned -5000, meaning response to ReadHoldingRegister was null");

                if (value == 0)
                    return false;

                // test is still running.
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "TestCurrentlyRunning"));

                // not sure what's going on, so just to be safe.
                TurnOff();

                return false;
            }
        }

        /// <summary>
        ///     Stores a test parameter for a bench component.  The bench component will behave
        ///     in a certain way, based off the value of that parameter.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="value"></param>
        internal void LoadTestParameter(ServoDrive.RegisterAddress location, int value)
        {
            _acDrive.StoreParameter(location, value);
        }

        /// <summary>
        ///     Stores a test parameter for a bench component.  The bench component will behave
        ///     in a certain way, based off the value of that parameter.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="value"></param>
        internal void LoadTestParameter(ServoDrive.RegisterAddress location, float value)
        {
            _acDrive.StoreParameter(location, value);
        }

        /// <summary>
        ///     Turns the test bench on, which has the effect of running the current test.
        /// </summary>
        internal void TurnOn()
        {
            /* a value of -1 (Boolean true for the AKD Basic program Fully Reversed Torsion Test.bas
             * is what the AKD Basic program needs in order to start the torque test.  So the 
             * parameter is stored on the device so the running AKD Basic program can read the value 
             * and respond accordingly.
             */
            LoadTestParameter(ServoDrive.RegisterAddress.TestInProcess, -1);
            _runModeDeterminate = true;
        }

        /// <summary>
        ///     Turns the test bench off, which has the effect of ending the current test.
        /// </summary>
        internal void TurnOff()
        {
            /* a value of 0 is what the AKD Basic program needs in order to stop
             * the torque test.  So the parameter is stored on the device so the 
             * running AKD Basic program can read the value and respond accordingly.
             */
            LoadTestParameter(ServoDrive.RegisterAddress.TestInProcess, 0);

            if (_currentTest != null)
            {
                // The test will need to be reinitialized to start.
                _currentTest.Initialized = false;
                _currentTest.WasShutDownEarly = true;
            }
        }

        private void OnTestStarted(TorqueTest test)
        {
            // let the world know.
            var handler = TestStarted;
            if (handler != null) TestStarted(this, new TorqueTestEventArgs(test));
        }

        private void OnTestCompleted()
        {
            // let the world know.
            var handler = TestCompleted;
            if (handler != null) TestCompleted(this, new TorqueTestEventArgs(_testToSave));
        }

        /// <summary>
        ///     Verifies that the test id has not already been used, is not null,
        ///     and is not an empty value.  If the number is verified, this call
        ///     will also save the _testToSave, and indicate that in the response
        ///     to the caller.
        /// </summary>
        /// <param name="testId">
        ///     The testId the user is trying to assign to the test that just completed.
        /// </param>
        /// <returns>
        ///     If the testId is null, whitespace, or a duplicate testId, an error
        ///     will be returned to the user.  If no error is detected, null will be
        ///     returned
        /// </returns>
        public string VerifyTestId(string testId)
        {
            try
            {
                // verification of number provided

                if (string.IsNullOrWhiteSpace(testId)) return Messages.BlankTorqueTestId();
                if (TorqueTestDb.InvalidTestId(testId)) return Messages.DuplicateTestId(testId);
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        ///     Call this method to save the last completed test to the database using a
        ///     unique test id.
        /// </summary>
        /// <param name="testId"></param>
        /// <returns></returns>
        public string SaveTestToDatabase(string testId)
        {
            try
            {
                /* 
                  The number has been validated, now we can use it.  Don't worry about it being 
                  null. If it is I want an exception to be thrown so there are clues to the user
                  as to why the test was not saved to the database.                  
                */
                _testToSave.TestId = testId;

                // passsed verification, so save to db.
                var recordsSaved = TorqueTestDb.SaveToDatabase(_testToSave);

                if (recordsSaved > 0)
                {
                    // add to completed tests, the view model will grab it next time it updates.
                    CompletedTests.Add(_testToSave);

                    // there is no longer a test that needs to be saved, so set to null.
                    _testToSave = null;

                    return Messages.DatabaseSaveSuccessful();
                }
                return Messages.DatabaseSaveUnsuccessful();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "SaveTestToDatabase"));
                return ex.Message;
            }
        }

        public void AddTestToListToSave()
        {
            // need to save test Data, add this test to the log of tests
            // performed on this test bench, and then set current test equal 
            // to null.
            lock (_objLock)
            {
                if (CurrentTest != null)
                {
                    TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time");
                    DateTimeOffset offsetCentralTime = TimeZoneInfo.ConvertTime(new DateTimeOffset(
                        DateTime.UtcNow, new TimeSpan(0, 0, 0)), timeZoneInfo);

                    CurrentTest.FinishTime = offsetCentralTime.TimeOfDay;

                    if (!_currentTest.WasShutDownEarly)
                    {
                        CompletedTests.Add(_currentTest);
                        CurrentTest.ShouldBeSaved = true;

                        // so there is still a reference to the CurrentTest for the 
                        // OnTestCompleted() method to have access to.
                        _testToSave = CurrentTest;
                    }
                    else
                    {
                        _testToSave = null;
                    }

                    CurrentTest = null;
                    OnTestCompleted();
                }

                _runModeDeterminate = false;
            }
        }

        /// <summary>
        ///     Allows the user to manually stop and trigger the test to be saved.
        /// </summary>
        public void ManuallyCompleteTestCycle()
        {
            /*
                Stop the test that is running, so the next time TestCurrentlyRunning() runs, 
                the test will indicate complete and begin the test complete events.  As always 
                the 0 indicates a value of false.
            */
            _acDrive.StoreParameter(ServoDrive.RegisterAddress.TestInProcess, 0);
        }
    }
}