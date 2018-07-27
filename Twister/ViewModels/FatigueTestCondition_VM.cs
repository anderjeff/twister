using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twister.Business.Tests;

namespace Twister.ViewModels
{
	public class FatigueTestCondition_VM : Base_VM
	{
		private FatigueTestCondition _testCondition;

		public FatigueTestCondition_VM(FatigueTestCondition testCondition)
		{
			_testCondition = testCondition;
		}

		public FatigueTestCondition Condition => _testCondition;
		
		public int ClockwiseTorque
		{
			get => _testCondition.ClockwiseTorque;
			set
			{
				_testCondition.ClockwiseTorque = value;
				OnPropertyChanged();
			}
		}

		public int CounterclockwiseTorque 
		{
			get => _testCondition.CounterclockwiseTorque;
			set
			{
				_testCondition.CounterclockwiseTorque = value;
				OnPropertyChanged();
			}
		}

		public int CyclesPerSecond
		{
			get => _testCondition.CyclesPerSecond;
			set
			{
				_testCondition.CyclesPerSecond = value;
				OnPropertyChanged();
			}
		}

		public int CalibrationInterval
		{
			get => _testCondition.CalibrationInterval;
			set
			{
				_testCondition.CalibrationInterval = value;
				OnPropertyChanged();
			}
		}

		public int CyclesRequired
		{
			get => _testCondition.CyclesRequired;
			set
			{
				_testCondition.CyclesRequired = value;
				OnPropertyChanged();
			}
		}

		public int CyclesCompleted
		{
			get => _testCondition.CyclesCompleted;
			set
			{
				_testCondition.CyclesCompleted = value;
				OnPropertyChanged();
			}
		}
		
	}
}
