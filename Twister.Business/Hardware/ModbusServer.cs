using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Twister.Business.Shared;

namespace Twister.Business.Hardware
{
    /// <summary>
    ///     This represents a Modbus TCP/IP device that can be interrogated for
    ///     information, a server in the Modbus TCP/IP paradigm.
    ///     Written by : Jeff Anderson with a great deal of help from
    ///     the ModbusTcp class found on Code Project at:
    ///     http://www.codeproject.com/Tips/16260/Modbus-TCP-class
    ///     Modbus implementation guide can be found here:
    ///     http://www.modbus.org/docs/Modbus_over_serial_line_V1_02.pdf
    /// </summary>
    public class ModbusServer : IDisposable
    {
        // Where to send packets
        private const ushort MODBUS_PORT = 502;

        // Modbus Function Codes
        private const byte FNC_READ_COIL = 1;

        private const byte FNC_READ_DISCRETE_INPUTS = 2;
        private const byte FNC_READ_HOLDING_REGISTER = 3;
        private const byte FNC_READ_INPUT_REGISTER = 4;
        private const byte FNC_WRITE_SINGLE_COIL = 5;
        private const byte FNC_WRITE_SINGLE_REGISTER = 6;
        private const byte FNC_WRITE_MULTIPLE_COILS = 15;
        private const byte FNC_WRITE_MULTIPLE_REGISTER = 16;
        private const byte FNC_READ_WRITE_MULTIPLE_REGISTER = 23;

        //
        // Exception codes from the MODBUS standard
        // General website:        http://www.modbus.org/specs.php
        // Protocol specification: http://www.modbus.org/docs/Modbus_Application_Protocol_V1_1b3.pdf
        //
        /// <summary>Constant for exception illegal function.</summary>
        private const byte excIllegalFunction = 1;

        /// <summary>Constant for exception illegal data address.</summary>
        private const byte excIllegalDataAdr = 2;

        /// <summary>Constant for exception illegal data value.</summary>
        private const byte excIllegalDataVal = 3;

        /// <summary>Constant for exception slave device failure.</summary>
        private const byte excSlaveDeviceFailure = 4;

        /// <summary>Constant for exception acknowledge.</summary>
        private const byte excAck = 5;

        /// <summary>Constant for exception slave is busy/booting up.</summary>
        private const byte excSlaveIsBusy = 6;

        /// <summary>Constant for exception gate path unavailable.</summary>
        private const byte excGatePathUnavailable = 10;

        /// <summary>Constant for exception not connected.</summary>
        private const byte excExceptionNotConnected = 253;

        /// <summary>Constant for exception connection lost.</summary>
        private const byte excExceptionConnectionLost = 254;

        /// <summary>Constant for exception response timeout.</summary>
        private const byte excExceptionTimeout = 255;

        /// <summary>Constant for exception wrong offset.</summary>
        private const byte excExceptionOffset = 128;

        /// <summary>Constant for exception send failt.</summary>
        private const byte excSendFailt = 100;

        // connection fields
        private static readonly ushort _timeout = 500;

        //private static ushort _refresh = 10; 
        private static bool _connected;

        protected string _ipAddress;

        // synchronous socket and buffer
        private Socket _tcpSynCl;

        private readonly byte[] _tcpSynClBuffer = new byte[2048];

        public ModbusServer(string ipAddress)
        {
            _ipAddress = ipAddress;
        }


        /// <summary>
        ///     Cleans up resources.
        /// </summary>
        public void Dispose()
        {
            // dispose the synchronous socket.
            if (_tcpSynCl != null)
            {
                if (_tcpSynCl.Connected)
                {
                    try
                    {
                        _tcpSynCl.Shutdown(SocketShutdown.Both);
                    }
                    catch
                    {
                    }
                    _tcpSynCl.Close();
                }
                _tcpSynCl = null;
            }
        }

        /// <summary>
        ///     Exception data event. This event is called when the data is incorrect
        /// </summary>
        protected event ExceptionData OnException;

        /// <summary>
        ///     Connects the client to the server.
        /// </summary>
        internal void Connect(string serverIpAddress)
        {
            try
            {
                //Debug.WriteLine("Entering Connect(string serverIpAddress) method \n\tConnecting to {0}...", new object[] { serverIpAddress });
                if (serverIpAddress == null)
                    throw new Exception("IPv4 addresss was never set.");

                IPAddress ip;
                if (IPAddress.TryParse(serverIpAddress, out ip) == false)
                {
                    IPHostEntry hst = Dns.GetHostEntry(serverIpAddress);
                    serverIpAddress = hst.AddressList[0].ToString();
                }

                // synchronous socket
                _tcpSynCl = new Socket(IPAddress.Parse(serverIpAddress).AddressFamily, SocketType.Stream,
                    ProtocolType.Tcp);
                _tcpSynCl.Connect(new IPEndPoint(IPAddress.Parse(serverIpAddress), MODBUS_PORT));
                _tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendTimeout, _timeout);
                _tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, _timeout);
                _tcpSynCl.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, 1);

