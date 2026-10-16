using Common.Can;
using Common;
using Model;
using AllData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class SleepingCurrent : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------休眠电流测试------");

           

            CommonData.TestCount++;
            return true;
            LogHelper.Debug("------休眠电流测试------");

            #region 一级休眠
            var par = pars.FirstOrDefault(m => m.Extend1 == "1");
            if (par == null)
            {
                //LogHelper.MessageBoxShow("一级休眠参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception("一级休眠参数未配置");
            }
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;
            CommonMethod.Sleep(1, false);
            var c = CommonMethod.GetSleep1CurrentFromPower();

            model.MeasureVal = c.Item2.ToString("0.000");
            model.Result = double.Parse(model.MeasureVal) <= par.MaxVal && double.Parse(model.MeasureVal) >= par.MinVal;
            model.ChildrenItem = par.ChildrenItem;
            model.MeasurementStandard = par.MeasurementStandard;

            #region 唤醒处理
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception("一级休眠后工装唤醒失败");
            }
            Thread.Sleep(1000);
            var dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 5);
            if (dl.Item1 == false)
            {
                //LogHelper.MessageBoxShow("一级休眠电流测试唤醒失败");
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception($"一级休眠电流测试唤醒失败，电流为{dl.Item2}mA");
            }
            else
            {
            }
            CommonMethod.StartSendAwaken();
            if (NewCanHelper.WakeOrSleep(true))
            {
            }
            if (CommonMethod.Unlock() == false)
            {
                //LogHelper.MessageBoxShow("解锁失败");
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception($"一级休眠电流测试解锁失败");
            }
  //          CommonMethod.StartSendUnlock();
            #endregion


            model.EndTime = DateTime.Now;
            AddModelAction(model);
            #endregion

            par = pars.FirstOrDefault(m => m.Extend1 == "2");
            if (par == null)
            {
                //LogHelper.MessageBoxShow("二级休眠参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception($"二级休眠参数未配置");
            }
            model = new TestResult();
            model.StartTime = DateTime.Now;
            CommonMethod.Sleep(2, false);
            c = CommonMethod.GetSleep2CurrentFromPower();
            model.EndTime = DateTime.Now;
            model.MeasureVal = c.Item2.ToString("0.000");
            model.Result = double.Parse(model.MeasureVal) <= par.MaxVal && double.Parse(model.MeasureVal) >= par.MinVal;
            model.ChildrenItem = par.ChildrenItem;
            model.MeasurementStandard = par.MeasurementStandard;
            AddModelAction(model);

            #region 唤醒处理
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                //LogHelper.ShowMsg("休眠后唤醒失败");
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception($"二级休眠电流测试休眠后唤醒失败");
            }
            CommonMethod.StartSendAwaken();
            Thread.Sleep(1000);
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 5);
            if (dl.Item1 == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception($"二级休眠电流测试唤醒失败");
            }
            else
            {
            }
            if (CommonMethod.Unlock() == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "休眠电流测试失败" });
                throw new Exception($"二级休眠电流测试解锁失败");
            }
     //       CommonMethod.StartSendUnlock();
            #endregion
            return true;
        }
    }
}
