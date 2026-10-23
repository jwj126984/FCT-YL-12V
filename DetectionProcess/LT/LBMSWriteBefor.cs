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
    internal class LBMSWriteBefor : DetectBase
    {
        static string BarCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
        static string BarCodeMonth = "123456789ABC";
        static string BarCodeDay = "123456789ABCDEFGHJKLMNPRSTVWXYZ";
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------LBMS写入------");

            

            if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" )
            {
                CommonData.TestCount++;
                return true;
            }
            CommonMethod.Unlock();
            
            var par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("硬件"));
            if (pars.Count() == 0)
            {
                //LogHelper.ShowMsg("版本号参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                throw new Exception("版本号参数未配置");
            }
            DateTime start = DateTime.Now;
            var barCode = CommonData.CurrentBarCode;

            if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {

                barCode = barCode.Substring(9);
          

          
                if (ConfigHelper.UdsParameters.REPORT == "GAV_V57") {
                    PlayUDSHelper.GoToAPP_V57();
                } else {
                    PlayUDSHelper.GoToAPP(); }

                Thread.Sleep(1000);
                CommonMethod.Unlock();
                PlayUDSHelper.BarCodeCalibrateGQ(barCode);
                Thread.Sleep(1000);
                PlayUDSHelper.Reset();
                Thread.Sleep(1000);
                
                CommonMethod.StartSendAwaken();
                Thread.Sleep(1000);
                CommonMethod.Unlock();
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                PlayUDSHelper.GoToGC();
                barCode = barCode.Substring(19);


                var barValue = barCode.Substring(0,6) +  barCode.Substring(8, 4);

                Thread.Sleep(500);
                PlayUDSHelper.BarCodeCalibrateC1046(barValue);
                Thread.Sleep(500);
                PlayUDSHelper.VersionCalibateC1046(par.MeasurementStandard);
                Thread.Sleep(500);
                PlayUDSHelper.VersionCalibateC1046JX(ConfigHelper.UdsParameters.KHYJ_DATA);
                Thread.Sleep(500);
                CommonData.TestCount++;
                return true;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" )
            {
                barCode = barCode.Substring(19);

                int barYear = int.Parse(barCode.Substring(0, 2));
                int barMonth = int.Parse(barCode.Substring(2, 2));
                int barDay = int.Parse(barCode.Substring(4, 2));
                var barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);

                CommonMethod.Sleep(50);
                if (PlayUDSHelper.BarCodeCalibrateCA(barCode) == false)
                {
                    //LogHelper.ShowMsg("序列号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("批次号写入失败");

                }



                Thread.Sleep(500);

                if (PlayUDSHelper.VersionCalibateDF(par.MeasurementStandard) == false)
                {
                }
                Thread.Sleep(1000);
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
            {
                string flag = "";
                if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1")
                {
                    flag = "100065";
                }
                else
                {
                    flag = "M20042";
                }
                barCode = barCode.Substring(19);
                int barYear = int.Parse(barCode.Substring(0, 2));
                int barMonth = int.Parse(barCode.Substring(2, 2));
                int barDay = int.Parse(barCode.Substring(4, 2));
                var barValue = barYear + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) +"0"+ barCode.Substring(8, 4) + flag;
                PlayUDSHelper.GoToGC();
                CommonMethod.Sleep(50);
                if (PlayUDSHelper.BarCodeCalibrateDF(barValue) == false)

                {
                    //LogHelper.ShowMsg("序列号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("批次号写入失败");


                }
                Thread.Sleep(500);

                if (PlayUDSHelper.VersionCalibateDF(par.MeasurementStandard) == false)
                {
                }
                Thread.Sleep(1000);
            }

            else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
            {
                barCode = barCode.Substring(19);


                if (PlayUDSHelper.BarCodeCalibrateVOYAH_H97(barCode) == false)
                {
                    //LogHelper.ShowMsg("序列号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("批次号写入失败");

                }
                Thread.Sleep(50);

                if (PlayUDSHelper.VersionCalibateVOYAH_H56C(par.MeasurementStandard) == false)
                {
                    //LogHelper.ShowMsg("硬件号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("硬件号写入失败");

                }





            }
            else
            {
                barCode = barCode.Substring(19);

                int barYear = int.Parse(barCode.Substring(0, 2));
                int barMonth = int.Parse(barCode.Substring(2, 2));
                int barDay = int.Parse(barCode.Substring(4, 2));
                var barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);



                if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR")
                {


                    if (PlayUDSHelper.BarCodeCalibrateCA(barCode) == false)
                    {
                        //LogHelper.ShowMsg("序列号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("批次号写入失败");

                    }



                    Thread.Sleep(500);

                    if (ConfigHelper.UdsParameters.jiesuo != "AVATR_E15")
                    {
                        if (PlayUDSHelper.VersionCalibateCA(par.MeasurementStandard) == false)
                        {

                            AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                            throw new Exception("硬件写入失败");

                        }
                    }


                }

                else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {


                  
                    PlayUDSHelper.GoToGC();
                    if (PlayUDSHelper.BarCodeCalibrateWL(barCode) == false)

                    {
                        //LogHelper.ShowMsg("序列号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("批次号写入失败");


                    }
                    Thread.Sleep(500);

                    PlayUDSHelper.SCCalibrateWL(barCode);
                    Thread.Sleep(500);

                    if (PlayUDSHelper.VersionCalibateWL(par.MeasurementStandard) == false)
                    {
                    }
                }
                
                else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C")
                {
                    barCode = CommonData.CurrentBarCode.Substring(11);
                    PlayUDSHelper.GOTOGCPK();
                    Thread.Sleep(500);
                    if (PlayUDSHelper.BarCodeCalibrateRC(barCode) == false)

                        Thread.Sleep(500);
                    CommonMethod.Unlock();
                    Thread.Sleep(500);
                    if (PlayUDSHelper.VersionCalibateRC(par.MeasurementStandard) == false)

                    {

                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("硬件写入失败");

                    }


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                {
                    barCode = CommonData.CurrentBarCode.Substring(11);
                    barCode = barCode.Substring(8,6) + barCode.Substring(16,4);
                    Thread.Sleep(500);
                    if (PlayUDSHelper.BarCodeCalibrateRC1B(barCode) == false)

                        Thread.Sleep(500);
                   
                    Thread.Sleep(500);
                    if (PlayUDSHelper.VersionCalibateRC(par.MeasurementStandard) == false)

                    {

                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("硬件写入失败");

                    }


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "JAC_X6")
                {


                    if (PlayUDSHelper.BarCodeCalibrateJH(barValue) == false)
                    {
                        //LogHelper.ShowMsg("序列号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("批次号写入失败");
                    }

                    Thread.Sleep(500);
                    if (PlayUDSHelper.VersionCalibateJH(par.MeasurementStandard) == false)
                    {

                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("硬件写入失败");

                    }
                    Thread.Sleep(500);
                    if (PlayUDSHelper.VersionCalibateRJJH(ConfigHelper.UdsParameters.Soft_ID) == false)
                    {

                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("软件写入失败");

                    }


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                {
                    barValue = barCode.Substring(0, 6) + barCode.Substring(8, 4);

                    if (PlayUDSHelper.BarCodeCalibrateMS(barValue) == false)
                    {
                        //LogHelper.ShowMsg("序列号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("序列号写入失败");

                    }
                    Thread.Sleep(50);

                    if (PlayUDSHelper.VersionCalibateMS(par.MeasurementStandard) == false)
                    {
                        //LogHelper.ShowMsg("硬件号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("硬件号写入失败");

                    }





                }
                else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                {


                    if (PlayUDSHelper.BarCodeCalibrateVOYAH_H56C(barCode) == false)
                    {
                        //LogHelper.ShowMsg("序列号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("批次号写入失败");

                    }
                    Thread.Sleep(50);

                    if (PlayUDSHelper.VersionCalibateVOYAH_H56C(par.MeasurementStandard) == false)
                    {
                        //LogHelper.ShowMsg("硬件号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("硬件号写入失败");

                    }





                }
                
                else if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                {


                    if (PlayUDSHelper.BarCodeCalibrateXP(barCode) == false)
                    {
                        //LogHelper.ShowMsg("序列号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("批次号写入失败");

                    }

                }
                else
                {

                    if (ConfigHelper.UdsParameters.jiesuo != "GreatWall_C03")
                    {
                        if (PlayUDSHelper.BarCodeCalibrateLT(barCode) == false)
                        {
                            //LogHelper.ShowMsg("序列号写入失败");
                            AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                            throw new Exception("批次号写入失败");

                        }

                    }

                    Thread.Sleep(50);

                    if (PlayUDSHelper.VersionCalibateLT(par.MeasurementStandard) == false)
                    {
                        //LogHelper.ShowMsg("硬件号写入失败");
                        AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                        throw new Exception("硬件写入失败");

                    }


                }

                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {

                    CommonData.TestCount++;
                    return true;
                }


                DateTime end = DateTime.Now;
                var span = (end - start).TotalSeconds / 2;

                if (ConfigHelper.UdsParameters.jiesuo != "XP_12V" && ConfigHelper.UdsParameters.jiesuo != "AVATR_E15")
                {
                    if (par != null)
                    {

                        TestResult model = new TestResult();
                        model.StartTime = start;
                        model.EndTime = start.AddSeconds(span);
                        model.ChildrenItem = $"硬件标定{par.MeasurementStandard}";
                        model.Extend1 = "1";
                        model.QualifiedValue = par.MeasurementStandard;
                        model.MeasurementStandard = par.MeasurementStandard;
                        model.MeasureVal = par.MeasurementStandard;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        AddModelAction(model);
                    }
                }



                if ( ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A" || ConfigHelper.UdsParameters.jiesuo == "XP_12V" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR")
                {
                    par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("批次号"));
                    if (par != null)
                    {

                        TestResult model = new TestResult();
                        model.StartTime = start;
                        model.EndTime = start.AddSeconds(span);
                        model.ChildrenItem = $"批次号标定{barCode}";
                        model.Extend1 = "1";
                        model.QualifiedValue = barCode;
                        model.MeasurementStandard = barCode;
                        model.MeasureVal = barCode;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        AddModelAction(model);
                    }

                }
                else if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                {
                    if (par != null)
                    {
                        TestResult model = new TestResult();
                        model = new TestResult();
                        model.StartTime = start;
                        model.EndTime = start.AddSeconds(span);
                        model.ChildrenItem = $"批次号标定{barValue}";
                        model.Extend1 = "1";
                        model.QualifiedValue = barValue;
                        model.MeasurementStandard = barValue;
                        model.MeasureVal = barValue;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        AddModelAction(model);
                    }
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "JAC_X6")
                {
                    TestResult model = new TestResult();
                    model.StartTime = start;
                    model.EndTime = start.AddSeconds(span);
                    model.ChildrenItem = $"Soft_ID标定{ConfigHelper.UdsParameters.Soft_ID}";
                    model.Extend1 = "1";
                    model.QualifiedValue = ConfigHelper.UdsParameters.Soft_ID;
                    model.MeasurementStandard = ConfigHelper.UdsParameters.Soft_ID;
                    model.MeasureVal = ConfigHelper.UdsParameters.Soft_ID;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("批次号"));
                    if (par != null)
                    {

                        model = new TestResult();
                        model.StartTime = start;
                        model.EndTime = start.AddSeconds(span);
                        model.ChildrenItem = $"批次号标定{barValue}";
                        model.Extend1 = "1";
                        model.QualifiedValue = barValue;
                        model.MeasurementStandard = barValue;
                        model.MeasureVal = barValue;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        AddModelAction(model);
                    }

                }
                else
                {
                    if (ConfigHelper.UdsParameters.jiesuo != "GreatWall_C03")
                    {
                        par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("批次号"));
                        if (par != null)
                        {

                            TestResult model = new TestResult();
                            model.StartTime = start;
                            model.EndTime = start.AddSeconds(span);
                            model.ChildrenItem = $"批次号标定{barCode.Remove(6, 2).Insert(6, "0")}";
                            model.Extend1 = "1";
                            model.QualifiedValue = barCode.Remove(6, 2).Insert(6, "0");
                            model.MeasurementStandard = barCode.Remove(6, 2).Insert(6, "0");
                            model.MeasureVal = barCode.Remove(6, 2).Insert(6, "0");
                            model.Result = model.MeasureVal == model.QualifiedValue;
                            AddModelAction(model);
                        }

                    }
                }
            }

                

                
            
            CommonData.TestCount++;
            return true;
        }
    }
}
