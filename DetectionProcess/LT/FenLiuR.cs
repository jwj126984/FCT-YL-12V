using Common.Can;
using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllData;
using Common.SerialportHelper;

namespace DetectionProcess.LT
{
    public class FenLiuR : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            CommonData.N69206H.SetOutput(false);
            CommonMethod.Sleep(150);
            CommonData.N69206H.SetCCModeh();
            //CommonMethod.Sleep(100);
            CommonData.N69206H.SetOutput(true);
            CommonMethod.Sleep(100);
            NewCanHelper.CtrolTotalCurrentCollection(false);
            M3905D.SetVoltage(10);
            M3905D.SetOUTPut(1);

            CommonData.M3905D.SetCurrentAndStable(300, 1, 1);

            

            CommonData.Relay.SetRelayCard1(13, true);
            CommonMethod.Sleep(100);
            var Volt = CommonData.NXI_6400.GetVResult();

            var model = new TestResult();
            model.ChildrenItem = "分流器阻抗测量";
            model.MeasurementStandard = "0 - 0.1V";
            model.QualifiedValue = "";
            model.MeasureVal = Volt.ToString("0.000");
            model.DifferenceVal = "";
            if (Volt > 0 && Volt < 0.1)
            {
                model.Result = true;
            }
            else
            {
                model.Result = false;
            }
            AddModelAction(model);

            M3905D.SetOUTPut(0);
           
            NewCanHelper.CloseTotalCurrentCollection();

            CommonData.N69206H.SetOutput(false);
            CommonData.Relay.SetRelayCard1(13, false);
            CommonData.TestCount++;
            return true;
        }

    }
}
