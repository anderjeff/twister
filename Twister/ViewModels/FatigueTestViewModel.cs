using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
		private float _currentCounterClockwiseTarget;
		private float _currentClockwiseTarget;
		private FatigueTest _fatigueTest;
		private FatigueTestCondition_VM _selectedTestConditionViewModel;
		private int _cycleCount;
		private int _cycleCountDirect;
		private bool _isSimulated;
		private float _currentAngle;
		private float _currentAngleDirect;

		private DispatcherTimer _updateUiTimer;
		private Thread _monitoringThread;
		private Thread _runningThread;
		private float _currentCwPercent;
		private float _currentCcwPercent;

		public FatigueTestViewModel()
		{
			CurrentAngle = 2.032f;
			CurrentClockwiseTarget = 5.25f;
			CurrentCounterClockwiseTarget = -4.23f;
			TestConditions = new ObservableCollection<FatigueTestCondition_VM>();

			RunCommand = new RelayCommand(StartTest);
			PauseCommand = new RelayCommand(PauseTest);
			FinishCommand = new RelayCommand(FinishTest);
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

		public RelayCommand RunCommand { get; private set; }
		public RelayCommand PauseCommand { get; private set; }
		public RelayCommand FinishCommand { get; private set; }
		
		public ObservableCollection<FatigueTestCondition_VM> TestConditions { get; set; }

		private void StartTest()
		{
			InitializeThreads();
			TestBench.Singleton.InformReady();
			TestBench.Singleton.BeginCurrentTest();
		}
		
		private void PauseTest()
		{
			
		}

		private void FinishTest()
		{
			
		}

		private void InitializeThreads()
		{
			// to update the UI, a timer.
			_updateUiTimer = new System.Windows.Threading.DispatcherTimer();
			_updateUiTimer.Interval = new TimeSpan(0, 0, 0, 0, 10); // 100 milliseconds
			_updateUiTimer.Tick += UpdateUiTimerOnTick;
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

		private void MonitorTest()
		{
			//throw new NotImplementedException();
		}

		private void MonitorSensors()
		{
			while (true)
			{
				UpdateCurrentValues();
			}
		}

		private void UpdateCurrentValues()
		{
			Sample mostRecent = TestBench.Singleton.GetState();

			if (mostRecent != null)
			{
				_currentAngleDirect = mostRecent.Angle;
				_cycleCountDirect = TestBench.Singleton.GetCycleCount();
			}
		}

		private void UpdateUiTimerOnTick(object sender, EventArgs e)
		{
			CurrentAngle = _currentAngleDirect;
			SelectedTestConditionViewModel.CyclesCompleted = CycleCount;
			CycleCount = _cycleCountDirect;

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
