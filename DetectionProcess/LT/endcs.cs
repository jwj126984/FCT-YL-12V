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

namespace DetectionProcess.LT
{
    public class endcs : DetectBase
    {
        static string BarCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
        static string BarCodeMonth = "123456789ABC";
        static string BarCodeDay = "123456789ABCDEFGHJKLMNPRSTVWXYZ";
        public override bool Start(IEnumerable<TestParameter> pars)
        {
           
            LogHelper.Debug("------清除故障------");

            string s2 = null;
            var model = new TestResult() { ChildrenItem = "清除故障" };
            PlayUDSHelper.ClearError();
            {
                model.Result = true;
                AddModelAction(model);
            }

            if (PlayUDSHelper.MOSControl2510(MOSEnum2510.BUCK断) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "MOS控制测试失败", Result = false });
                throw new Exception("MOS闭合失败");
            }

            CommonMethod.Sleep(100);
            model = new TestResult() { ChildrenItem = "重置EEPROM" };
            if (PlayUDSHelper.ResetEEPROM())
            {
                model.Result = true;
                AddModelAction(model);
            }
            else
            {
                model.Result = false;
                AddModelAction(model);
                return false;
            }
            CommonMethod.Sleep(100);
            PlayUDSHelper.ClearECU();


            CommonMethod.Sleep(1000);

            var barCode = CommonData.CurrentBarCode;
            barCode = barCode.Substring(21);

            
            var barValue = barCode.Substring(0, 6) + barCode.Substring(8, 1) + barCode.Substring(10, 4);
            barValue = "10_807001954AA_11_9DQ_12_" + barValue;

            //写入序列号
            PlayUDSHelper.BarCodeCalibrate1068(barValue);

            CommonMethod.Sleep(1000);

            PlayUDSHelper.VersionCalibateRJJH1068(ConfigHelper.UdsParameters.YJ_DATA);

            PlayUDSHelper.SetSOC1068(90);

            PlayUDSHelper.MOSControl2510(MOSEnum2510.取消控制);

            NewCanHelper.WakeOrSleep(false);
            CommonMethod.StopSendAwaken();
            CommonMethod.StopSendUnlock();
            CommonMethod.Sleep(1000);
            PlayUDSHelper.UdsSleep(1);

            var dl = CommonMethod.GetSleep1CurrentFromPower(awatiTime: 5, threshold: 0.3);
            CommonMethod.Sleep(100);

        
            CommonMethod.StartSendAwaken();
            //测量电流


            var result = PlayUDSHelper.GetBalancedAndAFE();

            model = new TestResult();
            model.ChildrenItem = "读取故障码";
            model.QualifiedValue = "";
            
