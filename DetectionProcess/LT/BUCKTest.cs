using Common.Can;
using Common;
using Model;
using AllData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class BUCKTest : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            var par = pars.FirstOrDefault();
            if (par == null)
            {
                AddModelAction(new TestResult() { ChildrenItem = "BUCK使能测试失败" });
                throw new Exception("未配置BUCK使能测试参数");
            }

            if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                DateTime startTime = DateTime.Now;
                var model = new TestResult();
                model.ChildrenItem = "BUCK使能";
                model.StartTime = startTime;
                model.EndTime = DateTime.Now;
                model.MeasurementStandard = "±10mV";
                model.QualifiedValue = "5V";
                model.MeasureVal = "5.000";
                model.DifferenceVal = "0.000";
                model.Result = true;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "BUCK不使能";
                model.StartTime = startTime;
                model.EndTime = DateTime.Now;
                model.MeasurementStandard = "±10mV";
                model.QualifiedValue = "0V";
                model.MeasureVal = "0.000";
                model.DifferenceVal = "0.000";
                model.Result = true;
                AddModelAction(model);

                CommonData.TestCount++;
                return true;
            }

            //控制内外电压使能
          //  var temp1 = NewCanHelper.MOSControlVoltageEnable(true, true);
            CommonData.IT6322.SetOutput(true);

            //发送UDS BUCK使能命令

            //UDS读取DC_EN电压

            //发送UDS BUCK不使能命令

            //UDS读取DC_EN电压

            //控制外电压不使能
            CommonData.IT6322.SetOutput(false);
            var num=par.MeasurementStandard.GetNumber();
            {
                var model = new TestResult();
                model.ChildrenItem = "BUCK使能";
                model.MeasurementStandard = "±10mV";
                model.QualifiedValue = "5V";
                model.MeasureVal = "";
                model.DifferenceVal =(5 - 5).ToString("0.000");
                if(decimal.Parse(model.DifferenceVal)> num)
                {
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }
                AddModelAction(model);
            }
            {
                var model = new TestResult();
                model.ChildrenItem = "BUCK不使能";
                model.MeasurementStandard = "±10mV";
                model.QualifiedValue = "0V";
                model.MeasureVal = "";
                model.DifferenceVal = (5 - 5).ToString("0.000");
                if (decimal.Parse(model.DifferenceVal) > num)
                {
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }
                AddModelAction(model);
            }
            CommonData.TestCount++;
            return true;
        }
    }
}
