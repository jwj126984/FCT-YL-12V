using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Common.CommonMethod;

namespace Common.SerialportHelper
{
    public class N69206H
    {
        public string PowerType { get; set; } = "N69206H";
        private SerialPort _serialPort;
        private readonly string _endMarker = "\n";
        public N69206H(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = 9600;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        public string SendAndWaitForResponse(string message, bool expectResponse = true, int cf = 0)
        {
            CommonMethod.Sleep(100);
            cf++;
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException($"{PowerType}串口未打开或者连接出现问题，请检查连接线以及数据库配置后重启软件");
            }

            string result = "";
            StringBuilder _receiveBuffer = new StringBuilder();
            ManualResetEvent responseReceivedEvent = new ManualResetEvent(false);
            responseReceivedEvent.Reset();
            _serialPort.DataReceived += serialPort_DataReceived;

            _serialPort.WriteLine(message);

            if (!expectResponse)
            {
                _serialPort.DataReceived -= serialPort_DataReceived;
                return string.Empty;
            }

            if (responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000)))
            {
                _serialPort.DataReceived -= serialPort_DataReceived;
                return result.Trim();
            }
            else
            {
                if (cf > 2)
                {
                    _serialPort.DataReceived -= serialPort_DataReceived;
                    throw new TimeoutException($"{PowerType}超时，请检查");
                }
                else
                {
                    return SendAndWaitForResponse(message, expectResponse, cf);
                }
            }

