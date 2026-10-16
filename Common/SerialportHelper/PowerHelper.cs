using AllData;
using Common.UDS;
using NationalInstruments.Visa;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Org.BouncyCastle.Math.Primes;

namespace Common.SerialportHelper
{
    public class PowerHelper1
    {

        public string PowerType { get; set; }
        private SerialPort _serialPort;
        private StringBuilder _receiveBuffer = new StringBuilder();
        private string _response; // 保存完整的响应数据
        private readonly string _endMarker = "\n"; // 结束标志
        private ManualResetEvent _responseReceivedEvent = new ManualResetEvent(false);
        string _portName = "";
        public PowerHelper1(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = 9600;
            _serialPort.DataReceived += SerialPort_DataReceived;
            _serialPort.Open();
            _portName = portName;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
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
                    _response = _receiveBuffer.ToString(0, endIndex + _endMarker.Length);

                    // 清理缓冲区已处理部分
                    _receiveBuffer.Remove(0, endIndex + _endMarker.Length);

                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{PowerType}Error in DataReceived: {ex.Message}");
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
        public string SendAndWaitForResponse(string message, bool expectResponse = true)
        {

            if (!_serialPort.IsOpen)
            {
                try
                {
                    _serialPort = new SerialPort(_portName);
                    _serialPort.BaudRate = 9600;
                    _serialPort.DataReceived += SerialPort_DataReceived;
                    _serialPort.Open();
                }
                catch (Exception ex)
                {
                    LogHelper.Debug($"{PowerType} 串口未打开，且再次打开失败。{ex.ToString()}");
                    throw new InvalidOperationException($"{PowerType} 串口未打开，且再次打开失败。{ex.Message}");
                }

            }

            _response = null;
            _responseReceivedEvent.Reset();

            // 发送数据
            _serialPort.WriteLine(message);

            if (!expectResponse)
            {
                // 如果不需要等待响应，直接返回空字符串
                return string.Empty;
            }

            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return _response.Trim();
            }
            else
            {
                throw new TimeoutException($"{PowerType} 通信超时");
            }
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }



        /// <summary>
        /// 上电版本读取
        /// </summary>
        /// <returns></returns>
        public string GetVision()
        {
            return SendAndWaitForResponse("SYST:VERS?");
        }
        /// <summary>
        /// 设置远程控制
        /// </summary>
        public void SetREM()
        {
            LogHelper.Debug(PowerType + " " + "设置远程控制 SYST:REM");
            SendAndWaitForResponse("SYST:REM", false);
        }
        /// <summary>
        /// 设置电压输出值
        /// </summary>
        /// <param name="v"></param>
        /// <returns>设置值</returns>
        public string SetVoltage(double v)
        {
            LogHelper.Debug(PowerType + " " + "设置电压输出值 VOLTage " + v);
            SendAndWaitForResponse("VOLTage " + v, false);
            ////回读
            //DateTime now = DateTime.Now;
            //while (true)
            //{
            //    var v1 = double.Parse(GetMEASVoltage());
            //    if (Math.Abs(v - v1) < CommonData.PowerVoltageOffset)
            //    {
            //        break;
            //    }
            //    if ((DateTime.Now - now).TotalSeconds > 5)
            //    {
            //        throw new Exception("设置电源超时");
            //    }
            //}
            ////设置使能
            //var outPut= SetOUTPut(1);
            //if (outPut.ToString().Contains( "1")==false) {
            //    throw new Exception("使能失败");
            //}


            LogHelper.Debug(PowerType + " " + "回读设置值 VOLTage?");
            return SendAndWaitForResponse("VOLTage?");
        }
        /// <summary>
        /// 设置电流值
        /// </summary>
        /// <param name="c"></param>
        /// <returns>设置值</returns>
        public string SetCurrent(double c)
        {
            LogHelper.Debug(PowerType + " " + "设置电流值 CURRent " + c);
            SendAndWaitForResponse("CURRent " + c, false);

            LogHelper.Debug(PowerType + " " + "回读设置值 CURRent?");
            return SendAndWaitForResponse("CURRent?");

        }
        /// <summary>
        /// 使能
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool SetOUTPut(int o)
        {
            LogHelper.Debug(PowerType + " " + "设置使能 OUTPut " + o);
            SendAndWaitForResponse("OUTPut " + o, false);

            LogHelper.Debug(PowerType + " " + "回读使能 OUTPut:STATe?");
            var output = SendAndWaitForResponse("OUTPut:STATe?");
            return output.Contains(o.ToString());

        }

