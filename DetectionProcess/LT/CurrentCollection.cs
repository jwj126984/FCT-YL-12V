using Common.Can;
using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllData;

namespace DetectionProcess.LT
{
    public class CurrentCollection : DetectBase
    {
        bool isSetOUTPut = false;
        int testnum = 0;
        double vol = double.MinValue;
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            CommonData.N69206H.SetMode("CC");

            if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                if (CommonMethod.Unlock() == false)
                {
                    LogHelper.Debug($"电流标定解锁失败");
                    TestFail?.Invoke("电流标定解锁失败");
                    return true;
                }

                byte TYPE = PlayUDSHelper.Getcurtype();
                PlayUDSHelper.GETBiaodin();

                var CUR0 = getProduceCurrent1068();
                List<double> currS = new List<double>();//电流数据集合
                List<double> currS1 = new List<double>();
                List<(double, double, double)> CalibrateTempVal = new List<(double, double, double)>();

                CalibrateTempVal.Add((0.5, 5, 50));
                CalibrateTempVal.Add((15, 5, 50));
                CalibrateTempVal.Add((100, 10, 300));
                CalibrateTempVal.Add((300, 10, 500));

                int curtype = 1;
                CommonData.N69206H.SetOutput(false);
                CommonMethod.Sleep(100);
                CommonData.N69206H.SetCCMode();
                CommonMethod.Sleep(100);
                CommonData.N69206H.SetOutput(true);
                int flagfz = 0;

                {
                    curtype = 0;
                    M3905D.SetOUTPut(0);
                    NewCanHelper.CloseTotalCurrentCollection();


                    CommonData.Relay.SetRelayCard2(19, true);
                    CommonData.Relay.SetRelayCard2(21, true);
                    CommonData.Relay.SetRelayCard2(22, true);
                    CommonData.IT6322.SetVol(CalibrateTempVal[0].Item2);
                    CommonMethod.Sleep(200);
                    CommonData.IT6322.SetOutput(true);
                    CommonMethod.Sleep(200);
                    CommonData.IT6322.SetCurrentAndStable(CalibrateTempVal[0].Item1, CalibrateTempVal[0].Item2, CalibrateTempVal[0].Item3 / 1000.0, curtype);
                    CommonMethod.Sleep(1000);
                    var curall = getProduceCurrent1068();
                    LogHelper.ShowMsg($"电流{curall}");

                    currS.Add(curall);


                }

                CommonData.Relay.SetRelayCard2(21, false);
                CommonData.Relay.SetRelayCard2(22, false);


                CommonData.Relay.SetRelayCard2(20, true);
                CommonData.Relay.SetRelayCard2(23, true);


                {
                    curtype = 0;

                    NewCanHelper.CloseTotalCurrentCollection();



                    CommonData.IT6322.SetVol(CalibrateTempVal[0].Item2);
                    CommonMethod.Sleep(200);
                    CommonData.IT6322.SetOutput(true);
                    CommonMethod.Sleep(200);
                    CommonData.IT6322.SetCurrentAndStable(CalibrateTempVal[0].Item1, CalibrateTempVal[0].Item2, CalibrateTempVal[0].Item3 / 1000.0, curtype);
                    CommonMethod.Sleep(1000);
                    var curall = getProduceCurrent1068();
                    LogHelper.ShowMsg($"电流{curall}");


                    currS1.Add(curall);


                }
                CommonData.Relay.SetRelayCard2(20, false);
                CommonData.Relay.SetRelayCard2(23, false);
                CommonData.Relay.SetRelayCard2(19, false);
                CommonData.Relay.CloseAllRelayCard2();

               
                if (M3905D.SetOUTPut(1) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    LogHelper.WriteLog("M3905D 开启使能失败", true);
                    throw new Exception("M3905D 开启使能失败");
                }

