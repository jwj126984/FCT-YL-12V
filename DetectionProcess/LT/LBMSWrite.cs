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

namespace DetectionProcess.LT
{
    public class LBMSWrite : DetectBase
    {
        static string BarCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
        static string BarCodeMonth = "123456789ABC";
        static string BarCodeDay = "123456789ABCDEFGHJKLMNPRSTVWXYZ";
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------LBMS写入------");

            

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
                string s1 = null;
                string s2 = null;
                string s3 = null;
                string s4 = null;
                string s5 = null;
                string s6 = null;

                s1 = PlayUDSHelper.GetYRGQ(0);
                s2 = PlayUDSHelper.GetYRGQ(1);
                s3 = PlayUDSHelper.GetYRGQ(2);
                s4 = PlayUDSHelper.GetYRGQ(3);
                s5 = PlayUDSHelper.GetYRGQ(4).Trim();
                s6 = PlayUDSHelper.GetYRGQ(5).Trim();

                TestResult model = new TestResult();
                model.ChildrenItem = "指纹信息";
                model.QualifiedValue = "FF000101FFFFFFFFFFFF";
              
                model.MeasureVal = s1;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "预编程次数";
                model.QualifiedValue = "0000";
            
                model.MeasureVal = s2;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "编程次数";
                model.QualifiedValue = "0000";
             
                model.MeasureVal = s3;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "刷写成功时间";
                model.QualifiedValue = "14000101";
            
                model.MeasureVal = s4;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "BOOT版本";
                model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                model.MeasureVal = s5;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "BMS条码";
                model.QualifiedValue = barCode;
                model.MeasureVal = s6;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

            }
            else if(ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                barCode = barCode.Substring(19);

                var barValue ="20" + barCode.Substring(0, 6) +"0000"+ barCode.Substring(8, 4);
                string s1 = null;
                string s2 = null;
                string s3 = null;
                s1 = PlayUDSHelper.GetVersionC1046(2).Trim();
                s2 = PlayUDSHelper.GetVersionC1046(3).Trim();
                s3 = PlayUDSHelper.GetVersionC1046(4).Trim();

                TestResult model = new TestResult();
                model.ChildrenItem = "LBMS生产序列号";
                model.StartTime = DateTime.Now;
                model.EndTime = DateTime.Now;
                model.QualifiedValue = barValue;
                model.MeasurementStandard = "";
                model.MeasureVal = s1;
                model.Result = true;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "LBMS硬件版本号";
                model.StartTime = DateTime.Now;
                model.EndTime = DateTime.Now;
                model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                model.MeasurementStandard = "";
                model.MeasureVal = s2;
                model.Result = true;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "OEM硬件基线号";
                model.StartTime = DateTime.Now;
                model.EndTime = DateTime.Now;
                model.QualifiedValue = ConfigHelper.UdsParameters.KHYJ_DATA;
                model.MeasurementStandard = "";
                model.MeasureVal = s3;
                model.Result = true;
                AddModelAction(model);



            }
            else if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
            {
                string s1 = "";
                string s2 = "";
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
                var barValue = barYear + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + "0" + barCode.Substring(8, 4) + flag;

                s1 = PlayUDSHelper.GetYJRJ(3).Trim();
                s2 = PlayUDSHelper.GetHardVersionMS().Trim();

                TestResult model = new TestResult();

                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasurementStandard = par.MeasurementStandard;
                model.MeasureVal = s2;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "批次号";
                model.Extend1 = "1";
                model.QualifiedValue = barValue;
                model.MeasurementStandard = barValue;
                model.MeasureVal = s1;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {
                string s1 = "";
                string s2 = "";

                barCode = barCode.Substring(19);

                string barValue = null;
                int barYear = int.Parse(barCode.Substring(0, 2));
                int barMonth = int.Parse(barCode.Substring(2, 2));
                int barDay = int.Parse(barCode.Substring(4, 2));
                barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);

                s1 = PlayUDSHelper.GetVersion(0).Trim();
                s2 = PlayUDSHelper.GetYJRJ(3);

                TestResult model = new TestResult();

                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasurementStandard = par.MeasurementStandard;
                model.MeasureVal = s1;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "批次号";
                model.Extend1 = "1";
                model.QualifiedValue = barValue;
                model.MeasurementStandard = barValue;
                model.MeasureVal = s2;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
            {

                string s1 = "";
                string s2 = "";

                barCode = barCode.Substring(19);
              
                s1 = PlayUDSHelper.GetBarCode().Trim();
                s2 = PlayUDSHelper.GetHardVersion().Trim();

                TestResult model = new TestResult();

                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasurementStandard = par.MeasurementStandard;
                model.MeasureVal = s2;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "批次号";
                model.Extend1 = "1";
                model.QualifiedValue = barCode;
                model.MeasurementStandard = barCode;
                model.MeasureVal = s1;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);
            }
            else
            {
                barCode = barCode.Substring(19);

                string barValue = null;
                if (ConfigHelper.UdsParameters.jiesuo != "HQ_E702")
                {
                    int barYear = int.Parse(barCode.Substring(0, 2));
                    int barMonth = int.Parse(barCode.Substring(2, 2));
                    int barDay = int.Parse(barCode.Substring(4, 2));
                    barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);
                }


