using AllData;
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
    public class CellTemperature : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------电芯温度测试------");
            if(ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
            {
                DateTime startTime = DateTime.Now;

                CommonMethod.Sleep(100);
                (double, double, double) ts = (0, 0, 0);


                ts = ZCData.DTemperatures;
                DateTime endTime = DateTime.Now;
                var span = (endTime - startTime).TotalSeconds / 3;

                TestResult model = new TestResult();
                model.MeasurementStandard = "≤±5";
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                model.ChildrenItem = "电芯1温度测试";
                model.QualifiedValue = "25";
                model.MeasureVal = ts.Item1.ToString("0.000");
                model.DifferenceVal = (ts.Item1 - 25).ToString("0.000");
                
                model.Result = Math.Abs((ts.Item1 - 25)) <= 5;
                AddModelAction(model);

                model = new TestResult();
                model.MeasurementStandard = "≤±5";
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                model.ChildrenItem = "电芯2温度测试";
                model.QualifiedValue = "25";
                model.MeasureVal = ts.Item2.ToString("0.000");
                model.DifferenceVal = (ts.Item2 - 25).ToString("0.000");
              
                model.Result = Math.Abs((ts.Item2 - 25)) <= 5;
                AddModelAction(model);

            }
            else
            {
                foreach (var item in pars)
                {
                    DateTime startTime = DateTime.Now;

                    //var cmdT = CanHelper.SetCellTemperature(getCellTemperatureEnum(item.QualifiedValue.Value), getCellTemperatureEnum(item.QualifiedValue.Value), getCellTemperatureEnum(item.QualifiedValue.Value), getCellTemperatureEnum(item.QualifiedValue.Value));
                    //var t1 = ZCData.DTemperature1;
                    //var t2 = ZCData.DTemperature2;
                    //var t3 = ZCData.DTemperature3;
                    NewCanHelper.SetCellTemperature(item.QualifiedValue.Value);
                    CommonMethod.Sleep(100);
                    (double, double, double) ts = (0, 0, 0);
                    if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y")
                    {
                        ts = ZCData.DXTemperatures;
                    }
                    else
                    {
                        ts = ZCData.DTemperatures;
                    }

                    if (Math.Abs(ts.Item1 - item.QualifiedValue.Value) > 2 || Math.Abs(ts.Item2 - item.QualifiedValue.Value) > 2 || Math.Abs(ts.Item3 - item.QualifiedValue.Value) > 2)
                    {
                        NewCanHelper.SetCellTemperature(item.QualifiedValue.Value);
                        CommonMethod.Sleep(300);
                        if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y")
                        {
                            ts = ZCData.DXTemperatures;
                        }
                        else
                        {
                            ts = ZCData.DTemperatures;
                        }

                    }
                    //if (item.QualifiedValue > 0)
                    //{
                    //    if (Math.Abs((ts.Item2 - item.QualifiedValue.Value)) <= 5 || Math.Abs((ts.Item2 - item.QualifiedValue.Value)) <= 5 || Math.Abs((ts.Item2 - item.QualifiedValue.Value)) <= 5)
                    //    {
                    //        Random random = new Random();
                    //        double d = 1;
                    //        ts = (item.QualifiedValue.Value - d, item.QualifiedValue.Value - d, item.QualifiedValue.Value - d);
                    //    }
                    //}

                    DateTime endTime = DateTime.Now;
                    var span = (endTime - startTime).TotalSeconds / 3;

                    TestResult model = new TestResult();
                    model.MeasurementStandard = item.MeasurementStandard;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    model.ChildrenItem = "电芯1" + item.ChildrenItem;
                    model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                    model.MeasureVal = ts.Item1.ToString("0.000");
                    model.DifferenceVal = (ts.Item1 - item.QualifiedValue.Value).ToString("0.000");
                    double num = (double)item.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs((ts.Item1 - item.QualifiedValue.Value)) <= num;
                    AddModelAction(model);

                    model = new TestResult();
                    model.MeasurementStandard = item.MeasurementStandard;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    model.ChildrenItem = "电芯2" + item.ChildrenItem;
                    model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                    model.MeasureVal = ts.Item2.ToString("0.000");
                    model.DifferenceVal = (ts.Item2 - item.QualifiedValue.Value).ToString("0.000");
                    num = (double)item.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs((ts.Item2 - item.QualifiedValue.Value)) <= num;
                    AddModelAction(model);


                    model = new TestResult();
                    model.MeasurementStandard = item.MeasurementStandard;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    model.ChildrenItem = "电芯3" + item.ChildrenItem;
                    model.QualifiedValue = item.QualifiedValue.Value.ToString("0.000");
                    model.MeasureVal = ts.Item3.ToString("0.000");
                    model.DifferenceVal = (ts.Item3 - item.QualifiedValue.Value).ToString("0.000");
                    num = (double)item.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs((ts.Item3 - item.QualifiedValue.Value)) <= num;
                    AddModelAction(model);
                }
            }
               
            NewCanHelper.SetCellTemperature(25);
            CommonData.TestCount++;
            return true;
        }

        CellTemperatureEnum getCellTemperatureEnum(double num)
        {
            switch (num)
            {
                case -20:
                    return CellTemperatureEnum.T_20;
                case 45:
                       return CellTemperatureEnum.T_45;
                case 85:
                    return CellTemperatureEnum.T_85;
            }
            return CellTemperatureEnum.T_20;
        }
    }
}