                _connected = true;

                //Debug.WriteLine("Exiting Connect(string serverIpAddress) method \n\tIpAddress: {0}...", new object[] { serverIpAddress });
            }
            catch (SocketException sktEx)
            {
                Debug.WriteLine(
                    "System.Net.Sockets.SocketException \n\tMessage:{0} \n\tErrorCode: {1} \n\tNativeErrorCode: {2} \n\tSocketErrorCode: {3} \n\tTime: {4: hh:mm:ss.fff tt} \n\tIPv4: {5}",
                    sktEx.Message, sktEx.ErrorCode, sktEx.NativeErrorCode, sktEx.SocketErrorCode, DateTime.Now,
                    serverIpAddress);
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }
        }


        // Create the Application Data Unit for reading input registers.  
        // Returns null if an exception is thrown. 
        private byte[] CreateReadAdu(ushort id, ushort startAddress, ushort length, byte functionCode)
        {
            var data = new byte[12];

            try
            {
                // Transaction Identifier (2 bytes)
                var transId = BitConverter.GetBytes((short) id);
                data[0] = transId[1]; // transaction id high byte
                data[1] = transId[0]; // transaction id low byte

                // Protocol Identifier (2 bytes) 0 for Modbus
                data[2] = 0;
                data[3] = 0;

                // Length field (2 bytes) unit id + function code + data
                data[4] = 0;
                data[5] = 6;

                // Unit Id (1 byte), always zero.
                data[6] = 0;

                // Function Code (1 byte)
                data[7] = functionCode;

                // Start address (2 bytes)
                var _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) startAddress));
                data[8] = _adr[0];
                data[9] = _adr[1];

                // Quantity of Input Registers
                var _length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) length));
                data[10] = _length[0];
                data[11] = _length[1];

                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "CreateReadAdu"));
                return null;
            }
        }

        // Create the Application Data Unit for a write action
        // Returns null if an exception is thrown.
        private byte[] CreateWriteAdu(ushort id, ushort startAddress, ushort numData,
            ushort numBytes, byte function)
        {
            var data = new byte[numBytes + 11];

            try
            {
                // transaction identifier (2 bytes)
                var transId = BitConverter.GetBytes((short) id);
                data[0] = transId[1]; // transaction id high byte
                data[1] = transId[0]; // transaction id low byte

                // Protocol Identifier (2 bytes) 0x00 means Modbus
                data[2] = 0;
                data[3] = 0;

                // Length field (2 bytes), signifies complete message size
                var _size = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) (5 + numBytes)));
                data[4] = _size[0];
                data[5] = _size[1];

                // unit id, always 0
                data[6] = 0;

                // Function Code (1 byte)
                data[7] = function;

                // Start address (2 bytes)
                var _adr = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) startAddress));
                data[8] = _adr[0];
                data[9] = _adr[1];

                if (function >= FNC_WRITE_MULTIPLE_COILS)
                {
                    // quantity of registers to write to
                    var _cnt = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short) numData));
                    data[10] = _cnt[0]; // Number of bytes
                    data[11] = _cnt[1]; // Number of bytes

                    // byte count
                    data[12] = (byte) (numBytes - 2);
                }

                return data;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "CreateWriteAdu"));
                return null;
            }
        }

        // ------------------------------------------------------------------------
        /// <summary>Read input registers from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchonous mode this id is given to the callback function.</param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="numInputs">Length of data.</param>
        /// <param name="values">Contains the result of function.</param>
        protected void ReadInputRegister(ushort transactionId, ushort startAddress, ushort numInputs, ref byte[] values)
        {
            // adu may be null if CreateReadAdu throws exception.
            var adu = CreateReadAdu(transactionId, startAddress, numInputs, FNC_READ_INPUT_REGISTER);

            if (adu != null)
                values = WriteSyncData(adu, transactionId);
            else
                Debug.WriteLine("Information:  adu was null in ReadInputRegister.");
        }

        // ------------------------------------------------------------------------
        /// <summary>Read holding registers from slave synchronous.</summary>
        /// <param name="id">Unique id that marks the transaction. In asynchonous mode this id is given to the callback function.</param>
        /// <param name="startAddress">Address from where the data read begins.</param>
        /// <param name="qtyRegisters">Length of data.</param>
        /// <param name="values">Contains the result of function.</param>
        protected void ReadHoldingRegister(ushort transactionId, ushort startAddress, ushort qtyRegisters,
            ref byte[] values)
        {
            // adu may be null if CreateReadAdu throws exception.
            var adu = CreateReadAdu(transactionId, startAddress, qtyRegisters, FNC_READ_HOLDING_REGISTER);

            if (adu != null)
                values = WriteSyncData(adu, transactionId);
            else
                Debug.WriteLine("Information:  adu was null in ReadHoldingRegister.");
        }

        // ------------------------------------------------------------------------
        /// <summary>Write multiple registers in slave synchronous.</summary>
        /// <param name="transactionId">
        ///     Unique id that marks the transaction. In asynchonous mode this id is given to the callback
        ///     function.
        /// </param>
        /// <param name="startAddress">Address to where the data is written.</param>
        /// <param name="values">Contains the register values to write.</param>
        /// <param name="result">Contains the result of the synchronous write.</param>
        protected void WriteMultipleRegister(ushort transactionId, ushort startAddress, byte[] values,
            ref byte[] result)
        {
            try
            {
                var numBytes = Convert.ToUInt16(values.Length);
                if (numBytes % 2 > 0)
                    numBytes++;

                byte[] data;
                data = CreateWriteAdu(transactionId, startAddress, Convert.ToUInt16(numBytes / 2),
                    Convert.ToUInt16(numBytes + 2), FNC_WRITE_MULTIPLE_REGISTER);

                if (values != null && data != null)
                {
                    Array.Copy(values, 0, data, 13, values.Length);
                    result = WriteSyncData(data, transactionId);
                }
                else
                {
                    Debug.WriteLine("Information: Either values or data was null \n\tLocation: WriteMultipleRegister");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "WriteMultipleRegister"));
            }
        }

        // ------------------------------------------------------------------------
        // Write data and and wait for response
        private byte[] WriteSyncData(byte[] writeData, ushort id)
        {
            //Debug.WriteLine("Entering WriteSyncData \n\t time: {0:hh:MM:ss.fff} \n\t id: {1}", new object[] {DateTime.Now, id });
            if (_tcpSynCl.Connected)
                try
                {
                    //Debug.WriteLine("In WriteSyncData, _tcpSynCl.Connected = true.");
                    //Debug.WriteLine("In WriteSyncData, before Send (request).");

                    _tcpSynCl.Send(writeData, 0, writeData.Length, SocketFlags.None);

                    //Debug.WriteLine("In WriteSyncData, after Send (request), before Receive (response).");

                    var result = _tcpSynCl.Receive(_tcpSynClBuffer, 0, _tcpSynClBuffer.Length, SocketFlags.None);

                    //Debug.WriteLine("In WriteSyncData, after Receive (response). \n\t Result: {0}", new object[] { result });

                    var unit = _tcpSynClBuffer[6];
                    var function = _tcpSynClBuffer[7];
                    byte[] data;

                    if (result == 0)
                        CallException(id, unit, writeData[7], excExceptionConnectionLost);

                    // Response data is slave exception
                    if (function > excExceptionOffset)
                    {
                        function -= excExceptionOffset;
                        CallException(id, unit, function, _tcpSynClBuffer[8]);
                        return null;
                    }

                    // Write response data
                    if (function >= FNC_WRITE_SINGLE_COIL && function != FNC_READ_WRITE_MULTIPLE_REGISTER)
                    {
                        data = new byte[2];
                        Array.Copy(_tcpSynClBuffer, 10, data, 0, 2);
                    }

                    // Read response data
                    else
                    {
                        data = new byte[_tcpSynClBuffer[8]];
                        Array.Copy(_tcpSynClBuffer, 9, data, 0, _tcpSynClBuffer[8]);
                    }
                    return data;
                }
                catch (SystemException ex)
                {
                    Debug.WriteLine(Messages.GeneralExceptionMessage(ex, "WriteSyncData-catch SystemException"));
                    CallException(id, writeData[6], writeData[7], excExceptionConnectionLost);
                }
            else
                CallException(id, writeData[6], writeData[7], excExceptionConnectionLost);

            return null;
        }

        protected void CallException(ushort id, byte unit, byte function, byte exception)
        {
            if (_tcpSynCl == null)
                return;

            if (exception == excExceptionConnectionLost)
                _tcpSynCl = null;

            if (OnException != null)
                OnException(id, unit, function, exception);
        }

        /// <summary>
        ///     Response data event. This event is called when new data arrives
        /// </summary>
        protected delegate void ResponseData(ushort id, byte unit, byte function, byte[] data);

        /// <summary>
        ///     Exception data event. This event is called when the data is incorrect
        /// </summary>
        protected delegate void ExceptionData(ushort id, byte unit, byte function, byte exception);
    }
}