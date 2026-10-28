using AllData;
using Common;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class ProduceRead : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------产品信息读取------");

        
            var par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("硬件"));
           
            par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("软件"));

            if (par != null)
            {

                if(ConfigHelper.UdsParameters.jiesuo == "T09_M")
                {
                    string s1 = null;
                    string s2 = null;
                    string s3 = null;
                    string s4 = null;
                    string s5 = null;

                    s1 = PlayUDSHelper.GetVersionGQ(0).Trim();
                    s2 = PlayUDSHelper.GetVersionGQ(1).Trim();
                    s3 = PlayUDSHelper.GetVersionGQ(2).Trim();
                    s4 = PlayUDSHelper.GetVersionGQ(3).Trim();
                    s5 = PlayUDSHelper.GetVersionGQ(4).Trim();

                    TestResult model = new TestResult();
                    model.ChildrenItem = "ECU软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "ECU硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "广汽ECU软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHRJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s3;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "广汽ECU硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHYJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s4;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "广汽零件号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.LJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s5;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
                {
                    string s1 = null;
                    string s2 = null;
                    string s3 = null;
                    string s4 = null;
                    string s5 = null;
                    string s6 = null;
                    s1 = PlayUDSHelper.GetVersionCN(0).Trim();
                    s2 = PlayUDSHelper.GetVersionCN(1).Trim();
                    s3 = PlayUDSHelper.GetVersionCN(2).Trim();
                    s4 = PlayUDSHelper.GetVersionCN(3).Trim();
                    s5 = PlayUDSHelper.GetVersionCN(4).Trim();
                    s6 = PlayUDSHelper.GetVersionCN(7).Trim();

                    TestResult model = new TestResult();
                    model.ChildrenItem = "OEM软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "OEM硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHRJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s3;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHYJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s4;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "自定义软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.LJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s5;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "Bootloader版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s6;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);
                }
                else if(ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
                {
                    string s2 = null;
                    string s1 = null;




                    TestResult model = new TestResult();
                    model.ChildrenItem = "软件APP版本号";
                    model.StartTime = DateTime.Now;


                    {

                        s1 = PlayUDSHelper.GetVersion1068(0).Trim();
                    }


                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    //model.Result = true;
                    AddModelAction(model);



                    model = new TestResult();
                    model.ChildrenItem = "软件BOOT版本号";
                    model.StartTime = DateTime.Now;


                    s2 = PlayUDSHelper.GetVersion1068(1).Trim();

                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    //model.Result = true;
                    AddModelAction(model);

                    CommonData.TestCount++;
                    return true;
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
                {
                    string s1 = null;
                    string s2 = null;
                    s1 = PlayUDSHelper.GetVersionC1046(0).Trim();
                    s2 = PlayUDSHelper.GetVersionC1046(1).Trim();

                    TestResult model = new TestResult();
                    model.ChildrenItem = "软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = true;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHRJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = true;
                    AddModelAction(model);

                 
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
                {
                    string s1 = null;
                    string s2 = null;
                    string s3 = null;
                    string s4 = null;
                    string s5 = null;
                    string s6 = null;

                    s1 = PlayUDSHelper.GetVersionHQ(0).Trim();
                    s2 = PlayUDSHelper.GetVersionHQ(1).Trim();
                    s3 = PlayUDSHelper.GetVersionHQ(2).Trim();
                    s4 = PlayUDSHelper.GetVersionHQ(3).Trim();
                    s5 = PlayUDSHelper.GetVersionHQ(4).Trim();
                    s6 = PlayUDSHelper.GetVersionHQ(5).Trim();

                    TestResult model = new TestResult();
                    model.ChildrenItem = "ECU软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "ECU硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHRJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s3;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHYJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s4;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商零件号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.LJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s5;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商BOOT版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s6;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
                {
                    string s1 = null;
                    string s2 = null;
                    string s3 = null;
                    string s4 = null;



                    s1 = PlayUDSHelper.GetVersionMS(0).Trim();
                    s2 = PlayUDSHelper.GetVersionMS(1).Trim();
                    s3 = PlayUDSHelper.GetVersionMS(2).Trim();
                    s4 = PlayUDSHelper.GetVersionHQ(0).TrimEnd('\0');



                    TestResult model = new TestResult();
                    model.ChildrenItem = "主机厂软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "主机厂硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "BOOT版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s3;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "供应商软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHYJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s4;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);



                }
                else
                {

                    TestResult model = new TestResult();

                    model.ChildrenItem = "客户软件版本号";
                    model.StartTime = DateTime.Now;
                    string s2 = null;
                    string s1 = null;
                    if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03")
                    {
                        s2 = PlayUDSHelper.GetVersion(1).Trim();
                        //       s1 = PlayUDSHelper.GetVersion(0).Trim();
                        //   s2 = ZCData.GetSoftVersion;

                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR")
                    {
                        s2 = PlayUDSHelper.GetYJRJ(0).Trim();
                        s1 = PlayUDSHelper.GetYJRJ(1).Trim();
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                    {
                        //  s2 = ZCData.GetSoftVersionXP;
                        s2 = PlayUDSHelper.GetVersionXP(0).Trim();
                        s1 = PlayUDSHelper.GetVersionXP(1).Trim();
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                    {
                        //   s2 = ZCData.GetSoftVersionMS;
                        s1 = PlayUDSHelper.GetVersionMS(1).Trim();
                        s2 = PlayUDSHelper.GetVersionMS(0).Trim();
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                    {
                        //  s2 = ZCData.GetSoftVersion;
                        s2 = PlayUDSHelper.GetVersion(1).Trim();
                        s1 = PlayUDSHelper.GetVersionKH(1).Trim();
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                    {
                        //  s2 = ZCData.GetSoftVersionXP;
                        s2 = PlayUDSHelper.GetVersionWL(0).Trim();
                        //     s1 = PlayUDSHelper.GetVersionWL(1).Trim();
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                    {

                        s2 = PlayUDSHelper.GetVersionPK(0).Trim();
                        s1 = PlayUDSHelper.GetVersionPK(1).Trim();
                    }
                    else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
                    {

                        s2 = PlayUDSHelper.GetVersionH97(0).Trim();
                        s1 = PlayUDSHelper.GetVersionH97(1).Trim();
                    }
                    else
                    {
                        s2 = PlayUDSHelper.GetVersion(1).Trim();
                        s1 = PlayUDSHelper.GetVersion(0).Trim();
                    }


                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    //model.Result = true;
                    AddModelAction(model);


                    if (ConfigHelper.UdsParameters.jiesuo != "GreatWall_C03" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "WL_F710S")
                    {
                        model = new TestResult();
                        if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                        {
                            model.ChildrenItem = "供应商软件版本号";
                        }
                        else
                        {
                            model.ChildrenItem = "客户硬件版本号";
                        }

                        model.StartTime = DateTime.Now;
                        // var s2 = PlayUDSHelper.GetVersion(1).Trim();
                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s1;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);
                    }


                    if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR"|| ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
                    {
                        model = new TestResult();
                        model.ChildrenItem = "Bootloder版本号";
                        model.StartTime = DateTime.Now;

                        string s3;

                        s3 = PlayUDSHelper.GetYJRJ(2);


                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s3;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);
                    }
                    if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
                    {
                        model = new TestResult();
                        model.ChildrenItem = "Bootloder版本号";
                        model.StartTime = DateTime.Now;

                        string s3;

                        s3 = PlayUDSHelper.GetVersionH97(2).Trim();


                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s3;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);
                    }

                    if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                    {
                        model = new TestResult();
                        model.ChildrenItem = "Bootloder版本号";
                        model.StartTime = DateTime.Now;

                        string s3;

                        s3 = PlayUDSHelper.GetYJRJ(4);


                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s3;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);
                    }
                    if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                    {
                        model = new TestResult();
                        model.ChildrenItem = "Bootloder版本号";
                        model.StartTime = DateTime.Now;

                        string s3;

                        s3 = PlayUDSHelper.GetVersionWL(1).Trim();


                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s3;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);

                        model = new TestResult();
                        model.ChildrenItem = "供应商软件版本号";
                        model.StartTime = DateTime.Now;

                        string s4;

                        s4 = PlayUDSHelper.GetVersionWL(2).Trim();


                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.KHYJ_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s4;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);
                    }

                }

                    
            }
            else
            {
                throw new Exception("软件版本参数未配置软件版本参数未配置");
            }
            
                


            //par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("零件号"));
            //if (par != null)
            //{

            //    TestResult model = new TestResult();
            //    model.ChildrenItem = par.ChildrenItem;
            //    model.StartTime = DateTime.Now;
            //    var ljh = PlayUDSHelper.GetVersion(2).Trim();
            //    model.EndTime = DateTime.Now;
            //    model.QualifiedValue = par.MeasurementStandard;
            //    model.MeasurementStandard = par.MeasurementStandard;
            //    model.MeasureVal = ljh;
            //    model.Result = model.MeasureVal == model.QualifiedValue;
            //    //model.Result = true;
            //    AddModelAction(model);
            //}
            //else
            //{
            //    throw new Exception("软件版本参数未配置软件版本参数未配置");
            //}
            CommonData.TestCount++;
            return true;

        }
    }
}
