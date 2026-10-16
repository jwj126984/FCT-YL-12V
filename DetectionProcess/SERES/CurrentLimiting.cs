using AllData;
using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// 限流
    /// </summary>
    public class CurrentLimiting : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------限流模块测试------");
            int testnum = 0;
            int mosflag=0;
            NewCanHelper.MOSControlVoltageEnable(true, false,false);
            CommonMethod.SetN8331(13.2);

            CommonData.IT6322.SetVol(13.55);


            TestResult model1 = new TestResult();
            TestResult model2 = new TestResult();
            model1.StartTime = DateTime.Now;
            model2.StartTime = DateTime.Now;
            DateTime start = DateTime.Now;
            var errorCode = PlayUDSHelper.GetBalancedAndAFE();
            LogHelper.Debug($"故障码：{string.Join(" ",errorCode.Select(m=>m.ToString("X2")))}");
   //         LogHelper.Debug($"低电压工作状态：{ZCData.WorkStatus}");
            NewCanHelper.WakeOrSleep(true);
            
            var dl = CommonMethod.GetWorkCurrentFromPower(threshold1: 100);
            if (dl.Item1 != true)
            {
                AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                throw new Exception("产品未处于工作状态");
            }

            while(true)
            {
                
              
              
                while (true)
                {


                    if (!PlayUDSHelper.MOSControl(MOSEnum.断开))
                    {
                        //LogHelper.MessageBoxShow("MOS 断开失败");
                        AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                        throw new Exception("MOS 断开失败");
                    }

                    var fCloseChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                    LogHelper.Debug("充电" + fCloseChargeVoltage.ToString());

                    var fCloseDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
                    LogHelper.Debug("放电" + fCloseDisChargeVoltage.ToString());

                    if (fCloseChargeVoltage < 15 && fCloseDisChargeVoltage < 5)
                    {
                        break;
                    }
                    else if (mosflag == 5)
                    {
                        break;
                    }
                    mosflag++;


                }
                
                //CommonMethod.Unlock();
                //开启限流模块
                if (!PlayUDSHelper.StartCurrentLimiting(true))
                {
                    //LogHelper.MessageBoxShow("开启限流失败");
                    AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                    throw new Exception("开启限流失败");
                }
                LogHelper.Debug($"故障码：{string.Join(" ", errorCode.Select(m => m.ToString("X2")))}");
                //CommonData.pro
                //CommonMethod.Sleep(1000);

                //工装控制内电压不使能
                //   NewCanHelper.MOSControlVoltageEnable(false, true);
                CommonData.IT6322.SetOutput(true);
                NewCanHelper.MOSControlVoltageEnable(true, true, true);
                CommonMethod.Sleep(500);
                NewCanHelper.MOSControlVoltageEnable(false, true, true);
                CommonMethod.Sleep(500);


                //工装采集内总电压
                var fRetData1 = CommonData.NXI_6400.GetVResult();

                DateTime now = DateTime.Now;
                while (true)
                {
                    if (fRetData1 >= 12.8 && fRetData1 < 13.4)
                    {
                        break;
                    }
                    if ((DateTime.Now - now).TotalSeconds > 10)
                    {
                        break;
                    }
                    fRetData1 = CommonData.NXI_6400.GetVResult();
                }


                //var temp = ZCData.NAndWZDY;
                //var uex = ZCData.Uex;
                //var mcu1 = PlayUDSHelper.GetChargeVoltage();
                //var mcu2 = PlayUDSHelper.GetDischargeVoltage();
                ////var mcu=PlayUDSHelper.AwaitStartChannelBalance-
                //var mesV = CommonData.N8331.GetVol4();
                //var sss = CanHelper.ReadExternalTotalVoltage();
                //var it6122bMesV = mesV.Sum();
                //fRetData1 = it6122bMesV;

                //CommonData.IT6322.SetOutput(true);
                //NewCanHelper.MOSControlVoltageEnable(true, false);
                ////MOS断开
                //while (true)
                //{
                //    CommonMethod.Unlock();
                //    if (!PlayUDSHelper.MOSControl(MOSEnum.断开))
                //    {
                //        //LogHelper.MessageBoxShow("MOS 断开失败");
                //        AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                //        throw new Exception("MOS 断开失败");
                //    }

                //    var fCloseChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                //    LogHelper.Debug("充电" + fCloseChargeVoltage.ToString());

                //    var fCloseDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
                //    LogHelper.Debug("放电" + fCloseDisChargeVoltage.ToString());

                //    if (fCloseChargeVoltage < 15 && fCloseDisChargeVoltage < 5)
                //    {
                //        break;
                //    }
                //    else if (mosflag == 3)
                //    {
                //        break;
                //    }
                //    mosflag++;


                //}
                //NewCanHelper.MOSControlVoltageEnable(true, true);
                //关闭限流模块
                if (!PlayUDSHelper.StartCurrentLimiting(false))
                {
                    //LogHelper.MessageBoxShow("关闭限流失败");
                    AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                    throw new Exception("关闭限流失败");
                }
                NewCanHelper.MOSControlVoltageEnable(true, true, true);
                CommonData.N8331.SetOutPut4(false);
                     CommonMethod.Sleep(500);
                var fRetData2 = CommonData.NXI_6400.GetVResult();
                while (true)
                {
                    if (fRetData2 <= 10)
                    {
                        break;
                    }
                    if ((DateTime.Now - now).TotalSeconds > 3)
                    {
                        break;
                    }
                    fRetData2 = CommonData.NXI_6400.GetVResult();
                }
              
                CommonMethod.SetN8331(13.2);
                //工装控制内电压使能、控制外电压不使能
                CommonData.N8331.SetOutPut4(true);
                CommonData.IT6322.SetOutput(false);

                //结束限流模块
                if (PlayUDSHelper.StopCurrentLimiting() == false)
                {
                    //LogHelper.MessageBoxShow("结束限流失败");
                    AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                    throw new Exception("结束限流失败");
                }
                DateTime end = DateTime.Now;
                var span = (end - start).TotalSeconds / 2;
                var par1 = pars.FirstOrDefault(m => m.ChildrenItem.Contains("闭合"));
                if (par1 != null)
                {
                    model1 = new TestResult();
                    model1.StartTime = start;
                    model1.ChildrenItem = par1.ChildrenItem;
                    model1.MeasurementStandard = par1.MeasurementStandard;
                    model1.MeasureVal = fRetData1.ToString("0.000"); ;
                    model1.QualifiedValue = (par1.QualifiedValue).Value.ToString("0.000"); ;
                    double num = (double)par1.MeasurementStandard.GetNumber();
                    model1.DifferenceVal = (fRetData1 - (par1.QualifiedValue).Value).ToString("0.000");
                    model1.Result = Math.Abs(double.Parse(model1.DifferenceVal)) <= num;
                    model1.EndTime = start.AddSeconds(span);
                    
                }

                var par2 = pars.FirstOrDefault(m => m.ChildrenItem.Contains("断开"));
                if (par2 != null)
                {
                    model2 = new TestResult();
                    model2.ChildrenItem = par2.ChildrenItem;
                    model2.MeasurementStandard = par2.MeasurementStandard;
                    model2.MeasureVal = (fRetData2).ToString("0.000");
                    model2.QualifiedValue = par2.QualifiedValue.Value.ToString("0.000");
                    double num = (double)par2.MeasurementStandard.GetNumber();
                    model2.Result = (fRetData2) <= par2.QualifiedValue;
                    model2.StartTime = start.AddSeconds(span);
                    model2.EndTime = DateTime.Now;
                    
                }

                if((model1.Result == true && model2.Result == true) || testnum == 3)
                {
                    testnum = 0;
                    break;

                }
                testnum++;
            }

            AddModelAction(model1);
            AddModelAction(model2);
            CommonData.IT6322.SetVol(13.7);
            CommonData.TestCount++;
            return true;
        }
    }
}
