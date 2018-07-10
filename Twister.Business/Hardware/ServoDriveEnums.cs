﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twister.Business.Hardware
{
	public class ServoDriveEnums
	{
		/// <summary>
		///     An enumeration that represents the register location on the ADK BASIC
		///     drive.  Values correspond to Figure 2 in the TT-4000 User Manual.
		/// </summary>
		public enum RegisterAddress
		{
			TestInProcess = 5000, // USER.INT
			SoftwareInitialized = 5002, // USER.INT
			TorqueValue = 5004, // USER.INT
			WatchdogValue = 5006, // USER.INT
			CcwTorqueLimit = 5008, // USER.INT 
			CwTorqueLimit = 5010, // USER.INT
			Runspeed = 5012, // USER.INT
			Manualspeed = 5014, // USER.INT
			DiffLimit = 5016, // USER.FLOAT    
			TorqueDirection = 5018, // USER.INT
			TestType = 5020 // USER.INT
		}
	}
}