                for (int t = 1; t <= 3; t++)
                {

                    {
                        if (Math.Abs(CalibrateTempVal[t].Item1) >= 100 && flagfz == 0)
                        {

                            CommonData.N69206H.SetOutput(false);
                            CommonMethod.Sleep(150);
                            CommonData.N69206H.SetCCModeh();
                            //CommonMethod.Sleep(100);
                            CommonData.N69206H.SetOutput(true);
                            CommonMethod.Sleep(100);
                            flagfz = 1;
                        }
                        curtype = 1;
                        NewCanHelper.CtrolTotalCurrentCollection(true);
                        M3905D.SetOUTPut(1);
                        M3905D.SetVoltage(CalibrateTempVal[t].Item2);
                        CommonData.M3905D.SetCurrentAndStable(CalibrateTempVal[t].Item1, CalibrateTempVal[t].Item2, CalibrateTempVal[t].Item3 / 1000.0);

                        var curall = getProduceCurrent1068();
                        LogHelper.ShowMsg($"电流{curall}");
                        CommonMethod.Sleep(1000);
                        M3905D.SetOUTPut(0);
                        currS.Add(curall);
                    }


                    {

                        if (Math.Abs(CalibrateTempVal[t].Item1) >= 100 && flagfz == 0)
                        {

                            CommonData.N69206H.SetOutput(false);
                            CommonMethod.Sleep(150);
                            CommonData.N69206H.SetCCModeh();
                            //CommonMethod.Sleep(100);
                            CommonData.N69206H.SetOutput(true);
                            CommonMethod.Sleep(100);
                            flagfz = 1;
                        }
                        curtype = 1;
                        NewCanHelper.CtrolTotalCurrentCollection(false);
                        M3905D.SetOUTPut(1);
                        M3905D.SetVoltage(CalibrateTempVal[t].Item2);
                        CommonData.M3905D.SetCurrentAndStable(CalibrateTempVal[t].Item1, CalibrateTempVal[t].Item2, CalibrateTempVal[t].Item3 / 1000.0);

                        var curall = getProduceCurrent1068();
                        LogHelper.ShowMsg($"电流{curall}");
                        CommonMethod.Sleep(1000);
                        M3905D.SetOUTPut(0);
                        currS1.Add(curall);
                    }
                }

                M3905D.SetOUTPut(0);
    






                NewCanHelper.CloseTotalCurrentCollection();

                M3905D.SetVoltageAndStable(0);

                M3905D.SetOUTPut(0);
                CommonData.IT6322.SetOutput(false);



                var model1 = new TestResult();
                model1.QualifiedValue = $"0";
                model1.ChildrenItem = $"测试电流0A";
                model1.MeasureVal = (CUR0).ToString("0.000");

                model1.DifferenceVal = "0";

                model1.MeasurementStandard = $" <±0.015";
                model1.Result = CUR0 < 0.015 && CUR0 > -0.015;



                AddModelAction(model1);


                for (int i = 0; i < 4; i++)
                {
                    var model = new TestResult();
                    model.QualifiedValue = $"-{CalibrateTempVal[i].Item1}";
                    model.ChildrenItem = $"测试电流-{CalibrateTempVal[i].Item1}A";
                    model.MeasureVal = (currS[i]).ToString("0.000");

                    model.DifferenceVal = Math.Abs(-CalibrateTempVal[i].Item1 - currS[i]).ToString("0.000");
                    if (Math.Abs(CalibrateTempVal[i].Item1) > 100)
                    {
                        model.MeasurementStandard = $" <± {CalibrateTempVal[i].Item1 * 0.015}";
                        model.Result = double.Parse(model.DifferenceVal) < (CalibrateTempVal[i].Item1 * 0.015);
                    }
                    else
                    {
                        model.MeasurementStandard = $" <± {(CalibrateTempVal[i].Item1 * 0.01 + 0.015)}";
                        model.Result = double.Parse(model.DifferenceVal) < (CalibrateTempVal[i].Item1 * 0.01 + 0.015);
                    }


                    AddModelAction(model);
                }

                for (int i = 0; i < 4; i++)
                {
                    var model = new TestResult();
                    model.QualifiedValue = $"{CalibrateTempVal[i].Item1}";
                    model.ChildrenItem = $"测试电流{CalibrateTempVal[i].Item1}A";
                    model.MeasureVal = (currS1[i]).ToString("0.000");

                    model.DifferenceVal = Math.Abs(CalibrateTempVal[i].Item1 - currS1[i]).ToString("0.000");
                    if (Math.Abs(CalibrateTempVal[i].Item1) > 100)
                    {
                        model.MeasurementStandard = $" <± {CalibrateTempVal[i].Item1 * 0.015}";
                        model.Result = double.Parse(model.DifferenceVal) < (CalibrateTempVal[i].Item1 * 0.015);
                    }
                    else
                    {
                        model.MeasurementStandard = $" <± {(CalibrateTempVal[i].Item1 * 0.01 + 0.015)}";
                        model.Result = double.Parse(model.DifferenceVal) < (CalibrateTempVal[i].Item1 * 0.01 + 0.015);
                    }

                    AddModelAction(model);

                }

