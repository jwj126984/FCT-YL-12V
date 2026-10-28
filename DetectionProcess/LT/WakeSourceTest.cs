using Common.Can;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common.UDS;
using Common.SerialportHelper;
using Model;
using AllData;

namespace DetectionProcess.LT
{
    public class WakeSourceTest : DetectBase
    {
        //public event Action<TestResult> AddModelAction;
        TestParameter par;
        double currentVot;
        bool isTestSleepCurr = false;
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                //判断是否存在休眠电流流程
                var tempPar = CommonData.CurrentUserDetects.Select(m => m.Item1).FirstOrDefault(m => m == "SleepingCurrent");
                isTestSleepCurr = !string.IsNullOrEmpty(tempPar);

                NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);
                LogHelper.Debug("------唤醒源测试------");
                par = pars.FirstOrDefault();
                if (par == null)
                {
                    //LogHelper.MessageBoxShow("未配置唤醒源参数");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试" });
                    throw new Exception($"未配置唤醒源参数");
                }
                currentVot = double.Parse(CommonData.N8331.GetVol4().Sum().ToString("0.0"));
                //    CommonData.IT6122B.SetVoltageAndStable(12.8);
                //CommonData.N8331.SetVol4(12 / 4);

                CommonMethod.SetN8331(12.8);



                bool isSuccess = true;
                bool b1 = true;
                TestResult model;


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "Can唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_CAN;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_CAN;
                b1 = CanHX1068();


                CommonMethod.StartSendAwaken();