            void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                try
                {
                    string data = _serialPort.ReadExisting();
                    _receiveBuffer.Append(data);

                    if (_receiveBuffer.ToString().Contains(_endMarker))
                    {
                        int endIndex = _receiveBuffer.ToString().IndexOf(_endMarker);
                        result = _receiveBuffer.ToString(0, endIndex + _endMarker.Length);
                        _receiveBuffer.Remove(0, endIndex + _endMarker.Length);
                        responseReceivedEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"{PowerType}Error in DataReceived: {ex.Message}");
                }
            }
        }

        public bool SetOutput(bool enable)
        {
            LogHelper.Debug($"{PowerType}设置电子负载开启或关闭输入");
            SendAndWaitForResponse($"INPut:STATe {(enable ? "ON" : "OFF")}\r", false);
            return true;
        }

        public bool SetMode(string mode)
        {
            LogHelper.Debug($"{PowerType}设置电子负载模式: {mode}");
            SendAndWaitForResponse($"INPUT:FUNCTION {mode}", false);
            return true;
        }

        public bool SetCCMode()
        {

            LogHelper.Debug($"{PowerType}设置恒流模式");
            SendAndWaitForResponse("STATic:CC:RANGe 1\r", false);

            //SendAndWaitForResponse("STATic:CC:LOW:SLEWRate:RAIse 50\r", false);
            //SendAndWaitForResponse("STATic:CC:LOW:SLEWRate:FALL 50\r", false);

            return true;
        }
        public bool SetCCModeh()
        {

            LogHelper.Debug($"{PowerType}设置恒流模式");
            SendAndWaitForResponse("STATic:CC:RANGe 0\r", false);
            //SendAndWaitForResponse("STATic:CC:HIGH:SLEWRate:RAIse 50\r", false);

            //SendAndWaitForResponse("STATic:CC:HIGH:SLEWRate:FALL 50\r", false);

            return true;
        }

        public bool SetCCModecur(double current)
        {
            LogHelper.Debug($"{PowerType}设置恒流模式: {current} A");

            CommonMethod.Sleep(100);
            SendAndWaitForResponse($"STATic:CC:LOW:LEVel {current}\r", false);
            return true;
        }

        public bool SetCCModecurh(double current)
        {
            LogHelper.Debug($"{PowerType}设置恒流模式: {current} A");

            CommonMethod.Sleep(100);
            SendAndWaitForResponse($"STATic:CC:HIGH:LEVel {current}\r", false);
            return true;
        }

        

        public bool SetCVMode()
        {

            LogHelper.Debug($"{PowerType}设置恒流模式");
            SendAndWaitForResponse("STATic:CV:RANGe 1\r", false);
            //SendAndWaitForResponse("STATic:CC:HIGH:SLEWRate:RAIse 50\r", false);

            //SendAndWaitForResponse("STATic:CC:HIGH:SLEWRate:FALL 50\r", false);

            return true;
        }

        public bool SetCVModevolt(double volt)
        {
            LogHelper.Debug($"{PowerType}设置恒压模式: {volt} A");

            CommonMethod.Sleep(100);
            SendAndWaitForResponse($"STATic:CV:LOW:LEVel {volt}\r", false);
            return true;
        }

        public bool SetCRMode(double resistance)
        {
            LogHelper.Debug($"{PowerType}设置恒阻模式: {resistance} Ω");
            SendAndWaitForResponse("INPUT:FUNCTION CR", false);
            SendAndWaitForResponse($"INPUT:RESISTANCE {resistance}", false);
            return true;
        }

        public bool SetCWMode(double power)
        {
            LogHelper.Debug($"{PowerType}设置恒功率模式: {power} W");
            SendAndWaitForResponse("INPUT:FUNCTION CW", false);
            SendAndWaitForResponse($"INPUT:POWER {power}", false);
            return true;
        }

        public bool SetDynamicMode()
        {
            LogHelper.Debug($"{PowerType}设置动态测试模式");
            SendAndWaitForResponse("INPUT:FUNCTION DYNAMIC", false);
            return true;
        }

        public bool SetDynamicSubMode(string subMode)
        {
            LogHelper.Debug($"{PowerType}设置动态子模式: {subMode}");
            SendAndWaitForResponse($"INPUT:DYNAMIC:MODE {subMode}", false);
            return true;
        }

        public bool SetCurrentHigh(double value)
        {
            LogHelper.Debug($"{PowerType}设置动态高电流值: {value} A");
            SendAndWaitForResponse($"INPUT:CURRENT:HIGH {value}", false);
            return true;
        }

        public bool SetCurrentLow(double value)
        {
            LogHelper.Debug($"{PowerType}设置动态低电流值: {value} A");
            SendAndWaitForResponse($"INPUT:CURRENT:LOW {value}", false);
            return true;
        }

        public bool SetCurrentSlewRise(double value)
        {
            LogHelper.Debug($"{PowerType}设置电流上升斜率: {value} A/us");
            SendAndWaitForResponse($"INPUT:CURRENT:SLEW:RISE {value}", false);
            return true;
        }

        public bool SetCurrentSlewFall(double value)
        {
            LogHelper.Debug($"{PowerType}设置电流下降斜率: {value} A/us");
            SendAndWaitForResponse($"INPUT:CURRENT:SLEW:FALL {value}", false);
            return true;
        }

        public bool SetDynamicFrequency(double frequency)
        {
            LogHelper.Debug($"{PowerType}设置动态频率: {frequency} Hz");
            SendAndWaitForResponse($"INPUT:DYNAMIC:FREQUENCY {frequency}", false);
            return true;
        }

        public bool SetDynamicDuty(double duty)
        {
            LogHelper.Debug($"{PowerType}设置动态占空比: {duty} %");
            SendAndWaitForResponse($"INPUT:DYNAMIC:DUTY {duty}", false);
            return true;
        }

        public bool SetDynamicPulseWidth(double width)
        {
            LogHelper.Debug($"{PowerType}设置动态脉冲宽度: {width} ms");
            SendAndWaitForResponse($"INPUT:DYNAMIC:PULSE:WIDTH {width}", false);
            return true;
        }

        public bool SetOCPLevel(double value)
        {
            LogHelper.Debug($"{PowerType}设置过流保护阈值: {value} A");
            SendAndWaitForResponse($"INPUT:PROTECT:OCP:LEVEL {value}", false);
            return true;
        }

        public bool SetOCPState(bool enable)
        {
            LogHelper.Debug($"{PowerType}设置过流保护状态: {(enable ? "ON" : "OFF")}");
            SendAndWaitForResponse($"INPUT:PROTECT:OCP:STATE {(enable ? "ON" : "OFF")}", false);
            return true;
        }

        public bool SetOCPAction(string action)
        {
            LogHelper.Debug($"{PowerType}设置过流保护动作: {action}");
            SendAndWaitForResponse($"INPUT:PROTECT:OCP:ACTION {action}", false);
            return true;
        }

        public bool SetOVPLevel(double value)
        {
            LogHelper.Debug($"{PowerType}设置过压保护阈值: {value} V");
            SendAndWaitForResponse($"INPUT:PROTECT:OVP:LEVEL {value}", false);
            return true;
        }

        public bool SetOVPState(bool enable)
        {
            LogHelper.Debug($"{PowerType}设置过压保护状态: {(enable ? "ON" : "OFF")}");
            SendAndWaitForResponse($"INPUT:PROTECT:OVP:STATE {(enable ? "ON" : "OFF")}", false);
            return true;
        }

        public bool SetOPPLimit(double value)
        {
            LogHelper.Debug($"{PowerType}设置过功率保护限制: {value} W");
            SendAndWaitForResponse($"INPUT:PROTECT:OPP:LIMIT {value}", false);
            return true;
        }

        public bool SetOPPState(bool enable)
        {
            LogHelper.Debug($"{PowerType}设置过功率保护状态: {(enable ? "ON" : "OFF")}");
            SendAndWaitForResponse($"INPUT:PROTECT:OPP:STATE {(enable ? "ON" : "OFF")}", false);
            return true;
        }

        public bool SetCurrentRange(string range)
        {
            LogHelper.Debug($"{PowerType}设置电流量程: {range}");
            SendAndWaitForResponse($"INPUT:CURRENT:RANGE {range}", false);
            return true;
        }

        public bool SetVoltageRange(string range)
        {
            LogHelper.Debug($"{PowerType}设置电压量程: {range}");
            SendAndWaitForResponse($"INPUT:VOLTAGE:RANGE {range}", false);
            return true;
        }

        public bool SetRemoteSense(bool enable)
        {
            LogHelper.Debug($"{PowerType}设置远端感应: {(enable ? "ON" : "OFF")}");
            SendAndWaitForResponse($"INPUT:SENSE:REMOTE {(enable ? "ON" : "OFF")}", false);
            return true;
        }

        public string GetMeasureVoltage()
        {
            var str = SendAndWaitForResponse("MEASURE:VOLTAGE?");
            LogHelper.Debug($"{PowerType}发送命令：MEASURE:VOLTAGE?，测量电压值：{str} V");
            return str;
        }

        public string GetMeasureCurrent()
        {

            var str = SendAndWaitForResponse("MEAS:CURR?\r");
            LogHelper.Debug($"{PowerType}发送命令：MEASURE:CURRENT?，测量电流值：{str} A");
            return str;
        }

        public string GetMeasurePower()
        {
            var str = SendAndWaitForResponse("MEASURE:POWER?");
            LogHelper.Debug($"{PowerType}发送命令：MEASURE:POWER?，测量功率值：{str} W");
            return str;
        }

        public void Reset()
        {
            LogHelper.Debug($"{PowerType}重置电子负载到默认状态");
            SendAndWaitForResponse("*RST", false);
        }

        public string GetIdentification()
        {
            var str = SendAndWaitForResponse("*IDN?");
            LogHelper.Debug($"{PowerType}发送命令：*IDN?，回复：{str}");
            return str;
        }

        public void SetRemote()
        {
            LogHelper.Debug($"{PowerType}设置远程控制模式");
            SendAndWaitForResponse("SYSTEM:REMOTE", false);
        }

        public bool CheckConnect()
        {
            try
            {
                var id = GetIdentification();
                return !string.IsNullOrEmpty(id);
            }
            catch
            {
                return false;
            }
        }
    }
}