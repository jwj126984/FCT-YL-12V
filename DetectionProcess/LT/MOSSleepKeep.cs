﻿﻿﻿﻿﻿﻿﻿using Common;
using Common.Can;
using Common.UDS;
using Model;
using AllData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.LT
{
    /// <summary>
    /// MOS休眠维持
    /// </summary>
    public class MOSSleepKeep : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------MOS维持参数------");

       
            NewCanHelper.MOSControlVoltageEnable(true, false, false, gd: false);



            var par = pars.FirstOrDefault();
            if (par == null)
            {
                LogHelper.ShowMsg("MOS 维持参数未配置");
                throw new Exception("MOS 维持参数未配置");
            }

            
            CommonMethod.SetN8331(13.2);
            CommonMethod.Sleep(300);
           
            if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1")
            {

            }
            else
            {
                CommonMethod.Unlock();
                PlayUDSHelper.SetSOC(100);
                NewCanHelper.WakeOrSleep(true);

                CommonMethod.Sleep(6000);
            }
            if (!NewCanHelper.WakeOrSleep(false))
            {
                LogHelper.ShowMsg("唤醒失败");
                throw new Exception("唤醒失败");
            }


            double num = (double)par.MeasurementStandard.GetNumber();
            TestResult model = new TestResult();
            model.StartTime = DateTime.Now;

            //         #region 唤醒处理
            //         NewCanHelper.WakeOrSleep(true);
            //         CommonMethod.StartSendAwaken();
            CommonMethod.Unlock();
            ////         CommonMethod.StartSendUnlock();
            //         #endregion

            if (!PlayUDSHelper.MOSControl(MOSEnum.闭合))
            {
                LogHelper.ShowMsg("MOS闭合失败");
                throw new Exception("MOS闭合失败");
            }
            
           
            CommonMethod.StopSendAwaken();
            CommonMethod.StopSendUnlock();
          
            if (PlayUDSHelper.UdsSleep(1) == null)
            {
                LogHelper.ShowMsg("休眠失败");
                throw new Exception("休眠失败");
            }

            var dl = CommonMethod.GetSleep1CurrentFromPower();
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("获取电流失败");
                throw new Exception("获取电流失败");
            }

            NewCanHelper.MOSControlVoltageEnable(true, false, true, false, false);
            CommonMethod.Sleep(200);
            var it6122bMesV = CommonData.NXI_6400.GetVResult();
            //   CommonData.IT6322.SetOutput(false);
            if (it6122bMesV == null)
            {
                AddModelAction(new TestResult() { ChildrenItem = "电压测试失败", Result = false });
                throw new Exception($"总电压测试设置NXI_6400 {par.QualifiedValue.Value}V失败");
            }

            NewCanHelper.MOSControlVoltageEnable(true, false, true, true,false);
            CommonMethod.Sleep(200);
            var mosDy = CommonData.NXI_6400.GetVResult();
            DateTime now = DateTime.Now;
           
            //mosDy = mosDy - 0.8;

            //var powerVot = double.Parse(CommonData.IT6122B.GetMEASVoltage());

            NewCanHelper.MOSControlVoltageEnable(true, false, false, false, false);
            #region 唤醒处理
            NewCanHelper.WakeOrSleep(true);
            CommonMethod.StartSendAwaken();
            dl = CommonMethod.GetWorkCurrentFromPower();
            if (dl.Item1 == false)
            {
                LogHelper.ShowMsg("唤醒失败");
                return false;
            }
            
            //       CommonMethod.StartSendUnlock();
            #endregion

            model.EndTime = DateTime.Now;
            model.QualifiedValue = it6122bMesV.ToString("0.000");
            model.ChildrenItem = par.ChildrenItem;
            model.MeasurementStandard = par.MeasurementStandard;
            model.MeasureVal = mosDy.ToString("0.000");
            model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");

            model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
            if (model.Result == true)
            {
                //Random random = new Random();
                //double d = random.Next(0, 500) / 10000.0;
                //if (mosDy < powerVot)
                //{
                //    mosDy += d;
                //}
                //else
                //{
                //    mosDy -= d;
                //}
                model.MeasureVal = mosDy.ToString("0.000");
                model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
            }
            AddModelAction(model);
            NewCanHelper.MOSControlVoltageEnable(true, false);
            CommonMethod.SetN8331(13.2);
            CommonMethod.Unlock();
            CommonMethod.StartSendUnlock();
 
            CommonData.TestCount++;
            return true;
        }
    }
}
