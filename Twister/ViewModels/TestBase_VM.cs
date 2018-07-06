using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Media;
using System.Windows.Threading;
using Twister.Business.Data;
using Twister.Business.Shared;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
    /// <summary>
    ///     A base view model for any view model that deals with a torque test.
    /// </summary>
    public abstract class TestBase_VM : Base_VM
    {
        protected readonly Brush ERROR_BRUSH = (Brush) new BrushConverter().ConvertFromString("Yellow");

        // constants
        protected readonly Brush EXCEPTION_BRUSH = (Brush) new BrushConverter().ConvertFromString("Red");

        public readonly Brush GOOD_MESSAGE_BRUSH = (Brush) new BrushConverter().ConvertFromString("LightGreen");
        protected readonly Brush TEST_FAILURE_BRUSH = (Brush) new BrushConverter().ConvertFromString("Orange");
        protected readonly Brush WAITING_BRUSH = (Brush) new BrushConverter().ConvertFromString("Gray");
        private float _angleValueCurrent;
        private string _closeProgramMessage;
        private string _displayAngle; // used for databinding to UI display label
        private string _displayTorque; // used for databinding to UI display label


        private string _information;
        private bool _isTestIdNeeded;
        private Brush _messageBackgroundColor;
        protected Thread _monitoringThread; // for UI activity (constant)
        protected Thread _runningThread; // when test in process
        private TestSession _session;
        private readonly List<Sample> _testData;
        private Chart _torqueAngleChart;
        private float _torqueValueCurrent;
        protected DispatcherTimer _updateUiTimer;
        private string _userProvidedTestId;

        // constructor
        public TestBase_VM()
        {
            StartTestCommand = new RelayCommand(StartTest, CanStartTest);
            StopTestCommand = new RelayCommand(StopTest, CanStopTest);
            ExitProgamCommand = new RelayCommand(ExitProgram, CanExitProgram);
            SubmitTestIdCommand = new RelayCommand(SubmitTestId);

            TestBench.Singleton.TestStarted += TestBench_TestStarted;
            TestBench.Singleton.TestCompleted += TestBench_TestCompleted;

            _testData = new List<Sample>();

            CloseProgramMessage = "Close Program";

            EditTestSettingsCommand = new RelayCommand(EditTestSettings, IsRunning);
        }

        /// <summary>
        ///     The unique number the user needs to enter to identify the test
        ///     the just completed.
        /// </summary>
        public string UserProvidedTestId
        {
            get { return _userProvidedTestId; }
            set
            {
                if (_userProvidedTestId != value)
                {
                    _userProvidedTestId = value;
                    OnPropertyChanged();
                }
            }
        }


        private bool IsRunning()
        {
            return !TestBench.Singleton.IsTesting;
        }

        private void EditTestSettings()
        {
            // open dialog box with test settings
            RunSpeedSettings_VM vm = new RunSpeedSettings_VM();
            
            TestTemplate template = TestBench.Singleton.RetrieveCurrentTestTemplate(Session.TestTemplate);
            vm.RunSpeed = template.RunSpeed;
            vm.MoveSpeed = template.MoveSpeed;

            if (vm.ShowDialog())
            {
                TestBench.Singleton.UpdateSpeedParameters(vm.RunSpeed, vm.MoveSpeed);
            }
        }

        /// <summary>
        ///     A method to call just before opening the test window.
        /// </summary>
        public void PrepareToTest(TestSession session)
        {
            Session = session;

            // get the threads rolling, so values update on the screen.
            InitializeThreads();

            // default behavior.
            TestBench.Singleton.LoadDefaultBenchParameters();

            // ready to start testing.  
            TestBench.Singleton.InformReady();
        }

        protected bool CanStopTest()
        {
            bool isTesting = TestBench.Singleton.IsTesting;
            return isTesting;
        }

        protected virtual bool CanStartTest()
        {
            if (TestBench.Singleton.IsTesting)
                return false;
            if (IsTestIdNeeded)
                return false;
            return true;
        }

        protected void StopTest()
        {
            TestBench.Singleton.EmergencyStop();

            StopTestCommand.RaiseCanExecuteChanged();
            EditTestSettingsCommand.RaiseCanExecuteChanged();
        }

        protected abstract void StartTest();

        // exact same logic as StartTest
        protected bool CanExitProgram()
        {
            return CanStartTest();
        }

        protected void ExitProgram()
        {
            if (CloseProgramMessage == Messages.ConfirmCloseApplication())
            {
                Application.Current.Shutdown();
            }
            else
            {
                CloseProgramMessage = Messages.ConfirmCloseApplication();
                // reset
                DispatcherTimer changeBack = new DispatcherTimer();
                changeBack.Interval = new TimeSpan(0, 0, 2);
                changeBack.Tick += ChangeBack_Tick;
                changeBack.Start();
            }
        }

        private void ChangeBack_Tick(object sender, EventArgs e)
        {
            (sender as DispatcherTimer).Stop();
            CloseProgramMessage = Messages.DisplayCloseApplication();
        }

        protected void TestBench_TestCompleted(object sender, TorqueTestEventArgs e)
        {
            if (e.Test != null)
            {
                if (e.Test.ShouldBeSaved && Session != null)
                    if (e.Test is UnidirectionalTorqueTest)
                    {
                        // make the control appear which the user can use 
                        // to enter the torque test id.
                        IsTestIdNeeded = true;

                        DisplayTempMessage(Messages.ScanBarcodeMessage(), GOOD_MESSAGE_BRUSH, 0);
                    }
                    else
                    {
                        // here is the spot that we check if actual test results fall within a 
                        // certain limit of the calibrated item.
                        TestResultAnalyzer testAnalyst = new TestResultAnalyzer();
                        string testResult;
                        if (testAnalyst.CannotAcceptResults(e.Test, Session.WorkId, out testResult))
                        {
                            // set these quick, so they will return to these after the 
                            // temporary test result error message.
                            Instructions = Business.Shared.Messages.StartButtonMessage();
                            MessageBackgroundColor = GOOD_MESSAGE_BRUSH;

                            DisplayTempMessage(testResult, TEST_FAILURE_BRUSH, 30);
                            e.Test.ShouldBeSaved = false;

                            // get ready to start another test.
                            ClearPointsFromChart();
                        }
                        else
                        {
                            // make the control appear which the user can use 
                            // to enter the torque test id.
                            IsTestIdNeeded = true;

                            DisplayTempMessage(Messages.ScanBarcodeMessage(), GOOD_MESSAGE_BRUSH, 0);
                        }
                    }
            }
            else
            {
                DisplayTempMessage(Messages.StartButtonMessage(), GOOD_MESSAGE_BRUSH, 0);
                DisplayTempMessage(Messages.TestCancelledMessage(), ERROR_BRUSH, 3);

                ClearPointsFromChart();
                // start a new test, the user cancelled the current one.
            }

            // order is important, do Stop THEN Start
            StopTestCommand.RaiseCanExecuteChanged();
            StartTestCommand.RaiseCanExecuteChanged();
            ExitProgamCommand.RaiseCanExecuteChanged();
            EditTestSettingsCommand.RaiseCanExecuteChanged();
        }

        protected void TestBench_TestStarted(object sender, TorqueTestEventArgs e)
        {
            if (e.Test != null)
                IsTestIdNeeded = false;
        }

        /// <summary>
        ///     Displays a message to the user for a set period of time, with a set background color. If the
        ///     secondsToDisplay value is 0 or less, the message will be shown until something else changes
        ///     the value.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="duration"></param>
        public void DisplayTempMessage(string message, Brush color, int secondsToDisplay)
        {
            var lastMessage = Instructions;
            Brush lastBrush = MessageBackgroundColor;
            object[] returnAppearance = {lastMessage, lastBrush};

            Instructions = message;
            MessageBackgroundColor = color;

            if (secondsToDisplay > 0)
            {
                DispatcherTimer updateInfoTimer = new DispatcherTimer();

                updateInfoTimer.Interval = new TimeSpan(0, 0, secondsToDisplay);
                updateInfoTimer.Tag = returnAppearance;
                updateInfoTimer.Tick += UpdateInfoTimer_Tick;
                updateInfoTimer.Start();
            }
        }

        private void UpdateInfoTimer_Tick(object sender, EventArgs e)
        {
            var previousState = (sender as DispatcherTimer).Tag as object[];

            if (previousState != null)
            {
                // resets the Instructions and color.
                Instructions = previousState[0] as string;
                MessageBackgroundColor = previousState[1] as Brush;
            }

            (sender as DispatcherTimer).Stop();
        }

        /// <summary>
        ///     The threads that will run throughout the test.
        /// </summary>
        private void InitializeThreads()
        {
            // to update the UI, a timer.
            _updateUiTimer = new DispatcherTimer();
            _updateUiTimer.Interval = new TimeSpan(0, 0, 0, 0, 100); // 100 milliseconds
            _updateUiTimer.Tick += _updateUiTimer_Tick;
            _updateUiTimer.Start();

            // create a thread for monitoring current values.
            _monitoringThread = new Thread(MonitorSensors);
            _monitoringThread.IsBackground = true; // so the application can close without it shutting down.
            _monitoringThread.Start();

            // create a thread to monitor the running test.
            _runningThread = new Thread(MonitorTest);
            _runningThread.IsBackground = true; // so the application can close without it shutting down.
            _runningThread.Start();
        }

        // The stuff that happens every time the UI gets updated.
        //
        private void _updateUiTimer_Tick(object sender, EventArgs e)
        {
            // update screen
            CurrentTorque = _torqueValueCurrent.ToString("n0");
            CurrentAngle = _angleValueCurrent.ToString("n2");

            // updates chart on screen
            if (TestBench.Singleton.IsTesting)
            {
                List<Sample> testData;
                // for calibration purposes, a hack
                if (TorqueAngleChart == null) return;

                // copy test data to not block _testData from getting updated.
                lock (_testData)
                {
                    testData = new List<Sample>();
                    testData.AddRange(_testData);

                    /* only want to add points that are missing from 
                       the graph, not every point every time. The '+ 1' 
                       is to account for the 'dummy' DataPoint, see ChartFactory.cs 
                    */
                    var pointsInGraph = TorqueAngleChart.Series["series1"].Points.Count + 1;

                    // don't need to update each time.
                    TorqueAngleChart.Series.SuspendUpdates();

                    for (var i = pointsInGraph; i < testData.Count; i++)
                        AddPointToChart(testData[i]);

                    var currentTorque = _torqueValueCurrent;
                    var currentAngle = _angleValueCurrent;
                    Application.Current.Dispatcher.BeginInvoke((Action) (() =>
                    {
                        ScaleChartAxis(currentTorque, currentAngle);
                    }));

                    // update chart
                    ScaleChartAxis(currentTorque, currentAngle);

                    // now update the graph.
                    TorqueAngleChart.Series.ResumeUpdates();
                }
            }
            else
            {
                // take the current test and add it to a completed tests list.  
                // it will be saved at a different time.                
                TestBench.Singleton.AddTestToListToSave();
                CleanupCurrentTest();
            }
        }

        // allows each subclass to implement based on the desired behavior.
        protected abstract void ScaleChartAxis(float currentTorque, float currentAngle);

        // this method is virtual in case the torque or angle values need to be modified (- value to + for example)
        // by one of the subclasses.
        protected virtual void AddPointToChart(Sample sample)
        {
            if (TorqueAngleChart != null)
                TorqueAngleChart.Series["series1"].Points.AddXY(sample.Angle, sample.Torque);
        }

        protected abstract void CleanupCurrentTest();

        protected void ClearPointsFromChart()
        {
            // for calibration, probably a hack
            if (TorqueAngleChart == null)
                return;

            try
            {
                TorqueAngleChart.Series.SuspendUpdates();

                // remove all points except the dummy point.
                while (TorqueAngleChart.Series["series1"].Points.Count > 1)
                {
                    DataPoint lastPoint = TorqueAngleChart.Series["series1"].Points.Last();
                    TorqueAngleChart.Series["series1"].Points.Remove(lastPoint);
                }

                TorqueAngleChart.Series.ResumeUpdates();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "ClearPointsFromChart"));
            }
        }

        // Gets the value of each sensor on the test bench, stores
        // the values for later.
        private void MonitorSensors()
        {
            while (true)
                UpdateCurrentValues();
        }

        // Monitors the state of a running test.
        private void MonitorTest()
        {
            while (true)
            {
                // only add data points when the test is running
                if (TestBench.Singleton.IsTesting)
                {
                    Sample dataPoint = TestBench.Singleton.GetState();
                    TestBench.Singleton.AddToCurrentTestData(dataPoint);

                    try
                    {
                        UpdateGraphData();
                    }
                    catch (Exception ex)
                    {
                        Messages.GeneralExceptionMessage(ex, "TestBase_VM.MonitorTest() UpdateGraphData just called.");
                    }
                }
                else
                {
                    ClearTestData();
                }

                TestBench.Singleton.VerifyAlive();
                Thread.Sleep(50);
            }
        }

        protected void ClearTestData()
        {
            lock (_testData)
            {
                _testData.Clear();
            }
        }

        private void UpdateCurrentValues()
        {
            Sample mostRecent = TestBench.Singleton.GetState();

            if (mostRecent != null)
            {
                _torqueValueCurrent = mostRecent.Torque;
                _angleValueCurrent = mostRecent.Angle;
            }
        }

        private void UpdateGraphData()
        {
            List<Sample> currentData = TestBench.Singleton.GetCurrentTestData();
            if (currentData == null)
                return;

            lock (_testData)
            {
                // only add what's missing.
                var startingIndex = currentData.Count - 1;

                if (startingIndex >= 0 && currentData.Count > _testData.Count)
                    for (var i = startingIndex; i < currentData.Count; i++)
                        _testData.Add(currentData[i]);
            }
        }

        private void SubmitTestId()
        {
            try
            {
                if (TestBench.Singleton.HasTestToSave())
                {
					string messageForUser = DataValidator.VerifyTestId(UserProvidedTestId);

					if (messageForUser != null)
                    {
                        DisplayTempMessage(messageForUser, ERROR_BRUSH, 2);
                    }
                    else
                    {
                        DisplayTempMessage("Saving test...", WAITING_BRUSH, 0);
                        SaveTest(UserProvidedTestId);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Business.Shared.Messages.GeneralExceptionMessage(ex, "SubmitTestId"));
            }
        }

        private void SaveTest(string userProvidedTestId)
        {
            try
            {
                // save on a background thread to not lock up the UI.
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += Worker_DoWork;
                worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
                worker.RunWorkerAsync(UserProvidedTestId);
            }
            catch (Exception ex)
            {
                DisplayTempMessage(ex.Message, EXCEPTION_BRUSH, 5);
            }
        }

        private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            var messageForUser = e.Result as string;

            if (messageForUser != null)
                if (messageForUser == Messages.DatabaseSaveSuccessful())
                {
                    // let the user know it's ok to start another test.
                    DisplayTempMessage(Messages.StartButtonMessage(), GOOD_MESSAGE_BRUSH, 0);
                    DisplayTempMessage(messageForUser, GOOD_MESSAGE_BRUSH, 2);
                }
                else
                {
                    DisplayTempMessage(messageForUser, ERROR_BRUSH, 3);
                }

            // hide the control again.
            IsTestIdNeeded = false;

            // this allows the ui timer to enter a portion of the loop that 
            // will add the current test into the DataGrid.
            TestIdReceived = true;
        }

        private void Worker_DoWork(object sender, DoWorkEventArgs e)
        {
            // here is where the test data is saved to the database.
            string messageForUser = TestBench.Singleton.PersistTestData(UserProvidedTestId);
            e.Result = messageForUser;
        }

        #region // Properties //

        public bool TestIdReceived { get; set; }

        /// <summary>
        ///     The current value of torque on the test bench.
        /// </summary>
        public string CurrentTorque
        {
            get { return _displayTorque; }
            set
            {
                if (_displayTorque != value)
                {
                    _displayTorque = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The current value of angle on the test bench.
        /// </summary>
        public string CurrentAngle
        {
            get { return _displayAngle; }
            set
            {
                if (_displayAngle != value)
                {
                    _displayAngle = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The torque test that is currently being displayed
        ///     or ran in the view.
        /// </summary>
        public TorqueTest CurrentTest => TestBench.Singleton.CurrentTest;

        /// <summary>
        ///     A graph to display the data.
        /// </summary>
        public Chart TorqueAngleChart
        {
            get { return _torqueAngleChart; }
            set
            {
                _torqueAngleChart = value;
                CreateDefaultChart();
            }
        }

        internal abstract void CreateDefaultChart();

        /// <summary>
        ///     The current test session.
        /// </summary>
        public TestSession Session
        {
            get { return _session; }
            set
            {
                if (_session != value)
                {
                    _session = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Instructions the viewmodel needs to share with the view (user).
        /// </summary>
        public string Instructions
        {
            get { return _information; }
            set
            {
                if (_information != value)
                {
                    _information = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     The ability to change this in the view model gives the flexibility
        ///     of logic based color changes not based on hard coded values (like
        ///     what XAML only would provide).
        /// </summary>
        public Brush MessageBackgroundColor
        {
            get { return _messageBackgroundColor; }
            set
            {
                if (_messageBackgroundColor != value)
                {
                    _messageBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     Indicates if a test id number is needed from the bench operator.
        /// </summary>
        public bool IsTestIdNeeded
        {
            get { return _isTestIdNeeded; }
            set
            {
                if (_isTestIdNeeded != value)
                {
                    _isTestIdNeeded = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     A command to start the test.
        /// </summary>
        public RelayCommand StartTestCommand { get; }

        /// <summary>
        ///     A command to stop a running test before it's complete.
        /// </summary>
        public RelayCommand StopTestCommand { get; }

        public RelayCommand ExitProgamCommand { get; }

        /// <summary>
        ///     A command that validates the torque test id that the bench
        ///     operator submitted.
        /// </summary>
        public RelayCommand SubmitTestIdCommand { get; }

        public RelayCommand EditTestSettingsCommand { get; }

        /// <summary>
        ///     A message displayed to the user asking them to verify if they really want to
        ///     close the program after they press a button.
        /// </summary>
        public string CloseProgramMessage
        {
            get { return _closeProgramMessage; }
            set
            {
                if (_closeProgramMessage != value)
                {
                    _closeProgramMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        #endregion
    }
}