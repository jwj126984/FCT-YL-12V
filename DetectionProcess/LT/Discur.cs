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
    public class Discur : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
         
            CommonMethod.Unlock();
             
          
            if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                throw new Exception("MOS闭合失败");
            }

            CommonData.Relay.SetRelayCard2(27, true);

            CommonData.N36200.SetVoltage(12.8);
            CommonData.N36200.SetCurrent(100);


            CommonData.N69206H.SetOutput(false);
            CommonMethod.Sleep(150);
            CommonData.N69206H.SetCCModeh();
            //CommonMethod.Sleep(100);
            CommonData.N69206H.SetOutput(true);

            CommonData.N69206H.SetCCModecurh(100);


            CommonData.N36200.SetOutput(true);

            CommonMethod.Sleep(5000);
            bool result = false;

            DateTime now = DateTime.Now;
         
            while (true)
            {
                var currStr = CommonData.N36200.GetMeasureCurrent();
                double cur = double.Parse(currStr);
                LogHelper.ShowMsg($"xm{cur}");
                if (cur > 90 && cur <110)
                {
                    CommonMethod.Sleep(2000);
                }
                //else if(cur > 1)
                //{
                //    CommonMethod.Sleep(2000);
                //}
                else
                {

                    break; 
                }

                if ((DateTime.Now - now).TotalMilliseconds > 30000)
                {
                    result =true;
                    break;
                }

            }
            DateTime endTime = DateTime.Now;
            var span = (endTime - now).TotalSeconds;
            var model = new TestResult();
            model.ChildrenItem = "放电测试";
            model.StartTime = endTime;
            model.EndTime = endTime.AddSeconds(span);
            model.MeasurementStandard = "100A";
            model.MeasureVal = "100A";
            model.MeasurementStandard = "";
            model.QualifiedValue = "";
            model.Result = result;
            
            AddModelAction(model);

          
           
       

            CommonData.N69206H.SetOutput(false);
            CommonData.Relay.SetRelayCard2(27, false);
            CommonData.N36200.SetOutput(false);
            CommonData.TestCount++;
            return true;
        }

    }
}
