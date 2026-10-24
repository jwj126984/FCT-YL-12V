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
    public class Cur0A : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            NewCanHelper.CloseTotalCurrentCollection();

            bool result = false;

            DateTime now = DateTime.Now;

            var curr = ZCData.GetDL;

            var model = new TestResult();
            model.ChildrenItem = "0A电流读取";
            model.MeasurementStandard = "[-0.5,0.5]";
            model.QualifiedValue = "";
            model.MeasureVal = $"{curr}";
            if(Math.Abs(curr) < 0.5)
            {
                model.Result = true;
            }
            else
            {
                model.Result = false;
            }
         
            
            AddModelAction(model);

          
           
       

            
            CommonData.TestCount++;
            return true;
        }

    }
}
