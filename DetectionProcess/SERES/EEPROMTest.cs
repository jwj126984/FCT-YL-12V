using AllData;
using Common;
using Common.Can;
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
    public class EEPROMTest : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------EEPROM测试------");

            var par = pars.FirstOrDefault();
            if (par == null)
            {
                //LogHelper.ShowMsg("EEPROM测试参数未配置");
                AddModelAction(new TestResult() { ChildrenItem = "EEPROM测试失败" });
                throw new Exception("EEPROM测试参数未配置");
            }

            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;

            bool b=false;
          
            AddModelAction(new TestResult() { ChildrenItem = "复位", Extend1 = "-1" });
         
            if (ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
            {

                var temp1 = PlayUDSHelper.GETEEPROM();

                List<byte> temp = new List<byte>();
                if (temp1.Count >8)
                {
                    CommonMethod.Sleep(1000);
                    byte[] bytes = new byte[] { 0x04, temp1[5], temp1[6], temp1[7], 0x01 };

                    temp = PlayUDSHelper.SETEEPROM(bytes);
                }
                else
                {
                    LogHelper.ShowMsg("未获取故障");
                    temp[16] = 0;
                }



                    model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";

                model.MeasureVal = temp[16].ToString();

                model.Result = temp[16] != 0;
                model.QualifiedValue = ">0";
                //model.MeasureVal = "0";
                AddModelAction(model);

            }
            else
            {
              
                var temp = PlayUDSHelper.EEPROM();
                if (temp == null)
                {
                    LogHelper.ShowMsg("读取快照失败");
                    return false;
                }
                model.EndTime = DateTime.Now;
                model.ChildrenItem = par.ChildrenItem;
                model.Extend1 = "1";
                model.MeasureVal = "";
                if (temp != null)
                {
                    foreach (var item in temp)
                    {
                        model.MeasureVal += item.ToString("X2") + " ";
                    }
                }
                model.Result = true;
                model.QualifiedValue = "0";
                //model.MeasureVal = "0";
                AddModelAction(model);
            }

            

            NewCanHelper.WakeOrSleep(true);
            CommonMethod.Sleep(200);
            CommonMethod.Unlock();
            CommonData.TestCount++;
            return true;
        }
    }
}
