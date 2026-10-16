using AllData;
using Common;
using Common.UDS;
using Model;
using Models_C.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    public class LBMSRead : DetectBase
    {
        static string BarCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
        static string BarCodeMonth = "123456789ABC";
        static string BarCodeDay = "123456789ABCDEFGHJKLMNPRSTVWXYZ";
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------LBMS读取------");
            var barCode = CommonData.CurrentBarCode;
            barCode = barCode.Substring(19);
            int barYear = int.Parse(barCode.Substring(0, 2));
            int barMonth = int.Parse(barCode.Substring(2, 2));
            int barDay = int.Parse(barCode.Substring(4, 2));
            var barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);
            var barValue1 = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) +"0"+ barCode.Substring(8, 4);
            var par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("软件"));
            if (par != null)
            {

                string s1 = null;
                string s2 = null;
                string s3 = null;
                string s4 = null;
                string s5 = null;
                string s6 = null;
                string s7 = null;
                if (ConfigHelper.UdsParameters.jiesuo == "SERES_F3")
                {
                    s2 = PlayUDSHelper.GetVersion(1).Trim();
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
                {
                    

                    PlayUDSHelper.BarCodeCalibrate(barValue);
                    

                    s1 = ZCData.Version;
                    s2 = ZCData.VersionHard;
                    s3 = PlayUDSHelper.GetVersionV32(0).Trim();
                    s4 = PlayUDSHelper.GetVersionV32(1).Trim();
                    s5 = PlayUDSHelper.GetVersionV32(2).Trim();
                    s6 = PlayUDSHelper.GetVersionV32(3).Trim();
                    s7 = PlayUDSHelper.GetVersionV32(4).Trim();
                }
                else
                {
                    s1 = PlayUDSHelper.GetVersionKH(1).Trim();
                    s2 = PlayUDSHelper.GetVersionKH(0).Trim(); 

                }
                    TestResult model = new TestResult();


                if (ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
                {

                    model = new TestResult();
                    model.ChildrenItem = "软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s1;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "硬件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "零件号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.LJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s3;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "ECU流水编号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = barValue1;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s4;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "BT版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.BOOT_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s5;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "序号代码";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.XH_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s6;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);

                    model = new TestResult();
                    model.ChildrenItem = "客户软件版本号";
                    model.StartTime = DateTime.Now;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.KHRJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s7;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    AddModelAction(model);
                }
                else
                {
                    model.ChildrenItem = "客户软件版本号";
                    model.StartTime = DateTime.Now;

                    //var s2 = ZCData.Version;
                    model.EndTime = DateTime.Now;
                    model.QualifiedValue = ConfigHelper.UdsParameters.RJ_DATA;
                    model.MeasurementStandard = "";
                    model.MeasureVal = s2;
                    model.Result = model.MeasureVal == model.QualifiedValue;
                    //model.Result = true;
                    AddModelAction(model);


                    if (ConfigHelper.UdsParameters.jiesuo != "SERES_F3")
                    {
                        model = new TestResult();
                        model.ChildrenItem = "客户硬件版本号";
                        model.StartTime = DateTime.Now;
                        //    var s1 = ZCData.VersionHard;
                        model.EndTime = DateTime.Now;
                        model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
                        model.MeasurementStandard = "";
                        model.MeasureVal = s1;
                        model.Result = model.MeasureVal == model.QualifiedValue;
                        //model.Result = true;
                        AddModelAction(model);
                    }
                }
                    

                


            }
            else
            {
                TestFail?.Invoke("软件版本参数未配置软件版本参数未配置");
                
            }
            CommonData.TestCount++;
            return true;

        }
    }
}