                //读取唤醒源
                GetWakeSource1068(model, 5);
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.ShowMsg("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();




                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);




                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "硬件唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_YJ;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_YJ;
                b1 = YingJian1068();
                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }

                CommonMethod.StartSendAwaken();
                GetWakeSource1068(model, 3);


                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                CommonMethod.StartSendAwaken();

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "帮电唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_GY;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_GY;

                b1 = GuoYa1068();


                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                CommonMethod.StartSendAwaken();
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源
                GetWakeSource1068(model, 2);

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);




                //CommonMethod.Unlock();
                //PlayUDSHelper.MOSControl(MOSEnum.全断);//MOS断开





                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "RTC唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_RTC;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_RTC;
                b1 = RTC1068();
                if (b1 == false)
                {
                    isSuccess = false;
                }
                else
                {
                    //model.Result = true;
                }
                CommonMethod.StartSendAwaken();
                GetWakeSource1068(model, 1);
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.ShowMsg("工装唤醒失败");
                }
                CommonMethod.Sleep(100);
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "电流唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_DL;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_DL;
                b1 = DianLiu1068();
                if (b1 == false)
                {
                    isSuccess = false;
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }
                CommonMethod.StartSendAwaken();

                GetWakeSource1068(model, 4);
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }

                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);

                //CommonData.IT6122B.SetVoltageAndStable(currentVot);
                CommonMethod.SetN8331(12.8);


                foreach (var item in sleepCurrResult.OrderByDescending(m => m.ChildrenItem))
                {
                    AddModelAction(item);
                }
                NewCanHelper.MOSControlVoltageEnable(true, false);
                CommonData.TestCount++;
                return isSuccess;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {
                //判断是否存在休眠电流流程
                var tempPar = CommonData.CurrentUserDetects.Select(m => m.Item1).FirstOrDefault(m => m == "SleepingCurrent");
                isTestSleepCurr = !string.IsNullOrEmpty(tempPar);

                NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);

                LogHelper.Debug("------唤醒源测试------");
                par = pars.FirstOrDefault();
                if (par == null)
                {
                    //LogHelper.MessageBoxShow("未配置唤醒源参数");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试" });
                    throw new Exception($"未配置唤醒源参数");
                }
                currentVot = double.Parse(CommonData.N8331.GetVol4().Sum().ToString("0.0"));

                CommonMethod.SetN8331(13.6);



                CommonData.IT6322.SetVol(13.2);

                CommonMethod.Unlock();
                PlayUDSHelper.SetSOC(100);

                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {


                    CommonMethod.Unlock();
                    CommonMethod.Sleep(200);
                    PlayUDSHelper.GoToGC();
                    CommonMethod.Sleep(200);
                    PlayUDSHelper.SetSOC(98);
                }


                List<(double, double, double)> CalibrateTempVal = new List<(double, double, double)>();
                CalibrateTempVal.Add((0, 0, 5));
                CalibrateTempVal.Add((10, 5, 20));
                CalibrateTempVal.Add((20, 5, 30));
                CalibrateTempVal.Add((30, 5, 30));
                CalibrateTempVal.Add((50, 5, 50));
                CalibrateTempVal.Add((100, 10, 50));
                CalibrateTempVal.Add((120, 10, 50));
                CalibrateTempVal.Add((150, 10, 50));

                bool isSuccess = true;
                bool b1 = true;
                TestResult model;

                if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" && ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) != "AVATR" &&
                    ConfigHelper.UdsParameters.jiesuo != "RC_PK2C" && ConfigHelper.UdsParameters.jiesuo != "WL_F710S" && ConfigHelper.UdsParameters.jiesuo != "T09_M" && ConfigHelper.UdsParameters.jiesuo != "WL_C1046")
                {
                    model = new TestResult();
                    model.StartTime = DateTime.Now;
                    model.ChildrenItem = "RTC唤醒";
                    model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_RTC;
                    model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_RTC;
                    b1 = RTC();
                    if (b1 == false)
                    {
                        isSuccess = false;
                    }
                    else
                    {
                        //model.Result = true;
                    }
                    CommonMethod.StartSendAwaken();
                    GetWakeSource(model, 1);
                    if (NewCanHelper.WakeOrSleep(true) == false)
                    {
                        LogHelper.ShowMsg("工装唤醒失败");
                    }
                    CommonMethod.Unlock();
                    CommonMethod.StartSendUnlock();
                    //读取唤醒源

                    model.EndTime = DateTime.Now;
                    if (model.Result == true)
                    {
                        model.DifferenceVal = "0.000";
                    }
                    else if (model.Result == false)
                    {
                        model.DifferenceVal = "1.000";
                    }
                    AddModelAction?.Invoke(model);

                }





                if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int t = 0; t < 7; t++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                model = new TestResult();

                                model.StartTime = DateTime.Now;


                                if (i == 0)
                                {
                                    if (j == 0)
                                    {
                                        model.MeasurementStandard = "[0.92,1.09]";
                                        model.ChildrenItem = $"回读正向[{CalibrateTempVal[t].Item1}-{CalibrateTempVal[t + 1].Item1}]K";
                                        GetK2(model, i * 14 + t * 2 + j);
                                    }
                                    else
                                    {
                                        model.MeasurementStandard = "[-1.2,1.2]";
                                        model.ChildrenItem = $"回读正向[{CalibrateTempVal[t].Item1} - {CalibrateTempVal[t + 1].Item1}]B";
                                        GetB(model, i * 14 + t * 2 + j);
                                    }

                                }
                                else
                                {
                                    if (j == 0)
                                    {
                                        model.MeasurementStandard = "[0.92,1.09]";
                                        model.ChildrenItem = $"回读反向[{CalibrateTempVal[t].Item1}-{CalibrateTempVal[t + 1].Item1}]K";
                                        GetK2(model, i * 14 + t * 2 + j);
                                    }
                                    else
                                    {
                                        model.MeasurementStandard = "[-1.2,1.2]";
                                        model.ChildrenItem = $"回读反向[{CalibrateTempVal[t].Item1} - {CalibrateTempVal[t + 1].Item1}]B";
                                        GetB(model, i * 14 + t * 2 + j);
                                    }
                                }

                                model.EndTime = DateTime.Now;

                                AddModelAction?.Invoke(model);
                            }

                        }
                    }

                }

                else if (ConfigHelper.UdsParameters.jiesuo != "VOYAH_H47A" && ConfigHelper.UdsParameters.jiesuo != "WL_C1046" && ConfigHelper.UdsParameters.jiesuo != "VOYAH_H97" &&
                    ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" &&
                    ConfigHelper.UdsParameters.jiesuo != "HQ_E702" && ConfigHelper.UdsParameters.jiesuo != "HQ_C1079" &&
                    ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) != "AVATR" && ConfigHelper.UdsParameters.jiesuo != "RC_PK2C" &&
                    ConfigHelper.UdsParameters.jiesuo != "WL_F710S" && ConfigHelper.UdsParameters.jiesuo != "T09_M" && ConfigHelper.UdsParameters.jiesuo != "DF_DH1" && ConfigHelper.UdsParameters.jiesuo != "DF_XY-A")
                {
                    model = new TestResult();

                    model.StartTime = DateTime.Now;
                    model.ChildrenItem = "电流标定K值读取";
                    model.MeasurementStandard = "0000 - FFFE";

                    GetK(model);

                    model.EndTime = DateTime.Now;

                    AddModelAction?.Invoke(model);
                }




               


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "Can唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_CAN;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_CAN;
                b1 = CanHX();
                if (b1 == false)
                {
                    isSuccess = false;
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }

                CommonMethod.StartSendAwaken();

                if (ConfigHelper.UdsParameters.jiesuo != "WL_F710S")
                {
                    GetWakeSource(model, 5);
                }
                //读取唤醒源

                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.ShowMsg("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();




                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);

                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "电流唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_DL;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_DL;
                b1 = DianLiu();
                if (b1 == false)
                {
                    isSuccess = false;
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }
                CommonMethod.StartSendAwaken();

                if (ConfigHelper.UdsParameters.jiesuo != "WL_F710S")
                {
                    GetWakeSource(model, 4);

                }
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }

                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "不可以帮电唤醒测试";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_CAN;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_CAN;

                CommonMethod.SetN8331(12.8);

                b1 = GuoYa(2);


                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                CommonMethod.StartSendAwaken();
                CommonMethod.Sleep(1000);
                GetWakeSource(model, 2);
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源


                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "硬件唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_YJ;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_YJ;
                b1 = YingJian();
                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }

                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {
                    NewCanHelper.WakeOrSleep(false);
                    GetWakeSource(model, 3);
                    CommonMethod.StartSendAwaken();
                }
                else
                {
                    CommonMethod.StartSendAwaken();


                    GetWakeSource(model, 3);
                }



                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                CommonMethod.StartSendAwaken();

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "可以帮电唤醒测试";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_GY;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_GY;

                b1 = GuoYa();


                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                CommonMethod.StartSendAwaken();
                CommonMethod.Sleep(50);
                GetWakeSource(model, 2);
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源


                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);

                CommonMethod.Unlock();
                PlayUDSHelper.MOSControl(MOSEnum.断开);//MOS断开




                //CommonData.IT6122B.SetVoltageAndStable(currentVot);
                CommonMethod.SetN8331(12.8);
                PlayUDSHelper.SetSOC(80);

                foreach (var item in sleepCurrResult.OrderByDescending(m => m.ChildrenItem))
                {
                    AddModelAction(item);
                }
                NewCanHelper.MOSControlVoltageEnable(true, false);
                CommonData.TestCount++;
                return isSuccess;
            }
            else
            {
                //判断是否存在休眠电流 流程
                var tempPar = CommonData.CurrentUserDetects.Select(m => m.Item1).FirstOrDefault(m => m == "SleepingCurrent");
                isTestSleepCurr = !string.IsNullOrEmpty(tempPar);

                NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);

                LogHelper.Debug("------唤醒源测试------");
                par = pars.FirstOrDefault();
                if (par == null)
                {
                    //LogHelper.MessageBoxShow("未配置唤醒源参数");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试" });
                    throw new Exception($"未配置唤醒源参数");
                }
                currentVot = double.Parse(CommonData.N8331.GetVol4().Sum().ToString("0.0"));

                CommonMethod.SetN8331(12);



                CommonData.IT6322.SetVol(13.2);

                CommonMethod.Unlock();
                PlayUDSHelper.SetSOC(100);

                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {


                    CommonMethod.Unlock();
                    CommonMethod.Sleep(200);
                    PlayUDSHelper.GoToGC();
                    CommonMethod.Sleep(200);
                    PlayUDSHelper.SetSOC(98);
                }


                List<(double, double, double)> CalibrateTempVal = new List<(double, double, double)>();
                CalibrateTempVal.Add((0, 0, 5));
                CalibrateTempVal.Add((10, 5, 20));
                CalibrateTempVal.Add((20, 5, 30));
                CalibrateTempVal.Add((30, 5, 30));
                CalibrateTempVal.Add((50, 5, 50));
                CalibrateTempVal.Add((100, 10, 50));
                CalibrateTempVal.Add((120, 10, 50));
                CalibrateTempVal.Add((150, 10, 50));

                bool isSuccess = true;
                bool b1 = true;
                TestResult model;

                if (ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" && ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) != "AVATR" &&
                    ConfigHelper.UdsParameters.jiesuo != "RC_PK2C" && ConfigHelper.UdsParameters.jiesuo != "WL_F710S" && ConfigHelper.UdsParameters.jiesuo != "T09_M" && ConfigHelper.UdsParameters.jiesuo != "WL_C1046")
                {
                    model = new TestResult();
                    model.StartTime = DateTime.Now;
                    model.ChildrenItem = "RTC唤醒";
                    model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_RTC;
                    model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_RTC;
                    b1 = RTC();
                    if (b1 == false)
                    {
                        isSuccess = false;
                    }
                    else
                    {
                        //model.Result = true;
                    }
                    CommonMethod.StartSendAwaken();
                    GetWakeSource(model, 1);
                    if (NewCanHelper.WakeOrSleep(true) == false)
                    {
                        LogHelper.ShowMsg("工装唤醒失败");
                    }
                    CommonMethod.Unlock();
                    CommonMethod.StartSendUnlock();
                    //读取唤醒源

                    model.EndTime = DateTime.Now;
                    if (model.Result == true)
                    {
                        model.DifferenceVal = "0.000";
                    }
                    else if (model.Result == false)
                    {
                        model.DifferenceVal = "1.000";
                    }
                    AddModelAction?.Invoke(model);

                }





                if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                {
                    for (int i = 0; i < 2; i++)
                    {
                        for (int t = 0; t < 7; t++)
                        {
                            for (int j = 0; j < 2; j++)
                            {
                                model = new TestResult();

                                model.StartTime = DateTime.Now;


                                if (i == 0)
                                {
                                    if (j == 0)
                                    {
                                        model.MeasurementStandard = "[0.92,1.09]";
                                        model.ChildrenItem = $"回读正向[{CalibrateTempVal[t].Item1}-{CalibrateTempVal[t + 1].Item1}]K";
                                        GetK2(model, i * 14 + t * 2 + j);
                                    }
                                    else
                                    {
                                        model.MeasurementStandard = "[-1.2,1.2]";
                                        model.ChildrenItem = $"回读正向[{CalibrateTempVal[t].Item1} - {CalibrateTempVal[t + 1].Item1}]B";
                                        GetB(model, i * 14 + t * 2 + j);
                                    }

                                }
                                else
                                {
                                    if (j == 0)
                                    {
                                        model.MeasurementStandard = "[0.92,1.09]";
                                        model.ChildrenItem = $"回读反向[{CalibrateTempVal[t].Item1}-{CalibrateTempVal[t + 1].Item1}]K";
                                        GetK2(model, i * 14 + t * 2 + j);
                                    }
                                    else
                                    {
                                        model.MeasurementStandard = "[-1.2,1.2]";
                                        model.ChildrenItem = $"回读反向[{CalibrateTempVal[t].Item1} - {CalibrateTempVal[t + 1].Item1}]B";
                                        GetB(model, i * 14 + t * 2 + j);
                                    }
                                }

                                model.EndTime = DateTime.Now;

                                AddModelAction?.Invoke(model);
                            }

                        }
                    }

                }

                else if (ConfigHelper.UdsParameters.jiesuo != "VOYAH_H47A" && ConfigHelper.UdsParameters.jiesuo != "WL_C1046" && ConfigHelper.UdsParameters.jiesuo != "VOYAH_H97" &&
                    ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" &&
                    ConfigHelper.UdsParameters.jiesuo != "HQ_E702" && ConfigHelper.UdsParameters.jiesuo != "HQ_C1079" &&
                    ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) != "AVATR" && ConfigHelper.UdsParameters.jiesuo != "RC_PK2C" &&
                    ConfigHelper.UdsParameters.jiesuo != "WL_F710S" && ConfigHelper.UdsParameters.jiesuo != "CN_D7" && ConfigHelper.UdsParameters.jiesuo != "T09_M" && ConfigHelper.UdsParameters.jiesuo != "DF_DH1" && ConfigHelper.UdsParameters.jiesuo != "DF_XY-A")
                {
                    model = new TestResult();

                    model.StartTime = DateTime.Now;
                    model.ChildrenItem = "电流标定K值读取";
                    model.MeasurementStandard = "0000 - FFFE";

                    GetK(model);

                    model.EndTime = DateTime.Now;

                    AddModelAction?.Invoke(model);
                }




                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "硬件唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_YJ;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_YJ;
                b1 = YingJian();
                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }

                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {
                    NewCanHelper.WakeOrSleep(false);
                    GetWakeSource(model, 3);
                    CommonMethod.StartSendAwaken();
                }
                else
                {
                    CommonMethod.StartSendAwaken();


                    GetWakeSource(model, 3);
                }



                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                CommonMethod.StartSendAwaken();

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);


                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "Can唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_CAN;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_CAN;
                b1 = CanHX();
                if (b1 == false)
                {
                    isSuccess = false;
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }

                CommonMethod.StartSendAwaken();

                if (ConfigHelper.UdsParameters.jiesuo != "WL_F710S")
                {
                    CommonMethod.Sleep(200);
                    GetWakeSource(model, 5);
                }
                //读取唤醒源

                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.ShowMsg("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();




                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);

                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "电流唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_DL;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_DL;
                b1 = DianLiu();
                if (b1 == false)
                {
                    isSuccess = false;
                    model.Result = false;
                }
                else
                {
                    model.Result = true;
                }
                CommonMethod.StartSendAwaken();

                if (ConfigHelper.UdsParameters.jiesuo != "WL_F710S")
                {
                    GetWakeSource(model, 4);

                }
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }

                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源

                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);

                if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
                {
                    PlayUDSHelper.ClearError();
                }
                model = new TestResult();
                model.StartTime = DateTime.Now;
                model.ChildrenItem = "帮电唤醒";
                model.QualifiedValue = ConfigHelper.UdsParameters.GetHX_GY;
                model.MeasurementStandard = ConfigHelper.UdsParameters.GetHX_GY;

                b1 = GuoYa();


                if (b1 == false)
                {
                    isSuccess = false;
                    //model.Result = false;
                }
                else
                {
                    //model.Result = true;
                }
                CommonMethod.StartSendAwaken();

                GetWakeSource(model, 2);
                if (NewCanHelper.WakeOrSleep(true) == false)
                {
                    LogHelper.MessageBoxShow("工装唤醒失败");
                }
                CommonMethod.Unlock();
                CommonMethod.StartSendUnlock();
                //读取唤醒源


                model.EndTime = DateTime.Now;
                if (model.Result == true)
                {
                    model.DifferenceVal = "0.000";
                }
                else if (model.Result == false)
                {
                    model.DifferenceVal = "1.000";
                }
                AddModelAction?.Invoke(model);

                CommonMethod.Unlock();
                PlayUDSHelper.MOSControl(MOSEnum.断开);//MOS断开




                //CommonData.IT6122B.SetVoltageAndStable(currentVot);
                CommonMethod.SetN8331(12.8);
                PlayUDSHelper.SetSOC(80);

                foreach (var item in sleepCurrResult.OrderByDescending(m => m.ChildrenItem))
                {
                    AddModelAction(item);
                }
                NewCanHelper.MOSControlVoltageEnable(true, false);
                CommonData.TestCount++;
                return isSuccess;
            }

            

        }

        bool XiuMian1068(int level, bool isMOS = false)
        {


            //第一步休眠
            PlayUDSHelper.Reset();
            CommonMethod.Sleep(1000);


            CommonMethod.Unlock();
            PlayUDSHelper.MOSControl2510(MOSEnum2510.充电放电断);


            //Class1.HuanXing(false);
            //发送休眠指令
            if (NewCanHelper.WakeOrSleep(false) == false)
            {
                LogHelper.ShowMsg("休眠失败");
                return false;
            }
            if (level == 0)
            {
                CommonMethod.Sleep(3000);
            }
            CommonMethod.StopSendAwaken();
            CommonMethod.StopSendUnlock();
            CommonMethod.Sleep(100);

            CommonMethod.Unlock();
            LogHelper.Debug("休眠产品");
            List<byte> data = new List<byte>();
            if (level == 0)
            {

                data = new List<byte> { 0x01, 0x74, 0x13, 0x1a };
            }

            else if (level == 1)
            {

                data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.XM1_DID);
            }
            else if (level == 2)
            {
                data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.XM2_DID);
            }
            //uds发送休眠
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x31);
            //if (level == 0)
            //{
            //    temp = PlayUDSHelper.Send(data.ToArray(), 0x31);
            //}
            if (temp == null)
            {
                temp = PlayUDSHelper.Send(data.ToArray(), 0x31);
                if (temp == null)
                {
                    LogHelper.ShowMsg("uds休眠失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("uds休眠失败");
                }

            }
            return true;
        }

        bool XiuMian(int level, bool isMOS = false)
        {
            //第一步休眠
            CommonMethod.Unlock();
            //Class1.HuanXing(false);
            //发送休眠指令
            
            
            
            if (isMOS)
            {
               
                if (NewCanHelper.WakeOrSleep(false) == false)
                {
                    LogHelper.ShowMsg("休眠失败");
                    return false;
                }
                CommonMethod.Sleep(100);
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("MOS闭合失败");
                }
            }
            else
            {
                if (NewCanHelper.WakeOrSleep(false) == false)
                {
                    LogHelper.ShowMsg("休眠失败");
                    return false;
                }
                CommonMethod.Sleep(100);
                if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
                {
                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("MOS断开失败");
                }

            }

            CommonMethod.StopSendAwaken();
            CommonMethod.StopSendUnlock();
            if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" && level == 1)
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("MOS闭合失败");
                }
            }
            LogHelper.Debug("休眠产品");
            List<byte> data = new List<byte>();
            if (level == 1)
                data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.XM1_DID);
            else if (level == 2)
            {
                data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.XM2_DID);
            }
            //uds发送休眠
            var temp = PlayUDSHelper.Send(data.ToArray(), byte.Parse(ConfigHelper.UdsParameters.XM1_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp == null)
            {
                temp = PlayUDSHelper.Send(data.ToArray(), byte.Parse(ConfigHelper.UdsParameters.XM1_SID, System.Globalization.NumberStyles.HexNumber));
                if (temp == null)
                {
                    LogHelper.ShowMsg("uds休眠失败");
                    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                    throw new Exception("uds休眠失败");
                }

            }
            return true;
        }


        void GetWakeSource1068(TestResult model, int index)
        {
            CommonMethod.Sleep(50);



            {
                CommonMethod.Sleep(200);
                //读取唤醒源
                var data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.GetHX_DID);
                //string huanxingFangshi = "";
                var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
                if (temp == null)
                {
                    LogHelper.ShowMsg("读取唤醒源失败");
                    return;
                }
                else
                {

                    {
                        string str = "";
                        foreach (var item in temp.Item2)
                        {
                            str += item.ToString("X2") + " ";
                        }
                        LogHelper.ShowMsg(str);
                        model.MeasureVal = temp.Item2[2].ToString("X2");
                        model.Result = model.MeasureVal == model.QualifiedValue;
                    }

                    //model.MeasurementStandard = str;
                }
            }

        }
        void GetWakeSource(TestResult model, int index)
        {
            CommonMethod.Sleep(50);
            if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046" ||
                ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "AVATR_E15" || ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {

                model.MeasureVal = ZCData.HXY.ToString();

                LogHelper.ShowMsg(model.MeasureVal);
                model.Result = model.MeasureVal == model.QualifiedValue;

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390")
            {
                if (index == 1)
                {
                    model.MeasureVal = ZCData.RTC.ToString();
                }
                else if (index == 2)
                {
                    model.MeasureVal = ZCData.GuoYa.ToString();
                }
                else if (index == 3)
                {
                    model.MeasureVal = ZCData.YingJian.ToString();
                }
                else if (index == 4)
                {
                    model.MeasureVal = ZCData.DianLiu.ToString();
                }
                else if (index == 5)
                {
                    model.MeasureVal = ZCData.CanHX.ToString();
                }
                LogHelper.ShowMsg($"{ZCData.GuoYa},{ZCData.YingJian},{ZCData.DianLiu},{ZCData.CanHX}");
                model.Result = model.MeasureVal == model.QualifiedValue;

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                CommonMethod.Sleep(1000);
                if (index == 1)
                {
                    model.MeasureVal = ZCData.RTCCN.ToString();
                }
                else if (index == 2)
                {
                    model.MeasureVal = ZCData.GuoYaCN.ToString();
                }
                else if (index == 3)
                {
                    CommonMethod.Sleep(2000);
                    model.MeasureVal = ZCData.YingJianCN.ToString();
                }
                else if (index == 4)
                {
                    model.MeasureVal = ZCData.DianLiuCN.ToString();
                }
                else if (index == 5)
                {
                    model.MeasureVal = ZCData.CanHXCN.ToString();
                }

                model.Result = model.MeasureVal == model.QualifiedValue;

            }

            else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
            {
                if (index == 1)
                {
                    model.MeasureVal = ZCData.RTCH97.ToString();
                }
                else if (index == 2)
                {
                    CommonMethod.Sleep(2000);
                    model.MeasureVal = ZCData.GuoYaH97.ToString();
                }
                else if (index == 3)
                {
                    CommonMethod.Sleep(4000);
                    model.MeasureVal = ZCData.YingJianH97.ToString();
                }
                else if (index == 4)
                {
                    CommonMethod.Sleep(4000);
                    model.MeasureVal = ZCData.DianLiuH97.ToString();
                }
                else if (index == 5)
                {
                    model.MeasureVal = ZCData.CanHXH97.ToString();
                }
               
                model.Result = model.MeasureVal == model.QualifiedValue;

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
            {
                if (index == 1)
                {
                    model.MeasureVal = ZCData.RTC612.ToString();
                }
                else if (index == 2)
                {
                    model.MeasureVal = ZCData.GuoYa612.ToString();
                }
                else if (index == 3)
                {
                    model.MeasureVal = ZCData.YingJian612.ToString();
                }
                else if (index == 4)
                {
                    model.MeasureVal = ZCData.DianLiu612.ToString();
                }
                else if (index == 5)
                {
                    model.MeasureVal = ZCData.CanHX612.ToString();
                }

                model.Result = model.MeasureVal == model.QualifiedValue;

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
            {
                if (index == 1)
                {
                    model.MeasureVal = ZCData.RTCRC.ToString();
                }
                else if (index == 2)
                {
                    model.MeasureVal = ZCData.GuoYaRC.ToString();
                }
                else if (index == 3)
                {
                    model.MeasureVal = ZCData.YingJianRC.ToString();
                }
                else if (index == 4)
                {
                    model.MeasureVal = ZCData.DianLiuRC.ToString();
                }
                else if (index == 5)
                {
                    model.MeasureVal = ZCData.CanHXRC.ToString();
                }

                model.Result = model.MeasureVal == model.QualifiedValue;

            }
            else if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
            {
                if (index == 1)
                {
                    model.MeasureVal = ZCData.RTCDF.ToString();
                }
                else if (index == 2)
                {
                    CommonMethod.Sleep(2000);
                    model.MeasureVal = ZCData.GuoYaDF.ToString();
                }
                else if (index == 3)
                {
                    CommonMethod.Sleep(4000);
                    model.MeasureVal = ZCData.YingJianDF.ToString();
                }
                else if (index == 4)
                {
                    CommonMethod.Sleep(4000);
                    model.MeasureVal = ZCData.DianLiuDF.ToString();
                }
                else if (index == 5)
                {

                    model.MeasureVal = ZCData.CanHXDF.ToString();
                }

                model.Result = model.MeasureVal == model.QualifiedValue;

            }
            else
            {
                CommonMethod.Sleep(200);
                //读取唤醒源
                var data = PlayUDSHelper.HexChange(ConfigHelper.UdsParameters.GetHX_DID);
                //string huanxingFangshi = "";
                var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
                if (temp == null)
                {
                    LogHelper.ShowMsg("读取唤醒源失败");
                    return;
                }
                else
                {
                    if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                    {
                        string str = "";
                        foreach (var item in temp.Item2)
                        {
                            str += item.ToString("X2") + " ";
                        }
                        LogHelper.ShowMsg(str);
                        model.MeasureVal = int.Parse((temp.Item2[2] & 0x07).ToString("X2")).ToString();
                        model.Result = model.MeasureVal == model.QualifiedValue;
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
                    {
                        string str = "";
                        foreach (var item in temp.Item2)
                        {
                            str += item.ToString("X2") + " ";
                        }
                        LogHelper.ShowMsg(str);
                        model.MeasureVal = int.Parse((temp.Item2[4] & 0x07).ToString("X2")).ToString();
                        model.Result = model.MeasureVal == model.QualifiedValue;
                    }
                    else
                    {
                        string str = "";
                        foreach (var item in temp.Item2)
                        {
                            str += item.ToString("X2") + " ";
                        }
                        LogHelper.ShowMsg(str);
                        model.MeasureVal = int.Parse(temp.Item2[2].ToString("X2")).ToString();
                        model.Result = model.MeasureVal == model.QualifiedValue;
                    }

                    //model.MeasurementStandard = str;
                }
            }

        }

        void GetK(TestResult model)
        {
            CommonMethod.Sleep(200);
            //读取唤醒源
            var data = HexChange(ConfigHelper.UdsParameters.DLB_DID);
            //string huanxingFangshi = "";
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
            if (temp == null)
            {
                LogHelper.ShowMsg("读取唤醒源失败");
                return;
            }
            else
            {
                string str = "";
                foreach (var item in temp.Item2)
                {
                    str += item.ToString("X2") + " ";
                }
                LogHelper.ShowMsg(str);
                model.MeasureVal = temp.Item2[4].ToString("X2") + temp.Item2[5].ToString("X2");
                model.Result = model.MeasureVal != "FFFF";
                //model.MeasurementStandard = str;
            }
        }

        void GetK2(TestResult model, int i)
        {
            CommonMethod.Sleep(200);
            //读取唤醒源
            var data = new byte[] { 0x08, (byte)(0x99 + i) };
            //string huanxingFangshi = "";
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
            if (temp == null)
            {
                LogHelper.ShowMsg("读取唤醒源失败");
                return;
            }
            else
            {
                string str = "";
                foreach (var item in temp.Item2)
                {
                    str += item.ToString("X2") + " ";
                }
                LogHelper.ShowMsg(str);

                model.MeasureVal = ((temp.Item2[4] * 256 + temp.Item2[5]) / 1000.0).ToString("0.000");
                model.Result = (temp.Item2[4] * 256 + temp.Item2[5] > 920) && (temp.Item2[4] * 256 + temp.Item2[5] < 1090);
                //model.MeasurementStandard = str;
            }
        }

        void GetB(TestResult model, int i)
        {
            CommonMethod.Sleep(200);
            //读取唤醒源
            var data = new byte[] { 0x08, (byte)(0x99 + i) };
            //string huanxingFangshi = "";
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
            if (temp == null)
            {
                LogHelper.ShowMsg("读取唤醒源失败");
                return;
            }
            else
            {
                string str = "";
                foreach (var item in temp.Item2)
                {
                    str += item.ToString("X2") + " ";
                }
                LogHelper.ShowMsg(str);

                model.MeasureVal = ((temp.Item2[4] * 256 + temp.Item2[5]) / 1000.0 - 60.0).ToString("0.000");
                model.Result = (((temp.Item2[4] * 256 + temp.Item2[5]) / 1000.0 - 60) > -1.2) && (((temp.Item2[4] * 256 + temp.Item2[5]) / 1000.0 - 60) < 1.2);
                //model.MeasurementStandard = str;
            }
        }
        public static List<byte> HexChange(string hexStr)
        {
            string[] hexParts = hexStr.Split(' ');
            List<byte> byteList = new List<byte>();

            foreach (string part in hexParts)
            {
                if (byte.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                {
                    byteList.Add(result);
                }
                else
                {
                    Console.WriteLine($"无效的十六进制数: {part}");
                }
            }
            return byteList;

        }
        public bool RTC()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();

            #region rtc唤醒
            XiuMian(1);

            if(ConfigHelper.UdsParameters.jiesuo != "RC_PK1B")
            {
                CommonMethod.Sleep(3000);
            }
            

            (bool, double) dl;
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 3);

            if (dl.Item1 == false)
            {
                //LogHelper.ShowMsg("RTC休眠失败");
                AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                throw new Exception("RTC休眠失败");
                //return false;
            }
            else
            {



                dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
                if (dl.Item1 == false)
                {
                    LogHelper.ShowMsg("RTC唤醒失败");
                    return false;
                }
                return true;
            }
            #endregion
        }


        public bool GuoYa(int flag = 1)
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;

            CommonMethod.SetN8331(12.8);
            //Class1.HuanXing(false);
            //发送休眠指令

            if (flag== 1)
            {


                CommonData.IT6322.SetVol(13.6);

            }
            else
            {
                PlayUDSHelper.SetSOC(20);
                CommonMethod.Sleep(60000);
                CommonData.IT6322.SetVol(13);
            }
            
            CommonMethod.Sleep(1000);


            #region 过压唤醒
            //关闭MOS
            ////MOS控制断开cmd
            //var temp = PlayUDSHelper.MOSControl(MOSEnum.断开);

            //if (temp == false)
            //{
            //    AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
            //    throw new Exception("过压唤醒MOS控制断开失败");
            //}
            ////二级休眠
            XiuMian(2);

            
            dl = CommonMethod.GetSleep2CurrentFromPower(awatiTime: 8, threshold: GetSleepCurrMax("2"));

            if (flag == 1)
            {

                if (dl.Item1 == false)
                {
                    AddSleepCurr("2", dl.Item2);
                    LogHelper.ShowMsg("过压唤醒二级休眠失败");

                }
                else
                {
                    AddSleepCurr("2", dl.Item2);
                }
            }


            
            NewCanHelper.MOSControlVoltageEnable(true, true, false, gd: false);

            //控制外电压使能
         

            CommonData.IT6322.SetOutput(true);
          

            if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B"  ||
                ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                CommonMethod.Sleep(10000);
            }
            else
            {
                CommonMethod.Sleep(5000);
            }

            if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {
                CommonMethod.Sleep(2000);
            }
            //控制外电压不使能
       //     CommonMethod.StartSendAwaken();
          //  CommonMethod.Sleep(5000);

            CommonData.IT6322.SetOutput(false);
            NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);

            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("过压唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("过压唤醒成功");
            }
       

            return true;
            #endregion
        }

        public bool YingJian()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            if (NewCanHelper.WakeOrSleep(false) == false)
            {
                LogHelper.ShowMsg("休眠失败");
                return false;
            }
            var temp = PlayUDSHelper.MOSControl(MOSEnum.断开);
            if (temp == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                throw new Exception("过压唤醒MOS控制断开失败");
            }

            #region 硬件唤醒
            XiuMian(2);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 8, threshold: GetSleepCurrMax("1"));
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("硬件唤醒休眠失败");
                return false;
            }

            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败");
            }
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败");
            }
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败电流过低");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("硬件唤醒成功");
            }

            if (ConfigHelper.UdsParameters.jiesuo == "T09_M" || ConfigHelper.UdsParameters.jiesuo == "CN_D7" ||ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97"|| ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
            {
                CommonMethod.Sleep(1000);
            }
            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                CommonMethod.Sleep(5000);
            }
            return true;

            #endregion
        }

        public bool DianLiu()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region 电流唤醒

            XiuMian(1, true);
            //CommonMethod.Sleep(1000);

            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 8, threshold: 10);
            if (dl.Item1 == false)
            {
                AddSleepCurr("1", dl.Item2);
                LogHelper.ShowMsg("电流唤醒休眠失败");
                return false;
            }
            else
            {
                AddSleepCurr("1", dl.Item2);
            }
           
            NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: true);
            CommonMethod.Sleep(1000);
            //控制电流正向采集
            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR" ||
                ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "T09_M" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
            {
                NewCanHelper.CtrolTotalCurrentCollection(false);
            }
            else
            {
                NewCanHelper.CtrolTotalCurrentCollection(true);
            }

            CommonData.N69206H.SetCCMode();
            CommonMethod.Sleep(100);
            CommonData.N69206H.SetOutput(true);
            CommonData.N69206H.SetCCModecur(5);
            //设置电源

            CommonData.M3905D.SetVoltage(1);
            CommonData.M3905D.SetCurrent(5);
            CommonData.M3905D.SetOUTPut(1);
            CommonMethod.Sleep(1000);
            NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 20);
            //if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
            //{
            //    Thread.Sleep(10000);
            //}

            TestResult test = new TestResult();

            CommonMethod.Sleep(5000);

            CommonData.N69206H.SetOutput(false);
            CommonData.M3905D.SetVoltage(0);
            CommonData.M3905D.SetOUTPut(0);
            NewCanHelper.CloseTotalCurrentCollection();

            LogHelper.ShowMsg("电流唤醒成功");
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("电流唤醒失败");
                return false;
            }
            CommonMethod.Sleep(1000);
            return true;
            #endregion
        }

        public bool CanHX()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region Can唤醒
            XiuMian(1);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 8, threshold: 10);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("Can唤醒休眠失败");
                return false;
            }
            CommonMethod.StartSendAwaken();

            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 20);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("Can唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("Can唤醒成功");
            }
     //       CommonMethod.Unlock();
            //         CommonMethod.StartSendUnlock();
            return true;
            #endregion
        }


        public bool RTC1068()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();

            #region rtc唤醒
            XiuMian1068(0);


            (bool, double) dl;
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: GetSleepCurrMax("1"));

            if (dl.Item1 == false)
            {
                //LogHelper.ShowMsg("RTC休眠失败");
                AddModelAction(new TestResult() { ChildrenItem = "唤醒源测试失败" });
                throw new Exception("RTC休眠失败");
                //return false;
            }
            else
            {



                dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
                if (dl.Item1 == false)
                {
                    LogHelper.ShowMsg("RTC唤醒失败");
                    return false;
                }
                return true;
            }
            #endregion
        }


        public bool GuoYa1068()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;



            //二级休眠
            XiuMian1068(2);
            dl = CommonMethod.GetSleep2CurrentFromPower(awatiTime: 5, threshold: GetSleepCurrMax("2"));

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("过压唤醒二级休眠失败");
                return false;
            }
            else
            {
                AddSleepCurr("2", dl.Item2);
            }



            //控制外电压使能
            NewCanHelper.MOSControlVoltageEnable(true, true, false, gd: false);
            CommonData.IT6322.SetVol(13.7);
            CommonData.IT6322.SetOutput(true);

            CommonMethod.Sleep(1000);
            //控制外电压不使能
            CommonData.IT6322.SetOutput(false);


            NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("过压唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("过压唤醒成功");
            }

            return true;

        }

        public bool YingJian1068()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region 硬件唤醒
            XiuMian1068(1);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: GetSleepCurrMax("1"));

            if (dl.Item1 == false)
            {
                AddSleepCurr("1", dl.Item2);
                LogHelper.ShowMsg("硬件唤醒休眠失败");
                return false;
            }
            else
            {
                AddSleepCurr("1", dl.Item2);
            }
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败");
            }
            if (NewCanHelper.WakeOrSleep(true) == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败");
            }
            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 50);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("硬件唤醒失败电流过低");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("硬件唤醒成功");
            }

            return true;

            #endregion
        }

        public bool DianLiu1068()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region 电流唤醒

            XiuMian1068(1, true);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 10);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("电流唤醒休眠失败");
                return false;
            }

            CommonData.N69206H.SetOutput(false);
            CommonMethod.Sleep(100);
            CommonData.N69206H.SetCCMode();
            CommonMethod.Sleep(100);
            CommonData.N69206H.SetOutput(true);

            CommonData.N69206H.SetCCModecur(1);
            //设置电源
            CommonData.M3905D.SetOUTPut(1);
            CommonData.M3905D.SetVoltage(1);
            CommonData.M3905D.SetCurrent(1);
            //      CommonMethod.Sleep(1000);
            //控制电流正向采集

            NewCanHelper.CtrolTotalCurrentCollection(true);




            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 20);

            //Thread.Sleep(2000);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("电流唤醒失败");
                return false;
            }
            CommonData.M3905D.SetVoltage(0);
            CommonData.M3905D.SetOUTPut(0);
            CommonData.N69206H.SetOutput(false);
            NewCanHelper.CloseTotalCurrentCollection();
            LogHelper.ShowMsg("电流唤醒成功");


            return true;
            #endregion
        }

        public bool CanHX1068()
        {
            DateTime now = DateTime.Now;
            List<byte> data = new List<byte>();
            (bool, double) dl;
            #region Can唤醒
            XiuMian1068(1);
            dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 10);

            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("Can唤醒休眠失败");
                return false;
            }
            CommonMethod.StartSendAwaken();

            dl = CommonMethod.GetWorkCurrentFromPower(awatiTime: 8, threshold: 20);
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("Can唤醒失败");
                return false;
            }
            else
            {
                LogHelper.ShowMsg("Can唤醒成功");
            }
            //       CommonMethod.Unlock();
            //         CommonMethod.StartSendUnlock();
            return true;
            #endregion
        }

        List<TestResult> sleepCurrResult = new List<TestResult>();
        public void AddSleepCurr(string level, double curr)
        {
            var par = CommonData.TestParameters.FirstOrDefault(m => m.Extend1 == level && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
            if (par == null)
            {
                LogHelper.Debug("休眠电流参数未配置");
            }
            else
            {
                if (isTestSleepCurr == true)
                {
                    TestResult model = new TestResult();
                    model.MeasureVal = curr.ToString("0.000");
                    model.Result = double.Parse(model.MeasureVal) <= par.MaxVal && double.Parse(model.MeasureVal) >= par.MinVal;
                    model.ChildrenItem = par.ChildrenItem;
                    model.MeasurementStandard = par.MeasurementStandard;
                    sleepCurrResult.Add(model);
                }
            }
        }
        public double GetSleepCurrMax(string level)
        {
            var par = CommonData.TestParameters.FirstOrDefault(m => m.Extend1 == level && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
            if (par == null)
            {
                LogHelper.Debug("休眠电流参数未配置");
                return 0;
            }
            else
            {
                return par.MaxVal.Value;
            }
        }
    }
}
