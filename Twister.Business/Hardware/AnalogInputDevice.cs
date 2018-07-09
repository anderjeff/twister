using System;

namespace Twister.Business.Hardware
{
    public class AnalogInputDevice : ITorqueCell
    {
	    private readonly ModbusServer _server;

	    // for multithreading
        private static readonly object _objLock = new object();
	    private const string IP_ADDRESS = "128.1.1.30";

        public AnalogInputDevice(ModbusServer server)
        {
	        _server = server;
        }

        /// <summary>
        ///     The torque from the torque cell, values scaled to
        ///     inch-pounds.
        /// </summary>
        public float Torque { get; private set; }

        public void RefreshTorque()
        {
            try
            {
                lock (_objLock)
                {
                    _server.Connect(IP_ADDRESS);

                    // each input register can hold only 2 bytes, we are only reading 1 register
                    var data = new byte[2];

                    // spelled out for clarity
                    ushort transactionId = 1;
                    ushort startAddress = 0;
                    ushort numInputs = 1;

                    _server.ReadInputRegister(transactionId, startAddress, numInputs, ref data);

                    // Data is big endian, windows is little endian, so need to reverse.
                    Array.Reverse(data);

                    // the signed integer value.
                    var value = BitConverter.ToInt16(data, 0);

                    Torque = 0.667f * value;

                    _server.Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}