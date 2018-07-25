using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Tests;

namespace Twister.ViewModels
{
	public class FatigueTest_VM : TestBase_VM
	{
		private float _currentCounterClockwiseTarget;
		private float _currentClockwiseTarget;
		private float _currentAngle;
		private FatigueTest _fatigueTest;
		private FatigueTestCondition_VM _selectedTestCondition;

		public FatigueTest_VM()
		{
			CurrentAngle = "2.032";
			CurrentClockwiseTarget = 5.235f;
			CurrentCounterClockwiseTarget = -4.273f;
			TestConditions = new ObservableCollection<FatigueTestCondition_VM>();
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
			}
		}

		public FatigueTestCondition_VM SelectedTestCondition
		{
			get => _selectedTestCondition;
			set
			{
				_selectedTestCondition = value;
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

		public ObservableCollection<FatigueTestCondition_VM> TestConditions { get; set; }

		protected override void StartTest()
		{
			throw new NotImplementedException();
		}

		protected override void ScaleChartAxis(float currentTorque, float currentAngle)
		{
			throw new NotImplementedException();
		}

		protected override void CleanupCurrentTest()
		{
			throw new NotImplementedException();
		}

		internal override void CreateDefaultChart()
		{
			throw new NotImplementedException();
		}
	}
}
