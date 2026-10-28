﻿﻿﻿﻿﻿﻿﻿using AllData;
using Common.Can;
using Common.SerialportHelper;
using Common.UDS;
using MathNet.Numerics.Distributions;
using Models_C;
using Models_C.Dal;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public static class CommonMethod
    {
        static int awaitTime = 5;
        static object _lock = new object();
        static CanOperate _product;
        static CanOperate _work;
        static Common.SerialportHelper.IT6322 _IT6322;
        static PowerHelperUSB _M3905D;
        /// <summary>
        /// 设置产品的Can
        /// </summary>
        /// <param name="product"></param>
        public static void SetProduct(CanOperate product)
        {
            _product = product;
        }
        /// <summary>
        /// 设置工装板的Can
        /// </summary>
        /// <param name="work"></param>
        public static void SetWork(CanOperate work)
        {

            _work = work;
        }
        #region 产品相关的 产品唤醒以及解锁
        static bool awakenStatus = false;//唤醒
        static bool ClearStatus = false;//qingchu
        static bool unlockStatus = false;//解锁后发的
        /// <summary>
        /// 开始发送唤醒命令
        /// </summary>
        public static void StartSendAwaken()
        {
            awakenStatus = true;

            Task.Run(async () =>
            {
                while (awakenStatus)
                {
                    if(ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                    {
                        _product?.Send(uint.Parse(ConfigHelper.UdsParameters.HX_ID, System.Globalization.NumberStyles.HexNumber), ConfigHelper.UdsParameters.HX_Data, isCanFd: true, canfdExp: false, 8, lentype: true);
                    }
                    else
                    {
                        _product?.Send(uint.Parse(ConfigHelper.UdsParameters.HX_ID, System.Globalization.NumberStyles.HexNumber), ConfigHelper.UdsParameters.HX_Data, isCanFd: false, canfdExp: false, 8, lentype: true);
                    }
                    
                   
                    await Task.Delay(200);
                }
            });
        }


        public static void SendAwaken()
        {


            _product?.Send(uint.Parse(ConfigHelper.UdsParameters.HX_ID, System.Globalization.NumberStyles.HexNumber), ConfigHelper.UdsParameters.HX_Data, isCanFd: false, canfdExp: false, 8, lentype: true);


        }
        public static void StartClear()
        {
            ClearStatus = true;

            Task.Run(async () =>
            {
                while (ClearStatus)
                {
                    string senddata = $"00 00 00 00 00 00 00 00";
                    _product.Send(0x021, senddata, true, true, 8, true);
                    CommonMethod.Sleep(20);
                    senddata = $"00 00 00 00 00 00 00 00";
                    _product.Send(0x48A, senddata, true, true, 8, true);
                    CommonMethod.Sleep(20);
                    senddata = $"00 00 00 00 00 00 00 00";
                    _product.Send(0x403, senddata, true, true, 8, true);
                    CommonMethod.Sleep(20);
                    senddata = $"17 A0 C0 50 E8 04 00 00";
                    _product.Send(0x510, senddata, true, true, 8, true);
                    CommonMethod.Sleep(20);
                    senddata = $"00 00 00 00 00 00 00 00";
                    _product.Send(0x33E, senddata, true, true, 8, true);
                    
                    await Task.Delay(50);
                }
            });
        }

        public static void StartClearXP()
        {
            ClearStatus = true;

            Task.Run(async () =>
            {
                while (ClearStatus)
                {
                    string senddata = $"00 00 00 00 00 00 00 00";
                    _product.Send(0x127, senddata, true, true, 8, true);
                 

                    await Task.Delay(50);
                }
            });
        }
        public static void StopClear()
        {
            ClearStatus = false;
        }
        public static void StopSendAwaken()
        {
            awakenStatus = false;
        }
        /// <summary>
        /// 开始发送解锁维持指令
        /// </summary>
        public static void StartSendUnlock()
        {
            unlockStatus = true;

            Task.Run(async () =>
            {
                while (unlockStatus)
                {
                    var bytes = new List<byte>() { 0x00 };//会回复
                    bytes = new List<byte>() { 0x80 };//不会回复
                                                      //_product.Send(bytes.ToArray(), 0x3E, 0x7DF);
                                                      //PlayUDSHelper.Send(bytes.ToArray(), 0x3E, 0x7DF,isAwaitAck:false);
                                                      //_product.Send(0x7DF)
                    _product?.Send(uint.Parse(ConfigHelper.UdsParameters.WC_ID, System.Globalization.NumberStyles.HexNumber), data: "02 3E 80", isCanFd: true, canfdExp: true, len: 8,true);
                    LogHelper.Debug("发送 0x7DF 3E 80");
                    await Task.Delay(2000);
                }
            });
        }
        public static void StopSendUnlock()
        {
            unlockStatus = false;
        }
        #endregion

        /// <summary>
        /// 解锁
        /// </summary>
        /// <returns></returns>
        public static bool Unlock()
        {
            if(ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                if (!PlayUDSHelper.GoToExtencion())
                {
                    LogHelper.Debug("进入拓展失败");
                    return false;
                }

                var seed = PlayUDSHelper.GetSeedWL();
                if (seed == null)
                {
                    LogHelper.Debug("获取种子失败");
                    return false;
                }
                if (!PlayUDSHelper.JieSuoWL(seed))
                {
                    LogHelper.Debug("解锁失败");
                    return false;
                }
            }
            else
            {
                if (!PlayUDSHelper.GoToExtencion())
                {
                    LogHelper.Debug("进入拓展失败");
                    return false;
                }

                if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
                {

                }
                else
                {
                    var seed = PlayUDSHelper.GetSeed();
                    if (seed == null)
                    {
                        LogHelper.Debug("获取种子失败");
                        return false;
                    }
                    if (!PlayUDSHelper.JieSuo(seed))
                    {
                        LogHelper.Debug("解锁失败");
                        return false;
                    }
                    

                }
            }
            
            

                
            _product?.Send(uint.Parse(ConfigHelper.UdsParameters.WC_ID, System.Globalization.NumberStyles.HexNumber), data: "02 3E 80", isCanFd: true, canfdExp: true, len: 8,true);
            LogHelper.Debug("02 3E 80");
            CommonMethod.Sleep(50);
            return true;
        }
        /// <summary>
        /// 休眠
        /// </summary>
        /// <param name="level">休眠等级</param>
        /// <param name="isMOS">是否执行闭合mos操作</param>
        /// <returns></returns>
        public static bool Sleep(int level, bool isMOS)
        {
            //第一步休眠
           
            //发送休眠指令
            if (NewCanHelper.WakeOrSleep(false) == false)
            {
                
                LogHelper.WriteLog("休眠失败");
                return false;
            }
            if (isMOS)
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    LogHelper.ShowMsg("MOS闭合失败");
                }
            }
            StopSendAwaken();
    //        StopSendUnlock();
            LogHelper.ShowMsg("休眠产品");
            List<byte> data = new List<byte>();
            if (level == 1)
                data = new List<byte>() { 0x61, 0x03, 0x03, 0x01 };
            else if (level == 2)
            {
                data = new List<byte>() { 0x61, 0x03, 0x03, 0x02 };
            }
            //uds发送休眠
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x2f);
            if (temp == null)
            {
                temp = PlayUDSHelper.Send(data.ToArray(), 0x2f);
                if (temp == null)
                {
                    LogHelper.ShowMsg("uds休眠失败");
                    return false;
                }

            }
            //var dl = CanHelper.WorkCurrentCollection();
            //if (dl > 20)
            //{

            //}
            CommonMethod.Sleep(300);
            return true;
        }

        public static void Sleep(int s)
        {
            LogHelper.Debug($"延时{s}毫秒");
            Thread.Sleep(s);
        }

        /// <summary>
        /// 流程之前判断  工作电流=>是否唤醒=>解锁（每个uds写）
        /// </summary>
        public static void UnlockJudgment()
        {

        }

        /// <summary>
        /// 从电源获取工作电流
        /// </summary>
        /// <returns></returns>
        public static (bool, double) GetWorkCurrentFromPower(double awatiTime = 10, double threshold = 50, double threshold1 = 100)
        {
            if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {

                threshold = 30;

            }
            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390")
            {
                threshold = 10;
            }
            if (ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
            {
                threshold = 20;
            }
            if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                threshold = 30;
            }
            if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046" || ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B" ||
                ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" ||
                ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97")

            {
                threshold = 25;
            }

            List<double> list = new List<double>();
            DateTime now = DateTime.Now;
            while (true)
            {
                var currStr = CommonData.NXI_6400.GetCResult(0.1);
                double curr = currStr.Item1.FirstOrDefault() * 1000;
                LogHelper.ShowMsg($"gz{curr}");
                list.Add(curr);
                if (curr >= threshold && curr <= threshold1)
                {
                    return (true, curr);
                }
                if ((DateTime.Now - now).TotalMilliseconds > awatiTime * 1000)
                {
                    LogHelper.ShowMsg("获取工作电流超时");
                    LogHelper.Debug($"临时查找标签 NXI-6400-电流采集 {awatiTime}s 采集电流（mA）： {string.Join(",", list)}");
                    return (false, curr);
                }
            }
        }
        /// <summary>
        /// 从电源获取一级休眠电流
        /// </summary>
        /// <returns></returns>
        public static (bool, double) GetSleep1CurrentFromPower(double awatiTime = 8, double threshold = 5)
        {
            var par = CommonData.TestParameters.FirstOrDefault(m => m.Extend1 == "1" && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
            DateTime now = DateTime.Now;
            List<double> list = new List<double>();
            LogHelper.ShowMsg($"threshold:{threshold}");
            while (true)
            {
                var currStr = CommonData.NXI_6400.GetCResult(0.1);
                double curr = currStr.Item1.FirstOrDefault() * 1000;
                list.Add(curr);
                LogHelper.ShowMsg($"xm{curr}");
                if (curr<5)
                {
                    while (true)
                    {
                        if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")//五菱这里传入的值不是610是710
                        {
                            currStr = CommonData.NXI_6400.GetCResult(0.01);
                            curr = currStr.Item1.FirstOrDefault() * 1000;
                        }
                        else {
                            currStr = CommonData.NXI_6400.GetCResult(0.001);
                            curr = currStr.Item1.FirstOrDefault() * 1000;
                        }
                        LogHelper.ShowMsg($"xm{curr}");
                        list.Add(curr);
                        if (curr > par.MinVal.Value && curr < par.MaxVal.Value)
                        {
                            return (true, curr);
                        }
                        if ((DateTime.Now - now).TotalMilliseconds > awatiTime * 1000)
                        {
                            //  LogHelper.MessageBoxShow("获取一级休眠电流超时");
                            LogHelper.Debug($"临时查找标签 NXI-6400-电流采集 {awatiTime}s 采集电流（mA）： {string.Join(",", list)}");
                            return (false, curr);
                        }
                    }
                }
               
                if ((DateTime.Now - now).TotalMilliseconds > awatiTime * 1000)
                {
                  //  LogHelper.MessageBoxShow("获取一级休眠电流超时");
                    LogHelper.Debug($"临时查找标签 NXI-6400-电流采集 {awatiTime}s 采集电流（mA）： {string.Join(",", list)}");
                    return (false, curr);
                }
                
            }
        }
        /// <summary>
        /// 从电源获取二级休眠电流
        /// </summary>
        /// <returns></returns>
        public static (bool, double) GetSleep2CurrentFromPower(double awatiTime = 2.6, double threshold = 0.2)
        {
            var par = CommonData.TestParameters.FirstOrDefault(m => m.Extend1 == "2" && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
            DateTime now = DateTime.Now;
            List<double> list = new List<double>();
            LogHelper.ShowMsg($"threshold:{threshold}");
            while (true)
            {
                var currStr = CommonData.NXI_6400.GetCResult(0.1);
                double curr = currStr.Item1.FirstOrDefault() * 1000;

               
              

                    list.Add(curr);
                LogHelper.ShowMsg($"2xm{curr}");
                //if (curr < threshold)
                if (curr > par.MinVal.Value && curr < par.MaxVal.Value)
                {
                    currStr = CommonData.NXI_6400.GetCResult(0.001);
                    curr = currStr.Item1.FirstOrDefault() * 1000;
                    LogHelper.ShowMsg($"2xm{curr}");
                        return (true, curr);
                  
                        
                }
                if ((DateTime.Now - now).TotalMilliseconds > awatiTime * 1000)
                {
                   // LogHelper.MessageBoxShow("获取二级休眠电流超时");
                    LogHelper.Debug($"临时查找标签 NXI-6400-电流采集 {awatiTime}s 采集电流（mA）： {string.Join(",", list)}");
                    return (false, curr);
                }
            }
        }
        #region 更新数据库配置文件
        /// <summary>
        /// 更新数据库配置文件
        /// </summary>
        public static void RefreshDatabaseData(string tableName = "")
        {
            if (tableName == "Key_Val" || string.IsNullOrEmpty(tableName))
            {
                var key_vals = Key_ValDal.GetAll();
                CommonData.Key_Vals = key_vals;
            }
            if (tableName == "TestParameter" || string.IsNullOrEmpty(tableName))
            {
                var pars = TestParDal.GetAllTestPar();
                CommonData.TestParameters = pars;
            }


            if (tableName == "Sys_User" || string.IsNullOrEmpty(tableName))
            {
                if (CommonData.CurrentUser != null)
                {
                    CommonData.CurrentUser = UserDal.Login(CommonData.CurrentUser.Account, CommonData.CurrentUser.Pwd);
                }
            }
            CommonData.PlayThreadControlEvent.Set();
        }
        #endregion

      

        

        public static (bool, double?)? SetN8331(double v, double await = 5)
        {
            LogHelper.Debug($"设置N8331模拟电芯电压1-4 {v / 4.0}V");
            if (CommonData.N8331.SetVol4(v / 4.0) == false)
            {
                throw new Exception($"设置N8331模拟电芯电压1-4 {v / 4.0}V 失败");
            }
            return (true, 0);


        }

        #region 创建与关闭死循环任务
        /// <summary>
        /// 创建死循环线程
        /// </summary>
        /// <param name="action">死循环执行代码</param>
        /// <param name="time">间隔时间</param>
        /// <param name="taskDes">线程描述</param>
        /// <returns></returns>
        public static Task PlayTaskRun(Action action, int time, string taskDes = "")
        {
            isTaskDo = true;
            return Task.Run(async () =>
            {
                var temp = new ManualResetEvent(false);
                manualResetEvents.Add((temp, taskDes));
                while (true)
                {
                    if (isTaskDo == false)
                    {
                        temp.Set();
                        return;
                    }
                    action?.Invoke();
                    await Task.Delay(time);
                }
            });
        }
        static bool isTaskDo = true;
        static List<(ManualResetEvent, string)> manualResetEvents = new List<(ManualResetEvent, string)>();
        public static void EndAllTask()
        {
            isTaskDo = false;
            foreach (var temp in manualResetEvents)
            {
                temp.Item1.Reset();
            }
            foreach (var temp in manualResetEvents)
            {
                if (temp.Item1.WaitOne(3000))
                {

                }
                else
                {
                    throw new Exception(temp.Item2 + "线程关闭失败");
                }
            }
        }
        #endregion


        /// <summary>
        /// 获取稳定的电芯模拟器电压
        /// </summary>
        /// <param name="v">设置的电压</param>
        /// <returns>稳定的四个电芯电压</returns>
        public static List<double> GetN8331Vol4(double v)
        {
            DateTime now = DateTime.Now;
            while (true)
            {
                var temp = CommonData.N8331.GetVol4();
                if (temp.Any(m => Math.Abs(m - v) > 0.002) == false)
                {
                    return temp;
                }
                if ((DateTime.Now - now).TotalSeconds > 5)
                {
                    return temp;
                }
            }

        }

        public static double GetN8331Volt()
        {
            DateTime now = DateTime.Now;
            
            var temp = CommonData.N8331.GetVol4();


            return temp[0] + temp[1] + temp[2] + temp[3];
        }

        #region 保存文件
        static string filePath = "";
        /// <summary>
        /// 条码
        /// </summary>
        /// <param name="no"></param>
        public static void InitFrameFile(string no)
        {
            lock (_lock)
            {
                filePath = $"{CommonData.FrameDirPath}{no}_{DateTime.Now.ToString("HH-mm-ss")}.asc";
                using (var writer = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    // 写入文件头
                    string formattedDate = DateTime.Now.ToString(
                        "ddd MMM dd hh:mm:ss tt yyyy",
                        CultureInfo.InvariantCulture  // 确保英文月份/星期
                    );
                    writer.WriteLine($"date {formattedDate}");

                    writer.WriteLine("base hex timestamps absolute");
                    writer.WriteLine("// version: 1");

                    CommonData.CanStartTime = DateTime.Now;
                }
            }

        }

        public static void AddFrameToFile(CanFrame frame)
        {
            lock (_lock)
            {
                using (var writer = new StreamWriter(filePath, true, Encoding.ASCII))
                {
                    frame.TimeStamp = (DateTime.Now - CommonData.CanStartTime).TotalMilliseconds;
                    string idHex = frame.IsExtended ? frame.Id.ToString("X8") + "x" : frame.Id.ToString("X");
                    // 格式化时间戳（六位小数）
                    string timestamp = frame.TimeStamp.ToString("0.000000", CultureInfo.InvariantCulture);
                    // 数据字节转为两位十六进制（空格分隔）
                    string dataStr = (frame.Dlc > 0)
                        ? string.Join(" ", frame.Data.Take(frame.Dlc).Select(b => b.ToString("X2")))
                        : "";

                    // 构建行数据
                    string line = $"{timestamp} {frame.Channel} {idHex} {frame.Direction} {frame.FrameType} {frame.Dlc} {dataStr}";
                    if (frame.CanType == "CANFD")
                    {
                        line = $"{timestamp} CANFD {frame.Channel} {idHex} {frame.Direction} 1 0 {frame.FrameType} {cale(frame.Dlc)} {frame.Dlc} {dataStr}";
                    }
                    writer.WriteLine(line);
                }
            }
            int cale(int d)
            {
                if (d <= 8)
                    return d;
                if (d <= 12)
                {
                    return 9;
                }
                if (d <= 16)
                {
                    return 10;
                }
                if (d <= 20)
                {
                    return 11;
                }
                if (d <= 24)
                {
                    return 12;
                }
                if (d <= 32)
                {
                    return 13;
                }
                if (d <= 48)
                {
                    return 14;
                }
                if (d <= 64)
                {
                    return 15;
                }

                return d;
            }
        }

        public static void EndWriteFrame()
        {
            lock (_lock)
            {
                using (var writer = new StreamWriter(filePath, true, Encoding.ASCII))
                {
                    writer.WriteLine("End Triggerblock");
                }
            }
        }
        #endregion
    }
}
