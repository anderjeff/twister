using System;
using System.Diagnostics;
using System.Linq;
using Twister.Business.Data;
using Twister.Utilities;
using Twister.Business.Shared;
using Twister.Business.Tests;

namespace Twister.ViewModels
{
    public class UnidirectionalTorqueTest_VM : TestBase_VM
    {
        private bool _isClockwise;

        private bool _isCounterclockwise;

        public UnidirectionalTorqueTest_VM()
        {
            IsTestIdNeeded = false;

            SelectClockwiseTestCommand = new RelayCommand(SetClockwiseTest, CanSelectDirection);
            SelectCounterclockwiseTestCommand = new RelayCommand(SetCounterclockwiseTest, CanSelectDirection);
            SaveDataCommand = new RelayCommand(SaveData, CanSaveData);

            // let user know what to do
            DisplayTempMessage(Messages.SelectDirectionMessage(), ERROR_BRUSH, 0);
        }

        public RelayCommand SelectClockwiseTestCommand { get; }
        public RelayCommand SelectCounterclockwiseTestCommand { get; }
        public RelayCommand SaveDataCommand { get; }

        /// <summary>
        ///     Indicates if the test will be running in the counterclockwise direction.
        /// </summary>
        public bool IsCounterclockwise
        {
            get { return _isCounterclockwise; }
            set
            {
                if (_isCounterclockwise != value)
                {
                    _isCounterclockwise = value;
                    OnPropertyChanged();
                    StartTestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Indicates if the test will be running in the clockwise direction.
        /// </summary>
        public bool IsClockwise
        {
            get { return _isClockwise; }
            set
            {
                if (_isClockwise != value)
                {
                    _isClockwise = value;
                    OnPropertyChanged();
                    StartTestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        ///     Saves all the available test data.
        /// </summary>
        private void SaveData()
        {
            /*
            Whatever the test is doing at the time...
              1. Stop test monitoring
              2. Enter torque test id
              3. Save all data to the database.            
            */
            TestBench.Singleton.ManuallyCompleteTestCycle();
        }

        /// <summary>
        ///     Indictates if the test is in a state where saving actually makes sense.
        /// </summary>
        public bool CanSaveData()
        {
            // I am returning true because it didn't work last time and I want to make sure 
            // it's not because this was not letting me save.
            return true;
        }

        private void SetCounterclockwiseTest()
        {
            IsCounterclockwise = true;
            IsClockwise = !IsCounterclockwise;
        }

        private void SetClockwiseTest()
        {
            IsClockwise = true;
            IsCounterclockwise = !IsClockwise;
        }

        private bool CanSelectDirection()
        {
            return base.CanStartTest();
        }

        protected override bool CanStartTest()
        {
            if (!IsClockwise && !IsCounterclockwise)
            {
                return false;
            }
            DisplayTempMessage(Messages.StartButtonMessage(), GOOD_MESSAGE_BRUSH, 0);
            return base.CanStartTest();
        }

        protected override void CleanupCurrentTest()
        {
            if (TestIdReceived)
            {
                if (TorqueAngleChart != null && TorqueAngleChart.Series["series1"].Points.Count > 0)
                {
                    TorqueTest completedTest = TestBench.Singleton.CompletedTests.LastOrDefault();
                    if (completedTest != null)
                    {
                        Session.CompletedTests.Add(completedTest);

                        // show the start button again.
                        StartTestCommand.RaiseCanExecuteChanged();
                        SaveDataCommand.RaiseCanExecuteChanged();
                        ExitProgamCommand.RaiseCanExecuteChanged();
                    }
                }

                //clear out the data points (except the dummy point)
                ClearPointsFromChart();

                // change it back.
                TestIdReceived = false;
            }
        }

        protected override void StartTest()
        {
            {
                if (CanStartTest())
                {
                    DisplayTempMessage("Test in process...", GOOD_MESSAGE_BRUSH, 0);

                    UnidirectionalTorqueTest test = Session.TestTemplate.TestInstance() as UnidirectionalTorqueTest;

                    test.Operator = Session.BenchOperator;
                    test.WorkOrder = Session.WorkId;

                    // used when creating the Unidirectional Chart to set the minimum or maximum y value.
                    int maxTorque = test.MaxTorque;

                    // set the test direction, based of what the user has chosen.
                    if (IsClockwise)
                    {
                        test.Direction = TestDirection.Clockwise;
                    }
                    else if (IsCounterclockwise)
                    {
                        maxTorque = test.MinTorque; // CCW is negative
                        test.Direction = TestDirection.Counterclockwise;
                    }
                    else
                    {
                        test.Direction = TestDirection.Unknown;
                        throw new Exception("Unknown test direction, please change to Clockwise or Counterclockwise.");
                    }

                    TestBench.Singleton.LoadTest(test);
                    TestBench.Singleton.BeginCurrentTest();

                    ChartFactory.CreateUnidirectionalChart(TorqueAngleChart, maxTorque);

                    // clear all saved Sample objects from base class list.
                    ClearTestData();

                    // reevalutate all commands can execute.
                    StartTestCommand.RaiseCanExecuteChanged();
                    SaveDataCommand.RaiseCanExecuteChanged();
                    ExitProgamCommand.RaiseCanExecuteChanged();
                    StopTestCommand.RaiseCanExecuteChanged();
                }
            }
        }

        internal override void CreateDefaultChart()
        {
            ChartFactory.CreateUnidirectionalChart(TorqueAngleChart, 20000);
        }

        protected override void AddPointToChart(Sample sample)
        {
            if (TorqueAngleChart != null)
                TorqueAngleChart.Series["series1"].Points.AddXY(Math.Abs(sample.Angle), sample.Torque);
        }

        protected override void ScaleChartAxis(float currentTorque, float currentAngle)
        {
            // for calibration purposes
            if (TorqueAngleChart == null) return;

            UnidirectionalTorqueTest runningTest = TestBench.Singleton.CurrentTest as UnidirectionalTorqueTest;

            try
            {
                TorqueAngleChart.ChartAreas[0].RecalculateAxesScale();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    Messages.GeneralExceptionMessage(ex, "UnidirectionalTorqueTest_VM.ScaleChartAxis"));
            }
        }
    }
}