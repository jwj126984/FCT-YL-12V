using AllData;
using System;
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace Common.SerialportHelper
{
    public class RelayHelper
    {
        private SerialPort _serialPort;
        private byte[] _receiveBuffer = new byte[1024];
        private int _receiveCount = 0;
        private ManualResetEvent _responseReceivedEvent = new ManualResetEvent(false);
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
        
        /// <summary>
        /// 串口是否已打开
        /// </summary>
        public bool IsOpen
        {
            get { return _serialPort != null && _serialPort.IsOpen; }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="portName">串口名称</param>
        public RelayHelper(string portName,int a=0)
        {
        
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = 115200;
            _serialPort.Parity = Parity.None;
            _serialPort.DataBits = 8;
            _serialPort.StopBits = StopBits.One;
            _serialPort.DataReceived += SerialPort_DataReceived;
            if (a == 0)
            {
                _serialPort.Open();
            }
            _portName = portName;
        }
        
        /// <summary>
        /// 控制继电器开关
        /// </summary>
        /// <param name="channel">继电器通道（1-22）</param>
        /// <param name="status">状态：true为开，false为关</param>
        /// <param name="slaveAddress">从机地址</param>
        /// <returns></returns>
        public bool SetRelayStatus(int channel, bool status, byte slaveAddress = 0x01)
        {
            if (channel < 1 || channel > 22)
            {
                throw new ArgumentException("继电器通道必须在1-22之间");
            }

            // 寄存器地址 = 通道号 - 1
            ushort registerAddress = (ushort)(channel - 1);
            ushort value = status ? (ushort)0x0001 : (ushort)0x0000;

            // 构建写单个寄存器命令 (FC06)
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x06; // 功能码 FC06
            command[2] = (byte)(registerAddress >> 8); // 寄存器地址高字节
            command[3] = (byte)(registerAddress & 0xFF); // 寄存器地址低字节
            command[4] = (byte)(value >> 8); // 值高字节
            command[5] = (byte)(value & 0xFF); // 值低字节

            

            try
            {
                // 发送命令并等待响应（响应长度为8字节）
                byte[] response = SendModbusCommand(command, 8);

                // 检查响应是否正确
                if (response.Length == 8 && response[0] == slaveAddress && response[1] == 0x06)
                {
                    // 延时600ms
                    Thread.Sleep(600);

                    // 读取状态确认
                    return GetRelayStatus(channel, slaveAddress) == status;
                }
                else
                {
                    LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2}) 响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2})控制失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 读取继电器状态
        /// </summary>
        /// <param name="channel">继电器通道（1-22）</param>
        /// <param name="slaveAddress">从机地址</param>
        /// <returns>状态：true为开，false为关</returns>
        public bool GetRelayStatus(int channel, byte slaveAddress = 0x01)
        {
            if (channel < 1 || channel > 22)
            {
                throw new ArgumentException("继电器通道必须在1-22之间");
            }

            // 寄存器地址 = 通道号 - 1
            ushort registerAddress = (ushort)(channel - 1);

            // 构建读寄存器命令 (FC03)
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x03; // 功能码 FC03
            command[2] = (byte)(registerAddress >> 8); // 起始地址高字节
            command[3] = (byte)(registerAddress & 0xFF); // 起始地址低字节
            command[4] = 0x00; // 寄存器数量高字节
            command[5] = 0x01; // 寄存器数量低字节（读取1个）

            LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2}) 读取通道{channel}状态 命令：{BitConverter.ToString(command)}");

            try
            {
                // 发送命令并等待响应（响应长度为7字节：从机地址(1) + 功能码(1) + 字节数(1) + 数据(2) + CRC(2)）
                byte[] response = SendModbusCommand(command, 7);

                // 检查响应是否正确
                if (response.Length == 7 && response[0] == slaveAddress && response[1] == 0x03 && response[2] == 0x02)
                {
                    // 解析数据
                    ushort value = (ushort)((response[3] << 8) | response[4]);
                    return value != 0;
                }
                else
                {
                    LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2}) 响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2})读取状态失败：{ex.Message}");
                return false;
            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                int bytesToRead = _serialPort.BytesToRead;
                if (bytesToRead > 0)
                {
                 
                    _serialPort.Read(_receiveBuffer, _receiveCount, bytesToRead);

                    _receiveCount += bytesToRead;
                    

                    _responseReceivedEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Relay Error in DataReceived: {ex.Message}");
            }
        }

        private void ParseToolControlFrame()
        {
            try
            {
                ushort ioBits = (ushort)((_receiveBuffer[8] << 8) | _receiveBuffer[9]);

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
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"治具控制卡 帧解析失败：{ex.Message}");
            }
        }
        
        /// <summary>
        /// 计算CRC16校验
        /// </summary>
        /// <param name="data">数据</param>
        /// <returns>CRC16值（低字节在前）</returns>
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
            return true;
        }

        /// <summary>
        /// 发送Modbus RTU命令并等待回复
        /// </summary>
        /// <param name="command">命令数据</param>
        /// <param name="expectedResponseLength">期望的响应长度</param>
        /// <returns>响应数据</returns>
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
                    LogHelper.Debug($"继电器控制板 串口未打开，且再次打开失败。{ex.ToString()}");
                    throw new InvalidOperationException($"继电器控制板 串口未打开，且再次打开失败。{ex.Message}");
                }
            }

            _receiveCount = 0;
            _responseReceivedEvent.Reset();

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
                    throw new TimeoutException("继电器控制板 通信超时");
                }

                if (!_responseReceivedEvent.WaitOne((int)remaining.TotalMilliseconds))
                {
                    throw new TimeoutException("继电器控制板 通信超时");
                }
                _responseReceivedEvent.Reset();
            }

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
            CommonMethod.Sleep(10);
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
                    LogHelper.Debug($"FC05(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC05(从机0x{slaveAddress:X2})写入失败：{ex.Message}");
                return false;
            }
        }

        public bool WriteMultipleCoils(ushort startAddress, int count, byte[] data, byte slaveAddress)
        {
            CommonMethod.Sleep(100);
            int byteCount = (count + 7) / 8;
            byte[] command = new byte[7 + byteCount];
            command[0] = slaveAddress;
            command[1] = 0x0F;
            command[2] = (byte)(startAddress >> 8);
            command[3] = (byte)(startAddress & 0xFF);
            command[4] = (byte)(count >> 8);
            command[5] = (byte)(count & 0xFF);
            command[6] = (byte)byteCount;
            Array.Copy(data, 0, command, 7, byteCount);

            try
            {
                byte[] response = SendModbusCommand(command, 8);
                if (response.Length == 8 && response[0] == slaveAddress && response[1] == 0x0F)
                {
                    return true;
                }
                else
                {
                    LogHelper.Debug($"FC0F(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC0F(从机0x{slaveAddress:X2})写入失败：{ex.Message}");
                return false;
            }
        }

        public bool[] ReadCoils(ushort startAddress, int count, byte slaveAddress)
        {
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x01;
            command[2] = (byte)(startAddress >> 8);
            command[3] = (byte)(startAddress & 0xFF);
            command[4] = (byte)(count >> 8);
            command[5] = (byte)(count & 0xFF);

            try
            {
                int responseLength = 3 + (count + 7) / 8 + 2;
                byte[] response = SendModbusCommand(command, responseLength);
                if (response.Length >= 3 && response[0] == slaveAddress && response[1] == 0x01)
                {
                    bool[] result = new bool[count];
                    int byteCount = response[2];
                    for (int i = 0; i < count; i++)
                    {
                        int byteIndex = i / 8;
                        int bitIndex = i % 8;
                        if (byteIndex < byteCount)
                        {
                            result[i] = (response[3 + byteIndex] & (1 << bitIndex)) != 0;
                        }
                    }
                    return result;
                }
                else
                {
                    LogHelper.Debug($"FC01(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC01(从机0x{slaveAddress:X2})读取失败：{ex.Message}");
                return null;
            }
        }

        public ushort[] ReadInputRegisters(ushort startAddress, int count, byte slaveAddress)
        {
            CommonMethod.Sleep(10);
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
                    LogHelper.Debug($"FC04(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC04(从机0x{slaveAddress:X2})读取失败：{ex.Message}");
                return null;
            }
        }

        public bool WriteSingleRegister(ushort address, ushort value, byte slaveAddress)
        {
            byte[] command = new byte[6];
            command[0] = slaveAddress;
            command[1] = 0x06;
            command[2] = (byte)(address >> 8);
            command[3] = (byte)(address & 0xFF);
            command[4] = (byte)(value >> 8);
            command[5] = (byte)(value & 0xFF);

            try
            {
                byte[] response = SendModbusCommand(command, 8);
                if (response.Length == 8 && response[0] == slaveAddress && response[1] == 0x06)
                {
                    return true;
                }
                else
                {
                    LogHelper.Debug($"FC06(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC06(从机0x{slaveAddress:X2})写入失败：{ex.Message}");
                return false;
            }
        }

        public bool WriteMultipleRegisters(ushort startAddress, ushort[] values, byte slaveAddress)
        {
            byte[] command = new byte[7 + values.Length * 2];
            command[0] = slaveAddress;
            command[1] = 0x10;
            command[2] = (byte)(startAddress >> 8);
            command[3] = (byte)(startAddress & 0xFF);
            command[4] = (byte)(values.Length >> 8);
            command[5] = (byte)(values.Length & 0xFF);
            command[6] = (byte)(values.Length * 2);
            for (int i = 0; i < values.Length; i++)
            {
                command[7 + i * 2] = (byte)(values[i] >> 8);
                command[8 + i * 2] = (byte)(values[i] & 0xFF);
            }

            try
            {
                byte[] response = SendModbusCommand(command, 8);
                if (response.Length == 8 && response[0] == slaveAddress && response[1] == 0x10)
                {
                    return true;
                }
                else
                {
                    LogHelper.Debug($"FC10(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC10(从机0x{slaveAddress:X2})写入失败：{ex.Message}");
                return false;
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
                    LogHelper.Debug($"FC03(从机0x{slaveAddress:X2})响应错误：{BitConverter.ToString(response)}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"FC03(从机0x{slaveAddress:X2})读取失败：{ex.Message}");
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

        private uint ConvertToUInt32(ushort high, ushort low)
        {
            return (uint)((high << 16) | low);
        }

        /// <summary>
        /// 初始化继电器控制板
        /// </summary>
        /// <param name="slaveAddress">从机地址</param>
        /// <returns></returns>
        public bool InitRelay(byte slaveAddress = 0x01)
        {
            try
            {
                // 读取所有继电器状态，验证通信是否正常
                for (int i = 1; i <= 22; i++)
                {
                    bool status = GetRelayStatus(i, slaveAddress);
                    LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2}) 通道{i}状态：{status}");
                }
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2})初始化失败：{ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 关闭所有继电器
        /// </summary>
        /// <param name="slaveAddress">从机地址</param>
        /// <returns></returns>
        public bool CloseAllRelays(byte slaveAddress = 0x01)
        {
            try
            {
                // 构建写多个寄存器命令 (FC10)，关闭所有22个继电器
                byte[] command = new byte[6 + 1 + 44]; // 6字节命令头 + 1字节字节数 + 44字节数据
                command[0] = slaveAddress;
                command[1] = 0x10; // 功能码 FC10
                command[2] = 0x00; // 起始地址高字节
                command[3] = 0x00; // 起始地址低字节
                command[4] = 0x00; // 寄存器数量高字节
                command[5] = 0x16; // 寄存器数量低字节（22个）
                command[6] = 0x2C; // 字节数（22×2=44）
                
                // 设置所有寄存器值为0（关闭）
                for (int i = 0; i < 22; i++)
                {
                    command[7 + i * 2] = 0x00; // 高字节
                    command[8 + i * 2] = 0x00; // 低字节
                }

                LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2}) 关闭所有继电器 命令：{BitConverter.ToString(command)}");

                // 发送命令并等待响应（响应长度为8字节）
                byte[] response = SendModbusCommand(command, 8);

                // 检查响应是否正确
                if (response.Length == 8 && response[0] == slaveAddress && response[1] == 0x10)
                {
                    // 延时600ms
                    Thread.Sleep(600);

                    // 验证所有继电器是否已关闭
                   
                    return true;
                }
                else
                {
                    LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2}) 响应错误：{BitConverter.ToString(response)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"继电器控制板(从机地址0x{slaveAddress:X2})关闭所有继电器失败：{ex.Message}");
                return false;
            }
        }

        #region 治具控制卡 (从站地址 0x01)

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

        #region 电阻模拟卡 (从站地址 0x02)

        public bool SetResistance(int channel, int kohms)
        {
            ushort startAddress = 0;
            switch (channel)
            {
                case 1: startAddress = 0x0200; break;
                case 2: startAddress = 0x0200; break;
                case 3: startAddress = 0x0202; break;
                case 4: startAddress = 0x0202; break;
                case 5: startAddress = 0x0204; break;
                case 6: startAddress = 0x0204; break;
                case 7: startAddress = 0x0206; break;
                case 8: startAddress = 0x0206; break;
                default: return false;
            }

            uint rawValue = (uint)(kohms / 50);
            ushort high = (ushort)(rawValue >> 16);
            ushort low = (ushort)(rawValue & 0xFFFF);
            return WriteMultipleRegisters(startAddress, new ushort[] { high, low }, 0x02);
        }

        public uint ReadResistance(int channel)
        {
            ushort startAddress = 0;
            switch (channel)
            {
                case 1: case 2: startAddress = 0x0200; break;
                case 3: case 4: startAddress = 0x0202; break;
                case 5: case 6: startAddress = 0x0204; break;
                case 7: case 8: startAddress = 0x0206; break;
                default: return 0;
            }

            ushort[] values = ReadHoldingRegisters(startAddress, 2, 0x02);
            if (values != null && values.Length == 2)
            {
                return ConvertToUInt32(values[0], values[1]);
            }
            return 0;
        }

        #endregion

        #region 模拟信号卡 (从站地址 0x03)

        public bool Set4_20mA_OUT1(ushort value)
        {
            return WriteSingleRegister(0x0300, value, 0x03);
        }

        public bool Set4_20mA_OUT2(ushort value)
        {
            return WriteSingleRegister(0x0301, value, 0x03);
        }

        public bool Set0_10V_OUT1(ushort value)
        {
            return WriteSingleRegister(0x0303, value, 0x03);
        }

        public bool Set0_10V_OUT2(ushort value)
        {
            return WriteSingleRegister(0x0302, value, 0x03);
        }

        public ushort Read0_10V_IN1()
        {
            ushort[] values = ReadInputRegisters(0x0304, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read0_10V_IN2()
        {
            ushort[] values = ReadInputRegisters(0x0305, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read4_20mA_IN1()
        {
            ushort[] values = ReadInputRegisters(0x0316, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read4_20mA_IN2()
        {
            ushort[] values = ReadInputRegisters(0x0317, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read0_30V_IN1()
        {
            ushort[] values = ReadInputRegisters(0x0318, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read0_30V_IN2()
        {
            ushort[] values = ReadInputRegisters(0x0319, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read0_30V_IN3()
        {
            ushort[] values = ReadInputRegisters(0x031A, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public ushort Read0_30V_IN4()
        {
            ushort[] values = ReadInputRegisters(0x031B, 1, 0x03);
            return values != null && values.Length == 1 ? values[0] : (ushort)0;
        }

        public uint ReadSW_Rx1()
        {
            ushort[] values = ReadInputRegisters(0x0306, 2, 0x03);
            if (values != null && values.Length == 2)
            {
                return ConvertToUInt32(values[0], values[1]);
            }
            return 0;
        }

        public uint ReadSW_Rx2()
        {
            ushort[] values = ReadInputRegisters(0x0308, 2, 0x03);
            if (values != null && values.Length == 2)
            {
                return ConvertToUInt32(values[0], values[1]);
            }
            return 0;
        }

        public uint ReadSW_Rx3()
        {
            ushort[] values = ReadInputRegisters(0x030A, 2, 0x03);
            if (values != null && values.Length == 2)
            {
                return ConvertToUInt32(values[0], values[1]);
            }
            return 0;
        }

        public uint ReadSW_Rx4()
        {
            ushort[] values = ReadInputRegisters(0x030C, 2, 0x03);
            if (values != null && values.Length == 2)
            {
                return ConvertToUInt32(values[0], values[1]);
            }
            return 0;
        }

        public float ReadAnalogNTC1()
        {
            ushort[] values = ReadInputRegisters(0x030E, 2, 0x03);
            if (values != null && values.Length == 2)
            {
                return ConvertToFloat32(values[0], values[1]);
            }
            return float.NaN;
        }

        #endregion

        #region 数字信号卡 (从站地址 0x04)

        public bool ReadDIH(int channel)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x041C; break;
                case 2: address = 0x0403; break;
                case 3: address = 0x041B; break;
                case 4: address = 0x0402; break;
                case 5: address = 0x041A; break;
                case 6: address = 0x0401; break;
                case 7: address = 0x0419; break;
                case 8: address = 0x0400; break;
                default: return false;
            }
            ushort[] values = ReadInputRegisters(address, 1, 0x04);
            return values != null && values.Length == 1 && values[0] != 0;
        }

        public bool ReadDIL(int channel)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x0418; break;
                case 2: address = 0x042A; break;
                case 3: address = 0x042B; break;
                case 4: address = 0x042C; break;
                case 5: address = 0x042D; break;
                case 6: address = 0x042E; break;
                case 7: address = 0x042F; break;
                case 8: address = 0x0430; break;
                default: return false;
            }
            ushort[] values = ReadInputRegisters(address, 1, 0x04);
            return values != null && values.Length == 1 && values[0] != 0;
        }

        public ushort[] ReadDI_PWM(int channel)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x041D; break;
                case 2: address = 0x0404; break;
                case 3: address = 0x0420; break;
                case 4: address = 0x0407; break;
                default: return null;
            }
            return ReadInputRegisters(address, 3, 0x04);
        }

        public bool SetDO_PWM(int channel, ushort frequency, ushort dutyCycle, ushort enable)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x040A; break;
                case 2: address = 0x040D; break;
                case 3: address = 0x0410; break;
                case 4: address = 0x0413; break;
                default: return false;
            }
            return WriteMultipleRegisters(address, new ushort[] { frequency, dutyCycle, enable }, 0x04);
        }

        public ushort[] ReadDO_PWM(int channel)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x040A; break;
                case 2: address = 0x040D; break;
                case 3: address = 0x0410; break;
                case 4: address = 0x0413; break;
                default: return null;
            }
            return ReadHoldingRegisters(address, 3, 0x04);
        }

        public bool SetDOH(int channel, bool enable)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x0417; break;
                case 2: address = 0x0416; break;
                case 3: address = 0x0429; break;
                case 4: address = 0x0437; break;
                case 5: address = 0x0428; break;
                case 6: address = 0x0436; break;
                case 7: address = 0x0427; break;
                case 8: address = 0x0435; break;
                default: return false;
            }
            return WriteSingleCoil(address, enable, 0x04);
        }

        public bool SetDOL(int channel, bool enable)
        {
            ushort address = 0;
            switch (channel)
            {
                case 1: address = 0x0426; break;
                case 2: address = 0x0434; break;
                case 3: address = 0x0425; break;
                case 4: address = 0x0433; break;
                case 5: address = 0x0424; break;
                case 6: address = 0x0432; break;
                case 7: address = 0x0423; break;
                case 8: address = 0x0431; break;
                default: return false;
            }
            return WriteSingleCoil(address, enable, 0x04);
        }

        #endregion

        #region 继电器转接卡1 (从站地址 0x05, 24路)

        public bool SetRelayCard1(int channel, bool enable)
        {
          
            if (channel < 1 || channel > 24) return false;
            ushort address = (ushort)(0x0500 + channel - 1);
            return WriteSingleCoil(address, enable, 0x05);
        }

        public bool CloseAllRelayCard1()
        {
          
            byte[] data = new byte[3];
            return WriteMultipleCoils(0x0500, 24, data, 0x05);
        }

        public bool[] ReadAllRelayCard1()
        {
           
            return ReadCoils(0x0500, 24, 0x05);
        }

        #endregion

        #region 继电器转接卡2 (从站地址 0x06, 32路)

        public bool SetRelayCard2(int channel, bool enable)
        {
           
            if (channel < 1 || channel > 32) return false;
            ushort address = (ushort)(0x0600 + channel - 1);
            return WriteSingleCoil(address, enable, 0x06);
        }

        public bool CloseAllRelayCard2()
        {
           
            byte[] data = new byte[4];
            return WriteMultipleCoils(0x0600, 32, data, 0x06);
        }

        public bool[] ReadAllRelayCard2()
        {
           
            return ReadCoils(0x0600, 32, 0x06);
        }

        #endregion
    }
}