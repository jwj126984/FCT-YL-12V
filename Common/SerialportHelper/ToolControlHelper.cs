using AllData;
using System;
using System.IO.Ports;
using System.Threading;

namespace Common.SerialportHelper
{
    public class ToolControlHelper
    {
        private SerialPort _serialPort;
        private byte[] _receiveBuffer = new byte[1024];
        private int _receiveCount = 0;
        private ManualResetEvent _responseReceivedEvent = new ManualResetEvent(false);
        private bool _isWaitingForModbusResponse = false;
        string _portName = "";

        private struct ToolControlData
        {
            public bool DIH1;
            public bool DIH2;
            public bool DIL1;
            public bool DIL2;
            public bool DIL3;
            public bool DIL4;
            public bool DIL5;
            public bool DIL6;
            public float NTC1;
            public float NTC3;
        }

        private ToolControlData _cachedData = new ToolControlData();
        private DateTime _lastReadTime = DateTime.MinValue;

        public bool IsOpen
        {
            get { return _serialPort != null && _serialPort.IsOpen; }
        }

        public ToolControlHelper(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            _portName = portName;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                    byte[] tempBuffer = new byte[bytesToRead];
                    _serialPort.Read(tempBuffer, 0, bytesToRead);

                    if (_isWaitingForModbusResponse)
                    {
                        for (int i = 0; i < bytesToRead; i++)
                        {
                            _receiveBuffer[_receiveCount++] = tempBuffer[i];
                        }
                    }
                    else
                    {
                        for (int i = 0; i < bytesToRead; i++)
                        {
                            _receiveBuffer[_receiveCount++] = tempBuffer[i];

                            if (_receiveCount >= 3)
                            {
                                if (_receiveBuffer[0] == 0xAA && _receiveBuffer[1] == 0x55 && _receiveBuffer[2] == 0x11)
                                {
                                    if (_receiveCount >= 18)
                                    {
                                        ParseToolControlFrame();
                                        _receiveCount = 0;
                                    }
                                }
                                else
                                {
                                    for (int j = 0; j < _receiveCount - 1; j++)
                                    {
                                        _receiveBuffer[j] = _receiveBuffer[j + 1];
                                    }
                                    _receiveCount--;

                                    if (_receiveCount >= 3 && _receiveBuffer[0] == 0xAA && _receiveBuffer[1] == 0x55 && _receiveBuffer[2] == 0x11)
                                    {
                                        if (_receiveCount >= 18)
                                        {
                                            ParseToolControlFrame();
                                            _receiveCount = 0;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    _responseReceivedEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ToolControl Error in DataReceived: {ex.Message}");
            }
        }

        private void ParseToolControlFrame()
        {
            try
            {
                ushort ioBits = (ushort)((_receiveBuffer[6] << 8) | _receiveBuffer[7]);

                _cachedData.DIL1 = (ioBits & 0x0001) != 0;
                _cachedData.DIL2 = (ioBits & 0x0002) != 0;
                _cachedData.DIL3 = (ioBits & 0x0004) != 0;
                _cachedData.DIL4 = (ioBits & 0x0008) != 0;
                _cachedData.DIL5 = (ioBits & 0x0010) != 0;
                _cachedData.DIL6 = (ioBits & 0x0020) != 0;
                _cachedData.DIH1 = (ioBits & 0x0040) != 0;
                _cachedData.DIH2 = (ioBits & 0x0080) != 0;

                byte[] ntc1Bytes = new byte[4];
                Array.Copy(_receiveBuffer, 10, ntc1Bytes, 0, 4);
                _cachedData.NTC1 = BitConverter.ToSingle(ntc1Bytes, 0);

                byte[] ntc3Bytes = new byte[4];
                Array.Copy(_receiveBuffer, 14, ntc3Bytes, 0, 4);
                _cachedData.NTC3 = BitConverter.ToSingle(ntc3Bytes, 0);

                _lastReadTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"治具控制卡 帧解析失败：{ex.Message}");
            }
        }

        private byte[] CalculateCRC16(byte[] data)
        {
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Length; i++)
            {
                crc ^= data[i];
                for (int j = 0; j < 8; j++)
                {
                    if ((crc & 0x0001) != 0)
                    {
                        crc >>= 1;
                        crc ^= 0xA001;
                    }
                    else
                    {
                        crc >>= 1;
                    }
                }
            }
            return new byte[] { (byte)(crc & 0xFF), (byte)(crc >> 8) };
        }

        private bool ReadToolControlData()
        {
            return _lastReadTime != DateTime.MinValue;
        }

        private byte[] SendModbusCommand(byte[] command, int expectedResponseLength)
        {
            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort = new SerialPort(_portName);
                    _serialPort.BaudRate = 115200;
                    _serialPort.Parity = Parity.None;
                    _serialPort.DataBits = 8;
                    _serialPort.StopBits = StopBits.One;
                    _serialPort.DataReceived += SerialPort_DataReceived;
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    LogHelper.Debug($"治具控制卡 串口未打开，且再次打开失败。{ex.ToString()}");
                    throw new InvalidOperationException($"治具控制卡 串口未打开，且再次打开失败。{ex.Message}");
                }
            }

            _receiveCount = 0;
            _responseReceivedEvent.Reset();
            _isWaitingForModbusResponse = true;

            byte[] crc = CalculateCRC16(command);
            byte[] fullCommand = new byte[command.Length + 2];
            Array.Copy(command, fullCommand, command.Length);
            Array.Copy(crc, 0, fullCommand, command.Length, 2);

            _serialPort.Write(fullCommand, 0, fullCommand.Length);

            DateTime deadline = DateTime.Now.AddMilliseconds(CommonData.AwaitTime * 1000);
            while (_receiveCount < expectedResponseLength)
            {
                TimeSpan remaining = deadline - DateTime.Now;
                if (remaining.TotalMilliseconds <= 0)
                {
                    _isWaitingForModbusResponse = false;
                    throw new TimeoutException("治具控制卡 通信超时");
                }

                if (!_responseReceivedEvent.WaitOne((int)remaining.TotalMilliseconds))
                {
                    _isWaitingForModbusResponse = false;
                    throw new TimeoutException("治具控制卡 通信超时");
                }
                _responseReceivedEvent.Reset();
            }

            _isWaitingForModbusResponse = false;
            byte[] response = new byte[expectedResponseLength];
            Array.Copy(_receiveBuffer, response, expectedResponseLength);
            return response;
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public bool WriteSingleCoil(ushort address, bool status, byte slaveAddress)
        {
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x05;
            command[2] = (byte)(address >> 8);
            command[3] = (byte)(address & 0xFF);
            command[4] = status ? (byte)0xFF : (byte)0x00;
            command[5] = 0x00;

            try
            {
                byte[] response = SendModbusCommand(command, 8);
                if (response.Length == 8 && response[0] == slaveAddress && response[1] == 0x05)
                {
                    return (response[4] == 0xFF && response[5] == 0x00) == status;
                }
                else
                {
                    LogHelper.Debug($"治具控制卡 FC05(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"治具控制卡 FC05(从机0x{slaveAddress:X2})写入失败：{ex.Message}");
                return false;
            }
        }

        public ushort[] ReadInputRegisters(ushort startAddress, int count, byte slaveAddress)
        {
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x04;
            command[2] = (byte)(startAddress >> 8);
            command[3] = (byte)(startAddress & 0xFF);
            command[4] = (byte)(count >> 8);
            command[5] = (byte)(count & 0xFF);

            try
            {
                int responseLength = 3 + count * 2 + 2;
                byte[] response = SendModbusCommand(command, responseLength);
                if (response.Length >= 3 && response[0] == slaveAddress && response[1] == 0x04)
                {
                    ushort[] result = new ushort[count];
                    for (int i = 0; i < count; i++)
                    {
                        result[i] = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
                    }
                    return result;
                }
                else
                {
                    LogHelper.Debug($"治具控制卡 FC04(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"治具控制卡 FC04(从机0x{slaveAddress:X2})读取失败：{ex.Message}");
                return null;
            }
        }

        public ushort[] ReadHoldingRegisters(ushort startAddress, int count, byte slaveAddress)
        {
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x03;
            command[2] = (byte)(startAddress >> 8);
            command[3] = (byte)(startAddress & 0xFF);
            command[4] = (byte)(count >> 8);
            command[5] = (byte)(count & 0xFF);

            try
            {
                int responseLength = 3 + count * 2 + 2;
                byte[] response = SendModbusCommand(command, responseLength);
                if (response.Length >= 3 && response[0] == slaveAddress && response[1] == 0x03)
                {
                    ushort[] result = new ushort[count];
                    for (int i = 0; i < count; i++)
                    {
                        result[i] = (ushort)((response[3 + i * 2] << 8) | response[4 + i * 2]);
                    }
                    return result;
                }
                else
                {
                    LogHelper.Debug($"治具控制卡 FC03(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"治具控制卡 FC03(从机0x{slaveAddress:X2})读取失败：{ex.Message}");
                return null;
            }
        }

        private float ConvertToFloat32(ushort high, ushort low)
        {
            byte[] bytes = new byte[4];
            bytes[0] = (byte)(high >> 8);
            bytes[1] = (byte)(high & 0xFF);
            bytes[2] = (byte)(low >> 8);
            bytes[3] = (byte)(low & 0xFF);
            return BitConverter.ToSingle(bytes, 0);
        }

        #region 治具控制卡方法 (从站地址 0x01)

        public float ReadNTC1()
        {
            if (ReadToolControlData())
            {
                return _cachedData.NTC1;
            }
            return float.NaN;
        }

        public float ReadNTC2()
        {
            if (ReadToolControlData())
            {
                return _cachedData.NTC3;
            }
            return float.NaN;
        }

        public bool ReadDIH1()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIH1;
            }
            return false;
        }

        public bool ReadDIH2()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIH2;
            }
            return false;
        }

        public bool ReadDIL1()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIL1;
            }
            return false;
        }

        public bool ReadDIL2()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIL2;
            }
            return false;
        }

        public bool ReadDIL3()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIL3;
            }
            return false;
        }

        public bool ReadDIL4()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIL4;
            }
            return false;
        }

        public bool ReadDIL5()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIL5;
            }
            return false;
        }

        public bool ReadDIL6()
        {
            if (ReadToolControlData())
            {
                return _cachedData.DIL6;
            }
            return false;
        }

        public bool SetDH1(bool enable)
        {
            return WriteSingleCoil(0x0106, enable, 0x01);
        }

        public bool SetDH2(bool enable)
        {
            return WriteSingleCoil(0x010F, enable, 0x01);
        }

        public bool SetDL1(bool enable)
        {
            return WriteSingleCoil(0x0111, enable, 0x01);
        }

        public bool SetDL2(bool enable)
        {
            return WriteSingleCoil(0x0108, enable, 0x01);
        }

        public bool SetDL3(bool enable)
        {
            return WriteSingleCoil(0x0110, enable, 0x01);
        }

        public bool SetDL4(bool enable)
        {
            return WriteSingleCoil(0x0107, enable, 0x01);
        }

        #endregion
    }
}