                string s1 = "";
                string s2 = "";
                //var s2 = PlayUDSHelper.GetVersion(1).Trim();
                string s3 = "";
                Thread.Sleep(500);
                if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR")
                {
                    if (ConfigHelper.UdsParameters.jiesuo != "AVATR_E15")
                    {
                        s1 = PlayUDSHelper.GetYJRJ(1);
                    }
                    s3 = PlayUDSHelper.GetYJRJ(3);
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C")
                {
                    barCode = CommonData.CurrentBarCode.Substring(11);
                    s1 = PlayUDSHelper.GetVersionPK(3).Trim();

                    s3 = PlayUDSHelper.GetVersionPK(2).Trim();


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                {
                    barCode = CommonData.CurrentBarCode.Substring(11);
                    barCode = barCode.Substring(8, 6) + barCode.Substring(16, 4);
                    s1 = PlayUDSHelper.GetVersionPK(3).Trim();

                    s3 = PlayUDSHelper.GetVersionPK(4).Trim();


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702")
                {
                    s1 = PlayUDSHelper.GetVersion(0).Trim();


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
                {
                    s1 = PlayUDSHelper.GetVersion(0).Trim();


                }
                else if (ConfigHelper.UdsParameters.jiesuo == "JAC_X6")
                {

                    s1 = PlayUDSHelper.GetYRJH(1).Trim();

                    s2 = PlayUDSHelper.GetYRJH(0).Trim();
                    s3 = PlayUDSHelper.GetYRJH(2);
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {

                    s1 = PlayUDSHelper.GetVersion(0).Trim();

                    s2 = PlayUDSHelper.GetVersion(3).Trim();
                    s3 = PlayUDSHelper.GetYJRJ(3).Trim();
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                {
                    s1 = ZCData.GetHardVersionXP;

                    s3 = PlayUDSHelper.GetBarCode().Trim();
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                {
                    barCode = barCode.Substring(0, 6) + barCode.Substring(8, 4);
                    s3 = PlayUDSHelper.GetYJRJ(3).Trim();
                    s1 = PlayUDSHelper.GetHardVersionMS().Trim();
                }
                
                else
                {
                    barCode = barCode.Remove(6, 2).Insert(6, "0");
                    if (ConfigHelper.UdsParameters.jiesuo != "GreatWall_C03")
                    {

                        s3 = PlayUDSHelper.GetBarCode().Trim();
                    }

                    s1 = PlayUDSHelper.GetHardVersion().Trim();

                }

                DateTime end = DateTime.Now;
                var span = (end - start).TotalSeconds / 3;

                if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702")
                {
                    if (par != null)
                    {

                        TestResult model = new TestResult();
                        model.StartTime = start;
                        model.EndTime = start.AddSeconds(span);
                        model.ChildrenItem = par.ChildrenItem;
                        model.Extend1 = "1";
                        model.QualifiedValue = par.MeasurementStandard;
                        model.MeasurementStandard = par.MeasurementStandard;
                        model.MeasureVal = s1;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        AddModelAction(model);


                    }
                }
                else
                {

                    if (par != null && ConfigHelper.UdsParameters.jiesuo != "AVATR_E15")
                    {

                        TestResult model = new TestResult();
                        model.StartTime = start;
                        model.EndTime = start.AddSeconds(span);
                        model.ChildrenItem = par.ChildrenItem;
                        model.Extend1 = "1";
                        model.QualifiedValue = par.MeasurementStandard;
                        model.MeasurementStandard = par.MeasurementStandard;
                        model.MeasureVal = s1;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        AddModelAction(model);
                    }



                    if (ConfigHelper.UdsParameters.jiesuo != "GreatWall_C03")
                    {
                        if (ConfigHelper.UdsParameters.jiesuo == "JAC_X6")
                        {
                            TestResult model = new TestResult();
                            model.StartTime = start;
                            model.EndTime = start.AddSeconds(span);
                            model.ChildrenItem = "软件版本";
                            model.Extend1 = "1";
                            model.QualifiedValue = ConfigHelper.UdsParameters.Soft_ID;
                            model.MeasurementStandard = ConfigHelper.UdsParameters.Soft_ID;
                            model.MeasureVal = s2;
                            model.Result = model.MeasureVal == model.QualifiedValue;
                            AddModelAction(model);

                            par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("批次号"));
                            if (par != null)
                            {

                                model = new TestResult();
                                model.StartTime = start;
                                model.EndTime = start.AddSeconds(span);
                                model.ChildrenItem = par.ChildrenItem;
                                model.Extend1 = "1";
                                model.QualifiedValue = barValue;
                                model.MeasurementStandard = barValue;
                                model.MeasureVal = s3;
                                model.Result = model.MeasureVal == model.QualifiedValue;
                                AddModelAction(model);
                            }

                        }
                        else
                        {
                            par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("批次号"));
                            if (par != null)
                            {

                                TestResult model = new TestResult();
                                model.StartTime = start;
                                model.EndTime = start.AddSeconds(span);
                                model.ChildrenItem = par.ChildrenItem;
                                model.Extend1 = "1";
                                model.QualifiedValue = barCode;
                                model.MeasurementStandard = barCode;
                                model.MeasureVal = s3;
                                model.Result = model.MeasureVal == model.QualifiedValue;
                                AddModelAction(model);
                            }

                            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                            {
                                TestResult model = new TestResult();
                                model.StartTime = start;
                                model.EndTime = start.AddSeconds(span);
                                model.ChildrenItem = "ECU生产日期";
                                model.Extend1 = "1";
                                model.QualifiedValue = "20" + barCode.Substring(0, 2) + barCode.Substring(2, 2) + barCode.Substring(4, 2);
                                model.MeasurementStandard = "20" + barCode.Substring(0, 2) + barCode.Substring(2, 2) + barCode.Substring(4, 2);
                                model.MeasureVal = s2;
                                model.Result = model.MeasureVal == model.QualifiedValue;
                                AddModelAction(model);
                            }
                        }


                    }
                }
            }

               

                
                
            CommonData.TestCount++;
            return true;
        }
    }
}
