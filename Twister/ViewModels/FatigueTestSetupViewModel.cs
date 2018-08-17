using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Tests;
using Twister.Utilities;

namespace Twister.ViewModels
{
	public class FatigueTestSetupViewModel : Base_VM
	{
		private bool _noConditionsDefined;
		private FatigueTest _fatigueTest;
		private bool _canSeeNext;
		private bool _isSimulatedAndCanSeeNext;
		private bool _isSimulated;
		private int _shaftStiffness;

		public FatigueTestSetupViewModel()
		{
			_fatigueTest = new FatigueTest
			{
				TestTemplateId = (int)TestType.FatigueTest
			};

			NoConditionsDefined = true;
			CanSeeNext = false;
			
			TestConditions = new ObservableCollection<FatigueTestCondition>();
			TestConditions.CollectionChanged += TestConditionsOnCollectionChanged;

			AddConditionCommand = new RelayCommand(AddCondition);
			NextCommand = new RelayCommand(GoToNextScreen);
			RemoveConditionCommand = new RelayCommand<FatigueTestCondition>(RemoveCondition);

			ShaftStiffness = 1;
		}

		/// <summary>
		/// Indicates if the test setup has defined any test conditions.
		/// </summary>
		public bool NoConditionsDefined
		{
			get => _noConditionsDefined;
			set
			{
				_noConditionsDefined = value;
				OnPropertyChanged();
			}
		}

		public bool CanSeeNext
		{
			get => _canSeeNext;
			set
			{
				_canSeeNext = value;
				OnPropertyChanged();

				IsSimulatedAndCanSeeNext = (_isSimulated && _canSeeNext);
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

		public bool IsSimulatedAndCanSeeNext
		{
			get => _isSimulatedAndCanSeeNext;
			set
			{
				_isSimulatedAndCanSeeNext = value;
				OnPropertyChanged();
			}
		}

		public int ShaftStiffness
		{
			get => _shaftStiffness;
			set
			{
				if (value > 0)
				{
					_shaftStiffness = value;
					OnPropertyChanged();
				}
			}
		}

		public ObservableCollection<FatigueTestCondition> TestConditions { get; set; }
		public RelayCommand AddConditionCommand { get; private set; }
		public RelayCommand NextCommand { get; private set; }
		public RelayCommand<FatigueTestCondition> RemoveConditionCommand { get; private set; }

		private void TestConditionsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			NoConditionsDefined =  TestConditions.Count == 0;
			CanSeeNext = TestConditions.Count > 0;
		}
		
		private void RemoveCondition(FatigueTestCondition condition)
		{
			_fatigueTest.TestConditions.Remove(condition);
			TestConditions.Remove(condition);
		}

		private void AddCondition()
		{
			var condition = new FatigueTestCondition();
			_fatigueTest.TestConditions.Add(condition);
			TestConditions.Add(condition);
		}

		private void GoToNextScreen()
		{
			var fatigueTestVm = MainWindow_VM.Instance.FatigueTestViewModel;
			fatigueTestVm.FatigueTest = _fatigueTest;

			fatigueTestVm.CurrentClockwiseTarget =
				(float) _fatigueTest.TestConditions.First().ClockwiseTorque / ShaftStiffness;
			fatigueTestVm.CurrentCounterClockwiseTarget =
				(float) _fatigueTest.TestConditions.First().CounterclockwiseTorque / ShaftStiffness;

			TestBench.Singleton.LoadTest(_fatigueTest);
			TestBench.Singleton.SetShaftStiffness(ShaftStiffness);
			MainWindow_VM.Instance.CurrentViewModel = fatigueTestVm;
		}
	}
}
