using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.SerialportHelper
{
    public class N8331
    {
        public string PowerType { get; set; } = "N8331";
        private SerialPort _serialPort;
        private readonly string _endMarker = "\n"; // 结束标志
        public N8331(string portName)
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
        public string SendAndWaitForResponse(string message, bool expectResponse = true,int cf=0)
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
        /// 查询电池模拟器相应通道的回显电压值(单位：V)。
        /// </summary>
        /// <returns></returns>
        public string SelectVol(int n)
        {
            var str = SendAndWaitForResponse($"MEASure{n}:VOLTage?");
            LogHelper.Debug($"{PowerType}发送命令：MEASure{n}:VOLTage?，查询{n}回显电压值：{str} V");
            return str;
        }
        /// <summary>
        /// 查询电池模拟器相应通道的回显电流值(单位：mA)。
        /// </summary>
        /// <returns></returns>
        public string SelectCurr(int n)
        {
            var str = SendAndWaitForResponse($"MEASure{n}:CURRent?");
            LogHelper.Debug($"{PowerType}发送命令：MEASure{n}:CURRent?，查询{n}回显电流值：{str} mA");
            return str;
        }
        /// <summary>
        /// 查询电池模拟器相应通道的实时功率值(单位：W)。
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public string SelectPower(int n)
        {
            var str = SendAndWaitForResponse($"MEASure{n}:POWer?");
            LogHelper.Debug($"{PowerType}发送命令：MEASure{n}:POWer?，查询{n}回显功率：{str} W");
            return str;
        }

        /// <summary>
        /// 设置电池模拟器相应通道开启或关闭输出
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public bool SetOutput(int n, bool enable)
        {
            LogHelper.Debug($"{PowerType}设置电池模拟器{n}通道开启或关闭输出");
            SendAndWaitForResponse($"OUTPut{n}:ONOFF {(enable ? "1" : "0")}", false);
            LogHelper.Debug($"{PowerType}发送命令：OUTPut{n}:ONOFF {(enable ? "1" : "0")}");
            var temp = SendAndWaitForResponse($"OUTP{n}:ONOFF?");
            LogHelper.Debug($"{PowerType}发送命令：OUTP{n}:ONOFF?，回复：{temp}");
            if (temp == (enable ? "1" : "0"))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 设置电池模拟器的功能模式。
        /// </summary>
        /// <param name="n"></param>
        /// <param name="model">电源模式false 扫描模式true</param>
        /// <returns></returns>
        public bool SetModel(int n, bool model = false)
        {
            LogHelper.Debug("设置电池模拟器的功能模式");
            SendAndWaitForResponse($"OUTPut{n}:MODE {(model ? "1" : "0")}", false);
            LogHelper.Debug($"{PowerType}发送命令：OUTPut{n}:MODE {(model ? "1" : "0")}");
            var temp = SendAndWaitForResponse($"OUTP{n}:MODE?");
            LogHelper.Debug($"{PowerType}发送命令：OUTP{n}:MODE?，回复：{temp}");
            if (temp == (model ? "1" : "0"))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 查询通道运行状态
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public string SelectState(int n)
        {
            var str = SendAndWaitForResponse($"OUTP{n}:STAT?");
            LogHelper.Debug($"{PowerType}发送命令：OUTP{n}:STAT?，查询{n}通道运行状态:{str}");
            return str;
        }
        /// <summary>
        /// 电源模式恒压值(单位：V)。
        /// </summary>
        /// <param name="n"></param>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool SetVol(int n, double v)
        {
            LogHelper.Debug($"{PowerType}设置{n}电源模式恒压值:{v} V");
            SendAndWaitForResponse($"SOURce{n}:VOLTage {v}", false);
            LogHelper.Debug($"{PowerType}发送命令：SOURce{n}:VOLTage {v}");
            var str = SendAndWaitForResponse($"SOUR{n}:VOLT?");
            LogHelper.Debug($"{PowerType}发送命令：SOUR{n}:VOLT?，回复：{str} V");
            return str == v.ToString();
        }
        /// <summary>
        /// 设置输出限流值(单位：mA)
        /// </summary>
        /// <param name="n"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool SetCurr(int n, double c)
        {
            LogHelper.Debug($"{PowerType}设置{n}输出限流值:{c} mA");
            SendAndWaitForResponse($"SOURce{n}:OUTCURR {c}", false);
            LogHelper.Debug($"{PowerType}发送命令：SOURce{n}:OUTCURR {c}");
            var str = SendAndWaitForResponse($"SOUR{n}:OUTCURR?");
            LogHelper.Debug($"{PowerType}发送命令：SOUR{n}:OUTCURR?，回复：{str} mA");
            return str == c.ToString();
        }
        /// <summary>
        /// 设置量程 0-大量程，1-小量程，3-自动量程
        /// </summary>
        /// <param name="n"></param>
        /// <param name="r">0-大量程，1-小量程，3-自动量程</param>
        /// <returns></returns>
        public bool SetRange(int n, int r = 3)
        {
            LogHelper.Debug($"{PowerType}设置{n}量程:{r}");
            SendAndWaitForResponse($"SOURce{n}:RANGe {r}", false);
            LogHelper.Debug($"{PowerType}发送命令：SOURce{n}:RANGe {r}");

            var str = SendAndWaitForResponse($"SOUR{n}:RANG?");
            LogHelper.Debug($"{PowerType}发送命令：SOUR{n}:RANG?，回复：{str}");


            return str == r.ToString();
        }


        /// <summary>
        /// 设置电芯1-4电压
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool SetVol4(double v, double c = 1000)
        {
            for (int i = 1; i < 5; i++)
            {
                ////关闭通道输出
                //if (SetOutput(i, false) == false)
                //{
                //    throw new Exception($"{PowerType}关闭通道{i}输出失败");
                //}
                //设置通道为电源模式
                if (SetModel(i) == false)
                {
                    throw new Exception($"{PowerType}设置通道{i}为电源模式失败");
                }
                //设置恒压值
                if (SetVol(i, v) == false)
                {
                    throw new Exception($"{PowerType}设置通道{i}恒压值:{v} V失败");
                };
                //设置电流输出限流值
                if (SetCurr(i, c) == false)
                {
                    throw new Exception($"{PowerType}设置通道{i}电流输出限流值:{c} mA失败");
                }
                ////开启通道输出
                //if (SetOutput(i, true) == false)
                //{
                //    throw new Exception($"{PowerType}开启通道{i}输出失败");
                //}
            }
            return true;
        }


        public bool SetVol41(double v, double c = 1000)
        {
            for (int i = 1; i < 5; i++)
            {

                if(i == 1)
                {
                    v = 5;
                }
                ////关闭通道输出
                //if (SetOutput(i, false) == false)
                //{
                //    throw new Exception($"{PowerType}关闭通道{i}输出失败");
                //}
                //设置通道为电源模式
                if (SetModel(i) == false)
                {
                    throw new Exception($"{PowerType}设置通道{i}为电源模式失败");
                }
                //设置恒压值
                if (SetVol(i, v) == false)
                {
                    throw new Exception($"{PowerType}设置通道{i}恒压值:{v} V失败");
                }
                ;
                //设置电流输出限流值
                if (SetCurr(i, c) == false)
                {
                    throw new Exception($"{PowerType}设置通道{i}电流输出限流值:{c} mA失败");
                }
                ////开启通道输出
                //if (SetOutput(i, true) == false)
                //{
                //    throw new Exception($"{PowerType}开启通道{i}输出失败");
                //}
            }
            return true;
        }
        /// <summary>
        /// 获取电芯1-4电压
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<double> GetVol4()
        {
            CommonMethod.Sleep(100);
            List<double> vols = new List<double>();
            for (int i = 1; i < 5; i++)
            {
                var str = SelectVol(i);
                if (double.TryParse(str, out double d) == false)
                {
                    throw new Exception($"{PowerType}获取电芯电压{i}，结果为:{str},转换为double失败");
                }
                vols.Add(d);
            }
            return vols;
        }
        /// <summary>
        /// 设置电芯1-4是否使能
        /// </summary>
        /// <returns></returns>
        public bool SetOutPut4(bool enable)
        {
            for (int i = 1; i < 5; i++)
            {
                if(SetOutput(i, enable) == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
