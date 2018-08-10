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
		private float _currentAngleDirect;
		private float _currentCwPercent;
		private float _currentCcwPercent;
		private int _cycleCorrectionCount;
		private int _pointsLogged;
		private string _dataLogPath;

		private DispatcherTimer _updateUiTimer;
		private Thread _monitoringThread;
		private Thread _loadingDataThread;
		private Thread _loggingDataThread;
		private bool _threadsInitialized;

		public FatigueTestViewModel()
		{
			TestConditions = new ObservableCollection<FatigueTestCondition_VM>();

			RunCommand = new RelayCommand(StartTest);
			StopCommand = new RelayCommand(StopTest);

			DataLogPath = "C:\\temp\\twister.dat";
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

		public float CurrentClockwiseTarget
		{
			get => _currentClockwiseTarget;
			set
			{
				_currentClockwiseTarget = value;
				OnPropertyChanged();
			}
		}

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
				TestBench.Singleton.UpdateCurrentCondition(_selectedTestConditionViewModel.Condition);
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

		public RelayCommand RunCommand { get; private set; }
		public RelayCommand StopCommand { get; private set; }
		
		public ObservableCollection<FatigueTestCondition_VM> TestConditions { get; set; }

		private void StartTest()
		{
			InitializeThreads();
			TestBench.Singleton.InformReady();
			TestBench.Singleton.BeginCurrentTest();
		}
		
		private void StopTest()
		{
			TestBench.Singleton.EmergencyStop();
		}
		
		private void InitializeThreads()
		{
			// only need to do this once.
			if (_threadsInitialized) return;

			// to update the UI, a timer.
			_updateUiTimer = new System.Windows.Threading.DispatcherTimer
			{
				Interval = new TimeSpan(0, 0, 0, 0, 10) // 10 milliseconds
			};
			_updateUiTimer.Tick += UpdateUiTimerOnTick;
			_updateUiTimer.Start();

			// create a thread for monitoring current values.
			_monitoringThread = new Thread(MonitorSensors)
			{
				IsBackground = true // so the application can close without it shutting down.
			};
			_monitoringThread.Start();

			// create a thread to get test data
			_loadingDataThread = new Thread(LoadData)
			{
				IsBackground = true // so the application can close without it shutting down.
			};
			_loadingDataThread.Start();

			_loggingDataThread = new Thread(LogData)
			{
				IsBackground = true
			};
			_loggingDataThread.Start();

			_threadsInitialized = true;
		}

		private void LogData()
		{
			// reset data file.
			File.Delete(DataLogPath);
			while (true)
			{
				Thread.Sleep(5000);
				var temp = new List<FatigueTestDataPoint>();
				temp.AddRange(FatigueTest.ProcessedData());

				using (var writer = new StreamWriter(DataLogPath, true))
				{
					foreach (var pt in temp)
					{
						writer.WriteLine($"{pt.CycleNumber},{pt.MaxTorque},{pt.MaxAngle:n3},{pt.MinTorque},{pt.MinAngle:n3}");
					}

					PointsLogged += temp.Count;
				}
			}
		}

		private void MonitorSensors()
		{
			while (true)
			{
				UpdateCurrentValues();
			}
		}

		private void LoadData()
		{
			while (true)
			{
				CreateDataPoint();

				// 15ms was a good value for generating enough data points
				// to catch the min and max for a cycle.
				System.Threading.Thread.Sleep(15); 
			}
		}

		private void CreateDataPoint()
		{
			FatigueTestDataPoint dataPoint;
			lock (_objLock)
			{
				dataPoint = new FatigueTestDataPoint(_cycleCountDirect)
				{
					MaxAngle = _currentAngleDirect,
					MaxTorque = _currentTorqueDirect
				};
			}
				
			FatigueTest.AddTestData(dataPoint);
		}

		private void UpdateCurrentValues()
		{
			Sample mostRecent = TestBench.Singleton.GetState();
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
		}

		private void UpdateUi()
		{
			lock (_objLock)
			{
				CurrentAngle = _currentAngleDirect;
				CycleCount = _cycleCountDirect;
				SelectedTestConditionViewModel.CyclesCompleted = _cycleCountDirect - _cycleCorrectionCount; 
			}
			UpdateCyclingGraphic();
		}

		private void CheckIfNextConditionShouldBeLoaded()
		{
			if (SelectedTestConditionViewModel.CyclesCompleted >= SelectedTestConditionViewModel.CyclesRequired)
			{
				var currentIndex = TestConditions.IndexOf(SelectedTestConditionViewModel);
				var anotherConditionRemains = currentIndex + 1 < TestConditions.Count; 
				if (anotherConditionRemains)
					LoadNextCondition(currentIndex);
				else
					TestBench.Singleton.ManuallyCompleteTestCycle();
			}
		}

		private void LoadNextCondition(int currentIndex)
		{
			// Add to the correction factor.
			_cycleCorrectionCount += SelectedTestConditionViewModel.CyclesCompleted;
			SelectedTestConditionViewModel = TestConditions[currentIndex + 1];
			
			// Let the calibration cycle complete.
			while (TestBench.Singleton.IsDueForCalibration())
			{
				UpdateUi();
				Thread.Sleep(10);
			}

			// Update targets based on the new test condition.
			var tuple = TestBench.Singleton.GetCurrentAngleLimits();
			CurrentClockwiseTarget = tuple.Item1;
			CurrentCounterClockwiseTarget = tuple.Item2;
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
