using System;
using System.Collections.Generic;
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

		public FatigueTest_VM()
		{
			CurrentAngle = 2.032f;
			CurrentClockwiseTarget = 5.235f;
			CurrentCounterClockwiseTarget = -4.273f;
		}

		public FatigueTest FatigueTest { get; set; }

		public float CurrentClockwiseTarget
		{
			get => _currentClockwiseTarget;
			set
			{
				_currentClockwiseTarget = value;
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

		public float CurrentCounterClockwiseTarget
		{
			get => _currentCounterClockwiseTarget;
			set
			{
				_currentCounterClockwiseTarget = value;
				OnPropertyChanged();
			}
		}

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
