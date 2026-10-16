using Common.UDS;
using Common;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Can;
using System.Threading;
using AllData;

namespace DetectionProcess.SERES
{
    public class VoltageTest : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------电压测试------");
            if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
            {
                LogHelper.ShowMsg("MOS闭合失败");
                return false;
            }
 //           CommonMethod.StartSendUnlock();
            //
        
          
            List<TestResult> result1 = new List<TestResult>();//单体电压结果
            List<TestResult> result2 = new List<TestResult>();//AFE 结果
            List<TestResult> result3 = new List<TestResult>();//MCU 结果
            List<TestResult> result4 = new List<TestResult>();//外总 结果

            foreach (var par in pars)
            {
                var measurementStandards = par.MeasurementStandard.Split('&');
                DateTime start1 = DateTime.Now;
                double v = par.QualifiedValue.Value;
                CommonData.IT6322.SetOutput(true);
                NewCanHelper.MOSControlVoltageEnable(true, true, true, false);
              

                if (ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
                {
                    CommonData.IT6322.SetVol(double.Parse(par.Extend1) * 4 + 0.03);
                }
                else
                {
                    CommonData.IT6322.SetVol(double.Parse(par.Extend1) * 4 + 0.05);
                }
                
               
                //设置电源
                LogHelper.Debug($"------------------------------------------- 设置 N8331 电压({(par.QualifiedValue.Value / 4.0).ToString()}V) --------------------------------------------");
                CommonData.N8331.SetVol4(double.Parse(par.Extend1));
                CommonMethod.Sleep(200);
                LogHelper.Debug("回读实际电压后续使用");
                var mesV = CommonMethod.GetN8331Vol4(double.Parse(par.Extend1));
                LogHelper.Debug($"实际电压值1-4(单位V):{string.Join(",", mesV)}");
                if ((double.Parse(par.Extend1) > 3) && (ConfigHelper.UdsParameters.jiesuo == "CHERY_EH3" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D"))
                {
                    CommonMethod.Sleep(10000);
                }
                var zcdys = ZCData.DYS;
                if(zcdys.Item1 == 0|| zcdys.Item2 == 0 || zcdys.Item3 == 0 || zcdys.Item4 == 0 )
                {
                    zcdys = ZCData.DYS;
                }
                //2025-4-15新增单体电压多判断一次，不合格再取一次，保证准确率
                {
                    double num = (double)measurementStandards[0].GetNumber();
                    if(Math.Abs(zcdys.Item1 - mesV[0]) <= num|| Math.Abs(zcdys.Item2 - mesV[1]) <= num|| Math.Abs(zcdys.Item3 - mesV[2]) <= num|| Math.Abs(zcdys.Item4 - mesV[3]) <= num)
                    {
                        
                        zcdys = ZCData.DYS;
                        if (zcdys.Item1 == 0 || zcdys.Item2 == 0 || zcdys.Item3 == 0 || zcdys.Item4 == 0)
                        {
                            zcdys = ZCData.DYS;
                        }
                    }

                }
                List<double> zcdysTemp = new List<double>() { zcdys.Item1, zcdys.Item2, zcdys.Item3, zcdys.Item4 };
                LogHelper.Debug($"产品获取单体电压（V）:{zcdys}");
                CommonData.N8331.SetVol4(par.QualifiedValue.Value/4);
                CommonData.IT6322.SetVol(par.QualifiedValue.Value);
                var span1 = (DateTime.Now - start1).TotalSeconds / 4;
                var start2 = DateTime.Now;

                NewCanHelper.MOSControlVoltageEnable(true, true, true, false);
                
                var afe = CommonData.NXI_6400.GetVResult();
                // var afe = NewCanHelper.GetInternalTotalVoltage();
                CommonData.IT6322.SetVol(par.QualifiedValue.Value);
                NewCanHelper.MOSControlVoltageEnable(true, true, true, true);

                var wdy = CommonData.NXI_6400.GetVResult();
                //var it6122bMesV = mesV.Sum();
                var nAndwZDY = ZCData.NAndWZDY;
                if (nAndwZDY.Item1 == 0 || nAndwZDY.Item2 == 0)
                {
                    nAndwZDY = ZCData.NAndWZDY;
                }
                var nz = nAndwZDY.Item1;
                var uex = nAndwZDY.Item2;
                double mcu = 0;
                //if (ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
                //{
                //    mcu = ZCData.MCUV;
                //}
                //else
                if (ConfigHelper.UdsParameters.jiesuo != "SERES_V32")
                {
                    mcu = PlayUDSHelper.GetChargeVoltage() * 0.1;
                }
               
                
              
                

                int readvoltnum = 0;
             
                

                

                var span2 = (DateTime.Now - start2).TotalSeconds / 3;

                TestResult model = new TestResult();

                #region 单体电压
                
                for (int i = 0; i < mesV.Count; i++)
                {
                    model = new TestResult();
                    model.StartTime = start1.AddSeconds(span1 * i); ;
                    model.EndTime = start1.AddSeconds(span1 * (i + 1));
                    model.ChildrenItem = $"电芯电压测试{i + 1}-{double.Parse(par.Extend1)}V";
                    model.MeasurementStandard = measurementStandards[0];
                    model.QualifiedValue = mesV[i].ToString("0.0000");
                    model.MeasureVal = zcdysTemp[i].ToString("0.0000");
                    model.DifferenceVal = (zcdysTemp[i] - mesV[i]).ToString("0.0000");
                    double num = (double)model.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                    result1.Add(model);
                }
                #endregion

                #region 总电压
                if (ConfigHelper.UdsParameters.jiesuo != "SERES_V32")
                {
                    model = new TestResult();


                    model.StartTime = start2;
                    model.EndTime = start2.AddSeconds(span2);
                    model.ChildrenItem = $"总电压测试-MCU内总{par.QualifiedValue}V";
                    model.MeasurementStandard = measurementStandards[1];
                    model.QualifiedValue = afe.ToString("0.000");
                    model.MeasureVal = mcu.ToString("0.000");
                    model.DifferenceVal = (mcu - afe).ToString("0.000");
                    double num = (double)model.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs(mcu - afe) <= num;
                    result3.Add(model);
                }

                {
                    model = new TestResult();


                    model.StartTime = start2.AddSeconds(span2);
                    model.EndTime = start2.AddSeconds(span2 * 2);
                    model.ChildrenItem = $"总电压测试-AFE内总{par.QualifiedValue}V";
                    model.MeasurementStandard = measurementStandards[2];
                    model.QualifiedValue = afe.ToString("0.000");
                    model.MeasureVal = nz.ToString("0.000");
                    model.DifferenceVal = (nz - afe).ToString("0.000");
                    double num = (double)model.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs(nz - afe) <= num;
                    result2.Add(model);
                }

                {
                    model = new TestResult();
                    model.StartTime = start2.AddSeconds(span2 * 2);
                    model.EndTime = start2.AddSeconds(span2 * 3);
                    model.ChildrenItem = $"总电压测试-外总{par.QualifiedValue}V";
                    model.MeasurementStandard = measurementStandards[3];
                    model.QualifiedValue = wdy.ToString("0.000");
                    model.MeasureVal = uex.ToString("0.000");
                    model.DifferenceVal = (uex - wdy).ToString("0.000");
                    double num = (double)model.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs(wdy - uex) <= num;
                    result4.Add(model);
                }
                #endregion

            }

            foreach (var model in result1)
            {
                AddModelAction(model);
            }
            for (int i = 0; i < result2.Count; i++)
            {
                
                if (result2[i].Result == false)
                {
                    var result = PlayUDSHelper.GetBalancedAndAFE();
                    LogHelper.Debug("电压测试故障码：" + string.Join(" ", result.Select(m => m.ToString("X2"))));
                }
                if (result4[i].Result == false)
                {
                    var result = PlayUDSHelper.GetBalancedAndAFE();
                    LogHelper.Debug("电压测试故障码：" + string.Join(" ", result.Select(m => m.ToString("X2"))));
                }
                if (ConfigHelper.UdsParameters.jiesuo != "SERES_V32")
                {
                    AddModelAction(result3[i]);
                }
                    
                AddModelAction(result2[i]);
                AddModelAction(result4[i]);
            }
            NewCanHelper.MOSControlVoltageEnable(true, false, false);
          
            CommonData.IT6322.SetOutput(false);
            CommonData.IT6322.SetVol(13.7);
            CommonMethod.SetN8331(12.8);
            CommonMethod.Unlock();
            CommonData.TestCount++;
            return true;
        }
    }
}
