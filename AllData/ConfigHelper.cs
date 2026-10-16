using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AllData
{
    public class ConfigHelper
    {
       
        [DllImport("kernel32.dll")]

        private static extern long WritePrivateProfileString(string section, string key, string value, string filepath);

        [DllImport("kernel32.dll")]

        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder returnvalue, int buffersize, string filepath);

        static string iniFilePath = System.IO.Directory.GetCurrentDirectory() + "\\config.ini";
        static ConfigHelper()
        {
            if (!System.IO.File.Exists(iniFilePath))
            {
                System.IO.File.Create(iniFilePath).Close();
            }
        }
        public static string GetValue(string section, string key)
        {

            StringBuilder stringBuilder = new StringBuilder();

            GetPrivateProfileString(section, key, "", stringBuilder, 1024, iniFilePath);

            return stringBuilder.ToString();

        }

        public static void SetValue(string section, string key, string content)
        {
            WritePrivateProfileString(section, key, content, iniFilePath);
        }


        public static CANConfig GetCanValue(string section, string key)
        {
            return new CANConfig
            {
                ID = (uint)ReadIniInt(section + ".ID", key, defaultValue: 0),
                Startbit = ReadIniInt(section + ".Startbit", key, defaultValue: 0),
                Len = ReadIniInt(section + ".Len", key, defaultValue: 0),
                factor = ReadIniDouble(section + ".factor", key, defaultValue: 1.0),
                offset = ReadIniInt(section + ".offset", key, defaultValue: 0)
            };
        }

        public static void SetCanValue(string section, string key, CANConfig content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            WritePrivateProfileString(section + ".ID", key, content.ID.ToString(), iniFilePath);
            WritePrivateProfileString(section + ".Startbit", key, content.Startbit.ToString(), iniFilePath);
            WritePrivateProfileString(section + ".Len", key, content.Len.ToString(), iniFilePath);
            WritePrivateProfileString(section + ".factor", key, content.factor.ToString(CultureInfo.InvariantCulture), iniFilePath);
            WritePrivateProfileString(section + ".offset", key, content.offset.ToString(), iniFilePath);
        }

        public static CANSend GetCanSend(string section, string key)
        {
            return new CANSend
            {
                ID = (uint)ReadIniInt(section + ".ID", key, defaultValue: 0),
                Data = ReadIniString(section + ".Data", key, defaultValue: "")

            };
        }

        public static void SetCanSend(string section, string key, CANSend content)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            WritePrivateProfileString(section + ".ID", key, content.ID.ToString(), iniFilePath);
            WritePrivateProfileString(section + ".Data", key, content.Data.ToString(), iniFilePath);
     
        }

        // Helper methods with null/empty handling
        private static int ReadIniInt(string section, string key, int defaultValue)
        {
            var sb = new StringBuilder(16);
            GetPrivateProfileString(section, key, "", sb, sb.Capacity, iniFilePath);

            if (string.IsNullOrEmpty(sb.ToString()))
            {
                return defaultValue;
            }

            return int.TryParse(sb.ToString(), out int result) ? result : defaultValue;
        }

        private static string ReadIniString(string section, string key, string defaultValue)
        {
            var sb = new StringBuilder(16);
            GetPrivateProfileString(section, key, "", sb, sb.Capacity, iniFilePath);

            if (string.IsNullOrEmpty(sb.ToString()))
            {
                return defaultValue;
            }

            return sb.ToString();
        }

        private static double ReadIniDouble(string section, string key, double defaultValue)
        {
            var sb = new StringBuilder(32);
            GetPrivateProfileString(section, key, "", sb, sb.Capacity, iniFilePath);

            if (string.IsNullOrEmpty(sb.ToString()))
            {
                return defaultValue;
            }

            return double.TryParse(sb.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result)
                ? result
                : defaultValue;
        }



        public static bool Cantype => UdsParameters.xieyi != "CAN";
        public static string Power2PortName
        {
            get { return GetValue("Power2", "PortNo"); }
            set { SetValue("Power2", "PortNo", value); }
        }

        /// <summary>
        /// 扫码枪端口
        /// </summary>
        public static string GunPort
        {
            get { return GetValue("PortName", "Gun"); }
            set { SetValue("PortName", "Gun", value); }
        }
        /// <summary>
        /// IT6122B端口
        /// </summary>
        public static string IT6122BPort
        {
            get { return GetValue("PortName", "IT6122B"); }
            set { SetValue("PortName", "IT6122B", value); }
        }

        /// <summary>
        /// M3905D端口
        /// </summary>
        public static string M3905DPort
        {
            get { return GetValue("PortName", "M3905D"); }
            set { SetValue("PortName", "M3905D", value); }
        }

        /// <summary>
        /// N8331端口
        /// </summary>
        public static string N8331Port
        {
            get { return GetValue("PortName", "N8331"); }
            set { SetValue("PortName", "N8331", value); }
        }
        /// <summary>
        /// IT6322端口
        /// </summary>
        public static string IT6322Port
        {
            get { return GetValue("PortName", "IT6322"); }
            set { SetValue("PortName", "IT6322", value); }
        }
        /// <summary>
        /// N69206H端口
        /// </summary>
        public static string N69206HPort
        {
            get { return GetValue("PortName", "N69206H"); }
            set { SetValue("PortName", "N69206H", value); }
        }

        /// <summary>
        /// N36200/N36300 可编程直流电源端口
        /// </summary>
        public static string N36200Port
        {
            get { return GetValue("PortName", "N36200"); }
            set { SetValue("PortName", "N36200", value); }
        }

        /// <summary>
        /// 继电器控制板端口
        /// </summary>
        public static string RelayPort
        {
            get { return GetValue("PortName", "Relay"); }
            set { SetValue("PortName", "Relay", value); }
        }

        /// <summary>
        /// 治具控制卡端口
        /// </summary>
        public static string ToolControlPort
        {
            get { return GetValue("PortName", "ToolControl"); }
            set { SetValue("PortName", "ToolControl", value); }
        }
        /// <summary>
        /// 存储上一次成功登录的账号
        /// </summary>
        public static string LastAccount
        {
            get { return GetValue("Account", "LastAccount"); }
            set { SetValue("Account", "LastAccount", value); }
        }
        /// <summary>
        /// 合格数量
        /// </summary>
        public static string PassNum
        {
            get { return GetValue("PassNum", "Record"); }
            set { SetValue("PassNum", "Record", value); }
        }
        /// <summary>
        /// 不合格数量
        /// </summary>
        public static string FailNum
        {
            get { return GetValue("FailNum", "Record"); }
            set { SetValue("FailNum", "Record", value); }
        }
        /// <summary>
        /// 数据库保存时间
        /// </summary>
        public static string DBSaveTime {
            get { return GetValue("DBSaveTime", "DB"); }
            set { SetValue("DBSaveTime", "DB", value); }
        }

        public static string SelectType
        {
            get { return GetValue("SelectType", "TEST"); }
            set { SetValue("SelectType", "TEST", value); }
        }

        
        public static CANConfig CANMOSFSTATE
        {
            get { return GetCanValue("CANMOSFSTATE", "CAN"); }
            set { SetCanValue("CANMOSFSTATE", "CAN", value); }
        }
        public static CANConfig CANMOSCSTATE
        {
            get { return GetCanValue("CANMOSCSTATE", "CAN"); }
            set { SetCanValue("CANMOSCSTATE", "CAN", value); }
        }
        
        

        [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
        public class UdsParameterAttribute : Attribute
        {
            public string TextBoxName { get; }

            public UdsParameterAttribute(string textBoxName)
            {
                TextBoxName = textBoxName;
            }
        }
        public static class UdsParameters
        {
            #region 帧ID
            [UdsParameter("TID")]
            public static string TID;
            [UdsParameter("RID")]
            public static string RID;
            #endregion
            #region 解锁
            [UdsParameter("jiesuo")]
            public static string jiesuo;
            [UdsParameter("xieyi")]
            public static string xieyi;
            [UdsParameter("chanpin")]
            public static string chanpin;
            [UdsParameter("WC_ID")]
            public static string WC_ID;
            #endregion
            #region 产品信息读取
            [UdsParameter("PRID1")]
            public static string PRID1;
            [UdsParameter("PRID2")]
            public static string PRID2;
            [UdsParameter("PRID3")]
            public static string PRID3;
            [UdsParameter("PRSTARTBIT1")]
            public static string PRSTARTBIT1;
            [UdsParameter("PRSTARTBIT2")]
            public static string PRSTARTBIT2;
            [UdsParameter("PRSTARTBIT3")]
            public static string PRSTARTBIT3;
            [UdsParameter("PRLEN1")]
            public static string PRLEN1;
            [UdsParameter("PRLEN2")]
            public static string PRLEN2;
            [UdsParameter("PRLEN3")]
            public static string PRLEN3;
            [UdsParameter("PRFACTOR1")]
            public static string PRFACTOR1;
            [UdsParameter("PRFACTOR2")]
            public static string PRFACTOR2;
            [UdsParameter("PRFACTOR3")]
            public static string PRFACTOR3;
            #endregion
            #region PCB温度测试
            [UdsParameter("PCBT_ID")]
            public static string PCBT_ID;
            [UdsParameter("PCBT_Startbit")]
            public static string PCBT_Startbit;
            [UdsParameter("PCBT_Len")]
            public static string PCBT_Len;
            [UdsParameter("PCBT_Factor")]
            public static string PCBT_Factor;
            [UdsParameter("PCBT_Offset")]
            public static string PCBT_Offset;

            

            [UdsParameter("MOSCT_ID")]
            public static string MOSCT_ID;
            [UdsParameter("MOSCT_Startbit")]
            public static string MOSCT_Startbit;
            [UdsParameter("MOSCT_Len")]
            public static string MOSCT_Len;
            [UdsParameter("MOSCT_Factor")]
            public static string MOSCT_Factor;
            [UdsParameter("MOSCT_Offset")]
            public static string MOSCT_Offset;

            [UdsParameter("MOSFT_ID")]
            public static string MOSFT_ID;
            [UdsParameter("MOSFT_Startbit")]
            public static string MOSFT_Startbit;
            [UdsParameter("MOSFT_Len")]
            public static string MOSFT_Len;
            [UdsParameter("MOSFT_Factor")]
            public static string MOSFT_Factor;
            [UdsParameter("MOSFT_Offset")]
            public static string MOSFT_Offset;

            [UdsParameter("XLT_ID")]
            public static string XLT_ID;
            [UdsParameter("XLT_Startbit")]
            public static string XLT_Startbit;
            [UdsParameter("XLT_Len")]
            public static string XLT_Len;
            [UdsParameter("XLT_Factor")]
            public static string XLT_Factor;
            [UdsParameter("XLT_Offset")]
            public static string XLT_Offset;

            [UdsParameter("PCBT_SID")]
            public static string PCBT_SID;
            [UdsParameter("PCBT_DID")]
            public static string PCBT_DID;

            [UdsParameter("MOSCT_SID")]
            public static string MOSCT_SID;
            [UdsParameter("MOSCT_DID")]
            public static string MOSCT_DID;

            [UdsParameter("MOSFT_SID")]
            public static string MOSFT_SID;
            [UdsParameter("MOSFT_DID")]
            public static string MOSFT_DID;

            [UdsParameter("XLT_SID")]
            public static string XLT_SID;
            [UdsParameter("XLT_DID")]
            public static string XLT_DID;
            #endregion
            #region MOS
            [UdsParameter("MOSCV_SID")]
            public static string MOSCV_SID;
            [UdsParameter("MOSCV_DID")]
            public static string MOSCV_DID;
            [UdsParameter("MOSCV_ID")]
            public static string MOSCV_ID;
            [UdsParameter("MOSCV_Startbit")]
            public static string MOSCV_Startbit;
            [UdsParameter("MOSCV_Len")]
            public static string MOSCV_Len;
            [UdsParameter("MOSCV_Factor")]
            public static string MOSCV_Factor;
            [UdsParameter("MOSCV_Offset")]
            public static string MOSCV_Offset;

            [UdsParameter("MOSFV_SID")]
            public static string MOSFV_SID;
            [UdsParameter("MOSFV_DID")]
            public static string MOSFV_DID;
            [UdsParameter("MOSFV_ID")]
            public static string MOSFV_ID;
            [UdsParameter("MOSFV_Startbit")]
            public static string MOSFV_Startbit;
            [UdsParameter("MOSFV_Len")]
            public static string MOSFV_Len;
            [UdsParameter("MOSFV_Factor")]
            public static string MOSFV_Factor;
            [UdsParameter("MOSFV_Offset")]
            public static string MOSFV_Offset;

            [UdsParameter("MOSControl_SID")]
            public static string MOSControl_SID;
            [UdsParameter("MOSControl_DID")]
            public static string MOSControl_DID;


            #endregion
            #region P+
            [UdsParameter("PZ_ID")]
            public static string PZ_ID;
            [UdsParameter("PZ_Startbit")]
            public static string PZ_Startbit;
            [UdsParameter("PZ_Len")]
            public static string PZ_Len;
            [UdsParameter("PZ_Factor")]
            public static string PZ_Factor;
            [UdsParameter("PZ_Offset")]
            public static string PZ_Offset;
            #endregion
            #region MCU电压
            [UdsParameter("MCUCV_SID")]
            public static string MCUCV_SID;
            [UdsParameter("MCUCV_DID")]
            public static string MCUCV_DID;
            [UdsParameter("MCUCV_ID")]
            public static string MCUCV_ID;
            [UdsParameter("MCUCV_Startbit")]
            public static string MCUCV_Startbit;
            [UdsParameter("MCUCV_Len")]
            public static string MCUCV_Len;
            [UdsParameter("MCUCV_Factor")]
            public static string MCUCV_Factor;
            [UdsParameter("MCUCV_Offset")]
            public static string MCUCV_Offset;
            #endregion
            #region 电流标定
            [UdsParameter("DLB_SID")]
            public static string DLB_SID;
            [UdsParameter("DLB_DID")]
            public static string DLB_DID;
            #endregion
            #region 电芯电压
            [UdsParameter("DXV1_ID")]
            public static string DXV1_ID;
            [UdsParameter("DXV1_Startbit")]
            public static string DXV1_Startbit;
            [UdsParameter("DXV1_Len")]
            public static string DXV1_Len;
            [UdsParameter("DXV1_Factor")]
            public static string DXV1_Factor;
            [UdsParameter("DXV1_Offset")]
            public static string DXV1_Offset;

            [UdsParameter("DXV2_ID")]
            public static string DXV2_ID;
            [UdsParameter("DXV2_Startbit")]
            public static string DXV2_Startbit;
            [UdsParameter("DXV2_Len")]
            public static string DXV2_Len;
            [UdsParameter("DXV2_Factor")]
            public static string DXV2_Factor;
            [UdsParameter("DXV2_Offset")]
            public static string DXV2_Offset;

            [UdsParameter("DXV3_ID")]
            public static string DXV3_ID;
            [UdsParameter("DXV3_Startbit")]
            public static string DXV3_Startbit;
            [UdsParameter("DXV3_Len")]
            public static string DXV3_Len;
            [UdsParameter("DXV3_Factor")]
            public static string DXV3_Factor;
            [UdsParameter("DXV3_Offset")]
            public static string DXV3_Offset;

            [UdsParameter("DXV4_ID")]
            public static string DXV4_ID;
            [UdsParameter("DXV4_Startbit")]
            public static string DXV4_Startbit;
            [UdsParameter("DXV4_Len")]
            public static string DXV4_Len;
            [UdsParameter("DXV4_Factor")]
            public static string DXV4_Factor;
            [UdsParameter("DXV4_Offset")]
            public static string DXV4_Offset;
            #endregion
            #region 内外总压
            [UdsParameter("NZV_ID")]
            public static string NZV_ID;
            [UdsParameter("NZV_Startbit")]
            public static string NZV_Startbit;
            [UdsParameter("NZV_Len")]
            public static string NZV_Len;
            [UdsParameter("NZV_Factor")]
            public static string NZV_Factor;
            [UdsParameter("NZV_Offset")]
            public static string NZV_Offset;

            [UdsParameter("WZV_ID")]
            public static string WZV_ID;
            [UdsParameter("WZV_Startbit")]
            public static string WZV_Startbit;
            [UdsParameter("WZV_Len")]
            public static string WZV_Len;
            [UdsParameter("WZV_Factor")]
            public static string WZV_Factor;
            [UdsParameter("WZV_Offset")]
            public static string WZV_Offset;
            #endregion
            #region 电流
            [UdsParameter("DL_ID")]
            public static string DL_ID;
            [UdsParameter("DL_Startbit")]
            public static string DL_Startbit;
            [UdsParameter("DL_Len")]
            public static string DL_Len;
            [UdsParameter("DL_Factor")]
            public static string DL_Factor;
            [UdsParameter("DL_Offset")]
            public static string DL_Offset;

            [UdsParameter("DL_SID")]
            public static string DL_SID;
            [UdsParameter("DL_DID")]
            public static string DL_DID;
            #endregion
            #region 电芯温度
            [UdsParameter("DXT1_ID")]
            public static string DXT1_ID;
            [UdsParameter("DXT1_Startbit")]
            public static string DXT1_Startbit;
            [UdsParameter("DXT1_Len")]
            public static string DXT1_Len;
            [UdsParameter("DXT1_Factor")]
            public static string DXT1_Factor;
            [UdsParameter("DXT1_Offset")]
            public static string DXT1_Offset;
          
            [UdsParameter("DXT1_SID")]
            public static string DXT1_SID;
            [UdsParameter("DXT1_DID")]
            public static string DXT1_DID;

            [UdsParameter("DXT2_ID")]
            public static string DXT2_ID;
            [UdsParameter("DXT2_Startbit")]
            public static string DXT2_Startbit;
            [UdsParameter("DXT2_Len")]
            public static string DXT2_Len;
            [UdsParameter("DXT2_Factor")]
            public static string DXT2_Factor;
            [UdsParameter("DXT2_Offset")]
            public static string DXT2_Offset;

            [UdsParameter("DXT2_SID")]
            public static string DXT2_SID;
            [UdsParameter("DXT2_DID")]
            public static string DXT2_DID;

            [UdsParameter("DXT3_ID")]
            public static string DXT3_ID;
            [UdsParameter("DXT3_Startbit")]
            public static string DXT3_Startbit;
            [UdsParameter("DXT3_Len")]
            public static string DXT3_Len;
            [UdsParameter("DXT3_Factor")]
            public static string DXT3_Factor;
            [UdsParameter("DXT3_Offset")]
            public static string DXT3_Offset;

            [UdsParameter("DXT3_SID")]
            public static string DXT3_SID;
            [UdsParameter("DXT3_DID")]
            public static string DXT3_DID;
            #endregion
            #region 均衡测试
            [UdsParameter("JHStart_SID")]
            public static string JHStart_SID;
            [UdsParameter("JHStart_DID")]
            public static string JHStart_DID;

            [UdsParameter("JHEnd_SID")]
            public static string JHEnd_SID;
            [UdsParameter("JHEnd_DID")]
            public static string JHEnd_DID;
            #endregion
            #region 唤醒指令
            [UdsParameter("HX_ID")]
            public static string HX_ID;
            [UdsParameter("HX_Data")]
            public static string HX_Data;

            
            #endregion
            #region 休眠  
            [UdsParameter("XM1_SID")]
            public static string XM1_SID;
            [UdsParameter("XM1_DID")]
            public static string XM1_DID;

            [UdsParameter("XM2_SID")]
            public static string XM2_SID;
            [UdsParameter("XM2_DID")]
            public static string XM2_DID;
            #endregion
            #region 获取唤醒源  
            [UdsParameter("GetHX_SID")]
            public static string GetHX_SID;
            [UdsParameter("GetHX_DID")]
            public static string GetHX_DID;

            [UdsParameter("GetHX_ID")]
            public static string GetHX_ID;
            [UdsParameter("GetHX_Startbit")]
            public static string GetHX_Startbit;
            [UdsParameter("GetHX_Len")]
            public static string GetHX_Len;
            [UdsParameter("GetHX_Factor")]
            public static string GetHX_Factor;
            [UdsParameter("GetHX_Offset")]
            public static string GetHX_Offset;


            [UdsParameter("GetHX_RTC")]
            public static string GetHX_RTC;
            [UdsParameter("GetHX_GY")]
            public static string GetHX_GY;
            [UdsParameter("GetHX_YJ")]
            public static string GetHX_YJ;
            [UdsParameter("GetHX_DL")]
            public static string GetHX_DL;
            [UdsParameter("GetHX_CAN")]
            public static string GetHX_CAN;
            #endregion
            #region 清除AH  
            [UdsParameter("ClearAH_SID")]
            public static string ClearAH_SID;
            [UdsParameter("ClearAH_DID")]
            public static string ClearAH_DID;
            #endregion
            #region 限流模块
            [UdsParameter("XLOpen_SID")]
            public static string XLOpen_SID;
            [UdsParameter("XLOpen_DID")]
            public static string XLOpen_DID;

            [UdsParameter("XLEnd_SID")]
            public static string XLEnd_SID;
            [UdsParameter("XLEnd_DID")]
            public static string XLEnd_DID;
            #endregion
            #region EEPROM  
            [UdsParameter("EEPROM_SID")]
            public static string EEPROM_SID;
            [UdsParameter("EEPROM_DID")]
            public static string EEPROM_DID;
            #endregion
            #region SOC SOH 
            [UdsParameter("SOCT_SID")]
            public static string SOCT_SID;
            [UdsParameter("SOCT_DID")]
            public static string SOCT_DID;

            [UdsParameter("SOCR_ID")]
            public static string SOCR_ID;
            [UdsParameter("SOCR_Startbit")]
            public static string SOCR_Startbit;
            [UdsParameter("SOCR_Len")]
            public static string SOCR_Len;
            [UdsParameter("SOCR_Factor")]
            public static string SOCR_Factor;
            [UdsParameter("SOCR_Offset")]
            public static string SOCR_Offset;

            [UdsParameter("SOHR_ID")]
            public static string SOHR_ID;
            [UdsParameter("SOHR_Startbit")]
            public static string SOHR_Startbit;
            [UdsParameter("SOHR_Len")]
            public static string SOHR_Len;
            [UdsParameter("SOHR_Factor")]
            public static string SOHR_Factor;
            [UdsParameter("SOHR_Offset")]
            public static string SOHR_Offset;
            #endregion
            #region 均衡与AFE诊断测试
            [UdsParameter("AFE_SID")]
            public static string AFE_SID;
            [UdsParameter("AFE_DID")]
            public static string AFE_DID;
            #endregion
            #region 产品信息
            [UdsParameter("PCH_Date")]
            public static string PCH_Date;
            [UdsParameter("YJ_Date")]
            public static string YJ_Date;
            [UdsParameter("RJ_Date")]
            public static string RJ_Date;
            #endregion
            #region 软件版本
            [UdsParameter("RJ_ID1")]
            public static string RJ_ID1;
            [UdsParameter("RJ_Startbit1")]
            public static string RJ_Startbit1;
            [UdsParameter("RJ_Len1")]
            public static string RJ_Len1;
            [UdsParameter("RJ_Factor1")]
            public static string RJ_Factor1;
            [UdsParameter("RJ_Offset1")]
            public static string RJ_Offset1;

            [UdsParameter("YJ_ID")]
            public static string YJ_ID;
            [UdsParameter("YJ_Startbit")]
            public static string YJ_Startbit;
            [UdsParameter("YJ_Len")]
            public static string YJ_Len;
            [UdsParameter("YJ_Factor")]
            public static string YJ_Factor;
            [UdsParameter("YJ_Offset")]
            public static string YJ_Offset;

            [UdsParameter("YJ_ID2")]
            public static string YJ_ID2;
            [UdsParameter("YJ_Startbit2")]
            public static string YJ_Startbit2;
            [UdsParameter("YJ_Len2")]
            public static string YJ_Len2;
            [UdsParameter("YJ_Factor2")]
            public static string YJ_Factor2;
            [UdsParameter("YJ_Offset2")]
            public static string YJ_Offset2;

            [UdsParameter("RJ_SID")]
            public static string RJ_SID;
            [UdsParameter("RJ_DID")]
            public static string RJ_DID;

            [UdsParameter("YJ_SID")]
            public static string YJ_SID;
            [UdsParameter("YJ_DID")]
            public static string YJ_DID;

            [UdsParameter("RJ_DATA")]
            public static string RJ_DATA;
            [UdsParameter("YJ_DATA")]
            public static string YJ_DATA;
            [UdsParameter("BOOT_DATA")]
            public static string BOOT_DATA;
            [UdsParameter("Soft_ID")]
            public static string Soft_ID;

            [UdsParameter("LJ_DATA")]
            public static string LJ_DATA;

            [UdsParameter("XH_DATA")]
            public static string XH_DATA;

            [UdsParameter("KHRJ_DATA")]
            public static string KHRJ_DATA;

            [UdsParameter("KHYJ_DATA")]
            public static string KHYJ_DATA;

            [UdsParameter("ECU_DATA")]
            public static string ECU_DATA;

            [UdsParameter("REPORT")]
            public static string REPORT;

            [UdsParameter("RJ_ID2")]
            public static string RJ_ID2;
            [UdsParameter("RJ_Startbit2")]
            public static string RJ_Startbit2;
            [UdsParameter("RJ_Len2")]
            public static string RJ_Len2;
            [UdsParameter("RJ_Factor2")]
            public static string RJ_Factor2;
            [UdsParameter("RJ_Offset2")]
            public static string RJ_Offset2;
            #endregion



        }


    }

    public class CANConfig
    {
        public uint ID;

        public int Startbit;

        public int Len;

        public double factor;

        public int offset;

    }

    public class CANSend
    {
        public uint ID;

        public string Data;

    }

    
}
