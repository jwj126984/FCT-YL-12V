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

namespace DetectionProcess.LT
{
    public class ModuleTemperature : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------电芯温度测试------");

            

            if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702"  || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || 
                ConfigHelper.UdsParameters.jiesuo == "T09_M" || ConfigHelper.UdsParameters.jiesuo == "CN_D7" || ConfigHelper.UdsParameters.jiesuo == "QR_C1068" || ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
            {
                DateTime startTime = DateTime.Now;
                (double, double, double) ts1 = (0, 0, 0);
                double pcb = 0;
                if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
                {
                    ts1 = ZCData.DXTemperatures1068;
                }
                else
                {
                    ts1 = ZCData.DTemperatures;
                }
               
                if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702")
                {
                     pcb = PlayUDSHelper.GetPCBTemperature() * 0.5 - 50;
                }
                else if(ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
                {
                    pcb = PlayUDSHelper.GetPCBTemperature1068() - 40 ;
                }
                else
                {
                     pcb = ZCData.PCBTemperature;
                }    
                    
                DateTime endTime = DateTime.Now;
                var span = (endTime - startTime).TotalSeconds / 3;

                TestResult model = new TestResult();
                model.MeasurementStandard = "≤±2℃";
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                model.ChildrenItem = "电芯1温度测试点";
                model.QualifiedValue = $"{pcb}";
                model.MeasureVal = ts1.Item1.ToString("0.000");
                model.DifferenceVal = (ts1.Item1 - pcb).ToString("0.000");
                double num = 2;
                model.Result = Math.Abs((ts1.Item1 - pcb)) <= num;
                AddModelAction(model);

                model = new TestResult();
                model.MeasurementStandard = "≤±2℃";
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                model.ChildrenItem = "电芯2温度测试点";
                model.QualifiedValue = $"{pcb}";
                model.MeasureVal = ts1.Item2.ToString("0.000");
                model.DifferenceVal = (ts1.Item2 - pcb).ToString("0.000");
                num = 2;
                model.Result = Math.Abs((ts1.Item2 - pcb)) <= num;
                AddModelAction(model);

                if (ConfigHelper.UdsParameters.jiesuo != "QR_C1068" )
                {
                    model = new TestResult();
                    model.MeasurementStandard = "≤±2℃";
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    model.ChildrenItem = "电芯3温度测试点";
                    model.QualifiedValue = $"{pcb}";
                    model.MeasureVal = ts1.Item3.ToString("0.000");
                    model.DifferenceVal = (ts1.Item3 - pcb).ToString("0.000");
                    num = 2;
                    model.Result = Math.Abs((ts1.Item3 - pcb)) <= num;
                    AddModelAction(model);
                }
                    
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

                  
                    if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390")
                    {
                        CommonMethod.Sleep(1500);
                    }
                    (double, double, double) ts = (0, 0, 0);
                    if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y")
                    {
                        ts = ZCData.DXTemperatures;
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03" || ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                    {
                        ts = ZCData.DXTemperaturesCC;
                    }
                    
                    else
                    {
                        ts = ZCData.DTemperatures;
                    }

                    //if (Math.Abs(ts.Item1 - item.QualifiedValue.Value) > 2 || Math.Abs(ts.Item2 - item.QualifiedValue.Value) > 2 || Math.Abs(ts.Item3 - item.QualifiedValue.Value) > 2)
                    //{
                    //    NewCanHelper.SetCellTemperature(item.QualifiedValue.Value);
                    //    CommonMethod.Sleep(300);
                    //    if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390")
                    //    {
                    //        CommonMethod.Sleep(1500);
                    //    }

                    //    if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y")
                    //    {
                    //        ts = ZCData.DXTemperatures;
                    //    }
                    //    else if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03" || ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                    //    {
                    //        ts = ZCData.DXTemperaturesCC;
                    //    }
                    //    else
                    //    {
                    //        ts = ZCData.DTemperatures;
                    //    }

                    //}

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

                    if (ConfigHelper.UdsParameters.jiesuo != "RC_PK2C" && ConfigHelper.UdsParameters.jiesuo != "AVATR_E15")
                    {
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
            }
            
           
            CommonData.TestCount++;
            return true;
        }

       
    }
}
