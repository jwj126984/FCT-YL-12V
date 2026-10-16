using Common.Can;
using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AllData;

namespace DetectionProcess.SERES
{
    public class CurrentCollection : DetectBase
    {
        bool isSetOUTPut = false;
        double vol = double.MinValue;
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------电流采集测试------");

            LogHelper.Debug("M3905D 开启使能");
            if (M3905D.SetOUTPut(1) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败" });
                LogHelper.WriteLog("M3905D 开启使能失败", true);
                throw new Exception("M3905D 开启使能失败");
            }
            isSetOUTPut = true;

            var parsTemp = pars.OrderByDescending(m => m.QualifiedValue);
            foreach (var parameter in parsTemp)
            {
                if (ConfigHelper.UdsParameters.jiesuo == "SERES_F3")
                {
                    if (parameter.QualifiedValue > 0)
                    {
                        //继电器正向
                        NewCanHelper.CtrolTotalCurrentCollection(true);
                    }
                    else if (parameter.QualifiedValue < 0)
                    {
                        //继电器反向
                        NewCanHelper.CtrolTotalCurrentCollection(false);
                    }
                }
                else
                {
                    if (parameter.QualifiedValue > 0)
                    {
                        //继电器反向
                        NewCanHelper.CtrolTotalCurrentCollection(false);
                    }
                    else if (parameter.QualifiedValue < 0)
                    {

                        //继电器正向
                        NewCanHelper.CtrolTotalCurrentCollection(true);
                    }
                }

                
                DateTime start = DateTime.Now;

                //使能判断
                if (parameter.QualifiedValue == 0)
                {
                    parameter.Extend1 = "0";//不使能将电压设置为0
                    if (isSetOUTPut)
                    {
                        LogHelper.Debug("M3905D 设置电流0A关闭使能操作");

                        if (M3905D.SetOUTPut(0) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败", Result = false });

                            LogHelper.WriteLog("M3905D 关闭使能失败", true);
                            throw new Exception("M3905D 关闭使能失败");
                        }
                        else
                        {
                            isSetOUTPut = false;
                        }
                    }
                }
                else
                {
                    if (isSetOUTPut == false)
                    {
                        LogHelper.Debug("M3905D 设置电流非0A开启使能操作");
                        if (M3905D.SetOUTPut(1) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败", Result = false });

                            LogHelper.WriteLog("M3905D 开启使能失败", true);
                            throw new Exception("M3905D 开启使能失败");
                        }
                        else
                        {
                            isSetOUTPut = true;
                        }
                    }
                }


                LogHelper.Debug($"--------------------------设置 M3905D 电压 {parameter.Extend1}V 设置 M3905D 电流 {parameter.QualifiedValue}A");

                //电压设置判断
                if (vol == double.MinValue)
                {
                    vol = double.Parse(parameter.Extend1);
                    M3905D.SetVoltage(vol);
                }
                else
                {
                    if (vol == double.Parse(parameter.Extend1))
                    {

                    }
                    else
                    {
                        M3905D.SetVoltage(double.Parse(parameter.Extend1));
                        vol = double.Parse(parameter.Extend1);
                    }
                }
                double mesC;
                try
                {
                    mesC = M3905D.SetCurrentAndStable(Math.Abs(parameter.QualifiedValue.Value), double.Parse(parameter.Extend1), double.Parse(parameter.Extend2)*1.5 / 1000);
                }
                catch (TimeoutException)
                {
                    mesC = M3905D.SetCurrentAndStable(Math.Abs(parameter.QualifiedValue.Value), double.Parse(parameter.Extend1), double.Parse(parameter.Extend2) * 1.5 / 1000);
                }
                if(Math.Abs(mesC- Math.Abs(parameter.QualifiedValue.Value))> double.Parse(parameter.Extend2) / 1000)
                {
                    try
                    {
                        mesC = M3905D.SetCurrentAndStable(Math.Abs(parameter.QualifiedValue.Value), double.Parse(parameter.Extend1), double.Parse(parameter.Extend2) * 1.5 / 1000);
                    }
                    catch (TimeoutException)
                    {
                        mesC = M3905D.SetCurrentAndStable(Math.Abs(parameter.QualifiedValue.Value), double.Parse(parameter.Extend1), double.Parse(parameter.Extend2) * 1.5 / 1000);
                    }
                }
                 
                if (parameter.QualifiedValue<0)
                {
                    mesC = -mesC;
                }
                DateTime now = DateTime.Now;
                var curr = ZCData.GetDL;

                while(true)
                {
                    if(Math.Abs(mesC - curr) <= (double)parameter.MeasurementStandard.GetNumber())
                    {
                        break;
                    }
                    if ((DateTime.Now - now).TotalSeconds > 5)
                    {
                        break;
                    }
                    CommonMethod.Sleep(20);
                    curr = ZCData.GetDL;

                }
                LogHelper.Debug($"电源采集数据 {mesC}A，整车采集数据 {curr}A");

                TestResult model = new TestResult();
                model.ChildrenItem = parameter.ChildrenItem;
                model.ChildrenItem = $"电流测试点{parameter.QualifiedValue}A";
                model.StartTime = start;
                model.EndTime = DateTime.Now;
                model.QualifiedValue = mesC.ToString("0.000");
                model.MeasureVal = curr.ToString("0.000");
                model.MeasurementStandard = parameter.MeasurementStandard;
                double num = (double)model.MeasurementStandard.GetNumber();
                model.DifferenceVal = (mesC - curr).ToString("0.000");
                model.Result = Math.Abs(mesC - curr) <= num;
                //AddModelAction(model);
                AddModelAction(model);
            }

            LogHelper.Debug("退出电流测试设置 M3905D 电压 0V");
            M3905D.SetVoltageAndStable(0);
            //继电器关闭
            NewCanHelper.CloseTotalCurrentCollection();
            
            LogHelper.Debug("M3905D 关闭使能");
            M3905D.SetOUTPut(0);

            LogHelper.Debug("UDS产品解锁");
            if (CommonMethod.Unlock() == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败" });
                throw new Exception("总电流测试解锁失败");
            }
            ;

            if (PlayUDSHelper.MOSControl(MOSEnum.断开)==false) {

                AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败" });
                throw new Exception("总电流测试MOS断开失败");
            }
            CommonData.TestCount++;
            return true;

            
        }
        

        /// <summary>
        /// UDS读取电流平均值
        /// </summary>
        /// <returns></returns>
        double getProduceCurrent()
        {
            List<double> currents = new List<double>();
            try
            {
                for (int i = 0; i < 5; i++)
                {
                    currents.Add(ZCData.GetDL);
                    CommonMethod.Sleep(20);
                }
              
            }
            catch (Exception) {
                AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败" });
                throw new Exception("总电流测试失败");
            }
            
            
            return currents.Average();
        }
    }
}
