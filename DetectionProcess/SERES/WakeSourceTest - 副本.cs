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

namespace DetectionProcess.SERES
{
    public class WakeSourceTest: DetectBase
    {
        //public event Action<cssj_SERES_F2> AddModelAction;
        cscs_SERES_F2 par;
        double currentVot;
        public override bool Start(IEnumerable<cscs_SERES_F2> pars)
        {
            par = pars.FirstOrDefault();
            if (par == null)
            {
                LogHelper.MessageBoxShow("未配置唤醒源参数");
                return false;
            }
            currentVot =double.Parse( power1.GetMEASVoltage());
            power1.SetVoltageAndStable(12);

            bool isSuccess = true;
            cssj_SERES_F2 model= new cssj_SERES_F2();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "RTC唤醒";
            model.QualifiedValue = 1;
            model.MeasurementStandard = "1";
            var b1 = RTC();
            if (b1 == false)
            {
                isSuccess = false;
            }
            else
            {
                //model.Result = true;
            }
            CommonMethod.StartSendAwaken();
            if (CanHelper.Awaken()==false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            //读取唤醒源
            GetWakeSource(model);
            model.EndTime = DateTime.Now;
            AddModelAction?.Invoke(model);


            model = new cssj_SERES_F2();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "过压唤醒";
            model.QualifiedValue = 4;
            model.MeasurementStandard = "4";
            b1 = GuoYa();
            
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
            if (CanHelper.Awaken() == false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            //读取唤醒源
            GetWakeSource(model);
            model.EndTime = DateTime.Now;
            AddModelAction?.Invoke(model);



            model = new cssj_SERES_F2();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "硬件唤醒";
            model.QualifiedValue = 8;
            model.MeasurementStandard = "8";
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
            if (CanHelper.Awaken() == false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            //读取唤醒源
            GetWakeSource(model);
            model.EndTime = DateTime.Now;
            AddModelAction?.Invoke(model);

            model = new cssj_SERES_F2();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "电流唤醒";
            model.QualifiedValue = 10;
            model.MeasurementStandard = "10";
            b1 = DianLiu();
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
            if (CanHelper.Awaken() == false)
            {
                LogHelper.MessageBoxShow("工装唤醒失败");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            //读取唤醒源
            GetWakeSource(model);
            model.EndTime = DateTime.Now;
            AddModelAction?.Invoke(model);

            PlayUDSHelper.MOSControl(MOSEnum.断开);//MOS断开


            model = new cssj_SERES_F2();
            model.StartTime = DateTime.Now;
            model.ChildrenItem = "Can唤醒";
            model.QualifiedValue = 20;
            model.MeasurementStandard = "20";
            b1 = CanHX();
            //读取唤醒源
            GetWakeSource(model);
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
            AddModelAction?.Invoke(model);

            power1.SetVoltageAndStable(currentVot);
            return isSuccess;

        }

        bool XiuMian(int level,bool isMOS=false)
        {
            //第一步休眠
            CommonMethod.StopSendAwaken();
            CommonMethod.StopSendUnlock();
            //Class1.HuanXing(false);
            //发送休眠指令
            if (CanHelper.Dormancy() == false)
            {
                LogHelper.MessageBoxShow("休眠失败");
                return false;
            }
            if (isMOS)
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    LogHelper.ShowMsg("MOS闭合失败");
                }
            }
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
                    LogHelper.MessageBoxShow("uds休眠失败");
                    return false;
                }

            }
            return true;
        }

        void GetWakeSource(cssj_SERES_F2 model)
        {
            //读取唤醒源
            var data = new List<byte>() { 0x01, 0x15 };
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
                model.MeasureVal = int.Parse(temp.Item2[2].ToString("X2"));
                model.Result = model.MeasureVal == model.QualifiedValue;
                //model.MeasurementStandard = str;
            }
        }

