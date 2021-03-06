﻿using System;
using System.Diagnostics;

namespace Twister.Business.Hardware
{
    public class ServoDrive : IServoDrive
    {
        // 16-bit resolution, 70 : 1 gear ratio
        private const int COUNTS_PER_REV = 65536 * 70;
	    private const string IP_ADDRESS = "128.1.1.35";

		// for multithreading
		private static object _objLock = new object();
	    private readonly ModbusServer _server;

	    /// <summary>
        ///     Constructor.  Requires a valid IP address to the servo drive.
        /// </summary>
        /// <param name="ipAddress">The IPV4 address for the servo drive.</param>
        /// <param name="server">The server that the values are retrieved from and stored to.</param>
        public ServoDrive(ModbusServer server)
        {
	        _server = server;
        }

        // The current angle of rotation of the gearbox
        public float GearboxAngle { get; private set; }

        /// <summary>
        ///     Gets the current rotational position of the output flange,
        ///     and saves the value to the GearboxAngle property.
        /// </summary>
        public void RefreshPosition()
        {
            try
            {
	            lock (_objLock)
	            {
		            _server.Connect(IP_ADDRESS);

		            // reading the loop position feedback value at index 588
		            // see Modbus Parameter Table, pg 344 of user guide.

		            var data = new byte[16];
		            ushort transId = 1;
		            ushort startAddr = 588;
		            ushort numInputs = 4;

		            _server.ReadHoldingRegister(transId, startAddr, numInputs, ref data);

		            if (data != null)
		            {
			            Array.Reverse(data);
			            var value = BitConverter.ToInt64(data, 0);

			            if (value == 0)
				            GearboxAngle = 0f;
			            else
			            {
				            // represents the PL.FB from the ADK drive.
				            long count = 0;

				            if (Math.Abs(value) < COUNTS_PER_REV)
					            count = value;
				            else
					            count = value % COUNTS_PER_REV;

				            GearboxAngle = count * (360f / COUNTS_PER_REV);
			            }
		            }

		            _server.Dispose();
	            }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "** EXCEPTION ** \n\tLocation: ServoDrive.RefreshPosition() \n\tLine:100 \n\t{0} \n\tServoDrive values:\n\t\tGearboxAngle: {1:n2}°",
                    ex.Message, GearboxAngle);
            }
        }

