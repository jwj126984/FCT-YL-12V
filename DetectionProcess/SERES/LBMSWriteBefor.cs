using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AllData;

namespace DetectionProcess.SERES
{
    internal class LBMSWriteBefor : DetectBase
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
            barCode = barCode.Substring(19);
            string barValue;
            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_EH3" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            {
                string barYear = barCode.Substring(0, 2);
                string barMonth = barCode.Substring(2, 2);
                string barDay = barCode.Substring(4, 2);
                barValue = barYear + barMonth + barDay +"0"+ barCode.Substring(8, 4);
                
                LogHelper.ShowMsg("序列号写入EH3");
                if (PlayUDSHelper.BarCodeCalibrateEH3(barValue) == false)
                {
                    //LogHelper.ShowMsg("序列号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("序列号写入失败");

                }

                Thread.Sleep(50);


                if (PlayUDSHelper.VersionCalibate(par.MeasurementStandard) == false)
                {
                    //LogHelper.ShowMsg("硬件号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("硬件号写入失败");

                }
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y")
            {
                string barYear = barCode.Substring(0, 2);
                string barMonth = barCode.Substring(2, 2);
                string barDay = barCode.Substring(4, 2);
                barValue = barYear + barMonth + barDay + barCode.Substring(7, 5);
                LogHelper.ShowMsg("序列号写入E0Y");
                if (PlayUDSHelper.BarCodeCalibrateE0Y(barValue) == false)
                {
                    LogHelper.ShowMsg("序列号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("序列号写入失败");

                }

                Thread.Sleep(50);


                if (PlayUDSHelper.VersionCalibateCan(par.MeasurementStandard) == false)
                {
                    //LogHelper.ShowMsg("硬件号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("硬件号写入失败");

                }
            }
            else
            {
                int barYear = int.Parse(barCode.Substring(0, 2));
                int barMonth = int.Parse(barCode.Substring(2, 2));
                int barDay = int.Parse(barCode.Substring(4, 2));
                barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);

                if (PlayUDSHelper.BarCodeCalibrate(barValue) == false)
                {
                    //LogHelper.ShowMsg("序列号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("序列号写入失败");

                }

                Thread.Sleep(50); 


                if (PlayUDSHelper.VersionCalibate(par.MeasurementStandard) == false)
                {
                    //LogHelper.ShowMsg("硬件号写入失败");
                    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                    throw new Exception("硬件号写入失败");

                }
            }


           

            
            
            DateTime end = DateTime.Now;
            var span = (end - start).TotalSeconds / 2;


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
            par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("序列号"));
            if (par != null)
            {

                TestResult model = new TestResult();
                model.StartTime = start;
                model.EndTime = start.AddSeconds(span);
                model.ChildrenItem = $"序列号标定{barValue}";
                model.Extend1 = "1";
                model.QualifiedValue = barValue;
                model.MeasurementStandard = barValue;
                model.MeasureVal = barValue;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);
            }
            CommonData.TestCount++;
            return true;
        }
    }
}
