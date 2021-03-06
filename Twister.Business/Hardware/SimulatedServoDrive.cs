﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Twister.Business.Hardware
{
	public class SimulatedServoDrive : IServoDrive
	{
        private const int COUNTS_PER_REV = 65536 * 70;
		private int? _previousTorque = null;

		private SimulatorEngine _engine;
		private Dictionary<ServoDriveEnums.RegisterAddress, string> _addressDictionary =
			new Dictionary<ServoDriveEnums.RegisterAddress, string>();

		public SimulatedServoDrive(SimulatorEngine engine)
		{
			_engine = engine;
			MapRegisterAddressesToProperties();
		}

		private void MapRegisterAddressesToProperties()
		{
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.TestInProcess, "TestInProcess");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.SoftwareInitialized, "IsInitialized");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.TorqueValue, "TorqueValue");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.WatchdogValue, "WatchdogValue");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CcwTorqueLimit, "CcwTorqueLimit");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CwTorqueLimit, "CwTorqueLimit");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.Runspeed, "Runspeed");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.Manualspeed, "Manualspeed");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.DiffLimit, "DiffLimit");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.TorqueDirection, "TorqueDirection");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.TestType, "TestType");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.OperatorEndsTest, "OperatorEndsTest");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CycleCount, "CycleCount");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.IsDueForCalibration, "IsDueForCalibration");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CalibrationInterval, "CalibrationInterval");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.ClockwiseAngleLimit, "ClockwiseAngleTicks");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CounterClockwiseAngleLimit, "CounterClockwiseAngleTicks");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CwTorqueLastCalibration, "CwTorqueLastCalibration");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CcwTorqueLastCalibration, "CcwTorqueLastCalibration");
			_addressDictionary.Add(ServoDriveEnums.RegisterAddress.CyclesPerSecond, "CyclesPerSecond");
		}

		#region AKD Basic Program Custom Registers.

		private int _testInProcess;

		private int TestInProcess
		{
			get { return _testInProcess;}
			set
			{
				if (value == -1)
				{
					_engine.StartSimulate();
				}
				else if (value == 0)
				{
					_engine.StopSimulate();
				}
				else
				{
					Console.WriteLine(
						$"Invalid test in process flag provide. -1 means start, 0 means stop, you provided {value}.");
				}
			}
		}
		private int IsInitialized { get; set; }
		private int TorqueValue { get; set; }
		private int WatchdogValue { get; set; }
		private int CwTorqueLimit { get; set; }
		private int CcwTorqueLimit { get; set; }
		private int Runspeed { get; set; }
		private int Manualspeed { get; set; }
		private float DiffLimit { get; set; }
		private int TorqueDirection { get; set; }
		private int TestType { get; set; }
		private int OperatorEndsTest { get; set; }
		private int CycleCount { get; set; }
		private int IsDueForCalibration { get; set; }
		private int CalibrationInterval { get; set; }
		private long ClockwiseAngleTicks { get; set; }
		private long CounterClockwiseAngleTicks { get; set; }
		private float ClockwiseAngleLimit { get; set; }
		private float CounterClockwiseAngleLimit { get; set; }
		private int CwTorqueLastCalibration { get; set; }
		private int CcwTorqueLastCalibration { get; set; }
		private int CyclesPerSecond { get; set; }

		#endregion
		
		/// <summary>
		/// The source of the angle torque values.
		/// </summary>
		public SimulatorEngine Engine => _engine;

		public float Stiffness { get; set; }
		public float GearboxAngle { get; private set; }

		public void RefreshPosition()
		{
			int currentTorque = RetrieveParameter(ServoDriveEnums.RegisterAddress.TorqueValue);
			if (Stiffness > 0)
			{
				GearboxAngle = currentTorque / Stiffness;
			}
			else
			{
				GearboxAngle = 0;
			}

			IncrementCycleCountIfNeeded(currentTorque);
		}

		/// <summary>
		/// If the two values cross the midpoint, then increase the
		/// counter by one.
		/// </summary>
		/// <param name="currentTorque">The current angle, measured when this method is called.</param>
		private void IncrementCycleCountIfNeeded(int currentTorque)
		{
			if (_previousTorque == null)
			{
				_previousTorque = currentTorque;
				// first measurement, no need to compare to anything.
				return;
			}

			int midpoint = _engine.CurrentCondition.VerticalShift;
			if (_previousTorque > midpoint && midpoint >= currentTorque)
			{
				_engine.CurrentCondition.CyclesCompleted++;
				CycleCount++;

				if (IsDueForCalibration == -1)
				{
					PerformCalibration();
				}
			}
			_previousTorque = currentTorque;
		}

		private void PerformCalibration()
		{
			// slow it down
			int tempRunspeed = Runspeed;
			Runspeed = 1;

			ClockwiseAngleLimit = Stiffness == 0 ? 0 : CwTorqueLimit / Stiffness;
			ClockwiseAngleTicks = (long) (COUNTS_PER_REV * ClockwiseAngleLimit / 360);
			CounterClockwiseAngleLimit = Stiffness == 0 ? 0 : CcwTorqueLimit / Stiffness;
			CounterClockwiseAngleTicks = (long) (COUNTS_PER_REV * CounterClockwiseAngleLimit / 360);
			Runspeed = tempRunspeed;
			IsDueForCalibration = 0;

			// simulate getting close to torque for calibration cycle.
			var random = new Random();
			int next = random.Next(97, 103);
			float multiplier = next / 100f;
			CwTorqueLastCalibration = (int) (CwTorqueLimit * multiplier);
			CcwTorqueLastCalibration = (int) (CcwTorqueLimit * multiplier);
		}
		
		public void StoreParameter(ServoDriveEnums.RegisterAddress location, int value)
		{
			GetType().GetProperty(_addressDictionary[location], BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this, value);
		}

		public void StoreParameter(ServoDriveEnums.RegisterAddress location, float value)
		{
			PropertyInfo property = GetType().GetProperty(_addressDictionary[location], BindingFlags.NonPublic | BindingFlags.Instance);
			if (property.PropertyType == typeof(int))
			{
				value = (int) value;
			}
			property.SetValue(this, value);
		}

		public int RetrieveParameter(ServoDriveEnums.RegisterAddress location)
		{
			if (location != ServoDriveEnums.RegisterAddress.DiffLimit)
			{
				return (int)GetType().GetProperty(_addressDictionary[location], BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);
			}

			return (int) this.DiffLimit;
		}

		public float RetrieveClockwiseLimit()
		{
			long cwTicks = (long) GetType().GetProperty(_addressDictionary[ServoDriveEnums.RegisterAddress.ClockwiseAngleLimit],
				BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			var angle = (float) 360 * cwTicks / COUNTS_PER_REV;
			return angle;
		}

		public float RetrieveCounterclockwiseLimit()
		{
			long ccwTicks = (long) GetType().GetProperty(_addressDictionary[ServoDriveEnums.RegisterAddress.CounterClockwiseAngleLimit],
				BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this);

			var angle = (float) 360 * ccwTicks / COUNTS_PER_REV;
			return angle;
		}

		public int RetrieveLastCwTorque()
		{
			return CwTorqueLastCalibration;
		}

		public int RetrieveLastCcwTorque()
		{
			return CcwTorqueLastCalibration;
		}

		public void RefreshLatestWhenPosition()
		{
            Console.WriteLine("Calling RefreshLatestWhenPosition");
		}

        public float RetrieveLastCwMaxPosition()
        {
            return 0f;
        }

        public float RetrieveLastCcwMaxPosition()
        {
            return 0f;
        }
    }
}
