using AllData;
using Common;
using Common.Can;
using Common.SerialportHelper;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// MOS控制
    /// </summary>
    public class MOSControl : DetectBase
    {
        IEnumerable<TestParameter> _pars;
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------MOS控制测试------");
            _pars = pars;

            DateTime start = DateTime.Now;
          
            if(ConfigHelper.Cantype)
            {
                CommonMethod.Unlock();
              //  CommonMethod.StartSendUnlock();
            }
            
            if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                throw new Exception("MOS闭合失败");
            }
            CommonMethod.Sleep(1000);
            NewCanHelper.MOSControlVoltageEnable(true, false, true, true);
            var uex2 = CommonData.NXI_6400.GetVResult();
            var fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
            var fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
            if (fOpenChargeVoltage < 23)
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS闭合失败");
                }
                CommonMethod.Sleep(200);
                uex2 = CommonData.NXI_6400.GetVResult();
                fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
            }



            if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                throw new Exception("MOS断开失败");
            }
            CommonMethod.Sleep(1500);
            //（断开）充放电P+电压
            double uex1 = CommonData.NXI_6400.GetVResult();
            var fCloseChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
            var fCloseDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();

            

            if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                throw new Exception("MOS断开失败");
            }
            var end = DateTime.Now;
            var span = (start - end).TotalSeconds;

            Result("1", true, start, span, fOpenChargeVoltage);
            Result("2", true, start.AddSeconds(span * 1), span, fOpenDisChargeVoltage);
            Result("3", true, start.AddSeconds(span * 2), span, uex2);
            Result("4", true, start.AddSeconds(span * 3), span, fCloseChargeVoltage);
            Result("5", false, start.AddSeconds(span * 4), span, fCloseDisChargeVoltage);
            Result("6", false, start.AddSeconds(span * 5), span, uex1);
            CommonData.TestCount++;
            return true;
        }
        void Result(string eIndex, bool fh, DateTime now, double span, double m)
        {
            TestParameter par = _pars.FirstOrDefault(x => x.Extend1 == eIndex);

            TestResult model = new TestResult();
            model.ChildrenItem = par.ChildrenItem;
            model.StartTime = now;
            model.EndTime = now.AddSeconds(span);
            model.MeasurementStandard = par.MeasurementStandard;
            model.MeasureVal = m.ToString("0.000");
            model.QualifiedValue = par.MeasurementStandard;
          
            double num = (double)par.MeasurementStandard.GetNumber();
            //大于等于
            model.Result = m >= par.MinVal.Value && m <= par.MaxVal.Value;
            AddModelAction(model);
        }
    }
}