            if (result == null)
            {
                //LogHelper.ShowMsg("获取均衡结果失败");
                AddModelAction(new TestResult() { ChildrenItem = "均衡与AFE诊断测试失败" });
                throw new Exception("获取均衡结果失败");

            }
            else
            {
                int startIndex = 0;
                for (int i = 0; i < result.Length; i++)
                {
                    if (result[i] != 0x00)
                    {
                        startIndex = i; break;
                    }
                }
                if (result[startIndex] == 0x03)
                {
                    model.MeasureVal = "";
                    int len = result[startIndex];
                    for (int i = startIndex + 1; i < len + 1; i++)
                    {
                        model.MeasureVal += result[i].ToString("X2") + " ";
                    }
                    model.Result = true;

                }
                else
                {
                    
                    model.MeasureVal = "";
                    int len = result[startIndex];
                    for (int i = startIndex + 1; i < len + 1 + startIndex; i++)
                    {
                        model.MeasureVal += result[i].ToString("X2") + " ";
                    }
                    if(len > 7)
                    {
                        for (int i = 1; i <= ((len - 3) / 4); i++)
                        {
                            if (result[i * 4 + 4] == 0x08)
                            {
                                LogHelper.ShowMsg($"{result[i * 4 + 4]}");
                                continue;
                            }
                            string response = result[i * 4+1].ToString("X2") + result[i * 4 + 2].ToString("X2") + result[i * 4 + 3].ToString("X2");
                            LogHelper.ShowMsg(response);
                            if (response == "96D401" || response == "96D402" || response == "96D403" || response == "C26C87" ||
                                response == "C19887" || response == "96D901" || response == "96DE01" || response == "96DE02" ||
                                response == "96DF01" || response == "96DF02" || response == "96E001" || response == "96E002")
                            {
                                model.Result = true;

                            }
                            else
                            {

                                model.Result = false;
                                break;
                            }
                        }
                    }
                    else
                    {
                        for (int i = 1; i <= ((len - 3) / 4); i++)
                        {
                            if (result[i * 4 + 3] == 0x08)
                            {
                                LogHelper.ShowMsg($"{result[i * 4 + 3]}");
                                continue;
                            }
                            string response = result[i * 4].ToString("X2") + result[i * 4 + 1].ToString("X2") + result[i * 4 + 2].ToString("X2");
                            LogHelper.ShowMsg(response);
                            if (response == "96D401" || response == "96D402" || response == "96D403" || response == "C26C87" ||
                                response == "C19887" || response == "96D901" || response == "96DE01" || response == "96DE02" ||
                                response == "96DF01" || response == "96DF02" || response == "96E001" || response == "96E002")
                            {
                                model.Result = true;

                            }
                            else
                            {

                                model.Result = false;
                                break;
                            }
                        }
                    }

                        
                }

            }
            model.MeasurementStandard = "";
            model.DifferenceVal = "";
            AddModelAction(model);
            CommonMethod.Sleep(100);
            //回读序列号
            var ECU = PlayUDSHelper.GetECU();
            model = new TestResult();
            model.ChildrenItem = "读取ECU序列号";
            model.QualifiedValue = barValue;
            model.MeasureVal = ECU;
          
            model.MeasurementStandard = "";
            model.DifferenceVal = "";
            model.Result = ECU == barValue;
            AddModelAction(model);


            model = new TestResult();
            model.ChildrenItem = "硬件版本号";
            model.StartTime = DateTime.Now;


            s2 = PlayUDSHelper.GetVersion1068(2).Trim();

            model.EndTime = DateTime.Now;
            model.QualifiedValue = ConfigHelper.UdsParameters.YJ_DATA;
            model.MeasurementStandard = "";
            model.MeasureVal = s2;
            model.Result = model.MeasureVal == model.QualifiedValue;
            //model.Result = true;
            AddModelAction(model);

            int soc = PlayUDSHelper.GetSOC();
            model = new TestResult();
            model.ChildrenItem = "SOC";
            model.QualifiedValue = 90.ToString("0.000");
            model.MeasureVal = soc.ToString("0.000");
            LogHelper.Debug($"整车SOC转换后为{model.MeasureVal}");
            model.MeasurementStandard = "";
            model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
            model.Result = Math.Abs(soc - 90) < 1;
            AddModelAction(model);

            

            var MOS = PlayUDSHelper.GetMOS();
            model = new TestResult();
            model.ChildrenItem = "MOS状态";
            model.QualifiedValue = "5";
            model.MeasureVal = MOS.ToString("");
          
            model.MeasurementStandard = "";
            model.DifferenceVal = "";
            model.Result = MOS == 5;
            AddModelAction(model);


            var SUOBAO = PlayUDSHelper.GetSB();
            model = new TestResult();
            model.ChildrenItem = "锁包状态";
            model.QualifiedValue = "0";
            model.MeasureVal = SUOBAO.ToString("");

            model.MeasurementStandard = "";
            model.DifferenceVal = "";
            model.Result = SUOBAO == 0;
            AddModelAction(model);

            
            CommonMethod.StopSendUnlock();
            CommonMethod.StopSendAwaken();

            CommonMethod.SendAwaken();
            CommonMethod.Unlock();
            CommonMethod.SendAwaken();
            var temp = PlayUDSHelper.UdsSleep(2);
            if (temp == null)
            {
                AddModelAction(new TestResult() { ChildrenItem = "二级休眠", Result = false, Extend3 = "sleep" });
            }
            else
            {
                AddModelAction(new TestResult() { ChildrenItem = "二级休眠", Result = null, Extend3 = "sleep" });
            }

            CommonData.TestCount++;
            return true;
        }
    }
}
