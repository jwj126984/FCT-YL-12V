using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// 均衡测试
    /// </summary>
    public class BalancedTest : DetectBase
    {
        //bool isDebug = true;
        public override bool Start(IEnumerable<TestParameter> pars)
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
            
            if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
            {
                LogHelper.ShowMsg("MOS断开失败");
                throw new Exception("均衡测试MOS断开失败");
            }
            //

            var result1 = new double[4] { -1,-1,-1,-1};
            {
                //开启均衡
                if (PlayUDSHelper.StartChannelBalance() == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                    throw new Exception("开启所有通道均衡失败");
                }
                {
                    DateTime now = DateTime.Now;
                    int awaitTime = 5;
                    while (true)
                    {
                        //if (PlayUDSHelper.AwaitStartChannelBalance() == true)
                        //{
                        //    break;
                        //}
                        if ((DateTime.Now - now).TotalSeconds > awaitTime)
                        {
                            LogHelper.Debug("等待开启所有通道均衡超时");
                            break;
                            //return false;
                        }
                    }
                }
                CommonData.IT6322.SetOutput(true);
                CommonData.IT6322.SetVol(13.2);
                NewCanHelper.MOSControlVoltageEnable(true, true, false);
                List<double> cc1 = new List<double>();
                List<double> cc2 = new List<double>();
                List<double> cc3 = new List<double>();
                List<double> cc4 = new List<double>();
                for (int i = 0; i < 60; i++)
                {
                    for (int j = 1; j < 5; j++)
                    {
                        var temp= double.Parse(CommonData.N8331.SelectCurr(j));
                        if (temp < par.MaxVal-2 && temp > par.MinVal+3)
                        {
                            if(result1[j - 1]==-1)
                            {
                                result1[j - 1] = temp;
                            }
                        }
                        LogHelper.ShowMsg($"{j}通道：{temp}");
                    }
                    if (result1[0] != -1&& result1[1]!=-1 && result1[2] != -1 && result1[3] != -1)
                    {
                        break;
                    }
                }
            }
       //     CommonMethod.Unlock();
            if (PlayUDSHelper.StopChannelBalance() == false)
            {

                LogHelper.Debug("结束通道均衡控制失败");
                AddModelAction(new TestResult() { ChildrenItem = "均衡测试失败" });
                throw new Exception("结束通道均衡控制失败");
            }

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
            NewCanHelper.MOSControlVoltageEnable(true, false);
            CommonData.IT6322.SetOutput(false);
            CommonData.IT6322.SetVol(13.7);
            CommonData.TestCount++;
            return true;

        }
    }
}