        /// <summary>
        /// 回读输出实际电压
        /// </summary>
        /// <returns></returns>
        public string GetMEASVoltage()
        {
            LogHelper.Debug(PowerType + " " + "回读实际电压 MEAS:VOLT?");
            var temp = SendAndWaitForResponse("MEAS:VOLT?");
            LogHelper.Debug(PowerType + " " + $"回读实际电压值为{temp}V");
            return temp;
            //Thread.Sleep(1000);

        }
        /// <summary>
        /// 回读输出实际电流
        /// </summary>
        /// <returns></returns>
        public string GetMEASCurrent()
        {
            LogHelper.Debug(PowerType + " " + "回读实际电流 MEAS:CURR?");
            var temp = SendAndWaitForResponse("MEAS:CURR?");

            LogHelper.Debug(PowerType + " " + $"回读实际电流值为：{temp}A");
            return temp;
        }
        /// <summary>
        /// 错误命令
        /// </summary>
        /// <returns></returns>
        public string GetErr()
        {

            return SendAndWaitForResponse("SYSTem:ERRor?");

        }


        /// <summary>
        /// 获取稳定值
        /// </summary>
        /// <param name="d"></param>
        /// <returns></returns>
        public bool GetStable(double d)
        {
            List<double> list = new List<double>();
            while (list.Count < 10)
            {
                list.Add(double.Parse(GetMEASCurrent()));
            }
            DateTime now = DateTime.Now;
            while (true)
            {
                if (d / 1000 >= list.Max() - list.Min())
                {
                    return true;
                }
                list.RemoveAt(0);
                list.Add(double.Parse(GetMEASCurrent()));
                if ((DateTime.Now - now).TotalSeconds > 5)
                {
                    LogHelper.ShowMsg("超时");
                    return false;
                }
            }
        }

        /// <summary>
        /// 设置电压（增加稳定处理）
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        public double SetVoltageAndStable(double v)
        {
            LogHelper.Debug(PowerType + " " + "设置电压输出值 VOLTage " + v);
            SendAndWaitForResponse("VOLTage " + v, false);
            LogHelper.Debug(PowerType + " " + "回读设置值 VOLTage?");
            string vTemp = SendAndWaitForResponse("VOLTage?");
            if ((double.Parse(vTemp) != v))
            {
                throw new Exception("电源设置失败");
            }
            LogHelper.Debug(PowerType + " " + "闭环控制开始");
            //回读
            DateTime now = DateTime.Now;
            double v1 = double.NaN;
            Random random = new Random();
            int count = 9;
            int i = 0;
            while (true)
            {
                i++;
                v1 = double.Parse(GetMEASVoltage());

                LogHelper.Debug(PowerType + " " + v1.ToString());
                if (Math.Abs(v - v1) <= 0.3)
                {
                    LogHelper.Debug(PowerType + " " + $"闭环成功,设置值：{v}V实际值：{v1}V，差值：{Math.Abs(v - v1)}<=阈值{0.3}V");
                    return v1;
                }
                if ((DateTime.Now - now).TotalSeconds > 5)
                {
                    LogHelper.Debug(PowerType + " " + $"闭环失败,设置值{v}V实际值{v1}V，差值：{Math.Abs(v - v1)}<=阈值{0.3}V");
                    throw new TimeoutException("设置电源超时");
                }
            }
            return v1;
        }
    }

    public class PowerHelperUSB
    {
        public string PowerType { get; set; }