        /// <summary>
        ///     Writes a user specified value to the specified register address.
        /// </summary>
        /// <param name="location">
        ///     The user specified RegisterAddress to which to save the value.
        /// </param>
        /// <param name="value">
        ///     The value to store in the specified RegisterAddress
        /// </param>
        public void StoreParameter(ServoDriveEnums.RegisterAddress location, int value)
        {
            try
            {
                lock (_objLock)
                {
                    //Debug.WriteLine("Entering ServoDrive.StoreParameter() \n\t value: {0}\n\t RegisterLocation: {1}", new object[] { value, location.ToString() });

                    _server.Connect(IP_ADDRESS);

                    // for clarity, defining parameters
                    var byteValue = BitConverter.GetBytes(value);
                    Array.Reverse(byteValue);
                    ushort transId = 1;
                    var startAddress = (ushort) location;
                    var response = new byte[5]; // per Modbus standard, response is 5 bytes.              

                    _server.WriteMultipleRegister(transId, startAddress, byteValue, ref response);

                    _server.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION: \n\t{0}", new object[] {ex.Message});
            }
        }

        /// <summary>
        ///     Writes a user specified value to the specified register address.
        /// </summary>
        /// <param name="location">
        ///     The user specified RegisterAddress to which to save the value.
        /// </param>
        /// <param name="value">
        ///     The value to store in the specified RegisterAddress
        /// </param>
        public void StoreParameter(ServoDriveEnums.RegisterAddress location, float value)
        {
            try
            {
                lock (_objLock)
                {
                    _server.Connect(IP_ADDRESS);

                    // for clarity, defining parameters
                    var byteValue = BitConverter.GetBytes(value);
                    Array.Reverse(byteValue);
                    ushort transId = 1;
                    var startAddress = (ushort) location;
                    var response = new byte[5]; // per Modbus standard, response is 5 bytes.              

                    _server.WriteMultipleRegister(transId, startAddress, byteValue, ref response);

                    _server.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION: \n\t{0}", new object[] {ex.Message});
            }
        }

        /// <summary>
        ///     Gets a value from a specific register address.
        /// </summary>
        /// <param name="location">
        ///     The user specified RegisterAddress
        /// </param>
        /// <returns>
        ///     A 32 bit signed integer representing the value in the
        ///     holding register on the servo drive.
        /// </returns>
        public int RetrieveParameter(ServoDriveEnums.RegisterAddress location)
        {
            try
            {
                // in case the value is not retrieved, I will know with this insane number.
                var value = -5000;

                lock (_objLock)
                {
                    _server.Connect(IP_ADDRESS);

                    // for clarity, defining parameters
                    ushort transId = 1;
                    var startAddress = (ushort) location;
                    ushort qtyRegisters = 2;
                    var response = new byte[8]; // per Modbus standard, response is 8 bytes.              

                    _server.ReadHoldingRegister(transId, startAddress, qtyRegisters, ref response);

                    // response null means the connection was temporarily lost
                    if (response != null)
                    {
                        Array.Reverse(response);
                        value = BitConverter.ToInt32(response, 0);
                    }

                    _server.Dispose();
                }

                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION: \n\t{0}", new object[] {ex.Message});
                return 22; // memorable value, at least today it is...
            }
        }

		public float RetrieveClockwiseLimit()
		{
			var location = ServoDriveEnums.RegisterAddress.ClockwiseAngleLimit;
			var cwLimit = RetrieveAngleLimits(location);
			return cwLimit;
		}

		public float RetrieveCounterclockwiseLimit()
		{
			var location = ServoDriveEnums.RegisterAddress.CounterClockwiseAngleLimit;
			var ccwLimit = RetrieveAngleLimits(location);
			return ccwLimit;
		}

	    private float RetrieveAngleLimits(ServoDriveEnums.RegisterAddress address)
	    {
		    float limit = 0f;
		    lock (_objLock)
		    {
			    _server.Connect(IP_ADDRESS);

			    ushort transId = 1;
			    ushort startAddr = (ushort) address;
			    ushort qtyRegisters = 4; // 64 bit value.
			    var data = new byte[16]; 

			    _server.ReadHoldingRegister(transId, startAddr, qtyRegisters, ref data);

			    if (data != null)
			    {
				    Array.Reverse(data);
				    var value = BitConverter.ToInt64(data, 0);

				    if (value == 0)
					    GearboxAngle = 0f;
				    else
				    {
					    // represents the PL.FB from the ADK drive.
					    long count = 0;

					    if (Math.Abs(value) < COUNTS_PER_REV)
						    count = value;
					    else
						    count = value % COUNTS_PER_REV;

					    limit = count * (360f / COUNTS_PER_REV);
				    }
			    }
			    _server.Dispose();
		    }
		    return limit;
	    }

		public int RetrieveLastCwTorque()
		{
			var location = ServoDriveEnums.RegisterAddress.CwTorqueLastCalibration;
			var cwTorqueLastCalibration = RetrieveParameter(location);
			return cwTorqueLastCalibration;
		}

		public int RetrieveLastCcwTorque()
		{
			var location = ServoDriveEnums.RegisterAddress.CcwTorqueLastCalibration;
			var ccwTorqueLastCalibration = RetrieveParameter(location);
			return ccwTorqueLastCalibration;
		}

		/// <summary>
		/// Very similar to RefreshPosition, only this method returns the current value
		/// of the WHEN.PLFB.  This should be the max or min value during the fatigue test
		/// as it is cycling back and forth.
		/// </summary>
	    public void RefreshLatestWhenPosition()
	    {
            try
            {
                lock (_objLock)
                {
                    _server.Connect(IP_ADDRESS);

                    // reading the loop position feedback value at index 1178
                    // see Modbus Parameter Table, pg 344 of user guide.

                    var data = new byte[16];
                    ushort transId = 1;
                    ushort startAddr = 1178;
                    ushort numInputs = 4;

                    _server.ReadHoldingRegister(transId, startAddr, numInputs, ref data);

                    if (data != null)
                    {
                        Array.Reverse(data);
                        var value = BitConverter.ToInt64(data, 0);

                        if (value == 0)
                            GearboxAngle = 0f;
                        else
                        {
                            // represents the WHEN.PLFB from the ADK drive.
                            long count = 0;

                            if (Math.Abs(value) < COUNTS_PER_REV)
                                count = value;
                            else
                                count = value % COUNTS_PER_REV;

                            GearboxAngle = count * (360f / COUNTS_PER_REV);
                        }
                    }

                    _server.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "** EXCEPTION ** \n\tLocation: ServoDrive.RefreshLatestWhenPosition() \n\tLine:100 \n\t{0} \n\tServoDrive values:\n\t\tGearboxAngle: {1:n2}°",
                    ex.Message, GearboxAngle);
            }
        }

        /// <summary>
        /// Gets the maximum position for the last
        /// </summary>
        /// <returns></returns>
        public float RetrieveLastCwMaxPosition()
        {
            float retPosition = 0f;
            try
            {
                lock (_objLock)
                {
                    _server.Connect(IP_ADDRESS);

                    var data = new byte[16];
                    ushort transId = 1;
                    ushort startAddr = (ushort)ServoDriveEnums.RegisterAddress.CwMaxLastCycle;
                    ushort numInputs = 4;

                    _server.ReadHoldingRegister(transId, startAddr, numInputs, ref data);

                    if (data != null)
                    {
                        Array.Reverse(data);
                        var value = BitConverter.ToInt64(data, 0);

                        if (value == 0)
                            retPosition = 0f;
                        else
                        {
                            // represents the WHEN.PLFB from the ADK drive.
                            long count = 0;

                            if (Math.Abs(value) < COUNTS_PER_REV)
                                count = value;
                            else
                                count = value % COUNTS_PER_REV;

                            retPosition = count * (360f / COUNTS_PER_REV);
                        }
                    }

                    _server.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "** EXCEPTION ** \n\tLocation: ServoDrive.RefreshLatestWhenPosition() \n\tLine:100 \n\t{0} \n\tServoDrive values:\n\t\tGearboxAngle: {1:n2}°",
                    ex.Message, GearboxAngle);
            }
            return retPosition;
        }

        public float RetrieveLastCcwMaxPosition()
        {
            float retPosition = 0f;
            try
            {
                lock (_objLock)
                {
                    _server.Connect(IP_ADDRESS);

                    var data = new byte[16];
                    ushort transId = 1;
                    ushort startAddr = (ushort)ServoDriveEnums.RegisterAddress.CcwMaxLastCycle;
                    ushort numInputs = 4;

                    _server.ReadHoldingRegister(transId, startAddr, numInputs, ref data);

                    if (data != null)
                    {
                        Array.Reverse(data);
                        var value = BitConverter.ToInt64(data, 0);

                        if (value == 0)
                            retPosition = 0f;
                        else
                        {
                            // represents the WHEN.PLFB from the ADK drive.
                            long count = 0;

                            if (Math.Abs(value) < COUNTS_PER_REV)
                                count = value;
                            else
                                count = value % COUNTS_PER_REV;

                            retPosition = count * (360f / COUNTS_PER_REV);
                        }
                    }

                    _server.Dispose();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(
                    "** EXCEPTION ** \n\tLocation: ServoDrive.RefreshLatestWhenPosition() \n\tLine:100 \n\t{0} \n\tServoDrive values:\n\t\tGearboxAngle: {1:n2}°",
                    ex.Message, GearboxAngle);
            }
            return retPosition;
        }
     }
}