using AllData;
using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    /// <summary>
    /// 限流
    /// </summary>
    public class CurrentLimiting : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                LogHelper.Debug("------限流模块测试------");
                int testnum = 0;
                int mosflag = 0;


                CommonData.N69206H.SetOutput(false);

                CommonMethod.Sleep(1000);


                CommonData.Relay.SetRelayCard2(1, true);
                CommonData.Relay.SetRelayCard2(24, true);

                CommonData.Relay.SetRelayCard2(4, true);
                CommonData.Relay.SetRelayCard2(25, true);


                // CommonData.IT6322.Selectindex(2);
                CommonMethod.Sleep(200);
                CommonData.IT6322.SetVol(14.5);
                CommonData.IT6322.SetCurr(5);
                CommonMethod.Sleep(20);

                CommonData.N69206H.SetMode("CV");

                CommonData.N69206H.SetCVMode();
                CommonMethod.Sleep(20);
                CommonData.N69206H.SetCVModevolt(8);
                //  CommonData.IT8500.SetCCMode(2);

                CommonData.N69206H.SetOutput(true);
                CommonData.IT6322.SetOutput(true);

                CommonMethod.Sleep(1000);

                CommonData.N8331.SetOutPut4(false);

                CommonMethod.Sleep(1000);
                CommonMethod.Unlock();

                PlayUDSHelper.MOSControl2510(MOSEnum2510.充电放电断);


                //  var volt = CommonData.NXI_6400_2.GetVResult();

             
                CommonMethod.Sleep(2000);

                //double BMSCURR = PlayUDSHelper.Getcur();

                //TestResult model = new TestResult();
                //model.ChildrenItem = "限流电路-BMS电流";

                //model.MeasurementStandard = "1 - 2A";
                //model.MeasureVal = BMSCURR.ToString("0.000");
                //model.QualifiedValue = "";

                //model.Result = BMSCURR >= 1 && BMSCURR <= 2;

                //AddModelAction(model);


                double powerCURR = double.Parse(CommonData.IT6322.SelectCurr());

                TestResult model = new TestResult();
                model.ChildrenItem = "限流电路8V-电源电流";

                model.MeasurementStandard = "< 2.5A";
                model.MeasureVal = powerCURR.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = powerCURR < 2.5;

                AddModelAction(model);

                var FZCUR = CommonData.N69206H.GetMeasureCurrent();
                double FZCURR = double.Parse(FZCUR.Remove(FZCUR.Length - 1));

                model = new TestResult();
                model.ChildrenItem = "限流电路8V-负载电流";

                model.MeasurementStandard = "1.5 - 2.5A";
                model.MeasureVal = FZCURR.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = FZCURR >= 1.5 && FZCURR <= 2.5;

                AddModelAction(model);

                CommonData.N69206H.SetCVModevolt(10);
                CommonMethod.Sleep(1000);
                powerCURR = double.Parse(CommonData.IT6322.SelectCurr());

                model = new TestResult();
                model.ChildrenItem = "限流电路10V-电源电流";

                model.MeasurementStandard = "< 2.5A";
                model.MeasureVal = powerCURR.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = powerCURR < 2.5;

                AddModelAction(model);


                FZCUR = CommonData.N69206H.GetMeasureCurrent();
                FZCURR = double.Parse(FZCUR.Remove(FZCUR.Length - 1));

                model = new TestResult();
                model.ChildrenItem = "限流电路10V-负载电流";

                model.MeasurementStandard = "1.5 - 2.5A";
                model.MeasureVal = FZCURR.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = FZCURR >= 1.5 && FZCURR <= 2.5;

                AddModelAction(model);


                CommonData.N69206H.SetCVModevolt(12);
                CommonMethod.Sleep(1000);
                powerCURR = double.Parse(CommonData.IT6322.SelectCurr());

                model = new TestResult();
                model.ChildrenItem = "限流电路12V-电源电流";

                model.MeasurementStandard = "< 2.5A";
                model.MeasureVal = powerCURR.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = powerCURR < 2.5;

                AddModelAction(model);


                FZCUR = CommonData.N69206H.GetMeasureCurrent();
                FZCURR = double.Parse(FZCUR.Remove(FZCUR.Length - 1));

                model = new TestResult();
                model.ChildrenItem = "限流电路12V-负载电流";

                model.MeasurementStandard = "1.5 - 2.5A";
                model.MeasureVal = FZCURR.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = FZCURR >= 1.5 && FZCURR <= 2.5;

                AddModelAction(model);

                //  PlayUDSHelper.MOSControl(MOSEnum.取消控制);
                CommonData.N8331.SetOutPut4(true);
                CommonData.Relay.SetRelayCard2(1, false);
                CommonData.Relay.SetRelayCard2(24, false);

                CommonData.Relay.SetRelayCard2(4, false);
                CommonData.Relay.SetRelayCard2(25, false);

                CommonData.N69206H.SetOutput(false);

                CommonData.N69206H.SetMode("CC");
                CommonData.IT6322.SetOutput(false);


                CommonData.TestCount++;
                return true;
            }
            else
            {
                LogHelper.Debug("------限流模块测试------");



                int testnum = 0;
                int mosflag = 0;
                NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);
                CommonMethod.SetN8331(13.2);

                CommonData.IT6322.SetVol(13.5);
                TestResult model1 = new TestResult();
                TestResult model2 = new TestResult();
                model1.StartTime = DateTime.Now;
                model2.StartTime = DateTime.Now;
                DateTime start = DateTime.Now;
                var errorCode = PlayUDSHelper.GetBalancedAndAFE();
                LogHelper.Debug($"故障码：{string.Join(" ", errorCode.Select(m => m.ToString("X2")))}");
                //         LogHelper.Debug($"低电压工作状态：{ZCData.WorkStatus}");
                NewCanHelper.WakeOrSleep(true);

                var dl = CommonMethod.GetWorkCurrentFromPower(threshold1: 100);
                if (dl.Item1 != true)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                    throw new Exception("产品未处于工作状态");
                }





                while (true)
                {
                    if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                    {
                        NewCanHelper.WakeOrSleep(false);
                    }

                    CommonMethod.Sleep(500);


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
                if (!PlayUDSHelper.MOSControl(MOSEnum.断开))
                {
                    //LogHelper.MessageBoxShow("MOS 断开失败");
                    AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                    throw new Exception("MOS 断开失败");
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
                NewCanHelper.MOSControlVoltageEnable(true, true, true, gd: false);
                CommonMethod.Sleep(200);
                NewCanHelper.MOSControlVoltageEnable(false, true, true, gd: false);

                CommonMethod.Sleep(500);


                //工装采集内总电压
                var fRetData1 = CommonData.NXI_6400.GetVResult();

                //    var limitstate = ZCData.CurrentLimitingst;

                //   LogHelper.ShowMsg($"限流状态{limitstate}");

                DateTime now = DateTime.Now;



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


                NewCanHelper.MOSControlVoltageEnable(true, true, true, gd: false);

                CommonData.N8331.SetOutPut4(false);
                CommonMethod.Sleep(500);
                var fRetData2 = CommonData.NXI_6400.GetVResult();
                //while (true)
                //    {
                //        if (fRetData2 <= 10)
                //        {
                //            break;
                //        }
                //        if ((DateTime.Now - now).TotalSeconds > 3)
                //        {
                //            break;
                //        }
                //        fRetData2 = CommonData.NXI_6400.GetVResult();
                //    }

                CommonMethod.SetN8331(13.2);
                //工装控制内电压使能、控制外电压不使能
                CommonData.N8331.SetOutPut4(true);
                CommonData.IT6322.SetOutput(false);

                //结束限流模块
                if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
                {

                }
                else
                {
                    //if (PlayUDSHelper.StopCurrentLimiting() == false)
                    //{
                    //    //LogHelper.MessageBoxShow("结束限流失败");
                    //    AddModelAction(new TestResult() { ChildrenItem = "限流模块测试失败" });
                    //    throw new Exception("结束限流失败");
                    //}
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



                AddModelAction(model1);
                AddModelAction(model2);

                CommonData.TestCount++;
                return true;
            }

                
        }
    }
}
