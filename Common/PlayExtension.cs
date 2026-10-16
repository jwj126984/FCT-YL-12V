using NPOI.SS.UserModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common
{
    public static class PlayExtension
    {

        /// <summary>
        /// 十六进制字符串转字符数组
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] ToHexByteArray(this string hexString)
        {
            hexString = CheakHexString(hexString);
            hexString = hexString.Replace(" ", "");
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        /// <summary>
        /// 校验16进制字符串
        /// </summary>
        /// <param name="strBuffHex">16进制字符串</param>
        /// <returns></returns>
        private static string CheakHexString(string strBuffHex)
        {
            strBuffHex = strBuffHex.Trim();     //去除前后空字符
            strBuffHex = strBuffHex.Replace(',', ' ');  //去掉英文逗号
            strBuffHex = strBuffHex.Replace('，', ' '); //去掉中文逗号
            strBuffHex = strBuffHex.Replace("0x", "");  //去掉0x
            strBuffHex = strBuffHex.Replace("0X", "");  //去掉0X
            strBuffHex = Regex.Replace(Regex.Replace(strBuffHex, @"(?i)[^a-f\d\s]+", ""), "\\w{3,}", m => string.Join(" ", Regex.Split(m.Value, @"(?<=\G\w{2})(?!$)").Select(x => x.PadLeft(2, '0')).ToArray())).ToUpper();
            return strBuffHex;
        }

        /// <summary>
        /// 小数位截取
        /// </summary>
        /// <param name="f"></param>
        /// <param name="decimalPlaces"></param>
        /// <returns></returns>
        public static double TruncateDouble(this double f, int decimalPlaces)
        {
            double factor = (double)Math.Pow(10, decimalPlaces);
            return (double)Math.Truncate(f * factor) / factor;
        }
        /// <summary>
        /// 获取字符串中的数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static decimal GetNumber(this string str)
        {
            string numberStr = str;
            numberStr = Regex.Replace(numberStr, @"[^\d.\d]", "");
            // 如果是数字，则转换为decimal类型
            if (Regex.IsMatch(numberStr, @"^[+-]?\d*[.]?\d*$"))
            {
                return decimal.Parse(numberStr);
            }
            return decimal.Zero;
        }

        /// <summary>
        /// 将整数转为指定个数的字符数组字符串
        /// </summary>
        /// <param name="decimalNumber"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string ConvertToFormattedHex(this short decimalNumber, int len)
        {
            // 转换为十六进制，填充到至少8位
            string hex = decimalNumber.ToString($"X{len * 2}");

            // 每两位插入一个空格
            string formattedHex = string.Join(" ", SplitStringByLength(hex, 2));

            return formattedHex;

            string[] SplitStringByLength(string input, int length)
            {
                int count = input.Length / length;
                string[] result = new string[count];

                for (int i = 0; i < count; i++)
                {
                    result[i] = input.Substring(i * length, length);
                }

                return result;
            }
        }
        /// <summary>
        /// 将整数转为指定个数的字符数组字符串
        /// </summary>
        /// <param name="decimalNumber"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        public static string ConvertToFormattedHex(this ushort decimalNumber, int len)
        {
            // 转换为十六进制，填充到至少8位
            string hex = decimalNumber.ToString($"X{len * 2}");

            // 每两位插入一个空格
            string formattedHex = string.Join(" ", SplitStringByLength(hex, 2));

            return formattedHex;

            string[] SplitStringByLength(string input, int length)
            {
                int count = input.Length / length;
                string[] result = new string[count];

                for (int i = 0; i < count; i++)
                {
                    result[i] = input.Substring(i * length, length);
                }

                return result;
            }
        }
        /// <summary>
        /// 获取流程对应的中文名称
        /// </summary>
        /// <returns></returns>
        public static string GetShowTxt(this string name)
        {
            if (name == "Awaken")
            {
                return "唤醒处理";
            }
            else if (name == "FenLiuR")
            {
                return "分流器阻抗测量";
            }
            else if (name == "endcs")
            {
                return "出厂设置测试";
            }
            else if (name == "GetProduceInfo")
            {
                return "获取产品信息";
            }
            else if (name == "EOLClose")
            {
                return "关闭EOL";
            }
            else if (name == "WakeCalibration")
            {
                return "唤醒电流阈值标定和读取";
            }
            else if (name == "ProduceRead")
            {
                return "获取产品信息";
            }
            else if (name == "WorkingCurrent")
            {
                return "工作电流测试";
            }
            else if (name == "PCBTemperature")
            {
                return "PCB温度测试";
            }
            else if (name == "H_D_OAndRelayControl")
            {
                return "高驱输出控制以及继电器控制测试";
            }
            else if (name == "VoltageCalibration")
            {
                return "电压标定";
            }
            else if (name == "VoltageTest")
            {
                return "单体电压、内总电压、外总电压测试";
            }
            else if (name == "ModuleTemperature")
            {
                return "模组温度测试";
            }
            else if (name == "ModuleTemperatureTest")
            {
                return "模组温度测试";
            }
            else if (name == "BalancedTest")
            {
                return "均衡测试";
            }
            else if (name == "CurrentCalibration")
            {
                return "电流标定";
            }
            else if (name == "CurrentTest")
            {
                return "电流测试";
            }
            else if (name == "Sleep")
            {
                return "休眠处理";
            }
            else if (name == "SleepCurrent")
            {
                return "休眠电流";
            }
            else if (name == "SleepingCurrent")
            {
                return "休眠电流";
            }
            else if (name == "AwakenSource")
            {
                return "唤醒源测试";
            }
            else if (name == "Awaken")
            {
                return "唤醒处理";
            }
            else if (name == "SOCAndSOH")
            {
                return "SOC标定以及SOH读取";
            }
            else if (name == "BusMalfunction")
            {
                return "故障诊断";
            }
            else if (name == "DateOfManufactureW_R")
            {
                return "产品日期回读";
            }
            else if (name == "DateOfManufactureW_RBefor")
            {
                return "产品日期写入";
            }
            else if (name == "IndividualVoltage")
            {
                return "单体电压测试";
            }
            else if (name == "TotalVoltage")
            {
                return "总电压测试";
            }
            else if (name == "ClearCalibration")
            {
                return "清除标定信息";
            }
            else if(name == "MOSControl")
            {
                return "MOS控制测试";
            }
            else if (name == "BalancedAndAFE")
            {
                return "均衡与AFE诊断";
            }
            else if (name == "CanTRTest")
            {
                return "终端电阻测试";
            }
            else if (name == "CellTemperature")
            {
                return "电芯温度测试";
            }
            else if (name == "ClearDTC")
            {
                return "清除故障";
            }
            else if (name == "CurrentLimiting")
            {
                return "限流模块测试";
            }
            else if (name == "EEPROMTest")
            {
                return "EEPROM测试";
            }
            else if (name == "LBMSRead")
            {
                return "产品信息读取";
            }
            else if (name == "LBMSWrite")
            {
                return "产品信息写入";
            }
            else if (name == "MOSSleepKeep")
            {
                return "MOS休眠维持";
            }
            else if (name == "WakeSourceTest")
            {
                return "唤醒源测试";
            }
            else if (name == "CurrentCollection")
            {
                return "总电流测试";
            }
            else if (name == "SleepingCurrent")
            {
                return "休眠电流测试";
            }
            else if (name == "ClearAH")
            {
                return "清除累计充放电AH";
            }

            return name;
        }
        /// <summary>
        /// bool值转 PASS或者Fail
        /// </summary>
        /// <returns></returns>
        public static string BoolToStr1(this bool b)
        {
            return b ? "PASS" : "Fail";
        }

        /// <summary>
        /// bool值转 合格或者不合格
        /// </summary>
        /// <returns></returns>
        public static string BoolToStr2(this bool b)
        {
            return b ? "合格" : "不合格";
        }


        /// <summary>
        /// bool值转 PASS或者Fail
        /// </summary>
        /// <returns></returns>
        public static string BoolToStr1(this bool? b)
        {
            if (b == null)
            {
                return "";
            }
            return b == true ? "PASS" : "Fail";
        }

        /// <summary>
        /// bool值转 合格或者不合格
        /// </summary>
        /// <returns></returns>
        public static string BoolToStr2(this bool? b)
        {
            if (b == null)
            {
                return "";
            }
            return b == true ? "合格" : "不合格";
        }
        /// <summary>
        /// 可空对象转数据库字符串
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public static string CanNullObjToStr(this object o)
        {
            if (o == null)
            {
                return "null";
            }
            return o.ToString();
        }


        public static string ExceptionGetMessage(this Exception exception)
        {
            return _(exception);

            string _( Exception e, string str = "")
            {
                str += e.Message + "\n";
                if (e.InnerException != null)
                {
                    return  _(e.InnerException, str);
                }
                return str;
            }
        }
    }
}