        List<cssj_SERES_F2> sleepResult = new List<cssj_SERES_F2>();
        public bool RTC()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            double dl = double.NaN;
            #region rtc唤醒
            XiuMian(1);
            dl = CommonMethod.GetSleep1CurrentFromPower();
            Thread.Sleep(1000);
            if (dl > CommonData.Sleep1CurrentThreshold)
            {
                LogHelper.MessageBoxShow("休眠失败");
                return false;
            }
            else
            {
                now = DateTime.Now;
                var currStr = power1.GetMEASCurrent();
                while (true)
                {
                    currStr = power1.GetMEASCurrent();
                    double curr = double.Parse(currStr)*1000;
                    if (curr > 20)
                    {
                        LogHelper.ShowMsg("RTC唤醒成功");
                        break;
                    }
                    if ((DateTime.Now - now).TotalSeconds > 8)
                    {
                        LogHelper.MessageBoxShow("RTC唤醒失败");
                        return false;
                    }
                }
                return true;
            }
            #endregion
        }


        public bool GuoYa()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            double dl = double.NaN;

            
            #region 过压唤醒
            //关闭MOS
            //MOS控制断开cmd
            var temp = PlayUDSHelper.MOSControl(MOSEnum.断开);
            if (temp == false)
            {
                LogHelper.MessageBoxShow("MOS控制断开失败");
                return false;
            }
            //二级休眠
            XiuMian(2);
            dl = CommonMethod.GetSleep2CurrentFromPower();
            if (dl > CommonData.Sleep2CurrentThreshold)
            {
                LogHelper.MessageBoxShow("二级休眠失败");
                return false;
            }
            //控制外电压使能
            var temp1 = CanHelper.MOSControlVoltageEnable(false, true);
            if (temp1.Item2 != true) {
                LogHelper.MessageBoxShow("外电压使能失败");
                return false;
            };

            Thread.Sleep(1000);
            //控制外电压不使能
            var temp2 = CanHelper.MOSControlVoltageEnable(false, false);
            if (temp2.Item2 != false)
            {
                LogHelper.MessageBoxShow("外电压不使能失败");
                return false;
            };

            dl = CommonMethod.GetWorkCurrentFromPower();
            if (dl < CommonData.WorkCurrentThreshold)
            {
                LogHelper.MessageBoxShow("过压唤醒失败");
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
            double dl = double.NaN;
            #region 硬件唤醒
            XiuMian(1);
            dl = CommonMethod.GetSleep1CurrentFromPower();
            if (dl > CommonData.Sleep1CurrentThreshold)
            {
                LogHelper.MessageBoxShow("休眠失败");
                return false;
            }
            if (CanHelper.Awaken() == false)
            {
                LogHelper.MessageBoxShow("硬件唤醒失败");
            }

            dl = CommonMethod.GetWorkCurrentFromPower();
            if (dl < CommonData.WorkCurrentThreshold)
            {
                LogHelper.MessageBoxShow("硬件唤醒失败电流过低");
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
            double dl = double.NaN;
            #region 电流唤醒
            
            //PlayUDSHelper.MOSControl(MOSEnum.闭合);//MOS闭合
            XiuMian(1,true);
            dl = CommonMethod.GetSleep1CurrentFromPower();
            if (dl > CommonData.Sleep1CurrentThreshold)
            {
                LogHelper.MessageBoxShow("休眠失败");
                return false;
            }

            //设置电源
            power2.SetCurrent(350);
            power2.SetVoltage(1);
            Thread.Sleep(1000);
            //控制电流正向采集
            CanHelper.CtrolTotalCurrentCollection(true);
            Thread.Sleep(2000);
            power2.SetVoltage(0);
            dl = CommonMethod.GetWorkCurrentFromPower();
            if (dl < CommonData.WorkCurrentThreshold)
            {
                LogHelper.MessageBoxShow("电流唤醒失败");
                return false;
            }
            LogHelper.ShowMsg("电流唤醒成功");

            
            return true;
            #endregion
        }

        public bool CanHX()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            double dl = double.NaN;
            #region Can唤醒
            XiuMian(1);
            dl = CommonMethod.GetSleep1CurrentFromPower();
            if (dl > CommonData.Sleep1CurrentThreshold)
            {
                LogHelper.ShowMsg("休眠失败");
                return false;
            }
            CommonMethod.StartSendAwaken();
            
            dl = CommonMethod.GetWorkCurrentFromPower();
            if (dl < CommonData.WorkCurrentThreshold)
            {
                LogHelper.MessageBoxShow("Can唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("Can唤醒成功");
            }
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
            return true;
            #endregion
        }
    }
}
