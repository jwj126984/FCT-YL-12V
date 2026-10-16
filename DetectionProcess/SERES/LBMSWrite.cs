using Common.Can;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using Model;
using Common.UDS;
using System.Threading;
using AllData;

namespace DetectionProcess.SERES
{
    public class LBMSWrite:DetectBase
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
                AddModelAction(new TestResult() { ChildrenItem = "LBMS读取失败" });
                throw new Exception("版本号参数未配置");
            }
            if (PlayUDSHelper.Reset() == false)
            {
                //LogHelper.ShowMsg("复位失败");
                AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
                throw new Exception($"复位失败");
            }
            DateTime start = DateTime.Now;
            var barCode = CommonData.CurrentBarCode;
            barCode = barCode.Substring(19);
            string barValue;
            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y" )
            {
                string barYear = barCode.Substring(0, 2);
                string barMonth = barCode.Substring(2, 2);
                string barDay = barCode.Substring(4, 2);
                barValue = barYear + barMonth + barDay + barCode.Substring(7, 5);

                
            }
            else if(ConfigHelper.UdsParameters.jiesuo == "CHERY_EH3" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            {
                string barYear = barCode.Substring(0, 2);
                string barMonth = barCode.Substring(2, 2);
                string barDay = barCode.Substring(4, 2);
                barValue = barYear + barMonth + barDay;
            }
            else
            {
                int barYear = int.Parse(barCode.Substring(0, 2));
                int barMonth = int.Parse(barCode.Substring(2, 2));
                int barDay = int.Parse(barCode.Substring(4, 2));
                barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);

            }

            //if (PlayUDSHelper.BarCodeCalibrate(barValue) == false)
            //{
            //    //LogHelper.ShowMsg("序列号写入失败");
            //    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
            //    throw new Exception("序列号写入失败");

            //}
            //Thread.Sleep(50);

            //if (PlayUDSHelper.VersionCalibate(par.MeasurementStandard) == false)
            //{
            //    //LogHelper.ShowMsg("硬件号写入失败");
            //    AddModelAction(new TestResult() { ChildrenItem = "LBMS写入失败" });
            //    throw new Exception("硬件号写入失败");

            //}
            var s1 = PlayUDSHelper.GetVersion(0).Trim();
            //var s2 = PlayUDSHelper.GetVersion(1).Trim();
            var s3 = PlayUDSHelper.GetVersion(2).Trim();
            DateTime end = DateTime.Now;
            var span = (end - start).TotalSeconds / 3;

            
            if (par != null) {

                TestResult model = new TestResult();
                model.StartTime = start;
                model.EndTime= start.AddSeconds(span);
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.QualifiedValue = par.MeasurementStandard;
                model.MeasurementStandard = par.MeasurementStandard;
                model.MeasureVal = s1;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);
            }
            par = pars.FirstOrDefault(m => m.ChildrenItem.Contains("序列号"));
            if (par != null)
            {

                TestResult model = new TestResult();
                model.StartTime = start;
                model.EndTime = start.AddSeconds(span);
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.QualifiedValue = barValue;
                model.MeasurementStandard= barValue;
                model.MeasureVal = s3;
                model.Result = model.MeasureVal == model.QualifiedValue;
                AddModelAction(model);
            }
            CommonData.TestCount++;
            return true;
        }
    }
}
