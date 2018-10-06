using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Twister.Business.Data;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
    public class FatigueTestViewModel : Base_VM
    {
        private object _objLock = new object();
        private float _currentCounterClockwiseTarget;
        private float _currentClockwiseTarget;
        private FatigueTest _fatigueTest;
        private FatigueTestCondition_VM _selectedTestConditionViewModel;
        private int _cycleCount;
        private int _cycleCountDirect;
        private bool _isSimulated;
        private float _currentAngle;
        private int _currentTorqueDirect;
        private float _maxCwAngleLastCycle;
        private float _maxCcwAngleLastCycle;
        private float _currentAngleDirect;
        private float _currentCwPercent;
        private float _currentCcwPercent;
        private int _cycleCorrectionCount;
        private int _pointsLogged;
        private string _dataLogPath;
        private int _cwTorqueLastCalibration;
        private int _ccwTorqueLastCalibration;
        private bool _backButtonVisible;
        private bool _testInProcess;

        private DispatcherTimer _updateUiTimer;
        private Thread _monitoringThread;
        private Thread _loadingDataThread;
        private Thread _loggingDataThread;
        private bool _threadsInitialized;
        private FatigueTestCalibration _currentCalibration;
        public List<FatigueTestCalibration> TestCalibrations;
        private bool _isCalibrating;
        private bool _isComplete;

        public FatigueTestViewModel()
        {
            TestConditions = new ObservableCollection<FatigueTestCondition_VM>();

            BackCommand = new RelayCommand(GoBack);
            RunCommand = new RelayCommand(StartTest, CanStartTest);
            StopCommand = new RelayCommand(StopTest, CanStopTest);
            CloseAppCommand = new RelayCommand(CloseApp);

            DataLogPath = "C:\\temp\\twister.dat";
            BackButtonVisible = true;

            // for persisting the calibrations used throughout the test.
            TestCalibrations = new List<FatigueTestCalibration>();
        }

        private void CloseApp()
        {
            System.Windows.Application.Current.Shutdown();
        }

        private void GoBack()
        {
            MainWindow_VM.Instance.CurrentViewModel = MainWindow_VM.Instance.FatigueTestSetupViewModel;
        }

        public FatigueTest FatigueTest
        {
            get => _fatigueTest;
            set
            {
                _fatigueTest = value;

                TestConditions.Clear();
                foreach (var condition in _fatigueTest.TestConditions)
                {
                    TestConditions.Add(new FatigueTestCondition_VM(condition));
                }

                TestBench.Singleton.LoadTest(_fatigueTest);
                SelectedTestConditionViewModel = TestConditions.First();
                IsSimulated = TestBench.Singleton.IsSimulated;
            }
        }

        public bool IsSimulated
        {
            get => _isSimulated;
            set
            {
                _isSimulated = value;
                OnPropertyChanged();
            }
        }

        public bool IsComplete
        {
            get => _isComplete;
            set
            {
                _isComplete = value;
                OnPropertyChanged();
            }
        }
        public bool BackButtonVisible
        {
            get => _backButtonVisible;
            set
            {
                _backButtonVisible = value;
                OnPropertyChanged();
            }
        }

        public float PreviousClockwiseTarget { get; set; }

        public float CurrentClockwiseTarget
        {
            get => _currentClockwiseTarget;
            set
            {
                _currentClockwiseTarget = value;
                OnPropertyChanged();
            }
        }

        public float PreviousCounterClockwiseTarget { get; set; }

        public float CurrentCounterClockwiseTarget
        {
            get => _currentCounterClockwiseTarget;
            set
            {
                _currentCounterClockwiseTarget = value;
                OnPropertyChanged();
            }
        }

        public float CurrentAngle
        {
            get => _currentAngle;
            set
            {
                _currentAngle = value;
                OnPropertyChanged();
            }
        }

        public int CycleCount
        {
            get => _cycleCount;
            set
            {
                _cycleCount = value;
                OnPropertyChanged();
            }
        }

        public float CurrentCwPercent
        {
            get => _currentCwPercent;
            set
            {
                _currentCwPercent = value;
                OnPropertyChanged();
            }
        }

        public float CurrentCcwPercent
        {
            get => _currentCcwPercent;
            set
            {
                _currentCcwPercent = value;
                OnPropertyChanged();
            }
        }

        public FatigueTestCondition_VM SelectedTestConditionViewModel
        {
            get => _selectedTestConditionViewModel;
            set
            {
                _selectedTestConditionViewModel = value;
                OnPropertyChanged();
                TestBench.Singleton.UpdateCurrentCondition(_selectedTestConditionViewModel?.Condition);
            }
        }

        /// <summary>
        /// Gets or sets the number of data points logged.
        /// </summary>
        public int PointsLogged
        {
            get => _pointsLogged;
            set
            {
                _pointsLogged = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets the path to the file where data points are logged.
        /// </summary>
        public string DataLogPath
        {
            get => _dataLogPath;
            set
            {
                _dataLogPath = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The value of the torque in the clockwise direction recorded by the servo
        /// drive at the time of the last calibration cycle.
        /// </summary>
        public int CwTorqueLastCalibration
        {
            get => _cwTorqueLastCalibration;
            set
            {
                _cwTorqueLastCalibration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// The value of the torque in the counterclockwise direction recorded by the servo
        /// drive at the time of the last calibration cycle.
        /// </summary>
        public int CcwTorqueLastCalibration
        {
            get => _ccwTorqueLastCalibration;
            set
            {
                _ccwTorqueLastCalibration = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets or sets whether calibration is in process.
        /// </summary>
	    public bool IsCalibrating
        {
            get => _isCalibrating;
            set
            {
                if(_isCalibrating && value == false)
                {
                    UpdateValuesFromCalibration();
                }

                _isCalibrating = value;
                OnPropertyChanged();
            }
        }


        public RelayCommand BackCommand { get; private set; }
        public RelayCommand RunCommand { get; private set; }
        public RelayCommand StopCommand { get; private set; }
        public RelayCommand CloseAppCommand { get; }

        public ObservableCollection<FatigueTestCondition_VM> TestConditions { get; set; }

        private void StartTest()
        {
            InitializeThreads();
            TestBench.Singleton.BeginCurrentTest();
            TestBench.Singleton.InformReady();

            // always start the test with a calibration.
            IsCalibrating = true;

            // don't let the user press Run when it's running or Stop until it's running.
            _testInProcess = true;
            RunCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();
        }

        private bool CanStartTest()
        {
            return !_testInProcess;
        }

        private void StopTest()
        {
            TestBench.Singleton.EmergencyStop();
            LogDataAndSwitchViews();
        }

        private bool CanStopTest()
        {
            return _testInProcess;
        }

        private void InitializeThreads()
        {
            // only need to do this once.
            if (_threadsInitialized) return;

            // to update the UI, a timer.
            _updateUiTimer = new System.Windows.Threading.DispatcherTimer();
            _updateUiTimer.Interval = new TimeSpan(0, 0, 0, 0, 200); // 200 milliseconds
            _updateUiTimer.Tick += UpdateUiTimerOnTick;
            _updateUiTimer.Start();

            // create a thread for monitoring current values.
            _monitoringThread = new Thread(MonitorSensors);
            _monitoringThread.IsBackground = true; // so the application can close without it shutting down.
            _monitoringThread.Start();

            // create a thread to get test data
            _loadingDataThread = new Thread(LoadData);
            _loadingDataThread.IsBackground = true; // so the application can close without it shutting down.
            _loadingDataThread.Start();

            // created to persist data
            _loggingDataThread = new Thread(LogData);
            _loggingDataThread.IsBackground = true;
            _loggingDataThread.Start();

            _threadsInitialized = true;
            BackButtonVisible = false;
        }

        private void LogData()
        {
            // reset data file.
            if (File.Exists(DataLogPath))
            {
                File.Copy(DataLogPath, $"{DataLogPath}_{DateTime.Now.Ticks}");
                File.Delete(DataLogPath);

                // write the header for the new data file.
                using (var writer = new StreamWriter(DataLogPath, true))
                {
                    writer.WriteLine("CycleNumber,MaxTorque,MaxAngle,MinTorque,MinAngle");
                }
            }
            while (true)
            {
                Thread.Sleep(10000);
                LogAvailableData();
            }
        }

        private void LogAvailableData()
        {
            // must have a calibration before we can log.
            if (_currentCalibration == null) return;

            var temp = new List<FatigueTestDataPoint>();
            temp.AddRange(FatigueTest.ProcessedData());

            using (var writer = new StreamWriter(DataLogPath, true))
            {
                foreach (var pt in temp)
                {
                    pt.MaxTorque = _currentCalibration.CalculatedTorqueFromAngle(pt.MaxAngle);
                    pt.MinTorque = _currentCalibration.CalculatedTorqueFromAngle(pt.MinAngle);
                    writer.WriteLine($"{pt.CycleNumber},{pt.MaxTorque},{pt.MaxAngle:n3},{pt.MinTorque},{pt.MinAngle:n3}");
                }

                PointsLogged += temp.Count;
            }
        }

        private void MonitorSensors()
        {
            while (true)
            {
                Thread.Sleep(30);
                UpdateCurrentValues();
                TestBench.Singleton.VerifyAlive();
            }
        }

        private void LoadData()
        {
            while (true)
            {
                FatigueTestDataPoint dataPoint;
                lock (_objLock)
                {
                    dataPoint = new FatigueTestDataPoint(_cycleCountDirect)
                    {
                        MaxAngle = TestBench.Singleton.GetMaxCwAngleLastCycle(),
                        MinAngle = TestBench.Singleton.GetMaxCcwAngleLastCycle()
                    };
                }
                FatigueTest.AddTestData(dataPoint);

                Thread.Sleep(105);
            }
        }

        private void UpdateCurrentValues()
        {
            Sample mostRecent = TestBench.Singleton.GetFatigueTestState();
            if (mostRecent != null)
            {
                lock (_objLock)
                {
                    _currentTorqueDirect = (int)mostRecent.Torque;
                    _currentAngleDirect = mostRecent.Angle;
                    _cycleCountDirect = TestBench.Singleton.GetCycleCount();
                }
            }
        }

        private void UpdateUiTimerOnTick(object sender, EventArgs e)
        {
            UpdateUi();
            CheckIfNextConditionShouldBeLoaded();
            IsCalibrating = TestBench.Singleton.IsDueForCalibration();        
        }

        private void UpdateUi()
        {
            lock (_objLock)
            {
                CurrentAngle = _currentAngleDirect;
                CycleCount = _cycleCountDirect;
                SelectedTestConditionViewModel.CyclesCompleted = _cycleCountDirect - _cycleCorrectionCount;
            }

            if (IsSimulated) UpdateCyclingGraphic();
        }

        private void CheckIfNextConditionShouldBeLoaded()
        {
            if (SelectedTestConditionViewModel.CyclesCompleted >= SelectedTestConditionViewModel.CyclesRequired)
            {
                var currentIndex = TestConditions.IndexOf(SelectedTestConditionViewModel);
                var anotherConditionRemains = currentIndex + 1 < TestConditions.Count;
                if (anotherConditionRemains)
                {
                    LoadNextCondition(currentIndex);
                }
                else
                {
                    // clean up and load next view.
                    TestBench.Singleton.ManuallyCompleteTestCycle();
                    _testInProcess = false;
                    StopCommand.RaiseCanExecuteChanged();
                    IsComplete = true;
                    //LogDataAndSwitchViews();
                }
            }
        }

        private void LogDataAndSwitchViews()
        {
            _loggingDataThread.Abort();
            while (_loggingDataThread.ThreadState == ThreadState.Running)
            {
                Thread.Sleep(50);
            }
            LogAvailableData();

            // just so we can see what is happening to the data points.
            Thread.Sleep(1000);

            IsComplete = true;
        }

        private void LoadNextCondition(int currentIndex)
        {
            // Add to the correction factor.
            _cycleCorrectionCount += SelectedTestConditionViewModel.CyclesCompleted;
            SelectedTestConditionViewModel = TestConditions[currentIndex + 1];
        }

        private void UpdateValuesFromCalibration()
        {
            // verify that a shutdown is not required.
            PreviousClockwiseTarget = CurrentClockwiseTarget;
            PreviousCounterClockwiseTarget = CurrentCounterClockwiseTarget;

            // Update targets based on the new test condition.
            var tuple = TestBench.Singleton.GetCalibrationResults();
            CurrentClockwiseTarget = tuple.Item1;
            CurrentCounterClockwiseTarget = tuple.Item2;
            CwTorqueLastCalibration = tuple.Item3;
            CcwTorqueLastCalibration = tuple.Item4;

            _currentCalibration = new FatigueTestCalibration(
                    CurrentClockwiseTarget, CurrentCounterClockwiseTarget,
                    CwTorqueLastCalibration, CcwTorqueLastCalibration);
        }

        private void UpdateCyclingGraphic()
        {
            if (CurrentAngle > 0)
            {
                if (CurrentClockwiseTarget == 0f)
                {
                    CurrentCwPercent = 0f;
                }
                else
                {
                    CurrentCwPercent = CurrentAngle / CurrentClockwiseTarget;
                }
                CurrentCcwPercent = 0f;
            }
            else if (CurrentAngle < 0)
            {
                if (CurrentCounterClockwiseTarget == 0f)
                {
                    CurrentCcwPercent = 0f;
                }
                else
                {
                    CurrentCcwPercent = CurrentAngle / CurrentCounterClockwiseTarget;
                }
                CurrentCwPercent = 0f;
            }
            else
            {
                CurrentCwPercent = 0f;
                CurrentCcwPercent = 0f;
            }
        }
    }
}
