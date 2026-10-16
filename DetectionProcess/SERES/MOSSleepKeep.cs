﻿﻿﻿﻿﻿using Common;
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
    /// <summary>
    /// MOS休眠维持
    /// </summary>
    public class MOSSleepKeep : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------MOS维持参数------");
            var par=pars.FirstOrDefault();
            if (par == null) {
                LogHelper.ShowMsg("MOS 维持参数未配置");
                throw new Exception("MOS 维持参数未配置");
            }
            NewCanHelper.MOSControlVoltageEnable(true, false, false);
            CommonMethod.SetN8331(13.2);
           
            
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
            if (!NewCanHelper.WakeOrSleep(false))
            {
                LogHelper.ShowMsg("唤醒失败");
                throw new Exception("唤醒失败");
            }
            CommonMethod.StopSendAwaken();
    //        CommonMethod.StopSendUnlock();
            
            if (PlayUDSHelper.UdsSleep(1)==null)
            {
                LogHelper.ShowMsg("休眠失败");
                throw new Exception("休眠失败");
            }
           
            var dl=CommonMethod.GetSleep1CurrentFromPower();
            if (dl.Item1==false)
            {
                LogHelper.ShowMsg("获取电流失败");
                throw new Exception("获取电流失败");
            }
            
           
            var it6122bMesV = CommonData.NXI_6400.GetVResult();
            //   CommonData.IT6322.SetOutput(false);
            if (it6122bMesV == null)
            {
                AddModelAction(new TestResult() { ChildrenItem = "电压测试失败", Result = false });
                throw new Exception($"总电压测试设置IT6322 {par.QualifiedValue.Value}V失败");
            }

            NewCanHelper.MOSControlVoltageEnable(true, false, false,true);
            var mosDy = CommonData.NXI_6400.GetVResult();
            DateTime now = DateTime.Now;
            while (true)
            {
                //客户提出需要判断产品是否处于休眠状态
                var currStr = CommonData.NXI_6400.GetCResult(0.1);
                double curr = currStr.Item1.FirstOrDefault() * 1000;
                if (curr > 5)
                {
                    break;
                }
                //mosDy = NewCanHelper.GetExternalTotalVoltage().Value + 0.06;
                mosDy = CommonData.NXI_6400.GetVResult();
                if (Math.Abs(it6122bMesV- mosDy)<= num)
                {
                    break;
                }
                if ((DateTime.Now - now).TotalSeconds > 5)
                {
                    break;
                }
                
            }
            //mosDy = mosDy - 0.8;

            //var powerVot = double.Parse(CommonData.IT6122B.GetMEASVoltage());
            

            #region 唤醒处理
            NewCanHelper.WakeOrSleep(true);
            CommonMethod.StartSendAwaken();
            dl = CommonMethod.GetWorkCurrentFromPower();
            if (dl.Item1==false)
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
            CommonMethod.SetN8331(12.8);
            CommonMethod.Unlock();
            CommonData.TestCount++;
            return true;
        }
    }
}
