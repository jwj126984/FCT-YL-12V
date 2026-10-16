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
    public class IT6322
    {
        public string PowerType { get; set; } = "IT6322";
        private SerialPort _serialPort;
        private readonly string _endMarker = "\n"; // 结束标志
        public IT6322(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = 9600;
            _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }
        /// <summary>
        /// 发送消息并等待回复
        /// </summary>
        /// <param name="message">发送的消息</param>
        /// <param name="expectResponse">是否等待回复</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string SendAndWaitForResponse(string message, bool expectResponse = true, int cf = 0)
        {
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


            // 发送数据
            _serialPort.WriteLine(message);

            if (!expectResponse)
            {
                _serialPort.DataReceived -= serialPort_DataReceived;
                // 如果不需要等待响应，直接返回空字符串
                return string.Empty;
            }

            // 等待接收到完整响应
            if (responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
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
                    // 读取串口数据
                    string data = _serialPort.ReadExisting();
                    _receiveBuffer.Append(data);

                    // 检查是否包含结束标志
                    if (_receiveBuffer.ToString().Contains(_endMarker))
                    {
                        // 提取完整数据
                        int endIndex = _receiveBuffer.ToString().IndexOf(_endMarker);
                        result = _receiveBuffer.ToString(0, endIndex + _endMarker.Length);

                        // 清理缓冲区已处理部分
                        _receiveBuffer.Remove(0, endIndex + _endMarker.Length);

                        // 通知主线程响应已接收
                        responseReceivedEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"{PowerType}Error in DataReceived: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 查询电源输出电压值(单位：V)。
        /// </summary>
        /// <returns></returns>
        public string SelectVol()
        {
            var str = SendAndWaitForResponse("MEASure:VOLTage?");
            LogHelper.Debug($"{PowerType}发送命令：MEASure:VOLTage?，查询回显电压值：{str} V");
            return str;
        }

        /// <summary>
        /// 查询电源输出电流值(单位：A)。
        /// </summary>
        /// <returns></returns>
        public string SelectCurr()
        {
            var str = SendAndWaitForResponse("MEASure:CURRent?");
            LogHelper.Debug($"{PowerType}发送命令：MEASure:CURRent?，查询回显电流值：{str} A");
            return str;
        }

        /// <summary>
        /// 查询电源输出功率值(单位：W)。
        /// </summary>
        /// <returns></returns>
        public string SelectPower()
        {
            var str = SendAndWaitForResponse("MEASure:POWer?");
            LogHelper.Debug($"{PowerType}发送命令：MEASure:POWer?，查询回显功率：{str} W");
            return str;
        }

        /// <summary>
        /// 设置电源开启或关闭输出
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public bool SetOutput(bool enable)
        {
            LogHelper.Debug($"{PowerType}设置电源开启或关闭输出");
            SendAndWaitForResponse($"OUTP {(enable ? "ON" : "OFF")}", false);
            LogHelper.Debug($"{PowerType}发送命令：OUTPut:STATE {(enable ? "ON" : "OFF")}");
            
            return true;
            
        }

        /// <summary>
        /// 设置电源的输出电压值(单位：V)。
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool SetVol(double v)
        {
            SendAndWaitForResponse("SYSTem:REMote", false);
            CommonMethod.Sleep(100);
            LogHelper.Debug($"{PowerType}设置电源输出电压值:{v} V");
            SendAndWaitForResponse($"VOLT {v}V", false);
            LogHelper.Debug($"{PowerType}发送命令：SOURce:VOLTage {v}");
            //var str = SendAndWaitForResponse("VOLT?");
            //LogHelper.Debug($"{PowerType}发送命令：SOURce:VOLTage?，回复：{str} V");
            //return double.TryParse(str, out double result);
            return true;
        }

        /// <summary>
        /// 设置电源的输出电流值(单位：A)
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool SetCurr(double c)
        {
            LogHelper.Debug($"{PowerType}设置电源输出电流值:{c} A");
            SendAndWaitForResponse($"CURR {c}A", false);
            LogHelper.Debug($"{PowerType}发送命令：SOURce:CURRent {c}");
            //var str = SendAndWaitForResponse("CURR?");
            //LogHelper.Debug($"{PowerType}发送命令：SOURce:CURRent?，回复：{str} A");
            //return double.TryParse(str, out double result);

            return true;
        }

        public bool Selectindex(int c)
        {
            LogHelper.Debug($"{PowerType}设置电源输出电流值:{c} A");
            if(c == 1)
            {
                SendAndWaitForResponse($"INST FIR", false);
            }
            else if (c == 2)
            {
                SendAndWaitForResponse($"INST SECO", false);
            }
            else if (c == 3)
            {
                SendAndWaitForResponse($"INST THI", false);
            }
            
    
       

            return true;
        }
        /// <summary>
        /// 重置电源到默认状态
        /// </summary>
        /// <returns></returns>
        public void Reset()
        {
            LogHelper.Debug($"{PowerType}重置电源到默认状态");
            SendAndWaitForResponse("*RST", false);
            LogHelper.Debug($"{PowerType}发送命令：*RST");
        }

        /// <summary>
        /// 查询电源的识别信息
        /// </summary>
        /// <returns></returns>
        public string GetIdentification()
        {
            var str = SendAndWaitForResponse("*IDN?");
            LogHelper.Debug($"{PowerType}发送命令：*IDN?，回复：{str}");
            return str;
        }

        public double SetCurrentAndStable(double c, double v = 0, double o = 0.050, int type = 0)
        {

            DateTime now = DateTime.Now;
            LogHelper.Debug(PowerType + " " + "设置电流值 CURRent " + c);
            SendAndWaitForResponse($"CURR {c}A", false);
            CommonMethod.Sleep(100);
            LogHelper.Debug(PowerType + " " + "回读设置值 CURRent?");
            string cTemp = SendAndWaitForResponse("CURRent?");
            double cur = 0;
            LogHelper.ShowMsg($"回读电流{cTemp}");
            LogHelper.ShowMsg($"设置电流{c}");
            
            LogHelper.Debug(PowerType + " " + "等待电流稳定");
            int i = 0;
            //等待稳定
            while (true)
            {
                if (c == 0)
                {
                    return 0;

                }
                CommonMethod.Sleep(100);

                
                
                cur = double.Parse(SelectCurr());
               

                if (Math.Abs((cur - c)) <= o)
                {

                    return cur;
                    break;
                }
                CommonMethod.Sleep(100);
                i++;
                LogHelper.Debug($"查找标签aaaaaa设置电流{c}A稳定第{i}次");
                if (i > 100)
                {
                    throw new TimeoutException("等待稳定超时");

                }
            }
            return cur;

        }
    }
}
