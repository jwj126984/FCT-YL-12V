

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
    public class VoltageTest : DetectBase
    {
        static RelayHelper _relay => CommonData.Relay;

        public override bool Start(IEnumerable<TestParameter> pars)
        {

            LogHelper.Debug("------电压测试------");
            CommonMethod.Unlock();

            if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                if (PlayUDSHelper.MOSControl2510(MOSEnum2510.BUCK断) == false)
                {
                    LogHelper.ShowMsg("MOS闭合失败");
                    return false;
                }
            }
            else
            {
                if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                {
                    LogHelper.ShowMsg("MOS闭合失败");
                    return false;
                }
            }
                
            //
            LogHelper.Debug("开启继电器");
            bool result = true;
            result &= _relay.SetRelayCard1(18, true);
            result &= _relay.SetRelayCard1(19, true);
            result &= _relay.SetRelayCard1(20, true);
            result &= _relay.SetRelayCard1(21, true);


            List<TestResult> result1 = new List<TestResult>();//单体电压结果
            List<TestResult> result2 = new List<TestResult>();//AFE 结果
            List<TestResult> result3 = new List<TestResult>();//MCU 结果
            List<TestResult> result4 = new List<TestResult>();//外总 结果

            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                PlayUDSHelper.GoToGC();
                PlayUDSHelper.Biaodinchuwldy();
                PlayUDSHelper.Reset();
                CommonMethod.Sleep(1000);
                NewCanHelper.MOSControlVoltageEnable(true, false);
                List<(double, double)> currS1 = new List<(double, double)>();
                List<(double, double)> currS2 = new List<(double, double)>();
                List<(double, double)> currS3 = new List<(double, double)>();
                List<(double, double)> currS4 = new List<(double, double)>();
                for (int i = 0; i < 3; i++)
                {
                    CommonData.N8331.SetVol4(i * 0.5 + 2.5);
                    CommonMethod.Sleep(100);
                    LogHelper.Debug("回读实际电压后续使用");
                    var mesV = CommonMethod.GetN8331Vol4(i * 0.5 + 2.5);
                    CommonMethod.Sleep(1000);

                    var zcdys = ZCData.DYS;

                    if (i == 0)
                    {
                        if (Math.Round(zcdys.Item1 - (i * 0.5 + 2.5),3) < -0.013 || Math.Round(zcdys.Item2 - (i * 0.5 + 2.5),3) < -0.013 ||
                            Math.Round(zcdys.Item3 - (i * 0.5 + 2.5), 3) < -0.013 || Math.Round(zcdys.Item4 - (i * 0.5 + 2.5), 3) < -0.013)
                        {
                            LogHelper.ShowMsg($"获取的单体电压差1、2、3、4（{zcdys.Item1 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item2 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item3 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item4 - (i * 0.5 + 2.5)}V）");
                            LogHelper.Debug($"获取的单体电1、2、3、4（{zcdys.Item1}V、" +
                                $"{zcdys.Item2}V、" +
                                $"{zcdys.Item3 }V、" +
                                $"{zcdys.Item4 }V）");
                            throw new Exception("获取单体电压超限制");
                        }
                        if (Math.Round(zcdys.Item1 - (i * 0.5 + 2.5),3) > 0.013 || Math.Round(zcdys.Item2 - (i * 0.5 + 2.5), 3) > 0.013 ||
                            Math.Round(zcdys.Item3 - (i * 0.5 + 2.5), 3) > 0.013 || Math.Round(zcdys.Item4 - (i * 0.5 + 2.5), 3) > 0.013)
                        {
                            LogHelper.ShowMsg($"获取的单体电压差1、2、3、4（{zcdys.Item1 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item2 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item3 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item4 - (i * 0.5 + 2.5)}V）");
                            LogHelper.Debug($"获取的单体电1、2、3、4（{zcdys.Item1}V、" +
                                $"{zcdys.Item2}V、" +
                                $"{zcdys.Item3}V、" +
                                $"{zcdys.Item4}V）");
                            throw new Exception("获取单体电压超限制");
                        }
                    }

                    if (i == 1)
                    {
                        if (Math.Round(zcdys.Item1 - (i * 0.5 + 2.5), 3) < -0.013 || Math.Round(zcdys.Item2 - (i * 0.5 + 2.5), 3) < -0.013 ||
                            Math.Round(zcdys.Item3 - (i * 0.5 + 2.5), 3) < -0.013 || Math.Round(zcdys.Item4 - (i * 0.5 + 2.5), 3) < -0.013)
                        {
                            LogHelper.ShowMsg($"获取的单体电压差1、2、3、4（{zcdys.Item1 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item2 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item3 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item4 - (i * 0.5 + 2.5)}V）");
                            LogHelper.Debug($"获取的单体电1、2、3、4（{zcdys.Item1}V、" +
                                $"{zcdys.Item2}V、" +
                                $"{zcdys.Item3}V、" +
                                $"{zcdys.Item4}V）");
                            throw new Exception("获取单体电压超限制");
                        }
                        if (Math.Round(zcdys.Item1 - (i * 0.5 + 2.5), 3) > 0.013 || Math.Round(zcdys.Item2 - (i * 0.5 + 2.5), 3) > 0.013 ||
                            Math.Round(zcdys.Item3 - (i * 0.5 + 2.5), 3) > 0.013 || Math.Round(zcdys.Item4 - (i * 0.5 + 2.5), 3) > 0.013)
                        {
                            LogHelper.ShowMsg($"获取的单体电压差1、2、3、4（{zcdys.Item1 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item2 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item3 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item4 - (i * 0.5 + 2.5)}V）");
                            LogHelper.Debug($"获取的单体电1、2、3、4（{zcdys.Item1}V、" +
                                $"{zcdys.Item2}V、" +
                                $"{zcdys.Item3}V、" +
                                $"{zcdys.Item4}V）");
                            throw new Exception("获取单体电压超限制");
                        }
                    }

                    if (i == 2)
                    {
                        if (Math.Round(zcdys.Item1 - (i * 0.5 + 2.5), 3) < -0.014 || Math.Round(zcdys.Item2 - (i * 0.5 + 2.5), 3) < -0.014 ||
                            Math.Round(zcdys.Item3 - (i * 0.5 + 2.5), 3) < -0.014 || Math.Round(zcdys.Item4 - (i * 0.5 + 2.5), 3) < -0.014)
                        {
                            LogHelper.ShowMsg($"获取的单体电压差1、2、3、4（{zcdys.Item1 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item2 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item3 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item4 - (i * 0.5 + 2.5)}V）");
                            LogHelper.Debug($"获取的单体电1、2、3、4（{zcdys.Item1}V、" +
                                $"{zcdys.Item2}V、" +
                                $"{zcdys.Item3}V、" +
                                $"{zcdys.Item4}V）");
                            throw new Exception("获取单体电压超限制");
                        }
                        if (Math.Round(zcdys.Item1 - (i * 0.5 + 2.5), 3) > 0.014 || Math.Round(zcdys.Item2 - (i * 0.5 + 2.5), 3) > 0.014 ||
                            Math.Round(zcdys.Item3 - (i * 0.5 + 2.5), 3) > 0.014 || Math.Round(zcdys.Item4 - (i * 0.5 + 2.5), 3) > 0.014)
                        {
                            LogHelper.ShowMsg($"获取的单体电压差1、2、3、4（{zcdys.Item1 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item2 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item3 - (i * 0.5 + 2.5)}V、" +
                                $"{zcdys.Item4 - (i * 0.5 + 2.5)}V）");
                            LogHelper.Debug($"获取的单体电1、2、3、4（{zcdys.Item1}V、" +
                                $"{zcdys.Item2}V、" +
                                $"{zcdys.Item3}V、" +
                                $"{zcdys.Item4}V）");
                            throw new Exception("获取单体电压超限制");
                        }
                    }

                    currS1.Add((mesV[0], zcdys.Item1));
                    currS2.Add((mesV[1], zcdys.Item2));
                    currS3.Add((mesV[2], zcdys.Item3));
                    currS4.Add((mesV[3], zcdys.Item4));
                }
                var a_b1 = GetAAndB(currS1.Select(m => m.Item2).ToArray(), currS1.Select(m => m.Item1).ToArray());
                var a_b2 = GetAAndB(currS2.Select(m => m.Item2).ToArray(), currS2.Select(m => m.Item1).ToArray());
                var a_b3 = GetAAndB(currS3.Select(m => m.Item2).ToArray(), currS3.Select(m => m.Item1).ToArray());
                var a_b4 = GetAAndB(currS4.Select(m => m.Item2).ToArray(), currS4.Select(m => m.Item1).ToArray());
                CommonMethod.Unlock();
                PlayUDSHelper.GoToGC();

                PlayUDSHelper.Biaodinwldy(a_b1.Value, a_b2.Value, a_b3.Value, a_b4.Value);
                CommonMethod.Sleep(500);

                if (!NewCanHelper.WakeOrSleep(false))
                {
                    LogHelper.ShowMsg("唤醒失败");
                    throw new Exception("唤醒失败");
                }
                CommonMethod.StopSendAwaken();
                CommonMethod.Unlock();
                PlayUDSHelper.UdsSleep(1);
                CommonMethod.Sleep(1000);
                NewCanHelper.WakeOrSleep(true);
                CommonMethod.StartSendAwaken();


             //   GetKBVolt();
            }
            

            

            
       
            foreach (var par in pars)
            {
                if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" && double.Parse(par.Extend1) == 4.5)
                {
                    break;
                }

                var measurementStandards = par.MeasurementStandard.Split('&');
                DateTime start1 = DateTime.Now;
                double v = par.QualifiedValue.Value;
                CommonData.IT6322.SetOutput(true);
             
                NewCanHelper.MOSControlVoltageEnable(true, true, true, false,false);
                if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" )
                {
                    CommonData.IT6322.SetVol(double.Parse(par.Extend1) * 4 + 0.02);
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR"
                    || ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S")

                {
                    CommonData.IT6322.SetVol(double.Parse(par.Extend1) * 4);
                }
                else
                {
                    CommonData.IT6322.SetVol(double.Parse(par.Extend1) * 4 + 0.05);
                }
                    
                
                
                    //设置电源
                    LogHelper.Debug($"------------------------------------------- 设置 N8331 电压({(par.QualifiedValue.Value / 4.0).ToString()}V) --------------------------------------------");
                CommonData.N8331.SetVol4(double.Parse(par.Extend1));
                CommonMethod.Sleep(100);
                LogHelper.Debug("回读实际电压后续使用");
                var mesV = CommonMethod.GetN8331Vol4(double.Parse(par.Extend1));
                LogHelper.Debug($"实际电压值1-4(单位V):{string.Join(",", mesV)}");


                (double, double, double, double) zcdys = (0,0,0,0);
                if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
                {
                    zcdys = PlayUDSHelper.GetDYS();
                    if (zcdys.Item1 == 0 || zcdys.Item2 == 0 || zcdys.Item3 == 0 || zcdys.Item4 == 0)
                    {
                        zcdys = PlayUDSHelper.GetDYS();
                    }
                }
                else
                {
                    zcdys = ZCData.DYS;
                    if (zcdys.Item1 == 0 || zcdys.Item2 == 0 || zcdys.Item3 == 0 || zcdys.Item4 == 0)
                    {
                        zcdys = ZCData.DYS;
                    }
                }
             
                
                
                //{
                //    double num = (double)measurementStandards[0].GetNumber();
                //    if (Math.Abs(zcdys.Item1 - mesV[0]) <= num || Math.Abs(zcdys.Item2 - mesV[1]) <= num || Math.Abs(zcdys.Item3 - mesV[2]) <= num || Math.Abs(zcdys.Item4 - mesV[3]) <= num)
                //    {
                //        zcdys = ZCData.DYS;
                //        if (zcdys.Item1 == 0 || zcdys.Item2 == 0 || zcdys.Item3 == 0 || zcdys.Item4 == 0)
                //        {
                //            zcdys = ZCData.DYS;
                //        }
                //    }

                //}
                List<double> zcdysTemp = new List<double>() { zcdys.Item1, zcdys.Item2, zcdys.Item3, zcdys.Item4, };
                LogHelper.Debug($"产品获取单体电压（V）:{zcdys}");
                CommonData.N8331.SetVol4(par.QualifiedValue.Value / 4);
                CommonData.IT6322.SetVol(par.QualifiedValue.Value);
                var span1 = (DateTime.Now - start1).TotalSeconds / 4;
                var start2 = DateTime.Now;


                //  CommonData.IT6322.SetOutput(false);


                //if (PlayUDSHelper.MOSControl(MOSEnum.闭合) == false)
                //{
                //    AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                //    throw new Exception("MOS闭合");
                //}

                CommonMethod.Sleep(100);
                
                CommonMethod.Sleep(10);
                var afe = CommonData.NXI_6400.GetVResult();
                NewCanHelper.MOSControlVoltageEnable(true, true, true, true,false);
                CommonMethod.Sleep(10);
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
                if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
                {
                    mcu = PlayUDSHelper.GetChargeVoltage() * 0.1;
                }
                

                
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
                    model.QualifiedValue = mesV[i].ToString("0.000");
                    model.MeasureVal = zcdysTemp[i].ToString("0.000");
                    model.DifferenceVal = (zcdysTemp[i] - mesV[i]).ToString("0.000");
                    double num = (double)model.MeasurementStandard.GetNumber();
                    model.Result = Math.Abs(double.Parse(model.DifferenceVal)) <= num;
                    result1.Add(model);
                }
                #endregion

                #region 总电压
                if (ConfigHelper.UdsParameters.jiesuo == "XP_12V")
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
                    model.ChildrenItem = $"总电压测试-内总{par.QualifiedValue}V";
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
                    model.QualifiedValue =  wdy.ToString("0.000");
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
                //AddModelAction(result3[i]);
                AddModelAction(result2[i]);
                AddModelAction(result4[i]);
            }
            CommonMethod.Unlock();
            CommonData.IT6322.SetOutput(false);
            CommonData.IT6322.SetVol(13.7);
            CommonMethod.SetN8331(13.2);
            CommonData.TestCount++;


            result &= _relay.SetRelayCard1(18, false);
            result &= _relay.SetRelayCard1(19, false);
            result &= _relay.SetRelayCard1(20, false);
            result &= _relay.SetRelayCard1(21, false);


            return true;
        }

        public static (double, double)? GetAAndB(double[] x, double[] y)
        {
            // 确保数组长度一致
            if (x.Length != y.Length)
            {
                Console.WriteLine("X和Y的数组长度必须相同！");
                return (double.NaN, double.NaN);
            }
            if (x.Distinct().Count() == 1)
            {
                Console.WriteLine("所有的 x 值相同，无法拟合直线。");
                return null;
            }

            if (y.Distinct().Count() == 1)
            {
                Console.WriteLine("所有的 y 值相同，拟合水平线。");
                double c = y[0];
                return (0, c);
            }

            int n = x.Length;

            double sX = x.Sum();
            double sY = y.Sum();
            double sXY = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sXY += x[i] * y[i];
            }
            double sX2 = x.Sum(m => m * m);
            double slope = (n * sXY - sX * sY) / (n * sX2 - sX * sX);
            double intercept = (sY - slope * sX) / n;

            ushort aW = (ushort)(Math.Abs(slope) * 1000);
            var aWTemp = aW / 1000.0;
            short bW = (short)(intercept * 1000);
            var bWTemp = bW / 1000.0;


            // 计算所需的和
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            // 计算斜率a和截距b
            double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - a * sumX) / n;
            return (aWTemp, bWTemp);
        }



        void GetKBVolt()
        {
            CommonMethod.Sleep(200);
            var data = new byte[] { 0x30, 0x31 };
            var temp = PlayUDSHelper.Send(data.ToArray(), 0x22);
            if (temp == null)
            {
                LogHelper.ShowMsg("读取KB值失败");
                return;
            }

            var responses = PlayUDSHelper.SendUDS2("30 00 00 AA AA AA AA AA");
            List<byte> allData = new List<byte>();
            allData.AddRange(temp.Item2);

            foreach (var response in responses)
            {
                if (response[0] == 0x21 || response[0] == 0x22 || response[0] == 0x23 || response[0] == 0x24)
                {
                    for (int i = 1; i < response.Length; i++)
                    {
                        allData.Add(response[i]);
                    }
                }
            }

            string str = "";
            foreach (var item in allData)
            {
                str += item.ToString("X2") + " ";
            }
            LogHelper.Debug($"KB值原始数据: {str}");

            List<double> VoltK = new List<double>();
            List<double> VoltB = new List<double>();

            for (int i = 0; i < 4; i++)
            {
                int cellOffset = 3 + i * 4;
                if (cellOffset + 1 < allData.Count)
                {
                    VoltK.Add((allData[cellOffset] * 256 + allData[cellOffset + 1]) / 1000.0);
                }
                if (cellOffset + 3 < allData.Count)
                {
                    VoltB.Add((allData[cellOffset + 2] * 256 + allData[cellOffset + 3]) / 1000.0);
                }
            }

            LogHelper.ShowMsg($"4个电芯KB值 - K1:{VoltK[0]:F3}, B1:{VoltB[0]:F3}, K2:{VoltK[1]:F3}, B2:{VoltB[1]:F3}, K3:{VoltK[2]:F3}, B3:{VoltB[2]:F3}, K4:{VoltK[3]:F3}, B4:{VoltB[3]:F3}");
            LogHelper.Debug($"4个电芯KB值 - K1:{VoltK[0]:F3}, B1:{VoltB[0]:F3}, K2:{VoltK[1]:F3}, B2:{VoltB[1]:F3}, K3:{VoltK[2]:F3}, B3:{VoltB[2]:F3}, K4:{VoltK[3]:F3}, B4:{VoltB[3]:F3}");
        }
    }
}