                CommonData.TestCount++;
                return true;
            }
            else
            {
                LogHelper.Debug("------电流采集测试------");



                TestResult model;
                LogHelper.Debug("M3905D 开启使能");
                if (M3905D.SetOUTPut(1) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败" });
                    LogHelper.WriteLog("M3905D 开启使能失败", true);
                    throw new Exception("M3905D 开启使能失败");
                }
                isSetOUTPut = true;
                CommonData.N69206H.SetOutput(true);
                var parsTemp = pars.OrderByDescending(m => m.QualifiedValue);

                int flagfz = 0;
                CommonData.N69206H.SetCCMode();
                foreach (var parameter in parsTemp)
                {
                    DateTime start = DateTime.Now;
                    if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" && parameter.QualifiedValue.Value == 1000)
                    {
                        continue;
                    }


                    //if (Math.Abs(parameter.QualifiedValue.Value) > 30)
                    //    continue;



                    if (ConfigHelper.UdsParameters.jiesuo == "RC_PK1B" || ConfigHelper.UdsParameters.jiesuo == "M18_3" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" ||
                        ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" ||
                        ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" ||
                        ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "T09_M")
                    {
                        if (parameter.QualifiedValue >= 0)
                        {
                            //继电器正向
                            NewCanHelper.CtrolTotalCurrentCollection(false);
                        }
                        else
                        {

                            //继电器反向
                            NewCanHelper.CtrolTotalCurrentCollection(true);
                        }
                    }
                    else
                    {
                        if (parameter.QualifiedValue >= 0)
                        {
                            //继电器正向
                            NewCanHelper.CtrolTotalCurrentCollection(true);
                        }
                        else
                        {
                            //继电器反向
                            NewCanHelper.CtrolTotalCurrentCollection(false);
                        }
                    }



                    if (Math.Abs(parameter.QualifiedValue.Value) >= 50 && flagfz == 0)
                    {
                        CommonData.N69206H.SetOutput(false);
                        CommonMethod.Sleep(300);
                        CommonData.N69206H.SetCCModeh();
                        CommonData.N69206H.SetOutput(true);
                        CommonMethod.Sleep(100);
                        flagfz = 1;
                    }
                    //else if () { }50-300属于CCM
                    else if (Math.Abs(parameter.QualifiedValue.Value) < 50 && flagfz == 1)
                    {
                        CommonData.N69206H.SetOutput(false);
                        CommonMethod.Sleep(150);
                        CommonData.N69206H.SetCCMode();
                        CommonData.N69206H.SetOutput(true);
                        CommonMethod.Sleep(100);
                        flagfz = 0;
                    }


                    //使能判断
                    if (parameter.QualifiedValue == 0)
                    {

                        //NewCanHelper.CloseTotalCurrentCollection();
                        //CommonData.N69206H.SetOutput(false);
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

                    mesC = M3905D.SetCurrentAndStable(Math.Abs(parameter.QualifiedValue.Value), double.Parse(parameter.Extend1), double.Parse(parameter.Extend2) / 1000);

                    if (Math.Abs(mesC - Math.Abs(parameter.QualifiedValue.Value)) > double.Parse(parameter.Extend2) / 1000)
                    {

                        mesC = M3905D.SetCurrentAndStable(Math.Abs(parameter.QualifiedValue.Value), double.Parse(parameter.Extend1), double.Parse(parameter.Extend2) / 1000);

                    }
                    if (parameter.QualifiedValue == 0 )
                    {
                        mesC = 0;
                    }
                    if (parameter.QualifiedValue < 0)
                    {
                        mesC = -mesC;
                    }
                    //   var curr = getProduceCurrent();
                    var curr = ZCData.GetDL;
                    if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
                    {
                        curr = -curr;
                    }
                    var now = DateTime.Now;
                    while (true)
                    {
                        if (Math.Abs(mesC - curr) <= (double)parameter.MeasurementStandard.GetNumber())
                        {
                            break;
                        }
                        else if ((DateTime.Now - now).TotalSeconds > 5)
                        {
                            break;
                        }
                        curr = ZCData.GetDL;
                        if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
                        {
                            curr = -curr;
                        }
                    }



                    LogHelper.Debug($"电源采集数据 {mesC}A，整车采集数据 {curr}A");

                    model = new TestResult();

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

                CommonMethod.Sleep(100);
                CommonData.N69206H.SetOutput(false);
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

                if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
                {

                    AddModelAction(new TestResult() { ChildrenItem = "总电流测试失败" });
                    throw new Exception("总电流测试MOS断开失败");
                }


                CommonData.TestCount++;
                return true;
            }
            


        }


        /// <summary>
        /// UDS读取电流平均值
        /// </summary>
        /// <returns></returns>
        double getProduceCurrent()
        {
            List<double> currents = new List<double>();
            for (int i = 0; i < 10; i++)
            {
                currents.Add(ZCData.GetDL);
                CommonMethod.Sleep(20);
            }
            currents.Remove(currents.Max());
            currents.Remove(currents.Min());
            return currents.Average();
        }


        double getProduceCurrent1068()
        {
            List<double> currents = new List<double>();
            try
            {
                for (int i = 0; i < 15; i++)
                {

                    var Currlin = PlayUDSHelper.Getcur1068();
                    currents.Add(Currlin);
                    LogHelper.Debug($"产品电流{Currlin}");
                    CommonMethod.Sleep(12);
                }


            }
            catch (Exception)
            {
                AddModelAction(new TestResult() { ChildrenItem = "电流标定失败" });
                throw new Exception("电流标定失败");
            }
            //if(currents.Average() > 0)
            //{
            //    return currents.Max();
            //}
            //else
            //{
            //    return currents.Min();
            //}
            return currents.Average();
        }

    }
}
