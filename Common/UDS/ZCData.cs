using AllData;
using Common.Can;
using NPOI.POIFS.Crypt.Dsig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.UDS
{
    public static class ZCData
    {

        static CanOperate _cp;
        private static ManualResetEvent _responseReceivedEvent = new ManualResetEvent(false);
        private static byte[] GetData(uint id=0x372)
        {
            byte[] data = null;
            ManualResetEvent r = new ManualResetEvent(false);
            if (_cp != null)
            {
                _cp.ReviceData += getData;
                r.Reset();
                if (r.WaitOne((int)(CommonData.AwaitTime * 1000)))
                {
                    _cp.ReviceData -= getData;
                    return data;
                }
                else
                {
                    _cp.ReviceData -= getData;
                    throw new Exception("产品通信超时");
                }
            }
            else
            {
                throw new Exception("产品Can未初始化");
            }
            void getData(uint id1, byte[] arr)
            {
                if (id1 == id)
                {
                    data = arr;
                    r.Set();
                }
            }
        }
         /// <summary>
         /// 电流
         /// </summary>
        public static double GetDL
        {
            get { 
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DL_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DL_Startbit), int.Parse(ConfigHelper.UdsParameters.DL_Len), double.Parse(ConfigHelper.UdsParameters.DL_Factor), -1000, 1000, int.Parse(ConfigHelper.UdsParameters.DL_Offset));

            }
        }
        public static void SetCP(CanOperate cp)
        {
            _cp = cp;
        }
        /// <summary>
        /// 产品接收数据
        /// </summary>
        /// <param name="id"></param>
        /// <param name="arr"></param>
        
        #region 属性
        public static double WorkStatus
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x373), 44, 3, 1, 0, 7, 0);
            }
        }

        /// <summary>
        /// 电流
        /// </summary>
        public static double Dl {
            get
            {
                var dl = PlayUDSHelper.GetCurrent();
                return dl;
          
            }
        }
        /// <summary>
        /// PCB温度
        /// </summary>
        public static double PCBTemperature {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.PCBT_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PCBT_Startbit), int.Parse(ConfigHelper.UdsParameters.PCBT_Len), double.Parse(ConfigHelper.UdsParameters.PCBT_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.PCBT_Offset));
            }
        }
        /// <summary>
        /// 放电MOS温度
        /// </summary>
        public static double DischargeMOSTemperature {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.MOSFT_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.MOSFT_Startbit), int.Parse(ConfigHelper.UdsParameters.MOSFT_Len), double.Parse(ConfigHelper.UdsParameters.MOSFT_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.MOSFT_Offset));
            }
        }

        /// <summary>
        /// 充电MOS温度
        /// </summary>
        public static double ChargeMOSTemperature {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.MOSCT_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.MOSCT_Startbit), int.Parse(ConfigHelper.UdsParameters.MOSCT_Len), double.Parse(ConfigHelper.UdsParameters.MOSCT_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.MOSCT_Offset));
            }
        }
        /// <summary>
        /// 放电MOS电压
        /// </summary>
        public static double DischargeMOSVolt
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.MOSFV_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.MOSFV_Startbit), int.Parse(ConfigHelper.UdsParameters.MOSFV_Len), double.Parse(ConfigHelper.UdsParameters.MOSFV_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.MOSFV_Offset));
            }
        }
        /// <summary>
        /// 充电MOS电压
        /// </summary>
        public static double ChargeMOSVolt
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.MOSCV_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.MOSCV_Startbit), int.Parse(ConfigHelper.UdsParameters.MOSCV_Len), double.Parse(ConfigHelper.UdsParameters.MOSCV_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.MOSCV_Offset));
            }
        }

        /// <summary>
        /// 放电MOS状态
        /// </summary>
        public static double DischargeMOSState
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(ConfigHelper.CANMOSFSTATE.ID), ConfigHelper.CANMOSFSTATE.Startbit, ConfigHelper.CANMOSFSTATE.Len, ConfigHelper.CANMOSFSTATE.factor, 0, 100, ConfigHelper.CANMOSFSTATE.offset);
            }
        }
        /// <summary>
        /// 充电MOS状态
        /// </summary>
        public static double ChargeMOSState
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(ConfigHelper.CANMOSCSTATE.ID), ConfigHelper.CANMOSCSTATE.Startbit, ConfigHelper.CANMOSCSTATE.Len, ConfigHelper.CANMOSCSTATE.factor, 0, 100, ConfigHelper.CANMOSCSTATE.offset);
            }
        }

        public static double CurrentLimitingst
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x3CD), 60, 4, 1, 0, 5);
            }
        }


        public static double CurrentLimitingVOLT
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x3CD), 49, 11, 0.01, 0, 22);
            }
        }



        /// <summary>
        /// 限流模块温度
        /// </summary>
        public static double CurrentLimitingModuleTemperature {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.XLT_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.XLT_Startbit), int.Parse(ConfigHelper.UdsParameters.XLT_Len), double.Parse(ConfigHelper.UdsParameters.XLT_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.XLT_Offset));
            }
        }
        /// <summary>
        /// 版本
        /// </summary>
        public static string Version
        {
            get
            {
                
                var v1 = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.RJ_ID1, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.RJ_Startbit1), int.Parse(ConfigHelper.UdsParameters.RJ_Len1), double.Parse(ConfigHelper.UdsParameters.RJ_Factor1), 0, 1000, int.Parse(ConfigHelper.UdsParameters.RJ_Offset1));
                var v2 = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.RJ_ID2, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.RJ_Startbit2), int.Parse(ConfigHelper.UdsParameters.RJ_Len2), double.Parse(ConfigHelper.UdsParameters.RJ_Factor2), 0, 1000, int.Parse(ConfigHelper.UdsParameters.RJ_Offset2));
                var strTemp = v1 + "." + v2;
                return strTemp;
            }
        }

        public static string VersionHard
        {
            get
            {

                var v1 = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.YJ_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.YJ_Startbit), int.Parse(ConfigHelper.UdsParameters.YJ_Len), double.Parse(ConfigHelper.UdsParameters.YJ_Factor), 0, 1000, int.Parse(ConfigHelper.UdsParameters.YJ_Offset));
                var v2 = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.YJ_ID2, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.YJ_Startbit2), int.Parse(ConfigHelper.UdsParameters.YJ_Len2), double.Parse(ConfigHelper.UdsParameters.YJ_Factor2), 0, 1000, int.Parse(ConfigHelper.UdsParameters.YJ_Offset2));
                var strTemp = v1 + "." + v2;
                return strTemp;
            }
        }
        /// <summary>
        /// UEX
        /// </summary>
        public static double Uex
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.PZ_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PZ_Startbit), int.Parse(ConfigHelper.UdsParameters.PZ_Len), double.Parse(ConfigHelper.UdsParameters.PZ_Factor), 0, 5, int.Parse(ConfigHelper.UdsParameters.PZ_Offset));
                    
            }
        }

        public static (double, double, double, double) DYS
        {
            get
            {
                return (DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXV1_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXV1_Startbit), int.Parse(ConfigHelper.UdsParameters.DXV1_Len), double.Parse(ConfigHelper.UdsParameters.DXV1_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.DXV1_Offset)),
                    DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXV2_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXV2_Startbit), int.Parse(ConfigHelper.UdsParameters.DXV2_Len), double.Parse(ConfigHelper.UdsParameters.DXV2_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.DXV2_Offset)),
                    DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXV3_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXV3_Startbit), int.Parse(ConfigHelper.UdsParameters.DXV3_Len), double.Parse(ConfigHelper.UdsParameters.DXV3_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.DXV3_Offset)),
                    DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXV4_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXV4_Startbit), int.Parse(ConfigHelper.UdsParameters.DXV4_Len), double.Parse(ConfigHelper.UdsParameters.DXV4_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.DXV4_Offset)));
            }
        }
        public static double HXY
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.GetHX_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.GetHX_Startbit), int.Parse(ConfigHelper.UdsParameters.GetHX_Len), double.Parse(ConfigHelper.UdsParameters.GetHX_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.GetHX_Offset));
            }
        }
        public static double CanHX
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x6DE), 24, 1, 1, 0, 5);
            }
        }
        public static double RTC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x6DE), 25, 1, 1, 0, 5);
            }
        }
        public static double DianLiu
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x6DE), 26, 1, 1, 0, 5);
            }
        }
        public static double GuoYa
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x6DE), 27, 1, 1, 0, 5);
            }
        }
        public static double YingJian
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x6DE), 28, 1, 1, 0, 5);
            }
        }

        public static double CanHXH97
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x580), 26, 1, 1, 0, 5,0,1);
            }
        }
        public static double RTCH97
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x580), 25, 1, 1, 0, 5, 0, 1);
            }
        }
        public static double DianLiuH97
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x580), 28, 1, 1, 0, 5, 0, 1);
            }
        }
        public static double GuoYaH97
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x580), 27, 1, 1, 0, 5, 0, 1);
            }
        }
        public static double YingJianH97
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x580), 24, 1, 1, 0, 5, 0, 1);
            }
        }
        public static double CanHX612
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 24, 1, 1, 0, 5);
            }
        }
        public static double RTC612
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 25, 1, 1, 0, 5);
            }
        }
        public static double DianLiu612
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 26, 1, 1, 0, 5);
            }
        }
        public static double GuoYa612
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 27, 1, 1, 0, 5);
            }
        }
        public static double YingJian612
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 28, 1, 1, 0, 5);
            }
        }

        public static double CanHXC1046
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 24, 1, 1, 0, 5);
            }
        }
        public static double RTCC1046
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 25, 1, 1, 0, 5);
            }
        }
        public static double DianLiuC1046
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 26, 1, 1, 0, 5);
            }
        }
        public static double GuoYaC1046
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 27, 1, 1, 0, 5);
            }
        }
        public static double YingJianC1046
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x552), 28, 1, 1, 0, 5);
            }
        }


        public static double CanHXRC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x62B), 3, 1, 1, 0, 5);
            }
        }
        public static double RTCRC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x62B), 1, 1, 1, 0, 5);
            }
        }
        public static double DianLiuRC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x62B), 2, 1, 1, 0, 5);
            }
        }
        public static double GuoYaRC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x62B), 0, 1, 1, 0, 5);
            }
        }
        public static double YingJianRC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x62B), 5, 3, 1, 0, 5);
            }
        }


        public static double CanHXDF
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x33B), 116, 1, 1, 0, 5);
            }
        }
        public static double RTCDF
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x33B), 114, 1, 1, 0, 5);
            }
        }
        public static double DianLiuDF
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x33B), 115, 1, 1, 0, 5);
            }
        }
        public static double GuoYaDF
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x33B), 112, 1, 1, 0, 5);
            }
        }
        public static double YingJianDF
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x33B), 113, 1, 1, 0, 5);
            }
        }
        /// <summary>
        /// 单体电压1
        /// </summary>
        public static double DY1 {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 11, 13, 0.001f, 0, 5);
            }
        }
        public static double DY2 {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 30,  13, 0.001f, 0, 5);
            }
        }
        public static double DY3 {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 33, 13, 0.001f, 0, 5);
            }
        }
        public static double DY4 {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 52, 13, 0.001f, 0, 5);
            }
        }
        /// <summary>
        /// 内总电压
        /// </summary>
        public static double NZDY {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 92, 15, 0.001f, 0, 20);
            }
        }
        /// <summary>
        /// 外总电压
        /// </summary>
        public static double WZDY {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 109, 15, 0.001f, 0, 20);
            }
        }

        public static (double,double,double) DTemperatures
        {
            get
            {
                return ( DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXT1_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXT1_Startbit), int.Parse(ConfigHelper.UdsParameters.DXT1_Len), double.Parse(ConfigHelper.UdsParameters.DXT1_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.DXT1_Offset)),
                DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXT2_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXT2_Startbit), int.Parse(ConfigHelper.UdsParameters.DXT2_Len), double.Parse(ConfigHelper.UdsParameters.DXT2_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.DXT2_Offset)),
                DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.DXT3_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.DXT3_Startbit), int.Parse(ConfigHelper.UdsParameters.DXT3_Len), double.Parse(ConfigHelper.UdsParameters.DXT3_Factor), -40, 125, int.Parse(ConfigHelper.UdsParameters.DXT3_Offset)));
            }
        }

        public static (double, double, double) DXTemperatures
        {
            get
            {
                return (PlayUDSHelper.GetDXTemperature1()-50, PlayUDSHelper.GetDXTemperature2() - 50, PlayUDSHelper.GetDXTemperature3() - 50);
                    
            }
        }

        public static (double, double, double) DXTemperaturesCC
        {
            get
            {
                return (PlayUDSHelper.GetDXTemperature1()*0.5 - 50, PlayUDSHelper.GetDXTemperature2()*0.5 - 50, PlayUDSHelper.GetDXTemperature3() * 0.5 - 50);

            }
        }

        public static (double, double, double) DXTemperatures1068
        {
            get
            {
                return (PlayUDSHelper.GetDXTemperature11068() - 40, PlayUDSHelper.GetDXTemperature21068() - 40,0);

            }
        }
        public static double SOC {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.SOCR_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.SOCR_Startbit), int.Parse(ConfigHelper.UdsParameters.SOCR_Len), double.Parse(ConfigHelper.UdsParameters.SOCR_Factor), 0, 100, int.Parse(ConfigHelper.UdsParameters.SOCR_Offset));
            }
        }
        public static double SOH {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.SOHR_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.SOHR_Startbit), int.Parse(ConfigHelper.UdsParameters.SOHR_Len), double.Parse(ConfigHelper.UdsParameters.SOHR_Factor), 0, 100, int.Parse(ConfigHelper.UdsParameters.SOHR_Offset));
            }
        }

        public static double MOSC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x373), 41, 3, 1, 0, 20);
            }
        }

        public static double MOSD
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x373), 54, 3, 1, 0, 20);
            }
        }
        public static double AHC
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 176, 32, 0.001f, 0, 20);
            }
        }

        public static double AHF
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(0x372), 208, 32, 0.001f, 0, 20);
            }
        }
        #endregion

        static double _soc=double.NaN;
        static object _lock = new object();
   

        static (double, double) _NAndWZDY;
        /// <summary>
        /// 获取内外总电压
        /// </summary>
        /// <returns></returns>
        public static (double,double) NAndWZDY
        {
            get
            {
                var nz = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.NZV_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.NZV_Startbit), int.Parse(ConfigHelper.UdsParameters.NZV_Len), double.Parse(ConfigHelper.UdsParameters.NZV_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.NZV_Offset));
                var wz = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.WZV_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.WZV_Startbit), int.Parse(ConfigHelper.UdsParameters.WZV_Len), double.Parse(ConfigHelper.UdsParameters.WZV_Factor), 0, 20, int.Parse(ConfigHelper.UdsParameters.WZV_Offset));
                return (nz ,wz);
                
            }
        }
        public static double MCUV
        {
            get
            {
                return DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.MCUCV_ID, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.MCUCV_Startbit), int.Parse(ConfigHelper.UdsParameters.MCUCV_Len), double.Parse(ConfigHelper.UdsParameters.MCUCV_Factor), 0, 100, int.Parse(ConfigHelper.UdsParameters.MCUCV_Offset));
            }
        }
        public static string GetSoftVersion
        {
            get
            {
                var temp1 = DataParsing.CanDataParsingStr(GetData(uint.Parse(ConfigHelper.UdsParameters.PRID1, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PRSTARTBIT1), int.Parse(ConfigHelper.UdsParameters.PRLEN1), double.Parse(ConfigHelper.UdsParameters.PRFACTOR1));
                var temp2 = DataParsing.CanDataParsingStr(GetData(uint.Parse(ConfigHelper.UdsParameters.PRID2, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PRSTARTBIT2), int.Parse(ConfigHelper.UdsParameters.PRLEN2), double.Parse(ConfigHelper.UdsParameters.PRFACTOR2));
                var temp3 = DataParsing.CanDataParsingStr(GetData(uint.Parse(ConfigHelper.UdsParameters.PRID3, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PRSTARTBIT3), int.Parse(ConfigHelper.UdsParameters.PRLEN3), double.Parse(ConfigHelper.UdsParameters.PRFACTOR3));

                return temp1 + temp2 + temp3;
            }
        }

        public static string GetSoftVersionXP
        {
            get
            {
                var temp1 = DataParsing.CanDataParsingStrXP(GetData(uint.Parse(ConfigHelper.UdsParameters.PRID1, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PRSTARTBIT1), int.Parse(ConfigHelper.UdsParameters.PRLEN1), double.Parse(ConfigHelper.UdsParameters.PRFACTOR1));
                var temp2 = DataParsing.CanDataParsingStrXP(GetData(uint.Parse(ConfigHelper.UdsParameters.PRID2, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PRSTARTBIT2), int.Parse(ConfigHelper.UdsParameters.PRLEN2), double.Parse(ConfigHelper.UdsParameters.PRFACTOR2));
                var temp3 = DataParsing.CanDataParsingStrXP(GetData(uint.Parse(ConfigHelper.UdsParameters.PRID3, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.PRSTARTBIT3), int.Parse(ConfigHelper.UdsParameters.PRLEN3), double.Parse(ConfigHelper.UdsParameters.PRFACTOR3));

                return temp1 +"."+ temp2 + "." + temp3;
            }
        }
        public static string GetHardVersionXP
        {
            get
            {
                var v1 = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.RJ_ID1, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.RJ_Startbit1), int.Parse(ConfigHelper.UdsParameters.RJ_Len1), double.Parse(ConfigHelper.UdsParameters.RJ_Factor1), 0, 1000, int.Parse(ConfigHelper.UdsParameters.RJ_Offset1));
                var v2 = DataParsing.CanDataParsing(GetData(uint.Parse(ConfigHelper.UdsParameters.RJ_ID2, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.RJ_Startbit2), int.Parse(ConfigHelper.UdsParameters.RJ_Len2), double.Parse(ConfigHelper.UdsParameters.RJ_Factor2), 0, 1000, int.Parse(ConfigHelper.UdsParameters.RJ_Offset2));

                return v1 + "." + v2;
            }
        }

        public static string GetSoftVersionMS
        {
            get
            {
                var temp1 = DataParsing.CanDataParsingStrMS(GetData(uint.Parse(ConfigHelper.UdsParameters.RJ_ID1, System.Globalization.NumberStyles.HexNumber)), int.Parse(ConfigHelper.UdsParameters.RJ_Startbit1), int.Parse(ConfigHelper.UdsParameters.RJ_Len1), double.Parse(ConfigHelper.UdsParameters.RJ_Factor1));
               
                return temp1;
            }
        }

    }
}
