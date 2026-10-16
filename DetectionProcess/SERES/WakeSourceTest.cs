using Common.Can;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.UDS;
using Common.SerialportHelper;
using Model;
using AllData;
using System.Reflection;

namespace DetectionProcess.SERES
{
    public class WakeSourceTest : DetectBase
    {
        //public event Action<TestResult> AddModelAction;
        TestParameter par;
        double currentVot;
        bool isTestSleepCurr = false;
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            //判断是否存在休眠电流流程
            var tempPar = CommonData.CurrentUserDetects.Select(m => m.Item1).FirstOrDefault(m => m == "SleepingCurrent");
            isTestSleepCurr = !string.IsNullOrEmpty(tempPar);

            NewCanHelper.MOSControlVoltageEnable(true, false,false);
            LogHelper.Debug("------唤醒源测试------");
            par = pars.FirstOrDefault();
            if (par == null)
            {
                //LogHelper.MessageBoxShow("未配置唤醒源参数");
                AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试" });
                throw new Exception($"未配置唤醒源参数");
            }
            currentVot = double.Parse(CommonData.N8331.GetVol4().Sum().ToString("0.0"));
            //CommonData.IT6122B.SetVoltageAndStable(12);
            //CommonData.N8331.SetVol4(12 / 4);
            CommonMethod.SetN8331(12);


            bool isSuccess = true;
            bool b1 = true;
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "RTC唤醒";
            model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_RTC;
            model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_RTC;
            b1 = RTC();
            if (b1 == false)
            {
                isSuccess = false;
            }
            else
            {
                //model.Result = true;
            }
    
            if(ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            {

                //读取唤醒源
                GetWakeSource(model);
                CommonMethod.StartSendAwaken();

                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.ShowMsg("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();

            }
            else
            {
                CommonMethod.StartSendAwaken();

                //读取唤醒源
                GetWakeSource(model);

                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.ShowMsg("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();

                
            }
                
            model.EndTime = DateTime.Now;
            if (model.Result == true)
            {
                model.DifferenceVal = "0.000";
            }
            else if (model.Result == false)
            {
                model.DifferenceVal = "1.000";
            }
            AddModelAction?.Invoke(model);


            model = new TestResult();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "帮电唤醒";
            model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_GY;
            model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_GY;
            NewCanHelper.MOSControlVoltageEnable(true, true,false);
            b1 = GuoYa();
            NewCanHelper.MOSControlVoltageEnable(true, false, false);
            if (b1 == false)
            {
                isSuccess = false;
                //model.Result = false;
            }
            else
            {
                //model.Result = true;
            }
            CommonMethod.StartSendAwaken();
            GetWakeSource(model);
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
            CommonMethod.Unlock();
           CommonMethod.StartSendUnlock();
            //读取唤醒源
           

            model.EndTime = DateTime.Now;
            if (model.Result == true)
            {
                model.DifferenceVal = "0.000";
            }
            else if (model.Result == false)
            {
                model.DifferenceVal = "1.000";
            }
            AddModelAction?.Invoke(model);


            if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" &&  ConfigHelper.UdsParameters.jiesuo != "SERES_V32")
            {
                model = new TestResult();

                model.StartTime = DateTime.Now;
                model.ChildrenItem = "电流标定K值读取";
                model.MeasurementStandard = "0000 - FFFE";

                GetK(model);

                model.EndTime = DateTime.Now;

                AddModelAction?.Invoke(model);
            }
                

            model = new TestResult();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "硬件唤醒";
            model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_YJ;
            model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_YJ;
            b1 = YingJian();
            if (b1 == false)
            {
                isSuccess = false;
                //model.Result = false;
            }
            else
            {
                //model.Result = true;
            }
            CommonMethod.StartSendAwaken();

            GetWakeSource(model);

            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            //读取唤醒源
            
            

            model.EndTime = DateTime.Now;
            if (model.Result == true)
            {
                model.DifferenceVal = "0.000";
            }
            else if (model.Result == false)
            {
                model.DifferenceVal = "1.000";
            }
            AddModelAction?.Invoke(model);

            model = new TestResult();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "电流唤醒";
            model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_DL;
            model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_DL;
            b1 = DianLiu();
            if (b1 == false)
            {
                isSuccess = false;
                model.Result = false;
            }
            else
            {
                model.Result = true;
            }
            CommonMethod.StartSendAwaken();
            GetWakeSource(model);
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
           
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            //读取唤醒源
            
            model.EndTime = DateTime.Now;
            if (model.Result == true)
            {
                model.DifferenceVal = "0.000";
            }
            else if (model.Result == false)
            {
                model.DifferenceVal = "1.000";
            }
            AddModelAction?.Invoke(model);

            CommonMethod.Unlock();
            PlayUDSHelper.MOSControl(MOSEnum.断开);//MOS断开


            model = new TestResult();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "Can唤醒";
            model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_CAN;
            model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_CAN;
          //  PlayUDSHelper.ClearError();
            b1 = CanHX();

            CommonMethod.Sleep(1000);
            CommonMethod.StartSendAwaken();
            GetWakeSource(model);
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("工装唤醒失败");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            
            //读取唤醒源
           


            if (b1 == false)
            {
                isSuccess = false;
                //model.Result = false;
            }
            else
            {
                //model.Result = true;
            }
            model.EndTime = DateTime.Now;
            if (model.Result == true)
            {
                model.DifferenceVal = "0.000";
            }
            else if (model.Result == false)
            {
                model.DifferenceVal = "1.000";
            }
            AddModelAction?.Invoke(model);

            //CommonData.IT6122B.SetVoltageAndStable(currentVot);
            CommonMethod.SetN8331(12.8);


            foreach (var item in sleepCurrResult.OrderByDescending(m=>m.ChildrenItem))
            {
                AddModelAction(item);
            }
            NewCanHelper.MOSControlVoltageEnable(true, false);
            CommonData.TestCount++;
            return isSuccess;

        }

        bool XiuMian(int level, bool isMOS = false)
        {
            //第一步休眠
            
            CommonMethod.Unlock();
            //Class1.HuanXing(false);
            //发送休眠指令
            if (NewCanHelper.WakeOrSleep(false) == false)
            {
                LogHelper.ShowMsg("休眠失败");
                return false;
            }
            if (isMOS)
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("MOS闭合失败");
                }
            }
            else
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
                {
                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("MOS断开失败");
                }

            }

