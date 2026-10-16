using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Can
{
    public static class CanCmd
    {
        /// <summary>
        /// 连接
        /// </summary>
        public static string ConnectCmd { get; set; } = "11 01 01 55 aa 55 aa";
        /// <summary>
        /// 断开连接
        /// </summary>
        public static string DisConnectCmd { get; set; } = "11 01 02 55 aa 55 aa";
        /// <summary>
        /// 开始测试
        /// </summary>
        public static string StartTestCmd { get; set; } = "13 01 01";
        /// <summary>
        /// 停止测试
        /// </summary>
        public static string StopTestCmd { get; set; } = "13 01 02";
        /// <summary>
        /// 唤醒
        /// </summary>
        public static string AwakenCmd { get; set; } = "14 01 01";
        /// <summary>
        /// 休眠
        /// </summary>
        public static string DormancyCmd { get; set; }= "14 01 02";
        /// <summary>
        /// 读取电源输出
        /// </summary>
        /// <param name="power1"></param>
        /// <param name="power2"></param>
        /// <returns></returns>
        public static string ReadPowerCmd(bool power1,bool power2)
        {
            string str = "15 01 ";
            if (power1)
            {
                str += "01 ";
            }
            else
            {
                str += "00 ";
            }
            if (power2)
            {
                str += "01 ";
            }
            else
            {
                str += "00 ";
            }
            return str;
        }
        /// <summary>
        /// 单体电压
        /// </summary>
        public static string IndividualVoltageCmd { get; set; } = "01 01";

        public static string AFEInternalTotalPressureCmd { get; set; } = "02 01";

        public static string ExternalTotalPressureCmd { get; set; } = "03 01";

        public static string CellTemperatureCmd(CellTemperatureEnum cell1, CellTemperatureEnum cell2, CellTemperatureEnum cell3, CellTemperatureEnum cell4)
        {
            return $"04 01 {CellTemperatureEnumToStr(cell1)} {CellTemperatureEnumToStr(cell2)} {CellTemperatureEnumToStr(cell3)} {CellTemperatureEnumToStr(cell4)}";
            string CellTemperatureEnumToStr(CellTemperatureEnum cell)
            {
                return ((byte)cell).ToString("X2");
            }
        }

        public static string BoardTemperatureCmd { get; set; } = "05 01";

        public static string TotalCurrentCollectionCmd(bool isJust = true)
        {
            if (isJust)
            {
                return "06 01 00";
            }
            else
            {
                return "06 01 01";
            }
        }
        public static string TotalCurrentCollectionCloseCmd { get; set; } = "06 01 02";
        public static string WorkCurrentCollectionCmd { get; set; } = "07 01";

        public static string MOSVoltageCollectionCmd { get; set; } = "08 01";

        public static string ControlVoltageEnableCmd(bool within,bool outside)
        {
            return $"09 01 {(within?"01":"00")} {(outside ? "01" : "00")}";
        }
        public static string ResistivePowerOnCmd(bool enable)
        {
            return $"0A 01 {(enable ? "01" : "00")}";
        }
        public static string ResistiveResultCmd { get; set; } = "0B 01";
        public static string TestResultCmd (bool isPass)
        {
            return $"0C 01 {(isPass ? "00" : "01")}";
        }

        public static string BalancedControlCmd(bool? isc1AndC3)
        {
            string b = "00";
            if (isc1AndC3 == true)
            {
                b = "00";
            }
            else if (isc1AndC3 == false)
            {
                b = "01";
            }
            else
            {
                b = "02";
            }
            return $"1F 01 {b}";
        }
    }
    public enum CellTemperatureEnum
    {
        /// <summary>
        /// -20
        /// </summary>
        T_20=0x00,
        /// <summary>
        /// 40
        /// </summary>
        T_45=0x01,
        /// <summary>
        /// 80
        /// </summary>
        T_85=0x02
    }
}
