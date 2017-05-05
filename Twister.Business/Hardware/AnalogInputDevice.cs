using System;

namespace Twister.Business.Hardware
{
    public class AnalogInputDevice : ModbusServer
    {
        // for multithreading
        private static readonly object _objLock = new object();

        public AnalogInputDevice(string ipAddress)
            : base(ipAddress)
        {
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
                    Connect(_ipAddress);

                    // each input register can hold only 2 bytes, we are only reading 1 register
                    var data = new byte[2];

                    // spelled out for clarity
                    ushort transactionId = 1;
                    ushort startAddress = 0;
                    ushort numInputs = 1;

                    ReadInputRegister(transactionId, startAddress, numInputs, ref data);

                    // Data is big endian, windows is little endian, so need to reverse.
                    Array.Reverse(data);

                    // the signed integer value.
                    var value = BitConverter.ToInt16(data, 0);

                    Torque = 0.667f * value;

                    Dispose();
                }
            }
            catch (Exception)
            {
            }
        }
    }
}