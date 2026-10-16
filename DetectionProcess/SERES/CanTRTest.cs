using Common;
using Common.Can;
using Model;
using Models_C;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// 终端电阻
    /// </summary>
    public class CanTRTest : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------终端电阻测试------");
            CommonData.CurrentCount++;
            var par = pars.FirstOrDefault();
            if (par == null)
            {
                //LogHelper.MessageBoxShow("未配置终端电阻参数");
                AddModelAction(new TestResult() { ChildrenItem = "终端电阻测试失败" });
                throw new Exception("未配置终端电阻参数");
            }
            TestResult model = new TestResult();
            model.Id = 1;
            model.StartTime = DateTime.Now;
            model.Id = CommonData.CurrentCount;
            //CommonMethod.Sleep(1000);
            //if (!CanHelper.ResistivePowerOn(true))
            //{
            //    //LogHelper.MessageBoxShow("终端电阻上电使能失败");
            //    AddModelAction(new TestResult() { ChildrenItem = "终端电阻测试失败" });
            //    throw new Exception("终端电阻上电使能失败");
            //}
            //CommonMethod.Sleep(1000);
            CommonData.Relay.SetRelayCard1(5,true);
            CommonMethod.Sleep(2000);
            double num = (double)par.MeasurementStandard.GetNumber();

            double dz = (double)NewCanHelper.GetCanTR();
      
            //LogHelper.ShowMsg($"{dz}");
            //while (true)
            //{
            //    CommonMethod.Sleep(500);
            //    dz = (double)NewCanHelper.GetCanTR();
            //    LogHelper.ShowMsg($"{dz}");
            //}
            model.MeasureVal = dz.ToString("0.000");
            model.Result = par.MinVal < dz && par.MaxVal > dz;
            
            //model.Result = true;



            model.MeaMinVal = par.MinVal;
            model.MeaMaxVal = par.MaxVal;
            model.ChildrenItem = par.ChildrenItem;
            model.MeasurementStandard = $"[{par.MinVal},{par.MaxVal}]";

            model.QualifiedValue = $"[{par.MinVal},{par.MaxVal}]";


            model.DifferenceVal = "";


            if (dz == double.NaN)
            {
                //LogHelper.MessageBoxShow("终端电阻获取失败");
                AddModelAction(new TestResult() { ChildrenItem = "终端电阻测试失败" });
                throw new Exception("终端电阻获取失败");
            }
            model.EndTime = DateTime.Now;
            AddModelAction?.Invoke(model);
            CommonData.Relay.SetRelayCard1(5, false);
            CommonData.TestCount++;
            return true;
        }
    }
}
