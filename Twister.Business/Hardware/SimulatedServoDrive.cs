﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace Twister.Business.Hardware
{
	public class SimulatedServoDrive : IServoDrive
	{
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
		}

		#region AKD Basic Program Custom Registers.

		private int TestInProcess { get; set; }
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
		
		#endregion

		public int CycleCount { get; set; }
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
			}

			_previousTorque = currentTorque;
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
	}
}
