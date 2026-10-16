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

namespace DetectionProcess.SERES
{
    public class BalancedAndAFE : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------均衡与AFE诊断测试------");
            var par = pars.FirstOrDefault();
            if (par == null)
            {
                //LogHelper.ShowMsg("均衡与AFE诊断测试参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "均衡与AFE诊断测试失败" });
                throw new Exception("均衡与AFE诊断测试参数未配置");
            }
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;
            

            if (PlayUDSHelper.Reset()==false)
            {
                //LogHelper.ShowMsg("复位失败");
                AddModelAction(new TestResult() { ChildrenItem = "均衡与AFE诊断测试失败" });
                throw new Exception("复位失败");
            }
            AddModelAction(new TestResult() { ChildrenItem = "复位", Extend1 = "-1" });
            CommonMethod.Sleep(6000);
            var result = PlayUDSHelper.GetBalancedAndAFE();
            model.EndTime = DateTime.Now;
            model.ChildrenItem = par.ChildrenItem;
            model.MeasurementStandard = par.MeasurementStandard;
            model.Extend1 = "1";
            if (result == null)
            {
                //LogHelper.ShowMsg("获取均衡结果失败");
                AddModelAction(new TestResult() { ChildrenItem = "均衡与AFE诊断测试失败" });
                throw new Exception("获取均衡结果失败");
            }
            else
            {
                if (ConfigHelper.UdsParameters.jiesuo == "CHERY_EH3")
                {
                    int startIndex = 0;
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i] != 0x00)
                        {
                            startIndex = i; break;
                        }
                    }
                    if (result[startIndex] == 0x07)
                    {
                        model.MeasureVal = "";
                        int len = result[startIndex];
                        for (int i = startIndex + 1; i < len + 1; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }
                        model.Result = true;
                    }
                    else
                    {
                        model.MeasureVal = "";
                        
                        int len = result[startIndex];
                        if (!ConfigHelper.Cantype)
                        {
                            len = 7;
                        }
                        for (int i = startIndex + 1; i < len + 1 + startIndex; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }

                        model.Result = false;
                    }

                }
                else
                {
                    int startIndex = 0;
                    for (int i = 0; i < result.Length; i++)
                    {
                        if (result[i] != 0x00)
                        {
                            startIndex = i; break;
                        }
                    }
                    if (result[startIndex] == 0x03)
                    {
                        model.MeasureVal = "";
                        int len = result[startIndex];
                        for (int i = startIndex + 1; i < len + 1; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }
                        model.Result = true;
                    }
                    else
                    {
                        model.MeasureVal = "";
                        int len = result[startIndex];
                        if (!ConfigHelper.Cantype)
                        {
                            len = 7;
                        }
                        for (int i = startIndex + 1; i < len + 1 + startIndex; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }

                        model.Result = false;
                    }

                }
                    
            }
            //else if (result[0] == 0x03)
            //{
            //    model.MeasureVal = "";
            //    int len = result[0] + 1;
            //    for (int i = 0; i < len; i++)
            //    {
            //        model.MeasureVal += result[i].ToString("X2") + " ";
            //    }
            //    model.Result = true;
            //}
            //else
            //{
            //    model.MeasureVal = "";
            //    int len = result[0] + 1;
            //    for (int i = 0; i < len; i++)
            //    {
            //        model.MeasureVal += result[i].ToString("X2") + " ";
            //    }

            //    model.Result = false;
            //}
            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            {
                CommonMethod.StopClear();
            }
            CommonMethod.Unlock();
            AddModelAction(model);
            CommonData.TestCount++;
            return true;
        }
    }
}
