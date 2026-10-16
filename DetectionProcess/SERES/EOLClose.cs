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
    public class EOLClose : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------关闭EOL模式------");
            var model = new TestResult();
           
            PlayUDSHelper.CloseEOL();

           

            

            PlayUDSHelper.Reset();
            CommonMethod.Sleep(5000);

            CommonMethod.Unlock();
            PlayUDSHelper.MOSControl(MOSEnum.断开);


          //  CommonMethod.Sleep(2000);
            var MOSC = ZCData.MOSC;
            var MOSD = ZCData.MOSD;
            LogHelper.ShowMsg($"充电ms：{MOSC}，放电ms：{MOSD}");
            if (MOSC != 2 || MOSC != 2)
            {
                throw new Exception("EOL关闭MOS未断开");
            }

            PlayUDSHelper.OpenEOL();
          //  CommonMethod.Sleep(2000);
            MOSC = ZCData.MOSC;
             MOSD = ZCData.MOSD;
            LogHelper.ShowMsg($"充电ms：{MOSC}，放电ms：{MOSD}");
            if (MOSC != 2 || MOSC != 2)
            {
                throw new Exception("EOL关闭MOS未断开");
            }
            CommonData.TestCount++;
            return true;
        }
    }
}
