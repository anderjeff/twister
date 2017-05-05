using System;
using System.Diagnostics;

namespace Twister.Business.Hardware
{
    public class ServoDrive : ModbusServer
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

        // 16-bit resolution, 70 : 1 gear ratio
        private const int COUNTS_PER_REV = 65536 * 70;

        // for multithreading
        private static object _objLock = new object();

        /// <summary>
        ///     Constructor.  Requires a valid IP address to the servo drive.
        /// </summary>
        /// <param name="ipAddress">The IPV4 address for the servo drive.</param>
        public ServoDrive(string ipAddress)
            : base(ipAddress)
        {
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
                lock (this)
                {
                    //Debug.WriteLine("Entering ServoDrive.RefreshPosition()");

                    Connect(_ipAddress);

                    // reading the loop position feedback value at index 588
                    // see Modbus Parameter Table, pg 344 of user guide.

                    var data = new byte[16];
                    ushort transId = 1;
                    ushort startAddr = 588;
                    ushort numInputs = 4;

                    ReadHoldingRegister(transId, startAddr, numInputs, ref data);

                    if (data != null)
                    {
                        Array.Reverse(data);
                        var value = BitConverter.ToInt64(data, 0);

                        if (value == 0)
                        {
                            GearboxAngle = 0f;
                        }
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

                    Dispose();

                    //Debug.WriteLine("Exiting ServoDrive.RefreshPosition()");
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
        public void StoreParameter(RegisterAddress location, int value)
        {
            try
            {
                lock (this)
                {
                    //Debug.WriteLine("Entering ServoDrive.StoreParameter() \n\t value: {0}\n\t RegisterLocation: {1}", new object[] { value, location.ToString() });

                    Connect(_ipAddress);

                    // for clarity, defining parameters
                    var byteValue = BitConverter.GetBytes(value);
                    Array.Reverse(byteValue);
                    ushort transId = 1;
                    var startAddress = (ushort) location;
                    var response = new byte[5]; // per Modbus standard, response is 5 bytes.              

                    WriteMultipleRegister(transId, startAddress, byteValue, ref response);

                    Dispose();

                    //Debug.WriteLine("Exiting ServoDrive.StoreParameter() \n\t value: {0}\n\t RegisterLocation: {1}", new object[] { value, location.ToString() });
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
        public void StoreParameter(RegisterAddress location, float value)
        {
            try
            {
                lock (this)
                {
                    //Debug.WriteLine("Entering ServoDrive.StoreParameter() \n\t value: {0}\n\t RegisterLocation: {1}", new object[] { value, location.ToString() });

                    Connect(_ipAddress);

                    // for clarity, defining parameters
                    var byteValue = BitConverter.GetBytes(value);
                    Array.Reverse(byteValue);
                    ushort transId = 1;
                    var startAddress = (ushort) location;
                    var response = new byte[5]; // per Modbus standard, response is 5 bytes.              

                    WriteMultipleRegister(transId, startAddress, byteValue, ref response);

                    Dispose();

                    //Debug.WriteLine("Exiting ServoDrive.StoreParameter() \n\t value: {0}\n\t RegisterLocation: {1}", new object[] { value, location.ToString() });
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
        public int RetrieveParameter(RegisterAddress location)
        {
            try
            {
                // in case the value is not retrieved, I will know with this insane number.
                var value = -5000;

                lock (this)
                {
                    //Debug.WriteLine("Entering ServoDrive.RetrieveParameter() \n\t RegisterAddress: {0}", new object[] { location.ToString() });

                    Connect(_ipAddress);

                    // for clarity, defining parameters
                    ushort transId = 1;
                    var startAddress = (ushort) location;
                    ushort qtyRegisters = 2;
                    var response = new byte[8]; // per Modbus standard, response is 8 bytes.              

                    ReadHoldingRegister(transId, startAddress, qtyRegisters, ref response);

                    // response null means the connection was temporarily lost
                    if (response != null)
                    {
                        Array.Reverse(response);
                        value = BitConverter.ToInt32(response, 0);
                    }

                    Dispose();

                    //Debug.WriteLine("Exiting ServoDrive.RetrieveParameter() \n\tRegisterAddress: {0}", new object[] { location.ToString() });
                }

                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("EXCEPTION: \n\t{0}", new object[] {ex.Message});
                return 22; // memorable value, at least today it is...
            }
        }
    }
}