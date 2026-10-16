using AllData;
using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    public class WakeCalibration : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            CommonData.N69206H.SetOutput(true);

            CommonData.N69206H.SetCCMode();
            M3905D.SetOUTPut(1);

            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                NewCanHelper.CtrolTotalCurrentCollection(true);
            }
            else
            {
                NewCanHelper.CtrolTotalCurrentCollection(false);
            }

            M3905D.SetVoltage(1);

            CommonData.M3905D.SetCurrentAndStable(1, 1, 0.03);



            var cur = PlayUDSHelper.GetcurCA();

            M3905D.SetVoltage(0);
            M3905D.SetOUTPut(0);
            NewCanHelper.CloseTotalCurrentCollection();
            CommonMethod.Sleep(200);
            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                PlayUDSHelper.GoToGC();
            }

            CommonMethod.Sleep(200);
            PlayUDSHelper.CurrCalibrate(cur);
            CommonMethod.Sleep(200);
            var cur2 = PlayUDSHelper.GetcurCA2();

            var model = new TestResult();
            model.ChildrenItem = "唤醒电流阈值标定和读取";
            model.MeasurementStandard = "";
            model.QualifiedValue = cur[2].ToString("x2") + cur[3].ToString("x2");
            model.MeasureVal = cur2[2].ToString("x2") + cur2[3].ToString("x2");
            model.DifferenceVal = "";
            if (cur[2] == cur2[2] && cur[3] == cur2[3])
            {
                model.Result = true;
            }
            else
            {
                model.Result = false;
            }
            AddModelAction(model);
            CommonData.N69206H.SetOutput(false);
            CommonData.TestCount++;
            return true;
        }
    }
}
