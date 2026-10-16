using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.Can
{
    public static class CanHelper
    {
        static CanOperate _operate;//工装板Can

        //static int (int)(CommonData.AwaitTime * 1000) = 0.5*1000;//超时时间
        static bool? isQiTing = null;//启停按钮是否按下
        static bool? isConnected = null;//是否连接
        static bool? isTest = null;//开始测试=true 停止测试=false
        static bool? isAwaken = null;//是否应答（没有返回值的）

        static double fResult1 = double.NaN;
        static double fResult2 = double.NaN;
        static double fResult3 = double.NaN;
        static double fResult4 = double.NaN;
        private static ManualResetEvent _responseReceivedEvent = new ManualResetEvent(false);
        #region 内外压使能
        static bool? within;
        static bool? outside;
        #endregion
        static bool heartbeatSendingWwitch = false;
        static short count = 0;
        public static void SetCan(CanOperate operate)
        {
            _operate = operate;
            operate.ReviceData += (id, arr) =>
            {

                //开始连接
                if (arr[0] == 0x11)
                {
                    if (arr[2] == 0x01)
                    {
                        isConnected = true;
                    }
                    else if (arr[2] == 0x02)
                    {
                        isConnected = false;
                    }
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "连接响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    str += "\n" + "连接的判断依据:" + arr[2].ToString("X2");
                    LogHelper.Debug(str);
                }
                //启停按钮
                else if (arr[0] == 0x12)
                {
                    //if (arr[2] == 0x01)
                    //{
                    //    isQiTing = true;
                    //}
                    //else if (arr[2] == 0x02)
                    //{
                    //    isQiTing = false;
                    //}
                    isAwaken = true;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "启停按钮状态响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    //str += "\n" + "启停按钮的判断依据:" + arr[2].ToString("X2");
                    LogHelper.Debug(str);
                }
                //开始测试
                else if (arr[0] == 0x13)
                {
                    if (arr[2] == 0x01)
                    {
                        isTest = true;
                    }
                    else if (arr[2] == 0x02)
                    {
                        isTest = false;
                    }
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "是否开始测试响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    str += "\n" + "开始测试的判断依据:" + arr[2].ToString("X2");
                    LogHelper.Debug(str);
                }
                //唤醒产品
                else if (arr[0] == 0x14)
                {
                    if (arr[2] == 0x01)
                    {
                        isAwaken = true;
                    }
                    else if (arr[2] == 0x02)
                    {
                        isAwaken = false;
                    }
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "唤醒/休眠产品响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    str += "\n" + "唤醒/休眠产品的判断依据:" + arr[2].ToString("X2");
                    LogHelper.Debug(str);
                }
                //读取电压
                else if (arr[0] == 0x15)
                {
                    double fTemp1 = fResult1 = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.001);
                    double fTemp2 = fResult2 = (double)(BitConverter.ToUInt16(new byte[] { arr[6], arr[5], arr[4] }, 0) * 0.001);
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "读取电源响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("读取电压/电流的计算方法:上传值*0.001（单位V/A）");
                    LogHelper.Debug($"电压值为：{fTemp1}V({arr[2].ToString("X2")}{arr[3].ToString("X2")}),电流值为：{fTemp2}A({arr[4].ToString("X2")}{arr[5].ToString("X2")}{arr[6].ToString("X2")})");
                }
                //单体电压
                else if (arr[0] == 0x01)
                {
                    fResult1 = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.001);
                    fResult2 = (double)(BitConverter.ToUInt16(new byte[] { arr[5], arr[4] }, 0) * 0.001);
                    fResult3 = (double)(BitConverter.ToUInt16(new byte[] { arr[7], arr[6] }, 0) * 0.001);
                    fResult4 = (double)(BitConverter.ToUInt16(new byte[] { arr[9], arr[8] }, 0) * 0.001);
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "单体电压响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("单体电压的计算方法:上传值*0.001（单位V）");
                    LogHelper.Debug($"获取单体电压1、2、3、4值为（{fResult1}V({arr[2].ToString("X2")}{arr[3].ToString("X2")})、{fResult2}V({arr[4].ToString("X2")}{arr[5].ToString("X2")})、{fResult3}V({arr[6].ToString("X2")}{arr[7].ToString("X2")})、{fResult4}V({arr[8].ToString("X2")}{arr[9].ToString("X2")})）");
                }
                ///AEF内总
                else if (arr[0] == 0x02)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.001);
                    if (temp > 20)
                        temp = 20;
                    if (temp < 0)
                        temp = 0;
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "AFE内总电压响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("AEF内总的计算方法:上传值*0.001（单位V）");
                    LogHelper.Debug($"获取AFE内总电压为：{fResult1}V({arr[2].ToString("X2")}{arr[3].ToString("X2")})");
                }
                //外总电压
                else if (arr[0] == 0x03)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.001);
                    if (temp > 20)
                        temp = 20;
                    if (temp < 0)
                        temp = 0;
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "外总电压响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("外总电压的计算方法:上传值*0.001（单位V）");
                    LogHelper.Debug($"获取外总电压为：{fResult1}V({arr[2].ToString("X2")}{arr[3].ToString("X2")})");
                }
                //电芯温度测试状态设置
                else if (arr[0] == 0x04)
                {
                    switch (arr[2])
                    {
                        case 0x00:
                            fResult1 = -20;
                            break;
                        case 0x01:
                            fResult1 = 40;
                            break;
                        case 0x02:
                            fResult1 = 80;
                            break;
                    }

                    switch (arr[3])
                    {
                        case 0x00:
                            fResult2 = -20;
                            break;
                        case 0x01:
                            fResult2 = 40;
                            break;
                        case 0x02:
                            fResult2 = 80;
                            break;
                    }

                    switch (arr[4])
                    {
                        case 0x00:
                            fResult3 = -20;
                            break;
                        case 0x01:
                            fResult3 = 40;
                            break;
                        case 0x02:
                            fResult3 = 80;
                            break;
                    }
                    switch (arr[5])
                    {
                        case 0x00:
                            fResult4 = -20;
                            break;
                        case 0x01:
                            fResult4 = 40;
                            break;
                        case 0x02:
                            fResult4 = 80;
                            break;
                    }
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "电芯温度响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    
                    LogHelper.Debug($"获取电芯温度1、2、3、4值为（{fResult1}°C（{arr[2].ToString("X2")}）、{fResult2}°C（{arr[3].ToString("X2")}）、{fResult3}°C（{arr[4].ToString("X2")}）、{fResult4}°C（{arr[5].ToString("X2")}））");
                }
                //读取板卡温度
                else if (arr[0] == 0x05)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.1 - 50);
                    if (temp < -50)
                    {

                        temp = -50;
                    }
                    if (temp > 200)
                    {
                        temp = 200;
                    }
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "读取板卡温度响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("读取板卡温度的计算方法:上传值*0.1 - 50（单位：°C）");
                    LogHelper.Debug($"获取板卡温度为：{fResult1}°C（{arr[2].ToString("X2")}{arr[3].ToString("X2")}）");
                }
                //总电流采集
                else if (arr[0] == 0x06)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.001 - 1000);
                    if (temp < -1000)
                    {

                        temp = -1000;
                    }
                    if (temp > 1000)
                    {
                        temp = 1000;
                    }
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "继电器控制响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    if (Math.Abs(temp) < 600)
                    {
                        LogHelper.Debug("总电流采集(继电器控制)的计算方法:上传值 * 0.001 - 1000（单位：A）");
                        LogHelper.Debug($"总电流采集(继电器控制)结果：{temp}A（{arr[2].ToString("X2")}{arr[3].ToString("X2")}）");
                    }
                }
                //工作电流采集
                else if (arr[0] == 0x07)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[4], arr[3], arr[2] }, 0) * 0.01);
                    if (temp < 0.01)
                    {

                        temp = 0.01f;
                    }
                    if (temp > 1000)
                    {
                        temp = 1000;
                    }
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "工作电流采集响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("工作电流采集的计算方法:上传值 * 0.01（单位mA）");
                    LogHelper.Debug($"工作电流采集结果：{temp}mA（{arr[2].ToString("X2")}{arr[3].ToString("X2")}{arr[4].ToString("X2")}）");
                }
                //MOS电压采集
                else if (arr[0] == 0x08)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.1);
                    if (temp < 0)
                    {

                        temp = 0;
                    }
                    if (temp > 50)
                    {
                        temp = 50;
                    }
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "MOS电压采集：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("MOS电压采集的计算方法:上传值 * 0.1（单位：V）");
                    LogHelper.Debug($"MOS电压采集结果：{temp}V（{arr[2].ToString("X2")}{arr[3].ToString("X2")}）");
                }
                //内外压使能
                else if (arr[0] == 0x09)
                {
                    if (arr[2] == 0x00)
                    {
                        within = false;
                    }
                    else
                    {
                        within = true;
                    }
                    if (arr[3] == 0x00)
                    {
                        outside = false;
                    }
                    else
                    {
                        outside = true;
                    }
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "内外压使能响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                }
                //Can终端电阻上电使能
                else if (arr[0] == 0x0A)
                {
                    isAwaken = true;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "终端电阻上电使能响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                }
                //终端电阻测试结果
                else if (arr[0] == 0x0B)
                {
                    var temp = (double)(BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.1);
                    if (temp < 0)
                    {

                        temp = 0;
                    }
                    if (temp > 300)
                    {
                        temp = 300;
                    }
                    fResult1 = temp;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "终端电阻结果响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("终端电阻测试结果的计算方法:上传值 * 0.1（单位：Ω）");
                    LogHelper.Debug($"终端电阻结果：{temp}Ω（{arr[2].ToString("X2")}{arr[3].ToString("X2")}）");
                }
                else if ((arr[0] == 0x0C))
                {
                    isAwaken = true;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "发送测试结果响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                }
                //均衡控制
                else if (arr[0] == 0x1F)
                {
                    var temp1 = (BitConverter.ToUInt16(new byte[] { arr[3], arr[2] }, 0) * 0.1);
                    if (temp1 < 0)
                        temp1 = 0;
                    if (temp1 > 1000)
                        temp1 = 1000;
                    fResult1 = temp1;

                    temp1 = (BitConverter.ToUInt16(new byte[] { arr[5], arr[4] }, 0) * 0.1);
                    if (temp1 < 0)
                        temp1 = 0;
                    if (temp1 > 1000)
                        temp1 = 1000;
                    fResult2 = temp1;

                    temp1 = (BitConverter.ToUInt16(new byte[] { arr[7], arr[6] }, 0) * 0.1);
                    if (temp1 < 0)
                        temp1 = 0;
                    if (temp1 > 1000)
                        temp1 = 1000;
                    fResult3 = temp1;

                    temp1 = (BitConverter.ToUInt16(new byte[] { arr[9], arr[8] }, 0) * 0.1);
                    if (temp1 < 0)
                        temp1 = 0;
                    if (temp1 > 1000)
                        temp1 = 1000;
                    fResult4 = temp1;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                    string str = "均衡控制响应：";
                    foreach (var item in arr)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.Debug(str);
                    LogHelper.Debug("均衡控制的计算方法:上传值 * 0.1（单位：mA）");
                    LogHelper.Debug($"均衡通道1、2、3、4值为（{fResult1}mA({arr[2].ToString("X2")}{arr[3].ToString("X2")})、{fResult2}mA({arr[4].ToString("X2")}{arr[5].ToString("X2")})、{fResult3}mA({arr[6].ToString("X2")}{arr[7].ToString("X2")})、{fResult4}mA({arr[8].ToString("X2")}{arr[9].ToString("X2")})）");
                }


            };
            CommonMethod.PlayTaskRun(() =>
            {
                if (heartbeatSendingWwitch == true)
                {
                    ManualResetEvent r = new ManualResetEvent(false);
                    r.Reset();
                    operate.ReviceData += reviceData;
                    operate.Send(data: $"1E 01 {count.ConvertToFormattedHex(2)}");
                    count++;
                    if (r.WaitOne((int)(CommonData.AwaitTime * 1000)))
                    {
                        operate.ReviceData -= reviceData;
                    }
                    else
                    {
                        operate.ReviceData -= reviceData;
                        //throw new Exception("Can通信心跳超时");
                    }

                    void reviceData(uint id, byte[] arr)
                    {
                        if (arr[0] == 0x1E)
                        {
                            r.Set();
                        }

                    }
                }
            },1000,"Can心跳线程");
            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
                    
            //        await Task.Delay(1000);
            //    }
            //});
        }

        private static List<byte> Send(uint id = 0x500, string data = "", bool isCanFd = true, bool canfdExp = true, int len = 16)
        {
            List<byte> result = new List<byte>();
            ManualResetEvent r = new ManualResetEvent(false);
            bool b= _operate.Send(id, data, isCanFd, canfdExp, len);
            if (b == false)
            {
                return null;
            }
            r.Reset();
            _operate.ReviceData += reviceData;

            if (r.WaitOne((int)(CommonData.AwaitTime * 1000)))
            {
                _operate.ReviceData -= reviceData;
                return result;
            }
            else
            {
                _operate.ReviceData -= reviceData;
                throw new Exception("Can通信超时");
            }

            void reviceData(uint id1, byte[] arr)
            {
                result = arr.ToList();
                r.Set();
            }
        }

        /// <summary>
        /// 连接
        /// </summary>
        /// <returns></returns>
        public static bool Conneted()
        {
            LogHelper.Debug("开始连接");
            isConnected = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.ConnectCmd);
            _operate.Send(data: CanCmd.ConnectCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime*1000))) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }
        }
        /// <summary>
        /// 断开连接
        /// </summary>
        /// <returns></returns>
        public static bool DisConted()
        {
            LogHelper.Debug("断开连接");
            isConnected = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.DisConnectCmd);
            _operate.Send(data: CanCmd.DisConnectCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }
        }
        /// <summary>
        /// 等待启停按钮按下
        /// </summary>
        /// <returns></returns>
        public static bool AwaitQiTing(bool isUp = false)
        {
            bool b = false;
            while (true)
            {
                var temp = Send(data: "12 01");
                if (temp == null)
                {
                    return false;
                }
                if (temp[2] == 0x01)
                {
                    b = true;
                }
                else
                {
                    b = false;
                }
                if (isUp == false && b)
                {
                    return true;
                }
                else if (isUp && !b)
                {
                    return true;
                }
                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 开始测试
        /// </summary>
        /// <returns></returns>
        public static bool StartTest()
        {
            var temp = Send(data: CanCmd.StartTestCmd);
            LogHelper.Debug($"开始测试：{CanCmd.StartTestCmd}");
            return temp[2] == 0x01;
        }
        /// <summary>
        /// 启停按钮按下的回复以及发送开始测试
        /// </summary>
        /// <returns></returns>
        public static bool QiTingAck()
        {
            _operate.Send(data: $"12 A1 01");
            LogHelper.Debug($"启停按钮答复：12 A1 01");
            _operate.Send(data: CanCmd.StartTestCmd);
            LogHelper.Debug($"开始测试：{CanCmd.StartTestCmd}");

            DateTime now = DateTime.Now;
            isTest = null;
            while (isTest != true)
            {
                //if ((DateTime.Now - now).TotalSeconds > CommonData.AwaitTime)
                //{
                //    return false;
                //}
                Thread.Sleep(50);
            }
            return true;
        }

        /// <summary>
        /// 启停按钮抬起的回复以及停止测试
        /// </summary>
        /// <returns></returns>
        public static bool QiTingUpAck()
        {
            _operate.Send(data: $"12 A1 02");
            LogHelper.Debug($"启停按钮答复：12 A1 02");
            return true;
        }
        /// <summary>
        /// 读取电源
        /// </summary>
        /// <returns>Item1 电压 Item2 电流</returns>
        public static (double, double) ReadPower(bool power1,bool power2)
        {
            fResult1 = double.NaN;
            fResult2 = double.NaN;
            _responseReceivedEvent.Reset();
            _operate.Send(data: CanCmd.ReadPowerCmd(power1, power1), canfdExp:false);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return (fResult1, fResult2);
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return (double.NaN,double.NaN); 
            }
        }
        /// <summary>
        /// 唤醒
        /// </summary>
        /// <returns></returns>
        public static bool Awaken()
        {
            LogHelper.Debug("唤醒指令");
            isAwaken = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.AwakenCmd);
            _operate.Send(data: CanCmd.AwakenCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return false;
            }
            
        }
        /// <summary>
        /// 读取单体电压
        /// </summary>
        /// <returns></returns>
        public static (double, double, double, double) ReadIndividualVoltage()
        {
            fResult1 = double.NaN;
            fResult2 = double.NaN;
            fResult3 = double.NaN;
            fResult4 = double.NaN;
            _responseReceivedEvent.Reset();
            _operate.Send(data: CanCmd.IndividualVoltageCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return (fResult1, fResult2, fResult3, fResult4);
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return (fResult1, fResult2, fResult3, fResult4);
            }
        }
        /// <summary>
        /// 读取AFE内总电压
        /// </summary>
        /// <returns></returns>
        public static double ReadAFEInternalTotalVoltage()
        {
            LogHelper.Debug("工装读取AFE内总电压");
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.AFEInternalTotalPressureCmd);
            _operate.Send(data: CanCmd.AFEInternalTotalPressureCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return fResult1;
            }
            
        }
        /// <summary>
        /// 读取外总电压
        /// </summary>
        /// <returns></returns>
        public static double ReadExternalTotalVoltage()
        {
            LogHelper.Debug("工装读取外总电压");
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.ExternalTotalPressureCmd);
            _operate.Send(data: CanCmd.ExternalTotalPressureCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return fResult1;
            }

            
        }
        /// <summary>
        /// 设置电芯温度
        /// </summary>
        /// <param name="cell1"></param>
        /// <param name="cell2"></param>
        /// <param name="cell3"></param>
        /// <param name="cell4"></param>
        /// <returns></returns>
        public static (double, double, double, double) SetCellTemperature(CellTemperatureEnum cell1, CellTemperatureEnum cell2, CellTemperatureEnum cell3, CellTemperatureEnum cell4)
        {
            LogHelper.Debug("设置电芯温度");
            fResult1 = double.NaN;
            fResult2 = double.NaN;
            fResult3 = double.NaN;
            fResult4 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.CellTemperatureCmd(cell1, cell2, cell3, cell4));
            _operate.Send(data: CanCmd.CellTemperatureCmd(cell1, cell2, cell3, cell4));
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return (fResult1, fResult2, fResult3, fResult4);
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return (fResult1, fResult2, fResult3, fResult4);
            }
           
        }
        /// <summary>
        /// 读取板卡温度
        /// </summary>
        /// <returns></returns>
        public static double ReadBoardTemperature()
        {
            LogHelper.Debug("读取板卡温度");
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.BoardTemperatureCmd);
            _operate.Send(data: CanCmd.BoardTemperatureCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return fResult1;
            }
           
        }
        /// <summary>
        /// 总电流采集
        /// </summary>
        /// <param name="isJust">正向或者反向</param>
        /// <returns></returns>
        public static double CtrolTotalCurrentCollection(bool isJust)
        {
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug($"继电器切{(isJust ? "正" : "反")} {CanCmd.TotalCurrentCollectionCmd(isJust)}");
            _operate.Send(data: CanCmd.TotalCurrentCollectionCmd(isJust));
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return fResult1;
            }
            

        }
        /// <summary>
        /// 关闭电流采集
        /// </summary>
        /// <returns></returns>
        public static bool CloseTotalCurrentCollection()
        {
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();

            LogHelper.Debug($"继电器关闭 {CanCmd.TotalCurrentCollectionCloseCmd}");
            _operate.Send(data: CanCmd.TotalCurrentCollectionCloseCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return false;
            }
           
        }
        /// <summary>
        /// 工作电流采集
        /// </summary>
        /// <returns></returns>
        public static double WorkCurrentCollection()
        {
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            _operate.Send(data: CanCmd.WorkCurrentCollectionCmd);

            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return fResult1;
            }
        }
        /// <summary>
        /// MOS控制内外使能
        /// </summary>
        /// <returns></returns>
        public static (bool, bool) MOSControlVoltageEnable(bool _within, bool _outside)
        {
            LogHelper.Debug($"工装切换继电器");
            within = null;
            outside = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.ControlVoltageEnableCmd(_within, _outside));
            _operate.Send(data: CanCmd.ControlVoltageEnableCmd(_within, _outside));
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return (within.Value, outside.Value);
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return (false, false);
            }
            
        }
        public static double MOSVoltageCollection()
        {
            LogHelper.Debug($"MOS电压采集");
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.MOSVoltageCollectionCmd);
            _operate.Send(data: CanCmd.MOSVoltageCollectionCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                //throw new TimeoutException("超时");
                LogHelper.ShowMsg("超时");
                return fResult1;
            }
            
        }

        /// <summary>
        /// 终端电阻上电使能
        /// </summary>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static bool ResistivePowerOn(bool enable)
        {
            LogHelper.Debug("终端电阻上电使能");
            isAwaken = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.ResistivePowerOnCmd(enable));
            _operate.Send(data: CanCmd.ResistivePowerOnCmd(enable));
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne(5000)) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }

        }
        /// <summary>
        /// 终端电阻测试结果获取
        /// </summary>
        /// <returns></returns>
        public static double ResistiveResult()
        {
            LogHelper.Debug("终端电阻获取结果");
            fResult1 = double.NaN;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.ResistiveResultCmd);
            _operate.Send(data: CanCmd.ResistiveResultCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne(5000)) // 超时时间：5秒
            {
                return fResult1;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return fResult1;
            }
        }
        /// <summary>
        /// 结束测试
        /// </summary>
        /// <returns></returns>
        public static bool StopTest()
        {
            LogHelper.Debug("结束测试");
            isTest = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.StopTestCmd);
            _operate.Send(data: CanCmd.StopTestCmd);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne(5000)) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }
            
        }
        /// <summary>
        /// 发送结果
        /// </summary>
        /// <param name="isPass"></param>
        /// <returns></returns>
        public static bool TestResult(bool isPass)
        {
            isAwaken = null;
            _responseReceivedEvent.Reset();
            _operate.Send(data: CanCmd.TestResultCmd(isPass));
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne(5000)) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }

        }
        /// <summary>
        /// 休眠
        /// </summary>
        /// <returns></returns>
        public static bool Dormancy()
        {
            LogHelper.Debug("工装休眠");
            isAwaken = null;
            _responseReceivedEvent.Reset();
            LogHelper.Debug(CanCmd.DormancyCmd);
            _operate.Send(data: CanCmd.DormancyCmd);
            DateTime now = DateTime.Now;
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne(5000)) // 超时时间：5秒
            {
                return isAwaken.Value== false;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }
           
        }
        /// <summary>
        /// 均衡控制
        /// </summary>
        /// <param name="isc1AndC3">true 开启1 3 false 开启2 4 null 获取结果</param>
        /// <returns></returns>
        public static (double, double, double, double) BalancedControl(bool? isc1AndC3)
        {
            fResult1 = double.NaN;
            fResult2 = double.NaN;
            fResult3 = double.NaN;
            fResult4 = double.NaN;
            _responseReceivedEvent.Reset();
            _operate.Send(data: CanCmd.BalancedControlCmd(isc1AndC3));
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne(5000)) // 超时时间：5秒
            {
                return (fResult1, fResult2, fResult3, fResult4);
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return (fResult1, fResult2, fResult3, fResult4);
            }
            
        }
    }
}