        private MessageBasedSession mbSession;
        public PowerHelperUSB(string usbStr)
        {
            using (var rmSession = new NationalInstruments.Visa.ResourceManager())
            {
                //var resources = rmSession.Find("(ASRL|GPIB|TCPIP|USB)?*");
                //foreach (string s in resources)
                //{

                //}
                //var usb = resources.FirstOrDefault(m => m.ToLower().Contains("usb"));
                if (string.IsNullOrEmpty(usbStr) == false)
                {
                    try
                    {
                        mbSession = (MessageBasedSession)rmSession.Open(usbStr);
                        //SetupControlState(true);
                    }
                    catch (InvalidCastException)
                    {
                        MessageBox.Show("Resource selected must be a message-based session");
                    }
                    catch (Exception exp)
                    {
                        MessageBox.Show(exp.Message);
                    }
                    finally
                    {
                        //Cursor.Current = Cursors.Default;
                    }
                }
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
        public string SendAndWaitForResponse(string message, bool expectResponse = true)
        {
            if (message.EndsWith("\n") == false)
            {
                message += "\n";
            }
            
            mbSession.RawIO.Write(message);
            try
            {
                if (expectResponse) 
                    return mbSession.RawIO.ReadString();
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message);
            }
            return "";
        }


        public void Close()
        {
            if (mbSession != null)
            {
                mbSession.Dispose();
            }
        }

        /// <summary>
        /// 上电版本读取
        /// </summary>
        /// <returns></returns>
        public string GetVision()
        {
            return SendAndWaitForResponse("SYST:VERS?");
        }
        /// <summary>
        /// 设置远程控制
        /// </summary>
        public void SetREM()
        {
            LogHelper.Debug(PowerType + " " + "设置远程控制 SYST:REM");
            SendAndWaitForResponse("SYST:REM", false);
        }
        /// <summary>
        /// 设置电压输出值
        /// </summary>
        /// <param name="v"></param>
        /// <returns>设置值</returns>
        public string SetVoltage(double v)
        {
            LogHelper.Debug(PowerType + " " + "设置电压输出值 VOLTage " + v);
            SendAndWaitForResponse("VOLTage " + v, false);
            ////回读
            //DateTime now = DateTime.Now;
            //while (true)
            //{
            //    var v1 = double.Parse(GetMEASVoltage());
            //    if (Math.Abs(v - v1) < CommonData.PowerVoltageOffset)
            //    {
            //        break;
            //    }
            //    if ((DateTime.Now - now).TotalSeconds > 5)
            //    {
            //        throw new Exception("设置电源超时");
            //    }
            //}
            ////设置使能
            //var outPut= SetOUTPut(1);
            //if (outPut.ToString().Contains( "1")==false) {
            //    throw new Exception("使能失败");
            //}


            LogHelper.Debug(PowerType + " " + "回读设置值 VOLTage?");
            return SendAndWaitForResponse("VOLTage?").Trim();
        }
        /// <summary>
        /// 设置电流值
        /// </summary>
        /// <param name="c"></param>
        /// <returns>设置值</returns>
        public string SetCurrent(double c)
        {
            LogHelper.Debug(PowerType + " " + "设置电流值 CURRent " + c);
            SendAndWaitForResponse("CURRent " + c, false);

            LogHelper.Debug(PowerType + " " + "回读设置值 CURRent?");
            return SendAndWaitForResponse("CURRent?");

        }
        /// <summary>
        /// 使能
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public bool SetOUTPut(int o)
        {

            

            LogHelper.Debug(PowerType + " " + "设置使能 OUTPut " + o);
            SendAndWaitForResponse("OUTPut " + o, false);

            LogHelper.Debug(PowerType + " " + "回读使能 OUTPut:STATe?");
            var output = SendAndWaitForResponse("OUTPut:STATe?");
            return output.Contains(o.ToString());

        }

        /// <summary>
        /// 回读输出实际电压
        /// </summary>
        /// <returns></returns>
        public double GetMEASVoltage()
        {
            LogHelper.Debug(PowerType + " " + "回读实际电压 MEAS:VOLT?");
            var temp = SendAndWaitForResponse("MEAS:VOLT?");
            LogHelper.Debug(PowerType + " " + $"回读实际电压值为{temp}V");
            return double.Parse(temp);
            //Thread.Sleep(1000);

        }
        /// <summary>
        /// 回读输出实际电流
        /// </summary>
        /// <returns></returns>
        public double GetMEASCurrent()
        {
           
            var temp = SendAndWaitForResponse("MEAS:CURR?");

            LogHelper.Debug(PowerType + " " + $"回读实际电流值为：{temp}mA");
            return double.Parse(temp);
        }
        /// <summary>
        /// 错误命令
        /// </summary>
        /// <returns></returns>
        public string GetErr()
        {

            return SendAndWaitForResponse("SYSTem:ERRor?");

        }


        /// <summary>
        /// 设置电压（增加稳定处理）
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        public double SetVoltageAndStable(double v)
        {
            LogHelper.Debug(PowerType + " " + "设置电压输出值 VOLTage " + v);
            SendAndWaitForResponse("VOLTage " + v, false);
            LogHelper.Debug(PowerType + " " + "回读设置值 VOLTage?");
            string vTemp = SendAndWaitForResponse("VOLTage?");
            if ((double.Parse(vTemp) != v))
            {
                throw new Exception("电源设置失败");
            }
            LogHelper.Debug(PowerType + " " + "闭环控制开始");
            //回读
            DateTime now = DateTime.Now;
            double v1 = double.NaN;
            Random random = new Random();
            int count = 9;
            int i = 0;
            while (true)
            {
                i++;
                v1 = GetMEASVoltage();

                LogHelper.Debug(PowerType + " " + v1.ToString());
                if (Math.Abs(v - v1) <= 0.3)
                {
                    LogHelper.Debug(PowerType + " " + $"闭环成功,设置值：{v}V实际值：{v1}V，差值：{Math.Abs(v - v1)}<=阈值{0.3}V");
                    return v1;
                }
                if ((DateTime.Now - now).TotalSeconds > 5)
                {
                    LogHelper.Debug(PowerType + " " + $"闭环失败,设置值{v}V实际值{v1}V，差值：{Math.Abs(v - v1)}<=阈值{0.3}V");
                    throw new TimeoutException("设置电源超时");
                }
            }
            return v1;
        }
        /// <summary>
        /// 设置电流（增加稳定处理）
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        /// <exception cref="TimeoutException"></exception>
        public double SetCurrentAndStable(double c, double v = 0, double o = 0.050)
        {
            if (Math.Abs(c) >= 100)
            {
                CommonData.N69206H.SetCCModecurh(c);

            }
            else
            {
                CommonData.N69206H.SetCCModecur(c);
            }



            DateTime now = DateTime.Now;
            LogHelper.Debug(PowerType + " " + "设置电流值 CURRent " + c);
            SendAndWaitForResponse("CURRent " + (c), false);
            LogHelper.Debug(PowerType + " " + "回读设置值 CURRent?");
            string cTemp = CommonData.N69206H.GetMeasureCurrent();
            //  double cTemp = CommonData.M3905D.GetMEASCurrent();
            LogHelper.Debug(PowerType + " " + "等待电流稳定");
            int i = 0;
            //等待稳定
            while (true)
            {
                CommonMethod.Sleep(10);
                cTemp = CommonData.N69206H.GetMeasureCurrent();
                double cur = double.Parse(cTemp.Remove(cTemp.Length - 1));
                //    cTemp = CommonData.M3905D.GetMEASCurrent();
                //  double cur = cTemp;
                if (v == 0)
                {
                    LogHelper.Debug(PowerType + " " + $"稳定成功,电压为0V，返回0A，无须判断，与余工讨论执行的");
                    //break;
                    return cur;
                }
                if (Math.Abs((cur - c)) <= o)
                {
                    LogHelper.Debug(PowerType + " " + $"稳定成功,设置值{c}A,稳定值{cur}A,差值{Math.Abs((cur - c))},电压{v}V，小于等于{o}A,累计用了{i}次");
                    return cur;
                    break;
                }
                i++;
                LogHelper.Debug($"查找标签aaaaaa设置电流{c}A稳定第{i}次");
                if (i > 100)
                {
                    //  return double.Parse(cTemp);
                    throw new TimeoutException("等待稳定超时");
                    //return false;
                }
            }
            //    return (double.Parse(cTemp));

        }
    }
}