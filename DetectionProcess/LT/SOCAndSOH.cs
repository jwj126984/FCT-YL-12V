using Common.Can;
using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllData;

namespace DetectionProcess.LT
{
    public class SOCAndSOH : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            LogHelper.Debug("------SOC标定与SOH测试------");

            

            var par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("soc"));
            if (par == null)
            {
                //LogHelper.ShowMsg("SOC测试参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                throw new Exception("SOC测试参数未配置");
            }
            //CommonData.IT6322.SetVol(13.4);
            CommonMethod.SetN8331(14);
           

           
            #region 唤醒处理
            NewCanHelper.WakeOrSleep(true);
            CommonMethod.StartSendAwaken();
            CommonMethod.Sleep(200);
            if (!CommonMethod.Unlock())
            {
                //LogHelper.ShowMsg("解锁失败");
                AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                throw new Exception("SOC测试解锁失败");
            }
            else
            {
                //LogHelper.ShowMsg("解锁成功");
            }
     //       CommonMethod.StartSendUnlock();
            #endregion
            CommonMethod.Sleep(100);
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;
            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                PlayUDSHelper.GoToGC();
            }
            if (PlayUDSHelper.SetSOC((int)(par.QualifiedValue))==false )
            {
                //LogHelper.ShowMsg("SOC设置失败");
                AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                throw new Exception($"SOC设置{par.QualifiedValue}失败");
            }
            CommonMethod.Sleep(1000);
            //         if (PlayUDSHelper.Reset() == false)
            //{
            //    LogHelper.ShowMsg("复位失败");
            //    AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
            //    throw new Exception($"复位失败");
            //}
            //AddModelAction(new TestResult() { ChildrenItem = "复位", Extend1 = "-1" });

            PlayUDSHelper.UdsSleep(2);
            CommonMethod.Sleep(1000);
            



        //    CommonData.IT6322.SetOutput(false);
        // //   CommonData.N8331.SetOutPut4(false);
        //    CommonMethod.StopSendUnlock();
        //    CommonMethod.StopSendAwaken();
        //    CommonMethod.Sleep(10000);
        //    CommonData.IT6322.SetOutput(true);
        ////    CommonData.N8331.SetOutPut4(true);
        //    //dl = CommonMethod.GetWorkCurrent();
            #region 唤醒处理
            NewCanHelper.WakeOrSleep(true);
        //    CommonMethod.StartSendAwaken();
        ////    CommonMethod.Sleep(500);
        //    if (!CommonMethod.Unlock())
        //    {
        //        //LogHelper.ShowMsg("解锁失败");
        //        AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
        //        throw new Exception("SOC测试解锁失败");
        //    }
        //    else
        //    {
        //        //LogHelper.ShowMsg("解锁成功");
        //    }
        //    CommonMethod.StartSendUnlock();
            #endregion

            model.EndTime = DateTime.Now;

            model.ChildrenItem = "SOC";
            model.QualifiedValue = par.QualifiedValue.Value.ToString("0.000");
            model.MeasureVal = ZCData.SOC.ToString("0.000");
            LogHelper.Debug($"整车SOC转换后为{model.MeasureVal}");
            model.MeasurementStandard = par.MeasurementStandard;
            model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
            model.Result = double.Parse(model.MeasureVal) == par.QualifiedValue;
            AddModelAction(model);

           // if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390")
            {
                par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("soh"));
                if (par == null)
                {
                    //LogHelper.ShowMsg("SOH测试参数未配置");
                    AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                    throw new Exception("SOH测试参数未配置");
                }
                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.EndTime = DateTime.Now;
                model.ChildrenItem = "SOH";
                model.QualifiedValue = par.QualifiedValue.Value.ToString("0.000");
                model.MeasureVal = ZCData.SOH.ToString("0.000");
                LogHelper.Debug($"整车SOH转换后为{model.MeasureVal}");
                model.MeasurementStandard = par.MeasurementStandard;
                model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
                model.Result = double.Parse(model.MeasureVal) == par.QualifiedValue;
                AddModelAction(model);
            }
                

            CommonData.TestCount++;
            //power1.SetVoltage(12.8);
            return true;
        }

        bool XiuMian(int level, bool isMOS = false)
        {
            //第一步休眠
            CommonMethod.StopSendAwaken();
  //          CommonMethod.StopSendUnlock();
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
            LogHelper.Debug("休眠产品");
            List<byte> data = new List<byte>();
            if (level == 1)
                data = new List<byte>() { 0x34, 0x03, 0x03, 0x0B };
            else if (level == 2)
            {
                data = new List<byte>() { 0x34, 0x03, 0x03, 0x02 };
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

    }
}
