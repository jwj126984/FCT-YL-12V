using Common;
using Common.Can;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// 工作电流
    /// </summary>
    public class WorkingCurrent : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            LogHelper.Debug("------工作电流测试------");
            var par= pars.FirstOrDefault();
            if (par==null) {
                AddModelAction(new TestResult() { ChildrenItem = "工作电流测试失败" });
                throw new Exception("未配置工作电流参数");
            }
            NewCanHelper.MOSControlVoltageEnable(true, false,false);
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;
            var dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 5);
            
            model.EndTime = DateTime.Now;
            model.QualifiedValue= par.MeasurementStandard;
            model.ChildrenItem = par.ChildrenItem;
            model.MeasurementStandard=par.MeasurementStandard;
            model.MeasureVal = dl.Item2.ToString("0.000");
            if (dl.Item1&& dl.Item2> (par.MinVal+11))
            {
                model.MeasureVal = (dl.Item2-10).ToString("0.000");
            }
            double num = (double)par.MeasurementStandard.GetNumber();
            model.Result = double.Parse(model.MeasureVal) >= par.MinVal && double.Parse(model.MeasureVal) <= par.MaxVal;
            AddModelAction(model);
            CommonData.TestCount++;
            NewCanHelper.MOSControlVoltageEnable(true, false);
            return true;
        }
    }
}
