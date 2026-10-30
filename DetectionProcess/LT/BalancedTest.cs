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

namespace DetectionProcess.LT
{
    /// <summary>
    /// 均衡测试
    /// </summary>
    public class BalancedTest : DetectBase
    {
        //bool isDebug = true;
        public override bool Start(IEnumerable<TestParameter> pars)
        {


            if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                LogHelper.Debug("------均衡测试------");
                var par = pars.FirstOrDefault();
                if (par == null)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                    throw new Exception("均衡测试未配置参数");
                }



                DateTime startTime = DateTime.Now;
                CommonMethod.SetN8331(12.8);

                NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);
                CommonMethod.Sleep(2000);
                var result1 = new double[4] { -1, -1, -1, -1 };
                CommonMethod.Unlock();
                var result2 = new double[4] { -1, -1, -1, -1 };
                {
                    {
                        //开启均衡1 3
                        if (PlayUDSHelper.StartChannelBalance1068(true) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启1、3通道均衡失败");
                        }


                        for (int i = 0; i < 60; i++)
                        {

                            //查询第一通道电流
                            var temp1 = double.Parse(CommonData.N8331.SelectCurr(1)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp1 < par.MaxVal && temp1 > par.MinVal)
                            {
                                if (result1[0] == -1)
                                {
                                    result1[0] = temp1;
                                }
                            }


                            //查询第三通道电流
                            var temp2 = double.Parse(CommonData.N8331.SelectCurr(3)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp2 < par.MaxVal && temp2 > par.MinVal)
                            {
                                if (result1[2] == -1)
                                {
                                    result1[2] = temp2;
                                }
                            }
                            CommonMethod.Sleep(10);
                            if (result1[0] != -1 && result1[2] != -1)
                            {
                                break;
                            }
                        }
                        if (PlayUDSHelper.StopChannelBalance1068() == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启1、3通道均衡失败");
                        }
                        CommonMethod.Sleep(2000);
                        var temp3 = Math.Abs(double.Parse(CommonData.N8331.SelectCurr(1)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000);

                        result2[0] = temp3;

                        var temp4 = Math.Abs(double.Parse(CommonData.N8331.SelectCurr(3)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000);

                        result2[2] = temp4;







                    }

                    {
                        //开启均衡2 4
                        if (PlayUDSHelper.StartChannelBalance1068(false) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启2、4通道均衡失败");
                        }
                        for (int i = 0; i < 60; i++)
                        {

                            //查询第一通道电流
                            var temp1 = double.Parse(CommonData.N8331.SelectCurr(2)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp1 < par.MaxVal && temp1 > par.MinVal)
                            {
                                if (result1[1] == -1)
                                {
                                    result1[1] = temp1;
                                }
                            }

                            //查询第三通道电流
                            var temp2 = double.Parse(CommonData.N8331.SelectCurr(4)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp2 < par.MaxVal && temp2 > par.MinVal)
                            {
                                if (result1[3] == -1)
                                {
                                    result1[3] = temp2;
                                }
                            }

                            CommonMethod.Sleep(10);
                            if (result1[1] != -1 && result1[3] != -1)
                            {
                                break;
                            }
                        }

                        if (PlayUDSHelper.StopChannelBalance1068() == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启1、3通道均衡失败");
                        }

                        CommonMethod.Sleep(2000);
                        var temp3 = Math.Abs(double.Parse(CommonData.N8331.SelectCurr(2)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000);

                        result2[1] = temp3;

                        var temp4 = Math.Abs(double.Parse(CommonData.N8331.SelectCurr(4)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000);

                        result2[3] = temp4;


                    }
                }


                CommonMethod.Unlock();
                if (PlayUDSHelper.StopChannelBalance1068() == false)
                {

                    LogHelper.Debug("结束通道均衡控制失败");
                    AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                    throw new Exception("结束通道均衡控制失败");
                }


                CommonMethod.SetN8331(12.8);
               
                (double, double, double, double) result = (result1[0], result1[1], result1[2], result1[3]);
                (double, double, double, double) resultclose = (result2[0], result2[1], result2[2], result2[3]);
                DateTime endTime = DateTime.Now;
                var span = (endTime - startTime).TotalSeconds / 4;


                TestResult model = new TestResult();
                model.ChildrenItem = "电芯均衡开启*测试".Replace('*', '1');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item1.ToString("0.000");
                model.Result = result.Item1 >= par.MinVal && result.Item1 <= par.MaxVal;
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡开启*测试".Replace('*', '2');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item2.ToString("0.000");
                model.Result = result.Item2 >= par.MinVal && result.Item2 <= par.MaxVal;
                model.StartTime = startTime.AddSeconds(span);
                model.EndTime = startTime.AddSeconds(span * 2);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡开启*测试".Replace('*', '3');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item3.ToString("0.000");
                model.Result = result.Item3 >= par.MinVal && result.Item3 <= par.MaxVal;
                model.StartTime = startTime.AddSeconds(span * 2);
                model.EndTime = startTime.AddSeconds(span * 3);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡开启*测试".Replace('*', '4');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item4.ToString("0.000");
                model.Result = result.Item4 >= par.MinVal && result.Item4 <= par.MaxVal;
                model.StartTime = startTime.AddSeconds(span * 3);
                model.EndTime = startTime.AddSeconds(span * 4);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡关闭*测试".Replace('*', '1');
                model.MeasurementStandard = "[0,5]";
                model.QualifiedValue = "[0,5]";
                model.MeasureVal = resultclose.Item1.ToString("0.000");
                model.Result = resultclose.Item1 <= 5;
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡关闭*测试".Replace('*', '2');
                model.MeasurementStandard = "[0,5]";
                model.QualifiedValue = "[0,5]";
                model.MeasureVal = resultclose.Item2.ToString("0.000");
                model.Result = resultclose.Item2 <= 5;
                model.StartTime = startTime.AddSeconds(span);
                model.EndTime = startTime.AddSeconds(span * 2);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡关闭*测试".Replace('*', '3');
                model.MeasurementStandard = "[0,5]";
                model.QualifiedValue = "[0,5]";
                model.MeasureVal = resultclose.Item3.ToString("0.000");
                model.Result = resultclose.Item3 <= 5;
                model.StartTime = startTime.AddSeconds(span * 2);
                model.EndTime = startTime.AddSeconds(span * 3);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "电芯均衡关闭*测试".Replace('*', '4');
                model.MeasurementStandard = "[0,5]";
                model.QualifiedValue = "[0,5]";
                model.MeasureVal = resultclose.Item4.ToString("0.000");
                model.Result = resultclose.Item4 <= 5;
                model.StartTime = startTime.AddSeconds(span * 3);
                model.EndTime = startTime.AddSeconds(span * 4);
                AddModelAction(model);

                NewCanHelper.MOSControlVoltageEnable(true, false, false);
          
                CommonData.TestCount++;
                return true;
            }

            else
            {

                LogHelper.Debug("------均衡测试------");


                CommonMethod.Unlock();


                var par = pars.FirstOrDefault();
                if (par == null)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                    throw new Exception("均衡测试未配置参数");
                }



                if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS断开失败");
                }

                DateTime startTime = DateTime.Now;
                CommonMethod.SetN8331(13.6);
                CommonData.IT6322.SetOutput(true);
                CommonData.IT6322.SetVol(13.6);
                CommonMethod.Sleep(200);
                NewCanHelper.MOSControlVoltageEnable(true, true, false, gd: false);

                var b11 = double.Parse(CommonData.N8331.SelectCurr(1));
                var b21 = double.Parse(CommonData.N8331.SelectCurr(2));
                var b31 = double.Parse(CommonData.N8331.SelectCurr(3));
                var b41 = double.Parse(CommonData.N8331.SelectCurr(4));
                var aa1 = CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;

                var result1 = new double[4] { -1, -1, -1, -1 };

                if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702")
                {

                    for (int n = 0; n < 4; n++)
                    {
                        if (PlayUDSHelper.StartChannelBalance(index: n) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启所有通道均衡失败");
                        }
                        CommonMethod.Sleep(500);
                        for (int i = 0; i < 60; i++)
                        {
                            result1[n] = double.Parse(CommonData.N8331.SelectCurr(n + 1)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            CommonMethod.Sleep(500);
                            if (result1[n] > 40 && result1[n] < 100)
                            {
                                break;
                            }
                        }

                    }
                }
                else
                {
                    {

                        //开启均衡1 3
                        if (PlayUDSHelper.StartChannelBalance(true) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启1、3通道均衡失败");
                        }
                        //NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);

                        var b1 = double.Parse(CommonData.N8331.SelectCurr(1));
                        var b2 = double.Parse(CommonData.N8331.SelectCurr(2));
                        var b3 = double.Parse(CommonData.N8331.SelectCurr(3));
                        var b4 = double.Parse(CommonData.N8331.SelectCurr(4));
                        var aa = CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;

                        for (int i = 0; i < 60; i++)
                        {

                            //查询第一通道电流
                            var temp1 = double.Parse(CommonData.N8331.SelectCurr(1)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp1 < par.MaxVal && temp1 > par.MinVal)
                            {
                                if (result1[0] == -1)
                                {
                                    result1[0] = temp1;
                                }
                            }

                            //查询第三通道电流
                            var temp2 = double.Parse(CommonData.N8331.SelectCurr(3)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp2 < par.MaxVal && temp2 > par.MinVal)
                            {
                                if (result1[2] == -1)
                                {
                                    result1[2] = temp2;
                                }
                            }
                            CommonMethod.Sleep(10);

                            if (result1[0] != -1 && result1[2] != -1)
                            {
                                break;
                            }
                        }

                    }

                    {
                        //开启均衡2 4
                        if (PlayUDSHelper.StartChannelBalance(false) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                            throw new Exception("开启2、4通道均衡失败");
                        }
                        for (int i = 0; i < 60; i++)
                        {

                            //查询第一通道电流
                            var temp1 = double.Parse(CommonData.N8331.SelectCurr(2)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                       
                            if (temp1 < par.MaxVal && temp1 > par.MinVal)
                            {
                                if (result1[1] == -1)
                                {
                                    result1[1] = temp1;
                                }
                            }

                            //查询第三通道电流
                            var temp2 = double.Parse(CommonData.N8331.SelectCurr(4)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                            if (temp2 < par.MaxVal && temp2 > par.MinVal)
                            {
                                if (result1[3] == -1)
                                {
                                    result1[3] = temp2;
                                }
                            }
                            CommonMethod.Sleep(10);
                            if (result1[1] != -1 && result1[3] != -1)
                            {
                                break;
                            }
                        }

                    }
                }



                if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
                {

                }
                else
                {
                    CommonMethod.Unlock();
                    if (PlayUDSHelper.StopChannelBalance() == false)
                    {

                        LogHelper.Debug("结束通道均衡控制失败");
                        AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                        throw new Exception("结束通道均衡控制失败");
                    }
                }
              




                CommonMethod.SetN8331(13.2);
                CommonData.IT6322.SetVol(13.7);
                CommonData.IT6322.SetOutput(false);
                (double, double, double, double) result = (result1[0], result1[1], result1[2], result1[3]);
                DateTime endTime = DateTime.Now;
                var span = (endTime - startTime).TotalSeconds / 4;


                TestResult model = new TestResult();
                model.ChildrenItem = par.ChildrenItem.Replace('*', '1');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item1.ToString("0.000");
                model.Result = result.Item1 >= par.MinVal && result.Item1 <= par.MaxVal;
                model.StartTime = startTime;
                model.EndTime = startTime.AddSeconds(span);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = par.ChildrenItem.Replace('*', '2');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item2.ToString("0.000");
                model.Result = result.Item2 >= par.MinVal && result.Item2 <= par.MaxVal;
                model.StartTime = startTime.AddSeconds(span);
                model.EndTime = startTime.AddSeconds(span * 2);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = par.ChildrenItem.Replace('*', '3');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item3.ToString("0.000");
                model.Result = result.Item3 >= par.MinVal && result.Item3 <= par.MaxVal;
                model.StartTime = startTime.AddSeconds(span * 2);
                model.EndTime = startTime.AddSeconds(span * 3);
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = par.ChildrenItem.Replace('*', '4');
                model.MeasurementStandard = par.MeasurementStandard;
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasureVal = result.Item4.ToString("0.000");
                model.Result = result.Item4 >= par.MinVal && result.Item4 <= par.MaxVal;
                model.StartTime = startTime.AddSeconds(span * 3);
                model.EndTime = startTime.AddSeconds(span * 4);
                AddModelAction(model);
                if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
                {
                    PlayUDSHelper.CloseChannelBalance();
                    CommonMethod.Sleep(3000);

                    double g1 = double.Parse(CommonData.N8331.SelectCurr(1)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;

                    var a = CommonData.N8331.SelectCurr(1);
                    var b = CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;

                    double g2 = double.Parse(CommonData.N8331.SelectCurr(2)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                    var a2 = CommonData.N8331.SelectCurr(2);
                    var b2 = CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                    double g3 = double.Parse(CommonData.N8331.SelectCurr(3)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                    var a3 = CommonData.N8331.SelectCurr(3);
                    var b3 = CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                    double g4 = double.Parse(CommonData.N8331.SelectCurr(4)) - CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;
                    var a4 = CommonData.N8331.SelectCurr(4);
                    var b4 = CommonData.NXI_6400.GetCResult(1).Item1.FirstOrDefault() * 1000;


                    model = new TestResult();
                    model.ChildrenItem = "均衡电流测试通道1关闭";
                    model.MeasurementStandard = "<1mA";
                    model.QualifiedValue = "<1mA";
                    model.MeasureVal = g1.ToString("0.000");
                    model.Result = g1 < 1;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "均衡电流测试通道2关闭";
                    model.MeasurementStandard = "<1mA";
                    model.QualifiedValue = "<1mA";
                    model.MeasureVal = g2.ToString("0.000");
                    model.Result = g2 < 1;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "均衡电流测试通道3关闭";
                    model.MeasurementStandard = "<1mA";
                    model.QualifiedValue = "<1mA";
                    model.MeasureVal = g3.ToString("0.000");
                    model.Result = g3 < 1;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "均衡电流测试通道4关闭";
                    model.MeasurementStandard = "<1mA";
                    model.QualifiedValue = "<1mA";
                    model.MeasureVal = g4.ToString("0.000");
                    model.Result = g4 < 1;
                    model.StartTime = startTime;
                    model.EndTime = startTime.AddSeconds(span);
                    AddModelAction(model);
                }


                NewCanHelper.MOSControlVoltageEnable(true, false, false);
                CommonData.IT6322.SetOutput(false);
                CommonData.IT6322.SetVol(13.7);
                CommonData.TestCount++;
                


                return true;
            }



            

        }
    }
}
