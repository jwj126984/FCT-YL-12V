    using AllData;
using Common;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class ClearAH : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            var model = new TestResult() { ChildrenItem = "清除累积充放电AH" };

            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                CommonData.TestCount++;
                return true;
            }
            if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                CommonMethod.Unlock();
                PlayUDSHelper.ClearAH();



                var ahc = ZCData.AHC;

                var ahf = ZCData.AHF;

                model.ChildrenItem = "充电AH数";
                model.MeasurementStandard = "<2";
                model.MeasureVal = ahc.ToString();
                model.Result = ahc < 2;
                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "放电AH数";
                model.MeasurementStandard = "<2";
                model.MeasureVal = ahf.ToString();
                model.Result = ahf < 2;
                AddModelAction(model);

                CommonData.TestCount++;
                return true;
            }
            LogHelper.Debug("------清除累积充放电AH------");
            //model.
            CommonMethod.Unlock();
            if (PlayUDSHelper.ClearAH())
            {
                model.Result = true;
                AddModelAction(model);
            }
            else
            {
                model.Result = false;
                AddModelAction(model);
                return false;
            }
            CommonData.TestCount++;
            return true;
        }
    }
}
