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
    public class MOSControl : DetectBase
    {
        IEnumerable<TestParameter> _pars;
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                LogHelper.Debug("------MOS控制测试------");
                _pars = pars;
                DateTime start = DateTime.Now;

                //CommonData.Relay.CloseAllRelays();

                CommonMethod.SetN8331(12.8);

                CommonMethod.Unlock();

                if (PlayUDSHelper.MOSControl2510(MOSEnum2510.BUCK断) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS闭合失败");
                }
               

                //CommonData.Relay.SetRelayStatus(1, true, 1);
                //CommonData.Relay.SetRelayStatus(3, true, 1);
                //CommonData.Relay.SetRelayStatus(5, true, 1);
                //CommonData.Relay.SetRelayStatus(6, true, 1);
                //CommonData.Relay.SetRelayStatus(17, true, 1);
                //CommonData.Relay.SetRelayStatus(18, true, 1);

                NewCanHelper.MOSControlVoltageEnable(true, false, false);

                CommonData.Relay.SetRelayCard2(1, true);
                CommonData.Relay.SetRelayCard2(4, true);
                CommonData.Relay.SetRelayCard2(5, true);

                CommonData.N69206H.SetCCMode();

                CommonData.N69206H.SetCCModecur(5);

                //    CommonData.IT6322.Selectindex(1);
                CommonMethod.Sleep(200);
                CommonData.IT6322.SetVol(3);
                CommonMethod.Sleep(50);
                CommonData.IT6322.SetCurr(5);
                CommonMethod.Sleep(50);
                CommonData.IT6322.SetOutput(true);
                CommonData.N69206H.SetOutput(true);


                CommonData.Relay.SetRelayCard1(10, true);
                CommonMethod.Sleep(2000);


                var fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                var fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();

                var wz = CommonData.NXI_6400.GetVResult();


                TestResult model = new TestResult();
                model.ChildrenItem = "充电MOS闭合正向 - G极电压";

                model.MeasurementStandard = "比供电电压大10V";
                model.MeasureVal = (fOpenChargeVoltage).ToString("0.000");
                model.QualifiedValue = "供电电压12.8V";
                model.DifferenceVal = $"{fOpenChargeVoltage - 12.8}";
                model.Result = fOpenChargeVoltage - 12.8 > 10;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "放电MOS闭合正向 - G极电压";

                model.MeasurementStandard = "比供电电压大10V";
                model.MeasureVal = (fOpenDisChargeVoltage).ToString("0.000");
                model.QualifiedValue = "供电电压12.8V";
                model.DifferenceVal = $"{fOpenChargeVoltage - 12.8}";
                model.Result = fOpenDisChargeVoltage - 12.8 > 10;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "闭合正向 - P+B+压差";

                model.MeasurementStandard = "0 - 0.01 V";
                model.MeasureVal = wz.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = wz <= 0.01 && wz > 0;

                AddModelAction(model);


                CommonMethod.Unlock();
                if (PlayUDSHelper.MOSControl2510(MOSEnum2510.全断) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS闭合失败");
                }
                CommonMethod.Sleep(2000);
                fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();

                wz = CommonData.NXI_6400.GetVResult();

                model = new TestResult();
                model.ChildrenItem = "充电MOS断开正向 - G极电压";

                model.MeasurementStandard = "0-1V";
                model.MeasureVal = fOpenChargeVoltage.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = fOpenChargeVoltage >= 0 && fOpenChargeVoltage <= 1;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "放电MOS断开正向 - G极电压";

                model.MeasurementStandard = "0-1V";
                model.MeasureVal = fOpenDisChargeVoltage.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = fOpenDisChargeVoltage >= 0 && fOpenDisChargeVoltage <= 1;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "断开正向 - P+B+压差";

                model.MeasurementStandard = "2.5 - 3 V";
                model.MeasureVal = wz.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = wz >= 2.5 && wz <= 3;

                AddModelAction(model);


                CommonData.IT6322.SetOutput(false);

                CommonData.Relay.SetRelayCard2(1, false);
                CommonData.Relay.SetRelayCard2(4, false);

                //CommonData.Relay.SetRelayStatus(3, false, 1);
                //CommonData.Relay.SetRelayStatus(5, false, 1);
                //CommonData.Relay.SetRelayStatus(2, true, 1);
                //CommonData.Relay.SetRelayStatus(4, true, 1);
                CommonData.Relay.SetRelayCard2(2, true);
                CommonData.Relay.SetRelayCard2(3, true);

                CommonData.IT6322.SetOutput(true);

                if (PlayUDSHelper.MOSControl2510(MOSEnum2510.BUCK断) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS闭合失败");
                }

                CommonMethod.Sleep(2000);

                fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();

                wz = CommonData.NXI_6400.GetVResult();


                model = new TestResult();
                model.ChildrenItem = "充电MOS闭合反向 - G极电压";

                model.MeasurementStandard = "比供电电压大10V";
                model.MeasureVal = (fOpenChargeVoltage).ToString("0.000");
                model.QualifiedValue = "供电电压12.8V";
                model.DifferenceVal = $"{fOpenChargeVoltage - 12.8}";
                model.Result = fOpenChargeVoltage - 12.8 > 10;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "放电MOS闭合反向 - G极电压";

                model.MeasurementStandard = "比供电电压大10V";
                model.MeasureVal = (fOpenDisChargeVoltage).ToString("0.000");
                model.QualifiedValue = "供电电压12.8V";
                model.DifferenceVal = $"{fOpenChargeVoltage - 12.8}";
                model.Result = (fOpenDisChargeVoltage - 12.8) > 10;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "MOS闭合反向 - P+B+压差";

                model.MeasurementStandard = "(-0.01) - 0 V";
                model.MeasureVal = wz.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = wz >= -0.01 && wz < 0;

                AddModelAction(model);


                CommonMethod.Unlock();
                if (PlayUDSHelper.MOSControl2510(MOSEnum2510.全断) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS闭合失败");
                }
                CommonMethod.Sleep(2000);
                fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();

                wz = CommonData.NXI_6400.GetVResult();

                model = new TestResult();
                model.ChildrenItem = "充电MOS断开反向 - G极电压";

                model.MeasurementStandard = "0-1V";
                model.MeasureVal = fOpenChargeVoltage.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = fOpenChargeVoltage >= 0 && fOpenChargeVoltage <= 1;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "放电MOS断开反向 - G极电压";

                model.MeasurementStandard = "0-1V";
                model.MeasureVal = fOpenDisChargeVoltage.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = fOpenDisChargeVoltage >= 0 && fOpenDisChargeVoltage <= 1;

                AddModelAction(model);

                model = new TestResult();
                model.ChildrenItem = "MOS断开反向 - P+B+压差";

                model.MeasurementStandard = "-3 - (-2.5) V";
                model.MeasureVal = wz.ToString("0.000");
                model.QualifiedValue = "";

                model.Result = wz <= -2.5 && wz >= -3;

                AddModelAction(model);
                CommonData.Relay.SetRelayCard1(10, false);
                CommonData.Relay.SetRelayCard2(5, false);
                CommonData.Relay.SetRelayCard2(2, false);
                CommonData.Relay.SetRelayCard2(3, false);
                CommonData.IT6322.SetOutput(false); 
                CommonData.N69206H.SetOutput(true);
                //  CommonData.Relay.CloseAllRelays();
                //     PlayUDSHelper.MOSControl(MOSEnum.取消控制);
                CommonData.TestCount++;
                return true;
            }
            else
            {

                LogHelper.Debug("------MOS控制测试------");
                _pars = pars;

                DateTime start = DateTime.Now;
                if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
                {
                    NewCanHelper.WakeOrSleep(false);
                }


                if (ConfigHelper.Cantype)
                {
                    CommonMethod.Unlock();
                    //  CommonMethod.StartSendUnlock();
                }
                //CommonMethod.Unlock();
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("MOS闭合失败");
                }
                CommonMethod.Sleep(500);
                var fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                var fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
                if (fOpenChargeVoltage < 23)
                {
                    if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                    {
                        AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                        throw new Exception("MOS闭合失败");
                    }
                    CommonMethod.Sleep(100);
                    fOpenChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                    fOpenDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
                }



                if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                    throw new Exception("断开");
                }
                CommonMethod.Sleep(1500);
                NewCanHelper.MOSControlVoltageEnable(true, false, true, true);
                //CommonMethod.Sleep(2000);
                //var wz = CommonData.NXI_6400.GetVResult();
                //if (wz > 2)
                //{
                //    if (PlayUDSHelper.MOSControl(MOSEnum.断开) == false)
                //    {
                //        AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                //        throw new Exception("MOS断开失败");
                //    }
                //    wz = CommonData.NXI_6400.GetVResult();


                //}
                var fCloseDisChargeVoltage = PlayUDSHelper.GetMOSDischargeVoltage();
                //（断开）充放电P+电压

                if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
                {
                    if (PlayUDSHelper.MOSControl(MOSEnum.放电闭合) == false)
                    {
                        AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                        throw new Exception("放电闭合");
                    }
                }
                CommonMethod.Sleep(1500);
                var fCloseChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                if (fCloseChargeVoltage > 12 && fCloseChargeVoltage < 10)
                {
                    if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
                    {
                        if (PlayUDSHelper.MOSControl(MOSEnum.放电闭合) == false)
                        {
                            AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                            throw new Exception("放电闭合");
                        }
                    }
                    CommonMethod.Sleep(1500);
                    fCloseChargeVoltage = PlayUDSHelper.GetMOSChargeVoltage();
                }



                var end = DateTime.Now;
                var span = (start - end).TotalSeconds;

                Result("1", true, start, span, fOpenChargeVoltage);
                Result("2", true, start.AddSeconds(span * 1), span, fOpenDisChargeVoltage);
                //Result("3", true, start.AddSeconds(span * 2), span, uex2);
                Result("4", true, start.AddSeconds(span * 3), span, fCloseChargeVoltage);
                Result("5", false, start.AddSeconds(span * 4), span, fCloseDisChargeVoltage);
                //if (ConfigHelper.UdsParameters.jiesuo != "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
                //{
                //    Result("6", false, start.AddSeconds(span * 5), span, wz);
                //}

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
}
