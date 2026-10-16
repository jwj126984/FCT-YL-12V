using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    public class SOCAndSOH : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            LogHelper.Debug("------SOC标定与SOH测试------");
            var par = pars.FirstOrDefault(m=>m.ChildrenItem.ToLower().Contains("soc"));
            if (par == null)
            {
                //LogHelper.ShowMsg("SOC测试参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                throw new Exception("SOC测试参数未配置");
            }
            //CommonData.IT6322.SetVol(13.4);
            CommonMethod.SetN8331(14);
            

            CommonMethod.Sleep(2000);
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
 //           CommonMethod.StartSendUnlock();
            #endregion
            CommonMethod.Sleep(100);
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;
            if (PlayUDSHelper.SetSOC((int)(par.QualifiedValue)) == false)
            {
                //LogHelper.ShowMsg("SOC设置失败");
                AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                throw new Exception($"SOC设置{par.QualifiedValue}失败");
            }
            CommonMethod.Sleep(1000);
            if (PlayUDSHelper.Reset() == false) {
                //LogHelper.ShowMsg("复位失败");
                AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                throw new Exception($"复位失败");
            }
            AddModelAction(new TestResult() { ChildrenItem = "复位", Extend1 = "-1" });

            CommonMethod.Sleep(6000);



            //CommonData.IT6322.SetOutput(false);
            //CommonData.N8331.SetOutPut4(false);
            //CommonMethod.StopSendUnlock();
            //CommonMethod.StopSendAwaken();
            //CommonMethod.Sleep(10000);
            //CommonData.IT6322.SetOutput(true);
            //CommonData.N8331.SetOutPut4(true);
            //dl = CommonMethod.GetWorkCurrent();
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
  //          CommonMethod.StartSendUnlock();
            #endregion

            model.EndTime = DateTime.Now;

            model.ChildrenItem = "SOC";
            model.QualifiedValue = par.QualifiedValue.Value.ToString("0.000");
            model.MeasureVal = ZCData.SOC.ToString("0.000");
            LogHelper.Debug($"整车SOC转换后为{model.MeasureVal}");
            model.MeasurementStandard = par.MeasurementStandard;
            model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
            model.Result = double.Parse(model.MeasureVal) >= (double)par.MeasurementStandard.GetNumber();
            AddModelAction(model);


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

            CommonData.TestCount++;
            //power1.SetVoltage(12.8);
            return true;
        }
        
    }
}
