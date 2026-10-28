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

namespace DetectionProcess.LT
{
    public class ClearDTC : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
           

            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            {
                CommonMethod.StartClear();

            }
            if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
            {
                CommonMethod.StartClearXP();

            }
            LogHelper.Debug("------清除故障------");
            #region 设置电源
            CommonMethod.SetN8331(13.4);
            //var fOutVoltage = CommonData.IT6122B.SetVoltageAndStable(13.4);

            //var fRetData01 = CanHelper.ReadPower(true, false).Item1;
            //if (Math.Abs(fRetData01 - fOutVoltage) > CommonData.PowerVoltageOffset)
            //{
            //    LogHelper.ShowMsg("电源设置失败");
            //    return false;
            //}
            #endregion

            //   NewCanHelper.SetCellTemperature(45);

            var model = new TestResult() { ChildrenItem = "清除故障" };
            //model.
            CommonMethod.Unlock();
            PlayUDSHelper.ClearError();
            CommonMethod.StopSendUnlock();

            CommonMethod.Sleep(200);


            model.Result = false;
            if (PlayUDSHelper.ClearError())
            {
                model.Result = true;
                AddModelAction(model);
            }
            else
            {
                int i;
                for (i = 0; model.Result == false && i < 19; i++)
                {
                    CommonMethod.Sleep(1000);
                    if (PlayUDSHelper.ClearError())
                    {
                        model.Result = true;
                        AddModelAction(model);
                        continue;
                    }

                }
                if (i == 10)
                {
                    model.Result = false;
                    AddModelAction(model);
                    return false;
                }

            }

            //}
            //if (PlayUDSHelper.ClearError())
            //{
            //    model.Result = true;
            //    AddModelAction(model);
            //}
            //else
            //{
            //    CommonMethod.Sleep(3000);
            //    if (PlayUDSHelper.ClearError())
            //    {
            //        model.Result = true;
            //        AddModelAction(model);
            //    }
            //    else
            //    {
            //        model.Result = false;
            //        AddModelAction(model);
            //        return false;
            //    }
            //}
            CommonMethod.StartSendUnlock();

            CommonData.TestCount++;
            return true;
        }
    }
}
