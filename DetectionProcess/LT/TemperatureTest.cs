using AllData;
using Common;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class PCBTemperature : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------温度测试------");

            

            TestResult model = new TestResult();
            var par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("pcb温度".ToLower()));

            DateTime start = DateTime.Now;
           
            double pcb;
            double chargeMOS;
            double disChargeMOS;
            double limit = 0;
           
            if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03"  || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {
                pcb = PlayUDSHelper.GetPCBTemperature()*0.5 - 50 - double.Parse(par.Extend2);
                chargeMOS = PlayUDSHelper.GetMOSCTemperature() * 0.5 - 50 - double.Parse(par.Extend2);
                disChargeMOS = PlayUDSHelper.GetMOSFTemperature() * 0.5 - 50 - double.Parse(par.Extend2);
                limit = PlayUDSHelper.GetXLTemperature() * 0.5 - 50 - double.Parse(par.Extend2);
            }
            else if( ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                pcb = PlayUDSHelper.GetPCBTemperature1068()  - 40 - double.Parse(par.Extend2);
                chargeMOS = PlayUDSHelper.GetMOSCTemperature1068()  - 40 - double.Parse(par.Extend2);
                disChargeMOS = PlayUDSHelper.GetMOSFTemperature1068()  - 40 - double.Parse(par.Extend2);
                limit = PlayUDSHelper.GetXLTemperature1068()  - 40 - double.Parse(par.Extend2);
            }
            else
            {
                pcb = ZCData.PCBTemperature - double.Parse(par.Extend2);
                chargeMOS = ZCData.ChargeMOSTemperature - double.Parse(par.Extend2);
                disChargeMOS = ZCData.DischargeMOSTemperature - double.Parse(par.Extend2);


                if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
                {
                    limit = PlayUDSHelper.GetXLTemperaturec1046()  - 50 - double.Parse(par.Extend2);
                }
                else if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" && ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) != "AVATR"  && ConfigHelper.UdsParameters.jiesuo != "RC_PK2C")
                {
                    limit = ZCData.CurrentLimitingModuleTemperature - double.Parse(par.Extend2);
                }
                

            }
            
            //var hjTemperature = CanHelper.ReadBoardTemperature();//环境温度
            DateTime endTime = DateTime.Now;
            var span = (endTime - start).TotalSeconds / 4;

            
            {
                var qStr = $"[{par.MinVal},{par.MaxVal}]";
                if (par != null)
                {
                    model = new TestResult();
                    model.StartTime = start;
                    model.ChildrenItem = par.ChildrenItem;
                    //model.MeasurementStandard = par.MeasurementStandard;
                    model.MeasurementStandard = par.MeasurementStandard;

                    model.QualifiedValue = $"[{chargeMOS}]";
                    double num = (double)par.MeasurementStandard.GetNumber();
                    model.MeasureVal = pcb.ToString("0.000");

                    //model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
                    //model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                    model.DifferenceVal = (double.Parse(model.MeasureVal) - chargeMOS).ToString("0.000");
                    model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                    model.EndTime = start.AddSeconds(span);
                    AddModelAction(model);
                }
                else
                {
                    //LogHelper.MessageBoxShow("pcb温度参数未配置");
                    AddModelAction(new TestResult() { ChildrenItem = "PCB温度测试失败", Result = false });
                    throw new Exception("pcb温度参数未配置");

                }

                par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("充电MOS".ToLower()));
                if (par != null)
                {
                    model = new TestResult();
                    model.StartTime = start.AddSeconds(span);
                    model.ChildrenItem = par.ChildrenItem;
                    model.MeasurementStandard = par.MeasurementStandard;
                    model.QualifiedValue = pcb.ToString("0.000");
                    double num = (double)par.MeasurementStandard.GetNumber();
                    model.MeasureVal = chargeMOS.ToString("0.000");

                    model.DifferenceVal = (double.Parse(model.MeasureVal) - pcb).ToString("0.000");
                    model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                    model.EndTime = start.AddSeconds(span * 2);
                    AddModelAction(model);
                }
                else
                {
                    //LogHelper.MessageBoxShow("充电MOS温度参数未配置");
                    AddModelAction(new TestResult() { ChildrenItem = "PCB温度测试失败", Result = false });
                    throw new Exception("充电MOS温度参数未配置");
                }

                par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("放电MOS".ToLower()));
                if (par != null)
                {
                    model = new TestResult();
                    model.StartTime = start.AddSeconds(span * 2);
                    model.ChildrenItem = par.ChildrenItem;
                    model.MeasurementStandard = par.MeasurementStandard;
                    model.QualifiedValue = pcb.ToString("0.000");
                    double num = (double)par.MeasurementStandard.GetNumber();
                    model.MeasureVal = disChargeMOS.ToString("0.000");
                    model.DifferenceVal = (double.Parse(model.MeasureVal) - pcb).ToString("0.000");
                    model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                    model.EndTime = start.AddSeconds(span * 3);
                    AddModelAction(model);
                }
                else
                {
                    //LogHelper.MessageBoxShow("放电MOS温度参数未配置");
                    AddModelAction(new TestResult() { ChildrenItem = "PCB温度测试失败", Result = false });
                    throw new Exception("放电MOS温度参数未配置");
                }

                if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" && ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) != "AVATR" &&
                    ConfigHelper.UdsParameters.jiesuo != "RC_PK2C")
                {
                    par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("限流模块".ToLower()));
                    if (par != null)
                    {
                        model = new TestResult();
                        model.StartTime = start.AddSeconds(span * 3);
                        model.ChildrenItem = par.ChildrenItem;
                        model.MeasurementStandard = par.MeasurementStandard;
                        model.QualifiedValue = pcb.ToString("0.000");
                        double num = (double)par.MeasurementStandard.GetNumber();
                        model.MeasureVal = limit.ToString("0.000");

                        model.DifferenceVal = (double.Parse(model.MeasureVal) - pcb).ToString("0.000");
                        model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                        model.EndTime = start.AddSeconds(span * 4);
                        AddModelAction(model);
                    }
                    else
                    {
                        //LogHelper.MessageBoxShow("限流模块温度参数未配置");
                        AddModelAction(new TestResult() { ChildrenItem = "PCB温度测试失败", Result = false });
                        throw new Exception("限流模块温度参数未配置");
                    }
                }
            }
                
            //throw new NotImplementedException();
            CommonData.TestCount++;
            return true;
        }
    }
}
