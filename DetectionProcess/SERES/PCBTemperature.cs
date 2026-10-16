using AllData;
using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    public class PCBTemperature: DetectBase
    {
        

        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------PCB温度测试------");

            TestResult model = new TestResult();
            
            DateTime start = DateTime.Now;
            
            var pcb= ZCData.PCBTemperature -2;
            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y")
            {
                pcb = PlayUDSHelper.GetPCBTemperature()-50-2;
            }
            var chargeMOS= ZCData.ChargeMOSTemperature - 2;
            var disChargeMOS= ZCData.DischargeMOSTemperature - 2;
            double limit = 0;
          
            limit = ZCData.CurrentLimitingModuleTemperature - 2;
            
            
            LogHelper.Debug($"充电MOS温度{chargeMOS}℃，放电MOS温度{disChargeMOS}℃，限流模块温度{limit}℃");
            //var hjTemperature = CanHelper.ReadBoardTemperature();//环境温度
            DateTime endTime= DateTime.Now;
            var span=(endTime - start).TotalSeconds/4;

            var par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("pcb温度".ToLower()));
            var qStr = $"[{par.MinVal},{par.MaxVal}]";
            if (par != null)
            {
                model = new TestResult();
                model.StartTime = start;
                model.ChildrenItem = par.ChildrenItem;
                //model.MeasurementStandard = par.MeasurementStandard;
                model.MeasurementStandard = par.MeasurementStandard;

                model.QualifiedValue = $"[{par.MinVal},{par.MaxVal}]";
                double num = (double)par.MeasurementStandard.GetNumber();
                model.MeasureVal = pcb.ToString("0.000");

                //model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
                //model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                model.Result = pcb >= par.MinVal && pcb <= par.MaxVal;
                if (model.Result==true)
                {
                    model.DifferenceVal = "0.000";
                }
                else
                {
                    model.DifferenceVal = "1.000";
                }
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
                model.QualifiedValue = qStr;
                double num = (double)par.MeasurementStandard.GetNumber();
                model.MeasureVal = chargeMOS.ToString("0.000");

                model.DifferenceVal = (double.Parse(model.MeasureVal) - pcb).ToString("0.000");
                model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                model.EndTime = start.AddSeconds(span*2);
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
                model.StartTime = start.AddSeconds(span*2);
                model.ChildrenItem = par.ChildrenItem;
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = qStr;
                double num = (double)par.MeasurementStandard.GetNumber();
                model.MeasureVal = disChargeMOS.ToString("0.000");
                model.DifferenceVal = (double.Parse(model.MeasureVal) - pcb).ToString("0.000");
                model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                model.EndTime = start.AddSeconds(span*3);
                AddModelAction(model);
            }
            else
            {
                //LogHelper.MessageBoxShow("放电MOS温度参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "PCB温度测试失败", Result = false });
                throw new Exception("放电MOS温度参数未配置");
            }

            
            par = pars.FirstOrDefault(m => m.ChildrenItem.ToLower().Contains("限流模块".ToLower()));
            if (par != null)
            {
                model = new TestResult();
                model.StartTime = start.AddSeconds(span * 3);
                model.ChildrenItem = par.ChildrenItem;
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = qStr;
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
            
            //throw new NotImplementedException();
            CommonData.TestCount++;
            return true;
        }
    }
}
