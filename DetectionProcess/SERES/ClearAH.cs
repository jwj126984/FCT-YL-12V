using Common;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    public class ClearAH : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------清除累积充放电AH------");
            var model = new TestResult();
            //model.
            CommonMethod.Unlock();
            PlayUDSHelper.ClearAH();
            
               
          
            var ahc =  ZCData.AHC;

            var ahf = ZCData.AHF;

            model.ChildrenItem = "充电AH数";
            model.MeasurementStandard = "<2";
            model.MeasureVal = ahc.ToString();
            model.Result = ahc<2;
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
    }
}
