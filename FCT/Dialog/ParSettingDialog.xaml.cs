using AllData;
using Common;
using Common.Can;
using DetectionProcess.LT;
using DetectionProcess.SERES;
using FCT.Ctrl.DetectCtrlGEELY;
using FCT.Ctrl.DetectCtrlLT;
using FCT.Ctrl.DetectCtrlSERES;
using FCT.Ctrl.PlayCtrl;
using FCT.PConvert;
using FCT.Properties;
using Microsoft.Win32;
using Model;
using Models_C.Dal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static AllData.ConfigHelper;
using static FCT.Dialog.ParSettingDialog;
using IDetectCtrlBaseGEELY = FCT.Ctrl.DetectCtrlGEELY.IDetectCtrlBase;
using IDetectCtrlBaseLT = FCT.Ctrl.DetectCtrlLT.IDetectCtrlBase;
using IDetectCtrlBaseSERES = FCT.Ctrl.DetectCtrlSERES.IDetectCtrlBase;





namespace FCT.Dialog
{
    /// <summary>
    /// ParSettingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ParSettingDialog : Window
    {
        List<GroupBox> DetectGBs = new List<GroupBox>();
        string backFilePath = System.IO.Directory.GetCurrentDirectory() + "\\backFile.json";
        private readonly Dictionary<string, TextBox> _parameterControls = new Dictionary<string, TextBox>();

        private Dictionary<string, TextBox> _udsParameterControls = new Dictionary<string, TextBox>();

        private List<ProtocolParameter> _protocolParameters;
        private List<UdsTestItem> _udsTestItems;

        FCT.Ctrl.DetectCtrlSERES.CanTRTestParSetting canTRTestParSetting;


        List<Tuple<string, string, int>> DetectsSERES()
        {
            string detect = null;
            if (ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
            {
                detect = "CanTRTest," +
                    "EOLClose," +
               "LBMSRead," +
               "PCBTemperature," +
               "MOSControl," +
               "VoltageTest," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "CellTemperature," +
               "WorkingCurrent," +
               "BalancedTest," +
               "CurrentLimiting," +
               "SleepingCurrent," +
               "WakeSourceTest," +
               "MOSSleepKeep," +
               "EEPROMTest," +
               "ClearAH," +
               "SOCAndSOH," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else
            {
                detect = "CanTRTest," +
               "LBMSRead," +
               "PCBTemperature," +
               "MOSControl," +
               "VoltageTest," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "CellTemperature," +
               "WorkingCurrent," +
               "BalancedTest," +
               "CurrentLimiting," +
               "SleepingCurrent," +
               "WakeSourceTest," +
               "MOSSleepKeep," +
               "EEPROMTest," +
               "SOCAndSOH," +
               "ClearDTC," +
               "LBMSWrite," +
               "BalancedAndAFE";
            }

            var list = detect.Split(',').ToList();
            return list.Select(m => new Tuple<string, string, int>(m, m.GetShowTxt(), list.IndexOf(m) + 1)).ToList();
        }



        List<Tuple<string, string, int>> DetectsGEELY()
        {
            string detect = "Awaken," +
                "WorkingCurrent," +
                "ClearCalibration," +
                "GetProduceInfo," +
                "DateOfManufactureW_RBefor," +
                "DateOfManufactureW_R," +
                "VoltageCalibration," +
                "VoltageTest," +
                "ModuleTemperatureTest," +
                "PCBTemperature," +
                "BalancedTest," +
                "H_D_OAndRelayControl," +
                "CurrentCalibration," +
                "CurrentTest," +
                "Sleep," +
                "SleepingCurrent," +
                "AwakenSource," +
                "Awaken," +
                "SOCAndSOH," +
                "BusMalfunction";
            var list = detect.Split(',').ToList();
            return list.Select(m => new Tuple<string, string, int>(m, m.GetShowTxt(), list.IndexOf(m) + 1)).ToList();
        }

        List<Tuple<string, string, int>> DetectsLT()
        {
            string detect = null;
            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
            {
                detect = "CanTRTest," +
                    "ProduceRead," +
               "PCBTemperature," +
               "ModuleTemperature," +
               "VoltageTest," +
               "MOSControl," +
               "BalancedTest," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeCalibration," +
               "MOSSleepKeep," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "EEPROMTest," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                detect = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
               "CurrentLimiting," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "AVATR_E15")
            {
                detect = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
               "MOSSleepKeep," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "EEPROMTest," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {
                detect = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +
                   
               "MOSControl," +
               "ModuleTemperature," +
               "Cur0A," +
                
               "VoltageTest," +
                  
               "BalancedTest," +
               "CurrentLimiting," +
               
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
                 "MOSSleepKeep," +
               "WakeSourceTest," +
               "SleepingCurrent," +
              

               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                detect = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeCalibration," +
               "MOSSleepKeep," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "EEPROMTest," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" )
            {

                detect = "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
            {

                detect = "CanTRTest," +
                    "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
                "CurrentLimiting," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
            {
                detect = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
                   "Discur," +
               "VoltageTest," +
               "BalancedTest," +
               "CurrentLimiting," +
                "MOSSleepKeep," +
               "CurrentCalibration," +
               "CurrentCollection," +
           
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                detect = 
               "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
               "CurrentLimiting," +
             
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
                "WakeCalibration," +
               "WakeSourceTest," +
               "SleepingCurrent," +
              
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                detect = "ModuleTemperature," +
                    "PCBTemperature," +
               "CurrentCalibration," +
                    "FenLiuR," +
               "ProduceRead," +
                "VoltageTest," +
               "BalancedTest," +
               "MOSControl," +
               "CurrentLimiting," +
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "CurrentCollection," +
               "endcs";
            }


            else
            {
                detect = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +
               "MOSControl," +
               "ModuleTemperature," +
               "VoltageTest," +
               "BalancedTest," +
               "CurrentLimiting," +
               "MOSSleepKeep," +
               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
               "WakeSourceTest," +
               "SleepingCurrent," +
               "EEPROMTest," +
               "ClearAH," +
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
            }
            var list = detect.Split(',').ToList();
            return list.Select(m => new Tuple<string, string, int>(m, m.GetShowTxt(), list.IndexOf(m) + 1)).ToList();
        }
        //添加显示项
        public void AddTestItem(List<UdsTestItem> testItem)
        {
            testItem.Add(new UdsTestItem
            {
                TestName = "UDS帧ID设置",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "请求报文ID", Description = "0x", TextBoxName = "TID", DefaultValue = "7C0" },
                        new UdsParameter { Name = "响应报文ID", Description = "0x", TextBoxName = "RID", DefaultValue = "7C8" }
                    }
            });
            testItem.Add(new UdsTestItem
            {
                TestName = "解锁",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "密钥算法名称", Description = "", TextBoxName = "jiesuo", DefaultValue = "SERES_F3" },
                        new UdsParameter { Name = "CAN协议", Description = "CAN/CANFD", TextBoxName = "xieyi", DefaultValue = "CAN" },
                        new UdsParameter { Name = "会话维持ID", Description = "0x", TextBoxName = "WC_ID", DefaultValue = "CAN" },
                        new UdsParameter { Name = "产品名称", Description = "0x", TextBoxName = "chanpin", DefaultValue = "CAN" }

                    }
            });

            if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C" || ConfigHelper.UdsParameters.jiesuo == "XP_12V")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "产品信息读取1",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "PRID1", DefaultValue = "580" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "PRSTARTBIT1", DefaultValue = "33" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "PRLEN1", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "PRFACTOR1", DefaultValue = "1" }

                    }

                });

                testItem.Add(new UdsTestItem
                {
                    TestName = "产品信息读取2",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "PRID2", DefaultValue = "580" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "PRSTARTBIT2", DefaultValue = "41" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "PRLEN2", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "PRFACTOR2", DefaultValue = "1" }

                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "产品信息读取3",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "PRID3", DefaultValue = "580" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "PRSTARTBIT3", DefaultValue = "49" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "PRLEN3", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "x", TextBoxName = "PRFACTOR3", DefaultValue = "1" }

                    }

                });
            }

           
                if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y" || ConfigHelper.UdsParameters.jiesuo == "QR_C1068" || ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "PCB板温度",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "PCBT_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "PCBT_DID", DefaultValue = "61 00" },
                    }

                });
            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "PCB板温度",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "PCBT_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "PCBT_Startbit", DefaultValue = "241" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "PCBT_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "PCBT_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "PCBT_Offset", DefaultValue = "-40" }

                    }

                });

            }

            if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03" || ConfigHelper.UdsParameters.jiesuo == "QR_C1068" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "充电MOS温度",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MOSCT_SID", DefaultValue = "345" },
                        new UdsParameter { Name = "DID", Description = "", TextBoxName = "MOSCT_DID", DefaultValue = "233" }


                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "放电MOS温度",
                    Parameters = new List<UdsParameter>
                    {

                        new UdsParameter { Name = "SID", Description = "", TextBoxName = "MOSFT_SID", DefaultValue = "1" },
                        new UdsParameter { Name = "DID", Description = "", TextBoxName = "MOSFT_DID", DefaultValue = "-40" }

                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "限流模块温度",
                    Parameters = new List<UdsParameter>
                    {

                        new UdsParameter { Name = "SID", Description = "", TextBoxName = "XLT_SID", DefaultValue = "1" },
                        new UdsParameter { Name = "DID", Description = "", TextBoxName = "XLT_DID", DefaultValue = "-40" }

                    }

                });
            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "充电MOS温度",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "MOSCT_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "MOSCT_Startbit", DefaultValue = "233" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "MOSCT_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "MOSCT_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "MOSCT_Offset", DefaultValue = "-40" }

                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "放电MOS温度",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "MOSFT_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "MOSFT_Startbit", DefaultValue = "225" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "MOSFT_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "MOSFT_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "MOSFT_Offset", DefaultValue = "-40" }

                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "限流模块温度",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "XLT_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "XLT_Startbit", DefaultValue = "249" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "XLT_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "XLT_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "XLT_Offset", DefaultValue = "-40" }

                    }

                });

            }


            testItem.Add(new UdsTestItem
            {
                TestName = "电流标定起始位",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "DLB_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "DLB_DID", DefaultValue = "61 00" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "P+电压",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "PZ_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "PZ_Startbit", DefaultValue = "249" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "PZ_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "PZ_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "PZ_Offset", DefaultValue = "-40" }
                    }

            });
            if (ConfigHelper.UdsParameters.jiesuo != null)
            {
                if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR" ||
                    ConfigHelper.UdsParameters.jiesuo == "SERES_V32" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046" || ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "T09_M")
                {
                    testItem.Add(new UdsTestItem
                    {
                        TestName = "MOS充电电压",
                        Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "MOSCV_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "MOSCV_Startbit", DefaultValue = "249" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "MOSCV_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "MOSCV_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "MOSCV_Offset", DefaultValue = "-40" }
                    }

                    });
                    testItem.Add(new UdsTestItem
                    {
                        TestName = "MOS放电电压",
                        Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "MOSFV_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "MOSFV_Startbit", DefaultValue = "249" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "MOSFV_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "MOSFV_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "MOSFV_Offset", DefaultValue = "-40" }
                    }

                    });
                }
                else
                {
                    testItem.Add(new UdsTestItem
                    {
                        TestName = "MOS充电电压",
                        Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MOSCV_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "MOSCV_DID", DefaultValue = "08 95" },
                    }

                    });
                    testItem.Add(new UdsTestItem
                    {
                        TestName = "MOS放电电压",
                        Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MOSFV_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "MOSFV_DID", DefaultValue = "08 94" },
                    }

                    });

                }
            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "MOS充电电压",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MOSCV_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "MOSCV_DID", DefaultValue = "08 95" },
                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "MOS放电电压",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MOSFV_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "MOSFV_DID", DefaultValue = "08 94" },
                    }

                });

            }


            testItem.Add(new UdsTestItem
            {
                TestName = "MOS控制",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MOSControl_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "MOSControl_DID", DefaultValue = "61 00 03" },
                    }

            });
            //if (ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
            //{
            //    testItem.Add(new UdsTestItem
            //    {
            //        TestName = "MCU充电电压",
            //        Parameters = new List<UdsParameter>
            //        {
            //            new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "MCUCV_ID", DefaultValue = "345" },
            //            new UdsParameter { Name = "起始位", Description = "", TextBoxName = "MCUCV_Startbit", DefaultValue = "1" },
            //            new UdsParameter { Name = "长度", Description = "", TextBoxName = "MCUCV_Len", DefaultValue = "13" },
            //            new UdsParameter { Name = "精度", Description = "", TextBoxName = "MCUCV_Factor", DefaultValue = "0.001" },
            //            new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "MCUCV_Offset", DefaultValue = "0" }

            //        }

            //    });
            //}
            //else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "MCU充电电压",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "MCUCV_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "MCUCV_DID", DefaultValue = "08 95" },
                    }

                });
            }

            testItem.Add(new UdsTestItem
            {
                TestName = "电芯电压1",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXV1_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXV1_Startbit", DefaultValue = "1" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXV1_Len", DefaultValue = "13" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXV1_Factor", DefaultValue = "0.001" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXV1_Offset", DefaultValue = "0" }

                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "电芯电压2",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXV2_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXV2_Startbit", DefaultValue = "14" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXV2_Len", DefaultValue = "13" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXV2_Factor", DefaultValue = "0.001" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXV2_Offset", DefaultValue = "0" }

                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "电芯电压3",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXV3_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXV3_Startbit", DefaultValue = "27" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXV3_Len", DefaultValue = "13" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXV3_Factor", DefaultValue = "0.001" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXV3_Offset", DefaultValue = "0" }

                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "电芯电压4",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXV4_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXV4_Startbit", DefaultValue = "40" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXV4_Len", DefaultValue = "13" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXV4_Factor", DefaultValue = "0.001" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXV4_Offset", DefaultValue = "0" }

                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "内总压",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "NZV_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "NZV_Startbit", DefaultValue = "78" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "NZV_Len", DefaultValue = "15" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "NZV_Factor", DefaultValue = "0.001" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "NZV_Offset", DefaultValue = "0" }

                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "外总压",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "WZV_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "WZV_Startbit", DefaultValue = "94" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "WZV_Len", DefaultValue = "14" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "WZV_Factor", DefaultValue = "0.0009765625" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "WZV_Offset", DefaultValue = "3" }

                    }

            });
            //if(ConfigHelper.SelectType == "seres")
            //{
            //    testItem.Add(new UdsTestItem
            //    {
            //        TestName = "电流",
            //        Parameters = new List<UdsParameter>
            //        {
            //            new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "DL_SID", DefaultValue = "22" },
            //            new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "DL_DID", DefaultValue = "01 0C" },
            //        }

            //    });

            //}

            testItem.Add(new UdsTestItem
            {
                TestName = "电流",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DL_ID", DefaultValue = "372" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DL_Startbit", DefaultValue = "75" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DL_Len", DefaultValue = "21" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DL_Factor", DefaultValue = "0.001" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DL_Offset", DefaultValue = "-1000" }

                    }

            });

            if (ConfigHelper.UdsParameters.jiesuo == "CHERY_E0Y" || ConfigHelper.UdsParameters.jiesuo == "QR_C1068" || ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03" || ConfigHelper.UdsParameters.jiesuo == "XP_12V")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "电芯温度1",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "DXT1_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "DXT1_DID", DefaultValue = "01 0C" },
                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "电芯温度2",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "DXT2_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "DXT2_DID", DefaultValue = "01 0C" },
                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "电芯温度3",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "DXT3_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "DXT3_DID", DefaultValue = "01 0C" },
                    }

                });


            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "电芯温度1",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXT1_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXT1_Startbit", DefaultValue = "110" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXT1_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXT1_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXT1_Offset", DefaultValue = "-40" }

                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "电芯温度2",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXT2_ID", DefaultValue = "345" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXT2_Startbit", DefaultValue = "118" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXT2_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXT2_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXT2_Offset", DefaultValue = "-40" }

                        }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "电芯温度3",
                    Parameters = new List<UdsParameter>
                        {
                            new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "DXT3_ID", DefaultValue = "345" },
                            new UdsParameter { Name = "起始位", Description = "", TextBoxName = "DXT3_Startbit", DefaultValue = "126" },
                            new UdsParameter { Name = "长度", Description = "", TextBoxName = "DXT3_Len", DefaultValue = "8" },
                            new UdsParameter { Name = "精度", Description = "", TextBoxName = "DXT3_Factor", DefaultValue = "1" },
                            new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "DXT3_Offset", DefaultValue = "-40" }

                        }

                });

            }

            testItem.Add(new UdsTestItem
            {
                TestName = "开启均衡通道",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "JHStart_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "JHStart_DID", DefaultValue = "34 02 03" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "结束均衡控制",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "JHEnd_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "JHEnd_DID", DefaultValue = "34 02 00" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "唤醒指令",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "ID", Description = "0x", TextBoxName = "HX_ID", DefaultValue = "600" },
                        new UdsParameter { Name = "数据", Description = "0x", TextBoxName = "HX_Data", DefaultValue = "00 00 00 00 00 00 00 00" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "一级休眠指令",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "XM1_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "XM1_DID", DefaultValue = "61 03 03 01" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "二级休眠指令",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "XM2_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "XM2_DID", DefaultValue = "61 03 03 02" },
                    }

            });
            if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046" ||
                ConfigHelper.UdsParameters.jiesuo == "AVATR_E15" || ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "获取唤醒源",
                    Parameters = new List<UdsParameter>
                        {
                            new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "GetHX_ID", DefaultValue = "345" },
                            new UdsParameter { Name = "起始位", Description = "", TextBoxName = "GetHX_Startbit", DefaultValue = "126" },
                            new UdsParameter { Name = "长度", Description = "", TextBoxName = "GetHX_Len", DefaultValue = "8" },
                            new UdsParameter { Name = "精度", Description = "", TextBoxName = "GetHX_Factor", DefaultValue = "1" },
                            new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "GetHX_Offset", DefaultValue = "-40" }

                        }

                });
            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "获取唤醒源",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "GetHX_SID", DefaultValue = "22" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "GetHX_DID", DefaultValue = "08 8F" },
                    }

                });
            }

            testItem.Add(new UdsTestItem
            {
                TestName = "唤醒源合格值",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "RTC", Description = "0x", TextBoxName = "GetHX_RTC", DefaultValue = "1" },
                        new UdsParameter { Name = "过压", Description = "0x", TextBoxName = "GetHX_GY", DefaultValue = "2" },
                        new UdsParameter { Name = "硬件", Description = "0x", TextBoxName = "GetHX_YJ", DefaultValue = "3" },
                        new UdsParameter { Name = "电流", Description = "0x", TextBoxName = "GetHX_DL", DefaultValue = "4" },
                        new UdsParameter { Name = "CAN", Description = "0x", TextBoxName = "GetHX_CAN", DefaultValue = "5" },
                    }

            });
            if (ConfigHelper.SelectType == "lt" || ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "清除累积充放电AH",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "ClearAH_SID", DefaultValue = "31" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "ClearAH_DID", DefaultValue = "01 64 81 01" },
                    }

                });

            }
                
            testItem.Add(new UdsTestItem
            {
                TestName = "启动限流模块控制",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "XLOpen_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "XLOpen_DID", DefaultValue = "34 01 03" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "结束限流模块控制",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "XLEnd_SID", DefaultValue = "2F" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "XLEnd_DID", DefaultValue = "34 01 00" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "读取EEPROM快照",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "EEPROM_SID", DefaultValue = "19" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "EEPROM_DID", DefaultValue = "08 8E" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "SOC标定",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "SOCT_SID", DefaultValue = "31" },
                        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "SOCT_DID", DefaultValue = "01 64 80" },
                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "SOC读取",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "SOCR_ID", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "SOCR_Startbit", DefaultValue = "161" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "SOCR_Len", DefaultValue = "8" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "SOCR_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "SOCR_Offset", DefaultValue = "0" }

                    }

            });
            testItem.Add(new UdsTestItem
            {
                TestName = "SOH读取",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "SOHR_ID", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "SOHR_Startbit", DefaultValue = "33" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "SOHR_Len", DefaultValue = "7" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "SOHR_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "SOHR_Offset", DefaultValue = "0" }

                    }

            });


            testItem.Add(new UdsTestItem
            {
                TestName = "LBMS写入",
                Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "批次号标定前三位", Description = "0x", TextBoxName = "PCH_Date", DefaultValue = "00" },
                        new UdsParameter { Name = "硬件版本标定前三位", Description = "0x", TextBoxName = "YJ_Date", DefaultValue = "00" },
                        new UdsParameter { Name = "软件版本标定前三位", Description = "0x", TextBoxName = "RJ_Date", DefaultValue = "00" },


                    }

            });

            if(ConfigHelper.UdsParameters.jiesuo == "M18_3")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软件版本号",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "RJ_ID1", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "RJ_Startbit1", DefaultValue = "33" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "RJ_Len1", DefaultValue = "7" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "RJ_Factor1", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "RJ_Offset1", DefaultValue = "0" },

                    }

                });
                testItem.Add(new UdsTestItem
                {
                    TestName = "硬件版本号",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID", Description = "0x", TextBoxName = "YJ_ID", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位", Description = "", TextBoxName = "YJ_Startbit", DefaultValue = "33" },
                        new UdsParameter { Name = "长度", Description = "", TextBoxName = "YJ_Len", DefaultValue = "7" },
                        new UdsParameter { Name = "精度", Description = "", TextBoxName = "YJ_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量", Description = "", TextBoxName = "YJ_Offset", DefaultValue = "0" },

                    }

                });
                //testItem.Add(new UdsTestItem
                //{
                //    TestName = "软件版本",
                //    Parameters = new List<UdsParameter>
                //    {
                //        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "RJ_SID", DefaultValue = "31" },
                //        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "RJ_DID", DefaultValue = "01 64 80" },
                //    }

                //});
                //testItem.Add(new UdsTestItem
                //{
                //    TestName = "硬件版本",
                //    Parameters = new List<UdsParameter>
                //    {
                //        new UdsParameter { Name = "SID", Description = "0x", TextBoxName = "YJ_SID", DefaultValue = "31" },
                //        new UdsParameter { Name = "DID", Description = "0x", TextBoxName = "YJ_DID", DefaultValue = "01 64 80" },
                //    }

                //});

            }
            else if(ConfigHelper.UdsParameters.jiesuo == "XP_12V")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "硬件版本号",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID1", Description = "0x", TextBoxName = "RJ_ID1", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位1", Description = "", TextBoxName = "RJ_Startbit1", DefaultValue = "33" },
                        new UdsParameter { Name = "长度1", Description = "", TextBoxName = "RJ_Len1", DefaultValue = "7" },
                        new UdsParameter { Name = "精度1", Description = "", TextBoxName = "RJ_Factor1", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量1", Description = "", TextBoxName = "RJ_Offset1", DefaultValue = "0" },

                        new UdsParameter { Name = "帧ID2", Description = "0x", TextBoxName = "RJ_ID2", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位2", Description = "", TextBoxName = "RJ_Startbit2", DefaultValue = "33" },
                        new UdsParameter { Name = "长度2", Description = "", TextBoxName = "RJ_Len2", DefaultValue = "7" },
                        new UdsParameter { Name = "精度2", Description = "", TextBoxName = "RJ_Factor2", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量2", Description = "", TextBoxName = "RJ_Offset2", DefaultValue = "0" }

                    }

                });


            }
            else if (ConfigHelper.UdsParameters.jiesuo == "CHERY_EH3" || ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软件版本号",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID1", Description = "0x", TextBoxName = "RJ_ID1", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位1", Description = "", TextBoxName = "RJ_Startbit1", DefaultValue = "33" },
                        new UdsParameter { Name = "长度1", Description = "", TextBoxName = "RJ_Len1", DefaultValue = "7" },
                        new UdsParameter { Name = "精度1", Description = "", TextBoxName = "RJ_Factor1", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量1", Description = "", TextBoxName = "RJ_Offset1", DefaultValue = "0" },

                        new UdsParameter { Name = "帧ID2", Description = "0x", TextBoxName = "RJ_ID2", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位2", Description = "", TextBoxName = "RJ_Startbit2", DefaultValue = "33" },
                        new UdsParameter { Name = "长度2", Description = "", TextBoxName = "RJ_Len2", DefaultValue = "7" },
                        new UdsParameter { Name = "精度2", Description = "", TextBoxName = "RJ_Factor2", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量2", Description = "", TextBoxName = "RJ_Offset2", DefaultValue = "0" }

                    }

                });

                testItem.Add(new UdsTestItem
                {
                    TestName = "硬件版本号",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID1", Description = "0x", TextBoxName = "YJ_ID", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位1", Description = "", TextBoxName = "YJ_Startbit", DefaultValue = "33" },
                        new UdsParameter { Name = "长度1", Description = "", TextBoxName = "YJ_Len", DefaultValue = "7" },
                        new UdsParameter { Name = "精度1", Description = "", TextBoxName = "YJ_Factor", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量1", Description = "", TextBoxName = "YJ_Offset", DefaultValue = "0" },

                        new UdsParameter { Name = "帧ID2", Description = "0x", TextBoxName = "YJ_ID2", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位2", Description = "", TextBoxName = "YJ_Startbit2", DefaultValue = "33" },
                        new UdsParameter { Name = "长度2", Description = "", TextBoxName = "YJ_Len2", DefaultValue = "7" },
                        new UdsParameter { Name = "精度2", Description = "", TextBoxName = "YJ_Factor2", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量2", Description = "", TextBoxName = "YJ_Offset2", DefaultValue = "0" }

                    }

                });


            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软件版本号",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "帧ID1", Description = "0x", TextBoxName = "RJ_ID1", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位1", Description = "", TextBoxName = "RJ_Startbit1", DefaultValue = "33" },
                        new UdsParameter { Name = "长度1", Description = "", TextBoxName = "RJ_Len1", DefaultValue = "7" },
                        new UdsParameter { Name = "精度1", Description = "", TextBoxName = "RJ_Factor1", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量1", Description = "", TextBoxName = "RJ_Offset1", DefaultValue = "0" },

                        new UdsParameter { Name = "帧ID2", Description = "0x", TextBoxName = "RJ_ID2", DefaultValue = "346" },
                        new UdsParameter { Name = "起始位2", Description = "", TextBoxName = "RJ_Startbit2", DefaultValue = "33" },
                        new UdsParameter { Name = "长度2", Description = "", TextBoxName = "RJ_Len2", DefaultValue = "7" },
                        new UdsParameter { Name = "精度2", Description = "", TextBoxName = "RJ_Factor2", DefaultValue = "1" },
                        new UdsParameter { Name = "偏移量2", Description = "", TextBoxName = "RJ_Offset2", DefaultValue = "0" }

                    }

                });

                
            }


            if(ConfigHelper.UdsParameters.jiesuo == "SERES_V32")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软硬件版本条件判断",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "软件", Description = "0x", TextBoxName = "RJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "硬件", Description = "0x", TextBoxName = "YJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "零件号", Description = "0x", TextBoxName = "LJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "ECU流水编号", Description = "0x", TextBoxName = "ECU_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "Bootloder", Description = "0x", TextBoxName = "BOOT_DATA", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "序号代码", Description = "0x", TextBoxName = "XH_DATA", DefaultValue = "31" },
                         new UdsParameter { Name = "客户软件", Description = "0x", TextBoxName = "KHRJ_DATA", DefaultValue = "31" },
                          new UdsParameter { Name = "报告名称", Description = "0x", TextBoxName = "REPORT", DefaultValue = "01 64 80" }
                    }

                });
            }

            else if (ConfigHelper.UdsParameters.jiesuo == "T09_M")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软硬件版本条件判断",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "软件", Description = "0x", TextBoxName = "RJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "硬件", Description = "0x", TextBoxName = "YJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "客户软件", Description = "0x", TextBoxName = "KHRJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "客户硬件", Description = "0x", TextBoxName = "KHYJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "零件号", Description = "0x", TextBoxName = "LJ_DATA", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "Bootloder", Description = "0x", TextBoxName = "BOOT_DATA", DefaultValue = "01 64 80" },
                         new UdsParameter { Name = "报告名称", Description = "0x", TextBoxName = "REPORT", DefaultValue = "01 64 80" }
                    }

                });
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软硬件版本条件判断",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "OEM软件", Description = "0x", TextBoxName = "RJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "OEM硬件", Description = "0x", TextBoxName = "YJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "供应商软件", Description = "0x", TextBoxName = "KHRJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "供应商硬件", Description = "0x", TextBoxName = "KHYJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "自定义软件", Description = "0x", TextBoxName = "LJ_DATA", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "Bootloder", Description = "0x", TextBoxName = "BOOT_DATA", DefaultValue = "01 64 80" }
                    }

                });
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软硬件版本条件判断",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "ECU软件", Description = "0x", TextBoxName = "RJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "ECU硬件", Description = "0x", TextBoxName = "YJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "供应商软件", Description = "0x", TextBoxName = "KHRJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "供应商硬件", Description = "0x", TextBoxName = "KHYJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "零件号", Description = "0x", TextBoxName = "LJ_DATA", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "Bootloder", Description = "0x", TextBoxName = "BOOT_DATA", DefaultValue = "01 64 80" },
                         new UdsParameter { Name = "报告名称", Description = "0x", TextBoxName = "REPORT", DefaultValue = "01 64 80" }
                    }

                });
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软硬件版本条件判断",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "软件版本号", Description = "0x", TextBoxName = "RJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "供应商软件版本号", Description = "0x", TextBoxName = "KHRJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "LBMS硬件版本号", Description = "0x", TextBoxName = "YJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "OEM硬件基线号", Description = "0x", TextBoxName = "KHYJ_DATA", DefaultValue = "31" },
                         new UdsParameter { Name = "报告名称", Description = "0x", TextBoxName = "REPORT", DefaultValue = "01 64 80" }
                    }

                });
            }
            else
            {
                testItem.Add(new UdsTestItem
                {
                    TestName = "软硬件版本条件判断",
                    Parameters = new List<UdsParameter>
                    {
                        new UdsParameter { Name = "客户软件", Description = "0x", TextBoxName = "RJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "客户硬件", Description = "0x", TextBoxName = "YJ_DATA", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "Bootloder", Description = "0x", TextBoxName = "BOOT_DATA", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "供应商软件版本号", Description = "0x", TextBoxName = "KHYJ_DATA", DefaultValue = "31" },
                        new UdsParameter { Name = "Soft_ID", Description = "0x", TextBoxName = "Soft_ID", DefaultValue = "01 64 80" },
                        new UdsParameter { Name = "报告名称", Description = "0x", TextBoxName = "REPORT", DefaultValue = "01 64 80" }
                    }

                });
            }
                


        }

        // 配置类用于序列化
        public class UdsConfig
        {
            #region 帧ID
            [UdsParameter("TID")]
            public string TID { get; set; }
            [UdsParameter("RID")]
            public string RID { get; set; }
            #endregion
            #region 解锁
            [UdsParameter("jiesuo")]
            public string jiesuo { get; set; }
            [UdsParameter("xieyi")]
            public string xieyi { get; set; }

            [UdsParameter("WC_ID")]
            public string WC_ID { get; set; }
            [UdsParameter("chanpin")]
            public string chanpin { get; set; }
            #endregion
            #region 产品信息读取
            [UdsParameter("PRID1")]
            public string PRID1 { get; set; }
            [UdsParameter("PRID2")]
            public string PRID2 { get; set; }
            [UdsParameter("PRID3")]
            public string PRID3 { get; set; }
            [UdsParameter("PRSTARTBIT1")]
            public string PRSTARTBIT1 { get; set; }
            [UdsParameter("PRSTARTBIT2")]
            public string PRSTARTBIT2 { get; set; }
            [UdsParameter("PRSTARTBIT3")]
            public string PRSTARTBIT3 { get; set; }
            [UdsParameter("PRLEN1")]
            public string PRLEN1 { get; set; }
            [UdsParameter("PRLEN2")]
            public string PRLEN2 { get; set; }
            [UdsParameter("PRLEN3")]
            public string PRLEN3 { get; set; }
            [UdsParameter("PRFACTOR1")]
            public string PRFACTOR1 { get; set; }
            [UdsParameter("PRFACTOR2")]
            public string PRFACTOR2 { get; set; }
            [UdsParameter("PRFACTOR3")]
            public string PRFACTOR3 { get; set; }
            #endregion
            #region PCB温度测试
            [UdsParameter("PCBT_ID")]
            public string PCBT_ID { get; set; }
            [UdsParameter("PCBT_Startbit")]
            public string PCBT_Startbit { get; set; }
            [UdsParameter("PCBT_Len")]
            public string PCBT_Len { get; set; }
            [UdsParameter("PCBT_Factor")]
            public string PCBT_Factor { get; set; }
            [UdsParameter("PCBT_Offset")]
            public string PCBT_Offset { get; set; }

            [UdsParameter("PCBT_SID")]
            public string PCBT_SID { get; set; }
            [UdsParameter("PCBT_DID")]
            public string PCBT_DID { get; set; }

            [UdsParameter("MOSCT_ID")]
            public string MOSCT_ID { get; set; }
            [UdsParameter("MOSCT_Startbit")]
            public string MOSCT_Startbit { get; set; }
            [UdsParameter("MOSCT_Len")]
            public string MOSCT_Len { get; set; }
            [UdsParameter("MOSCT_Factor")]
            public string MOSCT_Factor { get; set; }
            [UdsParameter("MOSCT_Offset")]
            public string MOSCT_Offset { get; set; }

            [UdsParameter("MOSFT_ID")]
            public string MOSFT_ID { get; set; }
            [UdsParameter("MOSFT_Startbit")]
            public string MOSFT_Startbit { get; set; }
            [UdsParameter("MOSFT_Len")]
            public string MOSFT_Len { get; set; }
            [UdsParameter("MOSFT_Factor")]
            public string MOSFT_Factor { get; set; }
            [UdsParameter("MOSFT_Offset")]
            public string MOSFT_Offset { get; set; }

            [UdsParameter("XLT_ID")]
            public string XLT_ID { get; set; }
            [UdsParameter("XLT_Startbit")]
            public string XLT_Startbit { get; set; }
            [UdsParameter("XLT_Len")]
            public string XLT_Len { get; set; }
            [UdsParameter("XLT_Factor")]
            public string XLT_Factor { get; set; }
            [UdsParameter("XLT_Offset")]
            public string XLT_Offset { get; set; }

            [UdsParameter("MOSCT_SID")]
            public string MOSCT_SID { get; set; }
            [UdsParameter("MOSCT_DID")]
            public string MOSCT_DID { get; set; }

            [UdsParameter("MOSFT_SID")]
            public string MOSFT_SID { get; set; }
            [UdsParameter("MOSFT_DID")]
            public string MOSFT_DID { get; set; }

            [UdsParameter("XLT_SID")]
            public string XLT_SID { get; set; }
            [UdsParameter("XLT_DID")]
            public string XLT_DID { get; set; }
            #endregion
            #region MOS
            [UdsParameter("MOSCV_SID")]
            public string MOSCV_SID { get; set; }
            [UdsParameter("MOSCV_DID")]
            public string MOSCV_DID { get; set; }

            [UdsParameter("MOSCV_ID")]
            public string MOSCV_ID { get; set; }
            [UdsParameter("MOSCV_Startbit")]
            public string MOSCV_Startbit { get; set; }
            [UdsParameter("MOSCV_Len")]
            public string MOSCV_Len { get; set; }
            [UdsParameter("MOSCV_Factor")]
            public string MOSCV_Factor { get; set; }
            [UdsParameter("MOSCV_Offset")]
            public string MOSCV_Offset { get; set; }



            [UdsParameter("MOSFV_SID")]
            public string MOSFV_SID { get; set; }
            [UdsParameter("MOSFV_DID")]
            public string MOSFV_DID { get; set; }

            [UdsParameter("MOSFV_ID")]
            public string MOSFV_ID { get; set; }
            [UdsParameter("MOSFV_Startbit")]
            public string MOSFV_Startbit { get; set; }
            [UdsParameter("MOSFV_Len")]
            public string MOSFV_Len { get; set; }
            [UdsParameter("MOSFV_Factor")]
            public string MOSFV_Factor { get; set; }
            [UdsParameter("MOSFV_Offset")]
            public string MOSFV_Offset { get; set; }



            [UdsParameter("MOSControl_SID")]
            public string MOSControl_SID { get; set; }
            [UdsParameter("MOSControl_DID")]
            public string MOSControl_DID { get; set; }
            #endregion
            #region P+
            [UdsParameter("PZ_ID")]
            public string PZ_ID { get; set; }
            [UdsParameter("PZ_Startbit")]
            public string PZ_Startbit { get; set; }
            [UdsParameter("PZ_Len")]
            public string PZ_Len { get; set; }
            [UdsParameter("PZ_Factor")]
            public string PZ_Factor { get; set; }
            [UdsParameter("PZ_Offset")]
            public string PZ_Offset { get; set; }
            #endregion
            #region MCU电压
            [UdsParameter("MCUCV_SID")]
            public string MCUCV_SID { get; set; }
            [UdsParameter("MCUCV_DID")]
            public string MCUCV_DID { get; set; }

            [UdsParameter("MCUFV_SID")]
            public string MCUFV_SID { get; set; }
            [UdsParameter("MCUFV_DID")]
            public string MCUFV_DID { get; set; }
            [UdsParameter("MCUCV_ID")]
            public string MCUCV_ID { get; set; }
            [UdsParameter("MCUCV_Startbit")]
            public string MCUCV_Startbit { get; set; }
            [UdsParameter("MCUCV_Len")]
            public string MCUCV_Len { get; set; }
            [UdsParameter("MCUCV_Factor")]
            public string MCUCV_Factor { get; set; }
            [UdsParameter("MCUCV_Offset")]
            public string MCUCV_Offset { get; set; }
            #endregion
            #region 电流标定
            [UdsParameter("DLB_SID")]
            public string DLB_SID { get; set; }
            [UdsParameter("DLB_DID")]
            public string DLB_DID { get; set; }
            #endregion
            #region 电芯电压
            [UdsParameter("DXV1_ID")]
            public string DXV1_ID { get; set; }
            [UdsParameter("DXV1_Startbit")]
            public string DXV1_Startbit { get; set; }
            [UdsParameter("DXV1_Len")]
            public string DXV1_Len { get; set; }
            [UdsParameter("DXV1_Factor")]
            public string DXV1_Factor { get; set; }
            [UdsParameter("DXV1_Offset")]
            public string DXV1_Offset { get; set; }

            [UdsParameter("DXV2_ID")]
            public string DXV2_ID { get; set; }
            [UdsParameter("DXV2_Startbit")]
            public string DXV2_Startbit { get; set; }
            [UdsParameter("DXV2_Len")]
            public string DXV2_Len { get; set; }
            [UdsParameter("DXV2_Factor")]
            public string DXV2_Factor { get; set; }
            [UdsParameter("DXV2_Offset")]
            public string DXV2_Offset { get; set; }

            [UdsParameter("DXV3_ID")]
            public string DXV3_ID { get; set; }
            [UdsParameter("DXV3_Startbit")]
            public string DXV3_Startbit { get; set; }
            [UdsParameter("DXV3_Len")]
            public string DXV3_Len { get; set; }
            [UdsParameter("DXV3_Factor")]
            public string DXV3_Factor { get; set; }
            [UdsParameter("DXV3_Offset")]
            public string DXV3_Offset { get; set; }

            [UdsParameter("DXV4_ID")]
            public string DXV4_ID { get; set; }
            [UdsParameter("DXV4_Startbit")]
            public string DXV4_Startbit { get; set; }
            [UdsParameter("DXV4_Len")]
            public string DXV4_Len { get; set; }
            [UdsParameter("DXV4_Factor")]
            public string DXV4_Factor { get; set; }
            [UdsParameter("DXV4_Offset")]
            public string DXV4_Offset { get; set; }
            #endregion
            #region 内外总压
            [UdsParameter("NZV_ID")]
            public string NZV_ID { get; set; }
            [UdsParameter("NZV_Startbit")]
            public string NZV_Startbit { get; set; }
            [UdsParameter("NZV_Len")]
            public string NZV_Len { get; set; }
            [UdsParameter("NZV_Factor")]
            public string NZV_Factor { get; set; }
            [UdsParameter("NZV_Offset")]
            public string NZV_Offset { get; set; }

            [UdsParameter("WZV_ID")]
            public string WZV_ID { get; set; }
            [UdsParameter("WZV_Startbit")]
            public string WZV_Startbit { get; set; }
            [UdsParameter("WZV_Len")]
            public string WZV_Len { get; set; }
            [UdsParameter("WZV_Factor")]
            public string WZV_Factor { get; set; }
            [UdsParameter("WZV_Offset")]
            public string WZV_Offset { get; set; }
            #endregion
            #region 电流
            [UdsParameter("DL_ID")]
            public string DL_ID { get; set; }
            [UdsParameter("DL_Startbit")]
            public string DL_Startbit { get; set; }
            [UdsParameter("DL_Len")]
            public string DL_Len { get; set; }
            [UdsParameter("DL_Factor")]
            public string DL_Factor { get; set; }
            [UdsParameter("DL_Offset")]
            public string DL_Offset { get; set; }

            [UdsParameter("DL_SID")]
            public string DL_SID { get; set; }
            [UdsParameter("DL_DID")]
            public string DL_DID { get; set; }
            #endregion
            #region 电芯温度
            [UdsParameter("DXT1_ID")]
            public string DXT1_ID { get; set; }
            [UdsParameter("DXT1_Startbit")]
            public string DXT1_Startbit { get; set; }
            [UdsParameter("DXT1_Len")]
            public string DXT1_Len { get; set; }
            [UdsParameter("DXT1_Factor")]
            public string DXT1_Factor { get; set; }
            [UdsParameter("DXT1_Offset")]
            public string DXT1_Offset { get; set; }

            [UdsParameter("DXT1_SID")]
            public string DXT1_SID { get; set; }
            [UdsParameter("DXT1_DID")]
            public string DXT1_DID { get; set; }

            [UdsParameter("DXT2_ID")]
            public string DXT2_ID { get; set; }
            [UdsParameter("DXT2_Startbit")]
            public string DXT2_Startbit { get; set; }
            [UdsParameter("DXT2_Len")]
            public string DXT2_Len { get; set; }
            [UdsParameter("DXT2_Factor")]
            public string DXT2_Factor { get; set; }
            [UdsParameter("DXT2_Offset")]
            public string DXT2_Offset { get; set; }

            [UdsParameter("DXT2_SID")]
            public string DXT2_SID { get; set; }
            [UdsParameter("DXT2_DID")]
            public string DXT2_DID { get; set; }

            [UdsParameter("DXT3_ID")]
            public string DXT3_ID { get; set; }
            [UdsParameter("DXT3_Startbit")]
            public string DXT3_Startbit { get; set; }
            [UdsParameter("DXT3_Len")]
            public string DXT3_Len { get; set; }
            [UdsParameter("DXT3_Factor")]
            public string DXT3_Factor { get; set; }
            [UdsParameter("DXT3_Offset")]
            public string DXT3_Offset { get; set; }

            [UdsParameter("DXT3_SID")]
            public string DXT3_SID { get; set; }
            [UdsParameter("DXT3_DID")]
            public string DXT3_DID { get; set; }
            #endregion
            #region 均衡测试
            [UdsParameter("JHStart_SID")]
            public string JHStart_SID { get; set; }
            [UdsParameter("JHStart_DID")]
            public string JHStart_DID { get; set; }

            [UdsParameter("JHEnd_SID")]
            public string JHEnd_SID { get; set; }
            [UdsParameter("JHEnd_DID")]
            public string JHEnd_DID { get; set; }
            #endregion
            #region 唤醒
            [UdsParameter("HX_ID")]
            public string HX_ID { get; set; }
            [UdsParameter("HX_Data")]
            public string HX_Data { get; set; }
            #endregion
            #region 休眠  
            [UdsParameter("XM1_SID")]
            public string XM1_SID { get; set; }
            [UdsParameter("XM1_DID")]
            public string XM1_DID { get; set; }

            [UdsParameter("XM2_SID")]
            public string XM2_SID { get; set; }
            [UdsParameter("XM2_DID")]
            public string XM2_DID { get; set; }
            #endregion
            #region 获取唤醒源  
            [UdsParameter("GetHX_SID")]
            public string GetHX_SID { get; set; }
            [UdsParameter("GetHX_DID")]
            public string GetHX_DID { get; set; }


            [UdsParameter("GetHX_ID")]
            public string GetHX_ID { get; set; }
            [UdsParameter("GetHX_Startbit")]
            public string GetHX_Startbit { get; set; }
            [UdsParameter("GetHX_Len")]
            public string GetHX_Len { get; set; }
            [UdsParameter("GetHX_Factor")]
            public string GetHX_Factor { get; set; }
            [UdsParameter("GetHX_Offset")]
            public string GetHX_Offset { get; set; }


            [UdsParameter("GetHX_RTC")]
            public string GetHX_RTC { get; set; }
            [UdsParameter("GetHX_GY")]
            public string GetHX_GY { get; set; }
            [UdsParameter("GetHX_YJ")]
            public string GetHX_YJ { get; set; }
            [UdsParameter("GetHX_DL")]
            public string GetHX_DL { get; set; }
            [UdsParameter("GetHX_CAN")]
            public string GetHX_CAN { get; set; }
            #endregion
            #region 清除AH  
            [UdsParameter("ClearAH_SID")]
            public string ClearAH_SID { get; set; }
            [UdsParameter("ClearAH_DID")]
            public string ClearAH_DID { get; set; }
            #endregion
            #region 限流模块
            [UdsParameter("XLOpen_SID")]
            public string XLOpen_SID { get; set; }
            [UdsParameter("XLOpen_DID")]
            public string XLOpen_DID { get; set; }

            [UdsParameter("XLEnd_SID")]
            public string XLEnd_SID { get; set; }
            [UdsParameter("XLEnd_DID")]
            public string XLEnd_DID { get; set; }
            #endregion
            #region EEPROM  
            [UdsParameter("EEPROM_SID")]
            public string EEPROM_SID { get; set; }
            [UdsParameter("EEPROM_DID")]
            public string EEPROM_DID { get; set; }
            #endregion
            #region SOC SOH 
            [UdsParameter("SOCT_SID")]
            public string SOCT_SID { get; set; }
            [UdsParameter("SOCT_DID")]
            public string SOCT_DID { get; set; }

            [UdsParameter("SOCR_ID")]
            public string SOCR_ID { get; set; }
            [UdsParameter("SOCR_Startbit")]
            public string SOCR_Startbit { get; set; }
            [UdsParameter("SOCR_Len")]
            public string SOCR_Len { get; set; }
            [UdsParameter("SOCR_Factor")]
            public string SOCR_Factor { get; set; }
            [UdsParameter("SOCR_Offset")]
            public string SOCR_Offset { get; set; }

            [UdsParameter("SOHR_ID")]
            public string SOHR_ID { get; set; }
            [UdsParameter("SOHR_Startbit")]
            public string SOHR_Startbit { get; set; }
            [UdsParameter("SOHR_Len")]
            public string SOHR_Len { get; set; }
            [UdsParameter("SOHR_Factor")]
            public string SOHR_Factor { get; set; }
            [UdsParameter("SOHR_Offset")]
            public string SOHR_Offset { get; set; }
            #endregion
            #region 均衡与AFE诊断测试
            [UdsParameter("AFE_SID")]
            public string AFE_SID { get; set; }
            [UdsParameter("AFE_DID")]
            public string AFE_DID { get; set; }
            #endregion
            #region 产品信息
            [UdsParameter("PCH_Date")]
            public string PCH_Date { get; set; }
            [UdsParameter("YJ_Date")]
            public string YJ_Date { get; set; }
            [UdsParameter("RJ_Date")]
            public string RJ_Date { get; set; }
            #endregion
            #region 软件版本
            [UdsParameter("RJ_ID1")]
            public string RJ_ID1 { get; set; }
            [UdsParameter("RJ_Startbit1")]
            public string RJ_Startbit1 { get; set; }
            [UdsParameter("RJ_Len1")]
            public string RJ_Len1 { get; set; }
            [UdsParameter("RJ_Factor1")]
            public string RJ_Factor1 { get; set; }
            [UdsParameter("RJ_Offset1")]
            public string RJ_Offset1 { get; set; }

            [UdsParameter("YJ_ID")]
            public string YJ_ID { get; set; }
            [UdsParameter("YJ_Startbit")]
            public string YJ_Startbit { get; set; }
            [UdsParameter("YJ_Len")]
            public string YJ_Len { get; set; }
            [UdsParameter("YJ_Factor")]
            public string YJ_Factor { get; set; }
            [UdsParameter("YJ_Offset")]
            public string YJ_Offset { get; set; }

            [UdsParameter("YJ_ID2")]
            public string YJ_ID2 { get; set; }
            [UdsParameter("YJ_Startbit2")]
            public string YJ_Startbit2 { get; set; }
            [UdsParameter("YJ_Len2")]
            public string YJ_Len2 { get; set; }
            [UdsParameter("YJ_Factor2")]
            public string YJ_Factor2 { get; set; }
            [UdsParameter("YJ_Offset2")]
            public string YJ_Offset2 { get; set; }

            [UdsParameter("RJ_SID")]
            public string RJ_SID { get; set; }
            [UdsParameter("RJ_DID")]
            public string RJ_DID { get; set; }

            [UdsParameter("YJ_SID")]
            public string YJ_SID { get; set; }
            [UdsParameter("YJ_DID")]
            public string YJ_DID { get; set; }

            [UdsParameter("RJ_DATA")]
            public string RJ_DATA { get; set; }
            [UdsParameter("YJ_DATA")]
            public string YJ_DATA { get; set; }

            [UdsParameter("BOOT_DATA")]
            public string BOOT_DATA { get; set; }

            [UdsParameter("Soft_ID")]
            public string Soft_ID { get; set; }


            [UdsParameter("LJ_DATA")]
            public string LJ_DATA { get; set; }

            [UdsParameter("XH_DATA")]
            public string XH_DATA { get; set; }

            [UdsParameter("KHRJ_DATA")]
            public string KHRJ_DATA { get; set; }
            [UdsParameter("KHYJ_DATA")]
            public string KHYJ_DATA { get; set; }

            [UdsParameter("ECU_DATA")]
            public string ECU_DATA { get; set; }

            [UdsParameter("REPORT")]
            public string REPORT { get; set; }

            [UdsParameter("RJ_ID2")]
            public string RJ_ID2 { get; set; }
            [UdsParameter("RJ_Startbit2")]
            public string RJ_Startbit2 { get; set; }
            [UdsParameter("RJ_Len2")]
            public string RJ_Len2 { get; set; }
            [UdsParameter("RJ_Factor2")]
            public string RJ_Factor2 { get; set; }
            [UdsParameter("RJ_Offset2")]
            public string RJ_Offset2 { get; set; }
            #endregion
        }




        // 在InitializeComponent之后添加发送指令的UI创建
        private void InitializeUdsTestParameters()
        {
            _udsParameterControls.Clear();
            UdsParameterContainer.Children.Clear();

            var testItems = new List<UdsTestItem>();


            AddTestItem(testItems);
            // 定义测试项及其UDS参数
            

            _udsTestItems = testItems;

            // 添加标题
            var header = new TextBlock
            {
                Text = "协议参数配置",
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 0, 0, 15),
                Foreground = new SolidColorBrush(Colors.SteelBlue)
            };
            UdsParameterContainer.Children.Add(header);

            // 为每个测试项创建UI元素
            foreach (var testItem in testItems)
            {
                // 测试项标题
                var testHeader = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(240, 245, 250)),
                    BorderBrush = new SolidColorBrush(Colors.LightGray),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(10, 8, 10, 8),
                    Child = new TextBlock
                    {
                        Text = testItem.TestName,
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 14,
                        Foreground = new SolidColorBrush(Colors.DarkSlateBlue)
                    }
                };
                UdsParameterContainer.Children.Add(testHeader);

                // 参数网格
                var paramGrid = new UniformGrid
                {
                    Columns = 5,
                    Margin = new Thickness(15, 10, 15, 15)
                };

                foreach (var param in testItem.Parameters)
                {
                    var paramPanel = new StackPanel
                    {
                        Margin = new Thickness(0, 0, 15, 10),
                        Orientation = Orientation.Vertical
                    };

                    // 参数标签
                    paramPanel.Children.Add(new TextBlock
                    {
                        Text = param.Name,
                        FontWeight = FontWeights.Medium,
                        Margin = new Thickness(0, 0, 0, 3)
                    });

                    // 参数描述
                    paramPanel.Children.Add(new TextBlock
                    {
                        Text = param.Description,
                        FontSize = 12,
                        Foreground = new SolidColorBrush(Colors.Gray),
                        Margin = new Thickness(0, 0, 0, 5)
                    });

                    // 参数输入框
                    var textBox = new TextBox
                    {
                        Name = param.TextBoxName,
                        Text = param.DefaultValue,
                        Margin = new Thickness(0, 0, 0, 2),
                        Padding = new Thickness(5),
                        VerticalContentAlignment = VerticalAlignment.Center,
                      
                    };
                    paramPanel.Children.Add(textBox);
                    _udsParameterControls.Add(textBox.Name, textBox);

                    paramGrid.Children.Add(paramPanel);
                }

                ImportConfig_Open();

                UdsParameterContainer.Children.Add(paramGrid);
            }




            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 20, 0, 10)
            };

            // 导入按钮
            var importButton = new Button
            {
                Content = "导入配置",
                Margin = new Thickness(0, 0, 15, 0),
                Padding = new Thickness(12, 5, 12, 5),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.SteelBlue),
                ToolTip = "从文件导入UDS配置"
            };
            importButton.Click += ImportConfig_Click;
            buttonPanel.Children.Add(importButton);

            // 导出按钮
            var exportButton = new Button
            {
                Content = "导出配置",
             
                Margin = new Thickness(0, 0, 15, 0),
                Padding = new Thickness(12, 5, 12, 5),
                Background = new SolidColorBrush(Colors.White),
                BorderBrush = new SolidColorBrush(Colors.SteelBlue),
                ToolTip = "将当前配置导出到文件"
            };
            exportButton.Click += ExportConfig_Click;
            buttonPanel.Children.Add(exportButton);

            // 保存按钮
            var saveButton = new Button
            {
                Content = "保存配置",
           
                Padding = new Thickness(12, 5, 12, 5),
                Background = new SolidColorBrush(Colors.SteelBlue),
                Foreground = new SolidColorBrush(Colors.White),
                ToolTip = "保存配置到默认位置"
            };
            saveButton.Click += SaveUdsParameters_Click;
            buttonPanel.Children.Add(saveButton);

            UdsParameterContainer.Children.Add(buttonPanel);



        }

        
        //导入配置文件
        private void ImportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建打开文件对话框
                var openFileDialog = new OpenFileDialog
                {
                    Title = "导入UDS配置文件",
                    Filter = "JSON配置文件|*.json|所有文件|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Multiselect = false
                };

                // 显示对话框
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // 读取文件
                    string json = File.ReadAllText(filePath);
                    var config = JsonConvert.DeserializeObject<UdsConfig>(json);

                    // 验证配置文件

                    UpdateStaticParameters(config);
                        // 更新界面
                        UpdateInterfaceFromConfig(config);
               
                        MessageBox.Show($"配置已成功导入: {System.IO.Path.GetFileName(filePath)}", "导入成功",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                   
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入配置文件时出错:\n{ex.Message}", "导入错误",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // 导出配置文件
        private void ExportConfig_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                

                // 创建保存文件对话框
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "导出UDS配置文件",
                    Filter = "JSON配置文件|*.json|所有文件|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    FileName = $"UDS_Config_{DateTime.Now:yyyyMMdd_HHmmss}.json",
                    DefaultExt = ".json",
                    AddExtension = true
                };

                // 显示对话框
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // 创建配置对象
                    var config = CreateConfigFromStaticParameters();

                    // 序列化为JSON
                    string json = JsonConvert.SerializeObject(config, Formatting.Indented);

                    // 保存文件
                    File.WriteAllText(filePath, json, Encoding.UTF8);

                    MessageBox.Show($"配置已成功导出到:\n{filePath}", "导出成功",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出配置文件时出错:\n{ex.Message}", "导出错误",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public static class UdsParameterMapper
        {
            // 从界面保存到静态变量
            public static void SaveToStaticVariables(Dictionary<string, TextBox> controls)
            {
                if (controls == null) return;

                Type staticType = typeof(UdsParameters);

                // 获取所有静态公共字段（非属性）
                var fields = staticType.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var field in fields)
                {
                    var attr = field.GetCustomAttribute<UdsParameterAttribute>();
                    if (attr != null && controls.TryGetValue(attr.TextBoxName, out TextBox textBox))
                    {
                        try
                        {
                            // 确保处理空值
                            string value = textBox?.Text?.Trim() ?? string.Empty;
                            field.SetValue(null, value);
                            Debug.WriteLine($"成功设置 {field.Name} = {value}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"设置 {field.Name} 时出错: {ex.Message}");
                        }
                    }
                    else if (attr != null)
                    {
                        Debug.WriteLine($"未找到文本框: {attr.TextBoxName}");
                    }
                }
            }

            // 从静态变量创建配置对象
            public static UdsConfig CreateConfigFromStaticParameters()
            {
                var config = new UdsConfig();
                Type configType = typeof(UdsConfig);
                Type staticType = typeof(UdsParameters);

                foreach (var prop in configType.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<UdsParameterAttribute>();
                    if (attr != null)
                    {
                        // 从静态字段获取值
                        var field = staticType.GetField(attr.TextBoxName, BindingFlags.Public | BindingFlags.Static);
                        if (field != null)
                        {
                            prop.SetValue(config, field.GetValue(null));
                        }
                    }
                }

                return config;
            }

            // 从配置对象更新静态变量
            public static void UpdateStaticParameters(UdsConfig config)
            {
                Type configType = config.GetType();
                Type staticType = typeof(UdsParameters);

                foreach (var prop in configType.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<UdsParameterAttribute>();
                    if (attr != null)
                    {
                        // 设置静态字段的值
                        var field = staticType.GetField(attr.TextBoxName, BindingFlags.Public | BindingFlags.Static);
                        if (field != null)
                        {
                            field.SetValue(null, prop.GetValue(config));
                        }
                    }
                }
            }

            // 从配置对象更新界面
            public static void UpdateInterfaceFromConfig(UdsConfig config, Dictionary<string, TextBox> controls)
            {
                Type configType = config.GetType();

                foreach (var prop in configType.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<UdsParameterAttribute>();
                    if (attr != null && controls.TryGetValue(attr.TextBoxName, out TextBox textBox))
                    {
                        textBox.Text = prop.GetValue(config)?.ToString() ?? string.Empty;
                    }
                }
            }

            // 从界面直接更新配置对象
            public static void UpdateConfigFromInterface(UdsConfig config, Dictionary<string, TextBox> controls)
            {
                Type configType = config.GetType();

                foreach (var prop in configType.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<UdsParameterAttribute>();
                    if (attr != null && controls.TryGetValue(attr.TextBoxName, out TextBox textBox))
                    {
                        prop.SetValue(config, textBox.Text.Trim());
                    }
                }
            }
        }

        private void CheckParameterMapping()
        {
            // 检查所有标记的静态字段
            Type staticType = typeof(UdsParameters);
            var fields = staticType.GetFields(BindingFlags.Public | BindingFlags.Static);

            Debug.WriteLine("=== 静态参数字段检查 ===");
            foreach (var field in fields)
            {
                var attr = field.GetCustomAttribute<UdsParameterAttribute>();
                Debug.WriteLine($"{field.Name}: {(attr != null ? $"映射到 {attr.TextBoxName}" : "未映射")}");
            }

            // 检查控件字典
            Debug.WriteLine("=== 控件字典检查 ===");
            foreach (var kvp in _udsParameterControls)
            {
                Debug.WriteLine($"控件: {kvp.Key}, 值: {kvp.Value.Text}");
            }
        }

        public void SaveParametersToStaticVariables()
        {
            UdsParameterMapper.SaveToStaticVariables(_udsParameterControls);
        }

        // 从静态变量创建配置对象
        private UdsConfig CreateConfigFromStaticParameters()
        {
            return UdsParameterMapper.CreateConfigFromStaticParameters();
        }
        // 从配置对象更新静态变量
        private void UpdateStaticParameters(UdsConfig config)
        {
            UdsParameterMapper.UpdateStaticParameters(config);
        }

        // 从配置对象更新界面
        private void UpdateInterfaceFromConfig(UdsConfig config)
        {
            UdsParameterMapper.UpdateInterfaceFromConfig(config, _udsParameterControls);
        }
        // 修改保存按钮功能
        private void SaveUdsParameters_Click(object sender, RoutedEventArgs e)
        {
           
            try
            {
                CheckParameterMapping();
                // 保存到静态变量
                SaveParametersToStaticVariables();

                Debug.WriteLine("=== 保存后静态变量值 ===");
                Debug.WriteLine($"TID = {UdsParameters.TID}");
                Debug.WriteLine($"RID = {UdsParameters.RID}");
                // 验证参数
                if (ValidateParameters())
                {
                    // 保存到默认位置
                    SaveToConfigFile();

                    MessageBox.Show("UDS参数保存成功！", "操作成功",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存参数时出错：{ex.Message}", "错误",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        public void ImportConfig_Open()
        {
          
            string filePath = System.IO.Directory.GetCurrentDirectory() + "\\config.json";
            // 读取并反序列化
            string json = File.ReadAllText(filePath);
            var config = JsonConvert.DeserializeObject<UdsConfig>(json);

             // 更新界面
            UpdateInterfaceFromConfig(config);

            UpdateStaticParameters(config);




        }
        public string GetTextBoxValue(string textBoxName)
        {
            if (_udsParameterControls.TryGetValue(textBoxName, out TextBox textBox))
            {
                return textBox.Text.Trim();
            }
            throw new KeyNotFoundException($"找不到文本框: {textBoxName}");
        }
        
        private bool ValidateParameters()
        {
            var errors = new List<string>();

         

            // 添加更多验证规则...

            if (errors.Count > 0)
            {
                MessageBox.Show($"参数验证失败:\n- {string.Join("\n- ", errors)}",
                              "验证错误", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidHex(string value, int expectedLength)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;

            // 移除可能的0x前缀
            string cleanValue = value.StartsWith("0x", StringComparison.OrdinalIgnoreCase)
                ? value.Substring(2)
                : value;

            // 检查长度和十六进制格式
            return cleanValue.Length == expectedLength &&
                   System.Text.RegularExpressions.Regex.IsMatch(cleanValue, "^[0-9A-Fa-f]+$");
        }

        private void SaveToConfigFile()
        {
            // 创建配置对象
            var config = CreateConfigFromStaticParameters();
            // 序列化为JSON
            string json = JsonConvert.SerializeObject(config, Formatting.Indented);

            string configPath = System.IO.Directory.GetCurrentDirectory() + "\\config.json";

            // 确保目录存在
            string configDir = System.IO.Path.GetDirectoryName(configPath);
            if (!Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            // 使用临时文件避免写入中断
            string tempPath = System.IO.Path.Combine(configDir, System.IO.Path.GetRandomFileName());

            // 写入临时文件
            File.WriteAllText(tempPath, json, Encoding.UTF8);

            // 替换原文件（原子操作）
            File.Replace(tempPath, configPath, null);

            Debug.WriteLine($"配置文件已成功保存到: {configPath}");
            // 保存到文件

        }



        //--------------------------------------------------------------分割线-----------------------------------------------------------------
        

        
        public ParSettingDialog()
        {
            InitializeComponent();

            DetectCtrlAdd();

            InitializeUdsTestParameters();

            Init();
            
            foreach (var item in CanTRTestGBStackPanel.Children)
            {
                if (item is GroupBox group)
                {
                    DetectGBs.Add(group);
                }
            }
        }


        // 动态添加 GroupBox 的方法
        private void AddGroupBoxToStackPanel(string header, UserControl contentControl)
        {
            var groupBox = new GroupBox
            {
                Style = (Style)FindResource("GroupBoxStyle"),
                Header = header,
                Content = contentControl
            };
            // 保存控件引用
          
            CanTRTestGBStackPanel.Children.Add(groupBox);
        }

    
        
        void DetectCtrlAdd()
        {
            if(ConfigHelper.SelectType  == "seres")
            {
                DisplayDetectSERES();
            }
            else if(ConfigHelper.SelectType == "lt")
            {
                DisplayDetectLT();
            }
            else if (ConfigHelper.SelectType == "geely")
            {
                DisplayDetectGEELY();
            }
            



        }

        

        void DisplayDetectSERES()
        {

            // 先清空现有内容（可选）
            CanTRTestGBStackPanel.Children.Clear();
           
            // 根据需求添加 GroupBox

            AddGroupBoxToStackPanel("终端电阻配置", new FCT.Ctrl.DetectCtrlSERES.CanTRTestParSetting());
            AddGroupBoxToStackPanel("产品信息配置", new FCT.Ctrl.DetectCtrlSERES.PIPCParSetting());
            AddGroupBoxToStackPanel("PCB温度配置", new FCT.Ctrl.DetectCtrlSERES.PCBParSetting());
            AddGroupBoxToStackPanel("MOS控制配置", new FCT.Ctrl.DetectCtrlSERES.MOSControlParSetting());
            AddGroupBoxToStackPanel("电压测试配置", new FCT.Ctrl.DetectCtrlSERES.VoltageParSetting());
            AddGroupBoxToStackPanel("电流测试配置", new FCT.Ctrl.DetectCtrlSERES.CurrParSetting());
            AddGroupBoxToStackPanel("电芯温度配置", new FCT.Ctrl.DetectCtrlSERES.CellTemParSetting());
            AddGroupBoxToStackPanel("工作与休眠电流配置", new FCT.Ctrl.DetectCtrlSERES.WAndSCurrParSetting());
            AddGroupBoxToStackPanel("均衡测试配置", new FCT.Ctrl.DetectCtrlSERES.BalancedTestParSetting());
            AddGroupBoxToStackPanel("限流模块测试配置", new FCT.Ctrl.DetectCtrlSERES.CurrentLimitingParSetting());
            AddGroupBoxToStackPanel("MOS休眠维持测试配置", new FCT.Ctrl.DetectCtrlSERES.MOSSKParSetting());
            AddGroupBoxToStackPanel("SOC与SOH测试配置", new FCT.Ctrl.DetectCtrlSERES.SOCParSetting());


        }

        void DisplayDetectLT()
        {
            // 先清空现有内容（可选）
            CanTRTestGBStackPanel.Children.Clear();

            AddGroupBoxToStackPanel("终端电阻配置", new FCT.Ctrl.DetectCtrlLT.CanTRTestParSetting());
            AddGroupBoxToStackPanel("产品信息配置", new FCT.Ctrl.DetectCtrlLT.PIPCParSetting());
            AddGroupBoxToStackPanel("PCB温度配置", new FCT.Ctrl.DetectCtrlLT.PCBParSetting());
            AddGroupBoxToStackPanel("MOS控制配置", new FCT.Ctrl.DetectCtrlLT.MOSControlParSetting());
            AddGroupBoxToStackPanel("电压测试配置", new FCT.Ctrl.DetectCtrlLT.VoltageParSetting());
            AddGroupBoxToStackPanel("电流测试配置", new FCT.Ctrl.DetectCtrlLT.CurrParSetting());
            AddGroupBoxToStackPanel("电芯温度配置", new FCT.Ctrl.DetectCtrlLT.CellTemParSetting());
            AddGroupBoxToStackPanel("工作与休眠电流配置", new FCT.Ctrl.DetectCtrlLT.WAndSCurrParSetting());
            AddGroupBoxToStackPanel("均衡测试配置", new FCT.Ctrl.DetectCtrlLT.BalancedTestParSetting());
            AddGroupBoxToStackPanel("限流模块测试配置", new FCT.Ctrl.DetectCtrlLT.CurrentLimitingParSetting());
            AddGroupBoxToStackPanel("MOS休眠维持测试配置", new FCT.Ctrl.DetectCtrlLT.MOSSKParSetting());
            AddGroupBoxToStackPanel("SOC与SOH测试配置", new FCT.Ctrl.DetectCtrlLT.SOCParSetting());

            // 根据需求添加 GroupBox


        }

        void DisplayDetectGEELY()
        {
            // 先清空现有内容（可选）
            CanTRTestGBStackPanel.Children.Clear();
            AddGroupBoxToStackPanel("工作与休眠电流配置", new FCT.Ctrl.DetectCtrlGEELY.WAndSCurrParSetting());
            AddGroupBoxToStackPanel("产品信息配置", new FCT.Ctrl.DetectCtrlGEELY.ProduceInfoParSetting());
            AddGroupBoxToStackPanel("电压测试配置", new FCT.Ctrl.DetectCtrlGEELY.VoltageParSetting());
            AddGroupBoxToStackPanel("模组温度配置", new FCT.Ctrl.DetectCtrlGEELY.ModuleTemParSetting());
            AddGroupBoxToStackPanel("PCB温度配置", new FCT.Ctrl.DetectCtrlGEELY.PCBParSetting());
            AddGroupBoxToStackPanel("均衡测试配置", new FCT.Ctrl.DetectCtrlGEELY.BalancedTestParSetting());
            AddGroupBoxToStackPanel("高驱与继电器控制配置", new FCT.Ctrl.DetectCtrlGEELY.MOSControlParSetting());
            AddGroupBoxToStackPanel("电流测试配置", new FCT.Ctrl.DetectCtrlGEELY.CurrParSetting());
            AddGroupBoxToStackPanel("SOC与SOH测试配置", new FCT.Ctrl.DetectCtrlGEELY.SOCParSetting());
            AddGroupBoxToStackPanel("故障信息配置", new FCT.Ctrl.DetectCtrlGEELY.ErrorParSetting());
            // 根据需求添加 GroupBox


        }

        void Init()
        {
            DalBase dalBase = new DalBase();
            #region 设备连接参数
            var ports = System.IO.Ports.SerialPort.GetPortNames();

            using (var rmSession = new NationalInstruments.Visa.ResourceManager())
            {
                try
                {
                    var resources = rmSession.Find("(ASRL|GPIB|TCPIP|USB)?*");
                    cbxM3905DPort.ItemsSource = resources.Select(m => m).OrderBy(m => m);
                    cbxIT6122BPort.ItemsSource = resources.Select(m => m).OrderBy(m => m);
                }
                catch (Exception ex)
                {
                    if (ex.Message == "位置信息不足或系统中不存在设备或资源。")
                    {
                        MessageBox.Show("USB未连接");
                    }
                }
                cbxM3905DPort.SelectedItem = ConfigHelper.M3905DPort;
                cbxIT6122BPort.SelectedItem = ConfigHelper.IT6122BPort;
            }



            cbxN8331Port.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxN8331Port.SelectedItem = ConfigHelper.N8331Port;

            cbxIT6322Port.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxIT6322Port.SelectedItem = ConfigHelper.IT6322Port;

            cbxN69206HPort.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxN69206HPort.SelectedItem = ConfigHelper.N69206HPort;

            cbxN36200Port.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxN36200Port.SelectedItem = ConfigHelper.N36200Port;

            cbxGunPort.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxGunPort.SelectedItem = ConfigHelper.GunPort;

            cbxRelayPort.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxRelayPort.SelectedItem = ConfigHelper.RelayPort;

            cbxToolControlPort.ItemsSource = ports.Select(m => m).OrderBy(m => m);
            cbxToolControlPort.SelectedItem = ConfigHelper.ToolControlPort;

            if (CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400") != null)
                txtNXI6400IP.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400").CommValue.Split('&')[1] ?? "";

            if (CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400_2") != null)
                txtNXI6400IP2.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400_2").CommValue.Split('&')[1] ?? "";

            #endregion
            #region 测试流程
            var detects = DetectsSERES();
            if (ConfigHelper.SelectType == "seres")
            {
                detects = DetectsSERES();
            }
            else if (ConfigHelper.SelectType == "lt")
            {
                detects = DetectsLT();
            }
            else if (ConfigHelper.SelectType == "geely")
            {
                detects = DetectsGEELY();
            }
            
            detectListBox.ItemsSource = detects;

            var selectItems = detects.Where(m => (CommonData.CurrentUserDetects.Select(x => x.Item1).ToList()).Contains(m.Item1));
            foreach (var item in selectItems)
            {
                //var temp = detect.ItemsSource.Cast<Tuple<string, string>>().FirstOrDefault(m => m.Item2 == item.Item1);

                detectListBox.SelectedItems.Add(item);
            }

            users.ItemsSource = UserDal.GetAllUser();
            if (users.Items.Count > 0)
            {
                users.SelectedIndex = 0;
            }

            testTypeCbx.ItemsSource = new List<string>() { "生产测试", "研发测试" };

            testDetectItem.Visibility = CommonData.UserRole == Role.管理员 ? Visibility.Visible : Visibility.Collapsed;

            
            #endregion

            if (System.IO.File.Exists(backFilePath))
            {
                backModel = JsonConvert.DeserializeObject<BackModel>(System.IO.File.ReadAllText(backFilePath));
                if (backModel != null)
                {
                    BackBlock.Text = backModel.BackTime.ToString();
                }
            }
            else
            {
                //System.IO.File
            }
            #region 其他参数
            toolNumberBox.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "工装编号")?.CommValue;
            stationBox.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "StationName")?.CommValue;
            rscBox.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "RscName")?.CommValue;
            uNameBox.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "UserName")?.CommValue;
            mesUrlBox.Text = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "MesURL")?.CommValue;
            produceNameBox.Text= CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "ProduceName")?.CommValue;

            dbSaveTimeBox.SelectedItem = (SaveTimeEnum)int.Parse(ConfigHelper.DBSaveTime);
            #endregion
        }


        private void SaveDevicePar_Click(object sender, RoutedEventArgs e)
        {

            ConfigHelper.IT6122BPort = cbxIT6122BPort.SelectedItem?.ToString();
            ConfigHelper.M3905DPort = cbxM3905DPort.SelectedItem?.ToString();
            ConfigHelper.N8331Port = cbxN8331Port.SelectedItem?.ToString();
            ConfigHelper.IT6322Port = cbxIT6322Port.SelectedItem?.ToString();
            ConfigHelper.N69206HPort = cbxN69206HPort.SelectedItem?.ToString();
            ConfigHelper.N36200Port = cbxN36200Port.SelectedItem?.ToString();
            ConfigHelper.GunPort = cbxGunPort.SelectedItem?.ToString();
            ConfigHelper.RelayPort = cbxRelayPort.SelectedItem?.ToString();
            ConfigHelper.ToolControlPort = cbxToolControlPort.SelectedItem?.ToString();

            var model = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400");
            model.CommValue = $"{model.CommValue.Split('&')[0]}&{txtNXI6400IP.Text.Trim()}&{model.CommValue.Split('&')[2]}";
            Key_ValDal.UpdateValue(model.Id, model);

            // 直接使用 UpdateValue 方法保存 NXI_6400_2 设置，该方法会自动处理添加或更新
            Key_ValDal.UpdateValue("NXI_6400_2", $"NXI-6400&{txtNXI6400IP2.Text.Trim()}&2");
            CommonMethod.RefreshDatabaseData("Key_Val");
            Key_ValDal.UpdateConfigTime(CommonData.CurrentUser.UserName);
            MessageBox.Show("保存成功重启软件生效");
        }

        private void SaveDetectPar_Click(object sender, RoutedEventArgs e)
        {
            bool b = false;
            foreach (var item in DetectGBs)
            {
                if (item.Visibility == Visibility.Visible)
                {
                    if (ConfigHelper.SelectType == "seres")
                    {
                        if (item.Content is IDetectCtrlBaseSERES detect)
                        {
                            if (detect.Save() == true)
                            {
                                b = true;
                            }
                        }
                    }
                    else if (ConfigHelper.SelectType == "lt")
                    {
                        if (item.Content is IDetectCtrlBaseLT detect)
                        {
                            if (detect.Save() == true)
                            {
                                b = true;
                            }
                        }
                    }
                    else if (ConfigHelper.SelectType == "geely")
                    {
                        if (item.Content is IDetectCtrlBaseGEELY detect)
                        {
                            if (detect.Save() == true)
                            {
                                b = true;
                            }
                        }
                    }
                    
                }
            }
            if (b)
            {
                
                CommonMethod.RefreshDatabaseData("TestParameter");
                Key_ValDal.UpdateConfigTime(CommonData.CurrentUser.UserName);
                MessageBox.Show("保存成功重启软件生效");
            }
        }

        private void Invert_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in detectListBox.ItemsSource)
            {
                if (detectListBox.SelectedItems.Contains(item))
                {
                    detectListBox.SelectedItems.Remove(item);
                }
                else
                {
                    detectListBox.SelectedItems.Add(item);
                }
            }
        }

        private void CheckAll_Click(object sender, RoutedEventArgs e)
        {
            if (checkAll.IsChecked == true)
            {
                detectListBox.SelectAll();
            }
            else
            {
                detectListBox.UnselectAll();
            }
        }

        private void detect_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (detectListBox.SelectedItems.Count == 0)
            {
                checkAll.IsChecked = false;
            }
            else if (detectListBox.SelectedItems.Count == detectListBox.Items.Count)
            {
                checkAll.IsChecked = true;
            }
            else
            {
                checkAll.IsChecked = null;
            }
        }


        private void TabItem_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is TabItem tab)
            {
                var detects = CommonData.CurrentUserDetects.Select(x => x.Item1);
                if (tab != tbCtrl.SelectedItem)
                {
                    if (detects.Contains("CanTRTest"))
                    {
                       // CanTRTestGB.Visibility = Visibility.Visible;
                    }
                    else
                    {
                       // CanTRTestGB.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void SaveDetect_Click(object sender, RoutedEventArgs e)
        {
            if (users.SelectedItem is Sys_User user)
            {
                bool b = false;
                user.TestType = (int)Enum.Parse(typeof(TestType), testTypeCbx.SelectedItem.ToString());
                user.TestBoard = testBordBox.Text.Trim();
                if (UserDal.Update(user))
                {
                    b = true;
                }
                ;
                var tempList = detectListBox.SelectedItems.Cast<Tuple<string, string, int>>().OrderBy(m => m.Item3).Select(m => m.Item1);
                bool inserted = false;
                if (!tempList.Contains("LBMSWrite"))
                {
                    inserted = true;
                }


               
                var newCollection = tempList
                    .SelectMany(s =>
                    {
                        if (!inserted && s == "SleepingCurrent")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s }; // 在第一个 "SleepingCurrent" 前插入 "LBMSWriteBefor"
                        }
                        if (!inserted && s == "WakeSourceTest")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s }; // 在第一个 "WakeSourceTest" 前插入 "LBMSWriteBefor"
                        }
                        if (!inserted && s == "EEPROMTest")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s }; // 在第一个 "EEPROMTest" 前插入 "LBMSWriteBefor"
                        }
                        if (!inserted && s == "SOCAndSOH")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s }; // 在第一个 "SOCAndSOH" 前插入 "LBMSWriteBefor"
                        }
                        if (!inserted && s == "ClearDTC")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s }; // 在第一个 "ClearDTC" 前插入 "LBMSWriteBefor"
                        }
                        if (!inserted && s == "LBMSWrite")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s }; // 在第一个 "LBMSWrite" 前插入 "LBMSWriteBefor"
                        }
                        return new[] { s };
                    })
                    .ToList(); // 如果需要 List<string>

                if (UserDal.UpdateTestItem(user.Id, string.Join(",", newCollection)))
                {
                    b = true;
                }
                if (b)
                {
                    CommonMethod.RefreshDatabaseData("Sys_User");

                    Key_ValDal.UpdateConfigTime(CommonData.CurrentUser.UserName);
                    MessageBox.Show("保存成功重启软件生效");
                }
                else
                {
                    MessageBox.Show("保存失败");
                }
            }

        }

        private void User_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (users.SelectedItem is Sys_User user)
            {
                #region 更新测试模式
                testTypeCbx.SelectedItem = ((TestType)(user.TestType??1)).ToString();
                testBordBox.Text = user.TestBoard;
                #endregion

                detectListBox.SelectedItems.Clear();
                List<(string, string)> result = new List<(string, string)>();
                if (!string.IsNullOrEmpty(user.TestItem))
                {
                    foreach (var item in user.TestItem.Split(','))
                    {
                        result.Add((item, item.GetShowTxt()));
                    }
                }


                var detects = DetectsSERES();
                if (ConfigHelper.SelectType == "seres")
                {
                    detects = DetectsSERES();
                }
                else if (ConfigHelper.SelectType == "lt")
                {
                    detects = DetectsLT();
                }
                else if (ConfigHelper.SelectType == "geely")
                {
                    detects = DetectsGEELY();
                }
                detectListBox.ItemsSource = detects;
                var selectItems = detects.Where(m => (result.Select(x => x.Item1).ToList()).Contains(m.Item1));
                foreach (var item in selectItems)
                {
                    //var temp = detect.ItemsSource.Cast<Tuple<string, string>>().FirstOrDefault(m => m.Item2 == item.Item1);

                    detectListBox.SelectedItems.Add(item);
                }
            }

        }



        BackModel backModel = new BackModel();
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            //backModel.NXI_6400 = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400").CommValue;
            backModel.GunPort = ConfigHelper.GunPort;
            backModel.IT6122BPort = ConfigHelper.IT6122BPort;
            backModel.M3905DPort = ConfigHelper.M3905DPort;
            backModel.N8331Port = ConfigHelper.N8331Port;
            backModel.N36200Port = ConfigHelper.N36200Port;
            backModel.BackTime = DateTime.Now;

            backModel.Key_Vals = Key_ValDal.GetAll().ToList();
            backModel.Parameters = TestParDal.GetAllTestPar();
            backModel.Users = UserDal.GetAllUser();

            System.IO.File.WriteAllText(backFilePath, JsonConvert.SerializeObject(backModel));

            BackBlock.Text = backModel.BackTime.ToString();
            MessageBox.Show("备份成功");
        }

        private void Revert_Click(object sender, RoutedEventArgs e)
        {
            //var model = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400");
            //model.CommValue = backModel.NXI_6400;
            //Key_ValDal.UpdateValue(model.Id, model);

            ConfigHelper.GunPort = backModel.GunPort;
            ConfigHelper.IT6122BPort = backModel.IT6122BPort;
            ConfigHelper.M3905DPort = backModel.M3905DPort;
            ConfigHelper.N8331Port = backModel.N8331Port;
            ConfigHelper.N36200Port = backModel.N36200Port;


            Key_ValDal.Reset(backModel.Key_Vals);
            UserDal.Reset(backModel.Users);
            TestParDal.Reset(backModel.Parameters);
            Key_ValDal.UpdateConfigTime(CommonData.CurrentUser.UserName);
            MessageBox.Show("还原成功，请重启软件");
        }
        private void SaveOtherPar_Click(object sender, RoutedEventArgs e)
        {
            ConfigHelper.DBSaveTime = ((int)dbSaveTimeBox.SelectedItem).ToString();

            bool b = false;
            if(Key_ValDal.UpdateValue("工装编号", toolNumberBox.Text.Trim()))
            {
                b = true;
            }
            if (Key_ValDal.UpdateValue("StationName", stationBox.Text.Trim()))
            {
                b = true;
            }
            if (Key_ValDal.UpdateValue("RscName", rscBox.Text.Trim()))
            {
                b = true;
            }
            if (Key_ValDal.UpdateValue("UserName", uNameBox.Text.Trim()))
            {
                b = true;
            }
            if (Key_ValDal.UpdateValue("MesURL", mesUrlBox.Text.Trim()))
            {
                b = true;
            }
            if (Key_ValDal.UpdateValue("ProduceName", produceNameBox.Text))
            {
                b = true;
            }
            if (b)
            {
                CommonMethod.RefreshDatabaseData("Key_Val");
                MessageBox.Show("保存成功，请重启软件");
            }
        }

        private void ImportFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建打开文件对话框
                var openFileDialog = new OpenFileDialog
                {
                    Title = "导入INI配置文件",
                    Filter = "INI配置文件|*.ini|所有文件|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    Multiselect = false
                };

                // 显示对话框
                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;

                    // 读取INI文件
                    LoadIniConfiguration(filePath);

                    DetectGBs.Clear();
                    DetectCtrlAdd();

                   
                    foreach (var item in CanTRTestGBStackPanel.Children)
                    {
                        if (item is GroupBox group)
                        {
                            DetectGBs.Add(group);
                        }
                    }

                    MessageBox.Show("配置文件导入成功！", "导入成功",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导入配置文件时出错:\n{ex.Message}", "导入错误",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExportFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建保存文件对话框
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "导出INI配置文件",
                    Filter = "INI配置文件|*.ini|所有文件|*.*",
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    FileName = $"UDS_Config_{DateTime.Now:yyyyMMdd_HHmmss}.ini",
                    DefaultExt = ".ini",
                    AddExtension = true  
                };

                // 显示对话框
                if (saveFileDialog.ShowDialog() == true)
                {
                    string filePath = saveFileDialog.FileName;

                    // 保存配置到INI文件
                    SaveIniConfiguration(filePath);

                    MessageBox.Show("配置文件导出成功！", "导出成功",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出配置文件时出错:\n{ex.Message}", "导出错误",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载INI配置文件
        /// </summary>
        private void LoadIniConfiguration(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("配置文件不存在");

            var lines = File.ReadAllLines(filePath);
            string currentSection = "";

            foreach (var line in lines)
            {
                string trimmedLine = line.Trim();

                // 跳过空行和注释
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith(";") || trimmedLine.StartsWith("#"))
                    continue;

                // 处理节(section)
                if (trimmedLine.StartsWith("[") && trimmedLine.EndsWith("]"))
                {
                    currentSection = trimmedLine.Substring(1, trimmedLine.Length - 2).Trim();
                    continue;
                }

                // 处理键值对
                int equalsIndex = trimmedLine.IndexOf('=');
                if (equalsIndex > 0)
                {
                    string key = trimmedLine.Substring(0, equalsIndex).Trim();
                    string value = trimmedLine.Substring(equalsIndex + 1).Trim();

                    // 根据节和键名设置对应的静态变量
                    SetConfigurationValue(currentSection, key, value);
                }
            }
        }

        /// <summary>
        /// 保存配置到INI文件
        /// </summary>
        private void SaveIniConfiguration(string filePath)
        {
            var configLines = new List<string>();

            // 添加文件头注释
            configLines.Add($"; UDS配置文件 - 生成时间: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            configLines.Add($"; 自动生成的配置文件，请勿手动修改");
            configLines.Add("");

            foreach (var item in CommonData.TestParameters)
            {

                // 网络配置节
                configLines.Add($"[{item.Id}]");
                configLines.Add($"TestItem = {item.TestItem}");
                configLines.Add($"MedthodName = {item.MedthodName}");
                configLines.Add($"ChildrenItem = {item.ChildrenItem}");
                configLines.Add($"QualifiedValue = {item.QualifiedValue}");
                configLines.Add($"MeasurementStandard = {item.MeasurementStandard}");
                configLines.Add($"MinVal = {item.MinVal}");
                configLines.Add($"MaxVal = {item.MaxVal}");
                configLines.Add($"Extend1 = {item.Extend1}");
                configLines.Add($"Extend2 = {item.Extend2}");
                configLines.Add($"Extend3 = {item.Extend3}");
              
                configLines.Add("");

            }

            // 写入文件
            File.WriteAllLines(filePath, configLines);
        }

        /// <summary>
        /// 根据节和键名设置配置值
        /// </summary>
        private void SetConfigurationValue(string section, string key, string value)
        {
            try
            {
                // 查找对应的测试参数项
                var testParam = CommonData.TestParameters.FirstOrDefault(p => p.Id.ToString() == section);
                if (testParam == null)
                {
                    // 如果没有找到对应的节点，可以选择记录日志或忽略
                    Console.WriteLine($"警告: 未找到对应的配置节点 [{section}]");
                    return;
                }

                // 根据键名设置对应的值
                switch (key)
                {
                    case "TestItem":
                            testParam.TestItem = value;
                        break;
                    case "MedthodName":
                        testParam.MedthodName = value;
                        break;
                    case "ChildrenItem":
                        testParam.ChildrenItem = value;
                        break;
                    case "MeasurementStandard":
                        testParam.MeasurementStandard = value;
                        break;
                    case "Extend1":
                        testParam.Extend1 = value;
                        break;
                    case "Extend2":
                        testParam.Extend2 = value;
                        break;
                    case "Extend3":
                        testParam.Extend3 = value;
                        break;
                    case "QualifiedValue":
                        if (double.TryParse(value, out double qualifiedValue))
                            testParam.QualifiedValue = qualifiedValue;
                        break;
                    case "MinVal":
                        if (double.TryParse(value, out double minVal))
                            testParam.MinVal = minVal;
                        break;
                    case "MaxVal":
                        if (double.TryParse(value, out double maxVal))
                            testParam.MaxVal = maxVal;
                        break;
                    default:
                        Console.WriteLine($"警告: 未知的配置键 {key} 在节点 [{section}]");
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"解析配置项 [{section}] {key} = {value} 时出错: {ex.Message}");
            }
        }
    }



    class BackModel
    {
        public DateTime BackTime { get; set; }
        public string GunPort { get; set; }

        public string IT6122BPort { get; set; }
        public string M3905DPort { get; set; }
        public string N8331Port { get; set; }
        public string N36200Port { get; set; }

        public string NXI_6400 { get; set; }

        public List<Key_Val> Key_Vals { get; set; }

        public List<Sys_User> Users { get; set; }

        public List<TestParameter> Parameters { get; set; }

    }
    public class ProtocolParameter
    {
        public string Name { get; set; }

        public string Binding { get; set; }
        public string IDTextBoxName { get; set; }
        public string StartBitTextBoxName { get; set; }
        public string LengthTextBoxName { get; set; }
        public string PrecisionTextBoxName { get; set; }
        public string OffsetTextBoxName { get; set; }
    }
    public class SendCommand
    {
        public string Name { get; set; }
        public string IDTextBoxName { get; set; }
        public string DataTextBoxName { get; set; }
    }

    public class UdsTestItem
    {
        public string TestName { get; set; }
        public List<UdsParameter> Parameters { get; set; }
    }

    // UDS参数类定义
    public class UdsParameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string TextBoxName { get; set; }
        public string DefaultValue { get; set; }
    }

    


   
}