            CommonMethod.StopSendAwaken();
            CommonMethod.StopSendUnlock();
            LogHelper.Debug("休眠产品");
            List<byte> data = new List<byte>();
            if (level == 1)
                data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.XM1_DID);
            else if (level == 2)
            {
                data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.XM2_DID);
            }
            //uds发送休眠
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x2f);
            if (temp == null)
            {
                temp = PlayUDSHelper.Send(data.ToArray(), 0x2f);
                if (temp == null)
                {
                    LogHelper.ShowMsg("uds休眠失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("uds休眠失败");
                }

            }
            return true;
        }

        void GetWakeSource(TestResult model)
        {
            CommonMethod.Sleep(200);

            //if (ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            //{
                
            //    model.MeasureVal = ZCData.HXY.ToString();
               
               
            //    model.Result = model.MeasureVal == model.QualifiedValue;

            //}
            //else
            {
                //读取唤醒源
                var data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.GetHX_DID);
                //string huanxingFangshi = "";
                var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);

                if (temp == null)
                {
                    LogHelper.ShowMsg("读取唤醒源失败");
                    return;
                }
                else
                {
                    string str = "";
                    foreach (var item in temp.Item2)
                    {
                        str += item.ToString("X2") + " ";
                    }
                    LogHelper.ShowMsg(str);
                    model.MeasureVal = int.Parse(temp.Item2[2].ToString("X2")).ToString();
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    //model.MeasurementStandard = str;
                }
            }
               
        }

        void GetK(TestResult model)
        {
            CommonMethod.Sleep(200);
            //读取唤醒源
            var data = HexChange(ConfigHelper.UdsParameters.DLB_DID);
            //string huanxingFangshi = "";
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
            if (temp == null)
            {
                LogHelper.ShowMsg("读取唤醒源失败");
                return;
            }
            else
            {
                string str = "";
                foreach (var item in temp.Item2)
                {
                    str += item.ToString("X2") + " ";
                }
                LogHelper.ShowMsg(str);
                model.MeasureVal = temp.Item2[4].ToString("X2") + temp.Item2[5].ToString("X2");
                model.Result = model.MeasureVal != "FFFF";
                //model.MeasurementStandard = str;
            }
        }



        public static List<byte> HexChange(string hexStr)
        {
            string[] hexParts = hexStr.Split(' ');
            List<byte> byteList = new List<byte>();

            foreach (string part in hexParts)
            {
                if (byte.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                {
                    byteList.Add(result);
                }
                else
                {
                    Console.WriteLine($"无效的十六进制数: {part}");
                }
            }
            return byteList;

        }
        public bool RTC()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();

            #region rtc唤醒
            XiuMian(1);
            (bool, double) dl;
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 3);

            if (dl.Item1 == false)
            {
                //LogHelper.ShowMsg("RTC休眠失败");
                AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                throw new Exception("RTC休眠失败");
                //return false;
            }
            else
            {
               


                dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
                if (dl.Item1 == false)
                {
                    LogHelper.ShowMsg("RTC唤醒失败");
                    return false;
                }
                return true;
            }
            #endregion
        }


        public bool GuoYa()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;


            #region 过压唤醒
            //关闭MOS
            //MOS控制断开cmd
            var temp = PlayUDSHelper.MOSControl(MOSEnum.断开);
            if (temp == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                throw new Exception("过压唤醒MOS控制断开失败");
            }
            //二级休眠
            XiuMian(2);
            dl = CommonMethod.GetSleep2CurrentFromPower(awatiTime: 5, threshold: 0.25);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("过压唤醒二级休眠失败");
                return false;
            }
            else
            {
                AddSleepCurr("2", dl.Item2);
            }


            CommonMethod.Sleep(5000);




            //控制外电压使能
           
            CommonData.IT6322.SetOutput(true);
           

            CommonMethod.Sleep(1500);
            NewCanHelper.MOSControlVoltageEnable(true, true, false, true);
            var volt = CommonData.NXI_6400.GetVResult();
            LogHelper.ShowMsg($"外总{volt}");
            //控制外电压不使能
            CommonData.IT6322.SetOutput(false);


            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("过压唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("过压唤醒成功");
            }

            return true;
            #endregion
        }

        public bool YingJian()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region 硬件唤醒
            XiuMian(1);
            dl = CommonMethod.GetSleep2CurrentFromPower(awatiTime: 5, threshold: 0.25);

            if (dl.Item1 == false)
            {
                AddSleepCurr("1", dl.Item2);
                LogHelper.ShowMsg("硬件唤醒休眠失败");
                return false;
            }
            else
            {
                AddSleepCurr("1", dl.Item2);
            }
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败");
            }
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败");
            }
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败电流过低");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("硬件唤醒成功");
            }

            return true;

            #endregion
        }

        public bool DianLiu()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region 电流唤醒
         
            XiuMian(1, true);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 10);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("电流唤醒休眠失败");
                return false;
            }

            //设置电源
            CommonData.M3905D.SetOUTPut(1);
            CommonData.M3905D.SetVoltage(1);
            CommonData.M3905D.SetCurrent(20);
      //      CommonMethod.Sleep(1000);
            //控制电流正向采集
            NewCanHelper.CtrolTotalCurrentCollection(true);
            //Thread.Sleep(2000);
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 20);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("电流唤醒失败");
                return false;
            }
            CommonData.M3905D.SetVoltage(0);
            CommonData.M3905D.SetOUTPut(0);
            NewCanHelper.CloseTotalCurrentCollection();
            LogHelper.ShowMsg("电流唤醒成功");


            return true;
            #endregion
        }

        public bool CanHX()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region Can唤醒
            XiuMian(1);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 10);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("Can唤醒休眠失败");
                return false;
            }
            CommonMethod.StartSendAwaken();

            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 20);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("Can唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("Can唤醒成功");
            }
         //   CommonMethod.Unlock();
   //         CommonMethod.StartSendUnlock();
            return true;
            #endregion
        }

        List<TestResult> sleepCurrResult = new List<TestResult>();
        public void AddSleepCurr(string level, double curr)
        {
            var par = CommonData.TestParameters.FirstOrDefault(m => m.Extend1 == level && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
            if (par == null)
            {
                LogHelper.Debug("休眠电流参数未配置");
            }
            else
            {
                if (isTestSleepCurr == true)
                {
                    TestResult model = new TestResult();
                    model.MeasureVal = curr.ToString("0.000");
                    model.Result = double.Parse(model.MeasureVal) <= par.MaxVal && double.Parse(model.MeasureVal) >= par.MinVal;
                    model.ChildrenItem = par.ChildrenItem;
                    model.MeasurementStandard = par.MeasurementStandard;
                    sleepCurrResult.Add(model);
                }
            }
        }
    }
}
