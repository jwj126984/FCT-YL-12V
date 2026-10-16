using Common.Can;
using Common;
using Common.SerialportHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.UDS;
using Model;
using System.Threading;
using System.Reflection;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// 单体电压
    /// </summary>
    public class IndividualVoltage: DetectBase
    {

        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------单体电压测试------");
            CommonData.TestCount++;
            return true;

            List<TestResult> tempModels = new List<TestResult>();
            var tempPars = pars.OrderBy(m => m.Extend2);
            foreach (var item in tempPars)
            {
               
                DateTime startTime = DateTime.Now;
                var dys = CanHelper.ReadIndividualVoltage();
                Thread.Sleep(250);

                var zcdys = ZCData.DYS;
                DateTime endTime = DateTime.Now;
                var timeSpan = (endTime - startTime).TotalSeconds / 4;
                TestResult model = new TestResult();
                model.StartTime = startTime;
                model.ChildrenItem = item.ChildrenItem.Replace('*','1');
                model.EndTime = startTime.AddSeconds(timeSpan);
                model.MeasurementStandard = item.MeasurementStandard;
                //if()
                bool b = (dys.Item1 - item.QualifiedValue) <= 0.1;
                if (!b)
                {
                    throw new Exception("工装采集电压异常");
                }
                model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                model.MeasureVal = zcdys.Item1.ToString("0.000");
                double num = (double)item.MeasurementStandard.GetNumber();
                model.DifferenceVal = (zcdys.Item1 - dys.Item1).ToString("0.000");
                model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                //AddModelAction(model);
                tempModels.Add(model);


                model = new TestResult();
                model.ChildrenItem = item.ChildrenItem.Replace('*', '2');
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(timeSpan);
                model.MeasurementStandard = item.MeasurementStandard;
                b = (dys.Item2 - item.QualifiedValue) <= 0.1;
                if (!b)
                {
                    throw new Exception("工装采集电压异常");
                }
                model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                model.MeasureVal = zcdys.Item2.ToString("0.000");
                num = (double)item.MeasurementStandard.GetNumber();
                model.DifferenceVal = (zcdys.Item2 - dys.Item1).ToString("0.000");
                model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                tempModels.Add(model);

                model = new TestResult();
                model.ChildrenItem = item.ChildrenItem.Replace('*', '3');
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(timeSpan);
                model.MeasurementStandard = item.MeasurementStandard;
                b = (dys.Item2 - item.QualifiedValue) <= 0.1;
                if (!b)
                {
                    throw new Exception("工装采集电压异常");
                }
                model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                model.MeasureVal = zcdys.Item3.ToString("0.000");
                num = (double)item.MeasurementStandard.GetNumber();
                model.DifferenceVal = (zcdys.Item3 - dys.Item1).ToString("0.000");
                model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                tempModels.Add(model);

                model = new TestResult();
                model.ChildrenItem = item.ChildrenItem.Replace('*', '4');
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(timeSpan);
                model.MeasurementStandard = item.MeasurementStandard;
                b = (dys.Item2 - item.QualifiedValue) <= 0.1;
                if (!b)
                {
                    throw new Exception("工装采集电压异常");
                }
                model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                model.MeasureVal = zcdys.Item4.ToString("0.000");
                num = (double)item.MeasurementStandard.GetNumber();
                model.DifferenceVal = (zcdys.Item4 - dys.Item1).ToString("0.000");
                model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                tempModels.Add(model);
            }
            foreach (var item in tempModels.OrderBy(m=>m.QualifiedValue))
            {
                AddModelAction(item);
            }   
            return true;
        }
    }
}
