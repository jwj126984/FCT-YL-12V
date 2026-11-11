using AllData;
using Common;
using Common.Can;
using Common.UDS;
using Model;
using Models_C.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class BalancedAndAFE : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------均衡与AFE诊断测试------");

          

            int testnum = 0;
            TestResult model;
            while (true)
            {
                var par = pars.FirstOrDefault();
                if (par == null)
                {
                    //LogHelper.ShowMsg("均衡与AFE诊断测试参数未配置");
                    AddModelAction(new TestResult() { ChildrenItem = "均衡与AFE诊断测试失败" });
                    throw new Exception("均衡与AFE诊断测试参数未配置");
                }
                model = new TestResult();
                model.StartTime = DateTime.Now;

                if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97")
                {
                    //NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);
                    //CommonMethod.Unlock();
                    //CommonMethod.StopSendAwaken();
                    //CommonMethod.StopSendUnlock();

                    //if (PlayUDSHelper.UdsSleep(1) == null)
                    //{
                    //    LogHelper.ShowMsg("休眠失败");

                    //}

                    //var dl = CommonMethod.GetSleep1CurrentFromPower();


                    //CommonMethod.StartSendAwaken();

                    //dl = CommonMethod.GetWorkCurrentFromPower();
                    //if (dl.Item1 == false)
                    //{
                    //    LogHelper.ShowMsg("唤醒失败");

                    //}
                    PlayUDSHelper.Reset();
                    CommonMethod.Sleep(5000);
                }
                    


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
                        break;
                        
                    }
                    else if(ConfigHelper.UdsParameters.jiesuo == "WL_F710S" && (result[1] == 0x59 || result[2] == 0x59))
                    {
                        model.MeasureVal = "";
                        int len = 7;
                        for (int i = startIndex + 1; i < len + 1 + startIndex; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }
                        testnum++;
                        model.Result = true;
                    }
                    else if(ConfigHelper.UdsParameters.jiesuo == "T09_M")
                    {
                        model.MeasureVal = "";
                        int len = result[startIndex];
                        for (int i = startIndex + 1; i < len + 1 + startIndex; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }
                        testnum++;
                        string response = result[4].ToString("X2") + result[5].ToString("X2") + result[6].ToString("X2");
                        if(response == "C12987" || response == "C15187" || response == "C14687" || response == "C11287" || response == "C11187" || response == "D08087" 
                            || response == "C14087" || response == "C19887" || response == "C07388" || response == "C45281" || response == "D8E081")
                        {
                            model.Result = true;
                        }
                        else
                        {
                            model.Result = false;
                        }
                            
                    }
                    else
                    {
                        model.MeasureVal = "";
                        int len = result[startIndex];
                        for (int i = startIndex + 1; i < len + 1 + startIndex; i++)
                        {
                            model.MeasureVal += result[i].ToString("X2") + " ";
                        }
                        testnum++;
                        model.Result = false;
                    }

                }
                if(testnum == 3)
                {
                    testnum = 0;
                    break;
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
            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y" || ConfigHelper.UdsParameters.jiesuo == "XP_12V" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            {
                CommonMethod.StopClear();
            }
            AddModelAction(model);
            CommonData.TestCount++;


            if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" )
            {
                try
                {
                    CommonMethod.StopSendAwaken();
                    CommonMethod.StopSendUnlock();

                    CommonMethod.Unlock();
                    if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C")
                    {
                        var temp = PlayUDSHelper.UdsSleep(1);
                        if (temp == null)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "一级休眠", Result = false });
                        }
                        else
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "一级休眠", Result = null });
                        }
                    }
                    else
                    {
                        var temp = PlayUDSHelper.UdsSleep(2);
                        if (temp == null)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "二级休眠", Result = false });
                        }
                        else
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "二级休眠", Result = null });
                        }
                    }




                }
                catch (Exception)
                {
                   
                        AddModelAction(new TestResult() { ChildrenItem = "二级休眠", Result = false });
                    
                  
                }
            }

            return true;
        }
    }
}
