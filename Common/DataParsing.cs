﻿﻿﻿using AllData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class DataParsing
    {
        /// <summary>
        /// 产品CAN解析
        /// </summary>
        /// <param name="bytes">can接收的数据</param>
        /// <param name="start">开始位置</param>
        /// <param name="len">长度</param>
        /// <param name="factor">乘以系数</param>
        /// <param name="min">最小值</param>
        /// <param name="max">最大值</param>
        /// <param name="offset">偏移量</param>
        /// <returns>原始结果，需要乘以系数这些进行换算</returns>
        public static double CanDataParsing(byte[] bytes, int start, int len, double factor = 1, int min = int.MinValue, int max = int.MaxValue, int offset = 0,int hxflag = 0)
        {
            if(ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
            {

                if (bytes == null || start < 0 || len <= 0 || (start + len) > bytes.Length * 8)
                    return 0;

                // 计算起始字节和位偏移
                int startByte = start / 8;
                int startBit = start % 8;
    
                long value = 0;
    
                // 按位提取数据（小端序）
                for (int i = 0; i < len; i++)
                {
                    int currentBit = startBit + i;
                    int byteIndex = startByte + (currentBit / 8);
                    int bitIndex = currentBit % 8;
        
                    if (byteIndex >= bytes.Length) break;
        
                    // 提取位并设置到对应位置
                    int bit = (bytes[byteIndex] >> bitIndex) & 1;
                    value |= (long)bit << i;
                }

                // 有符号数处理
                if (len < 64 && (value & (1L << (len - 1))) != 0)
                {
                    value |= ~((1L << len) - 1);
                }

                double result = value * factor + offset;
                return Math.Max(min, Math.Min(max, result));
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C" || ((ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A") && hxflag == 0))
            {

                var strings = ConvertToBinary(bytes);
                //合并字节
                string str = "";
                foreach (var s in strings)
                {
                    str += s;
                }
                string res = "";
                for (int i = start - 1; i < len + start - 1; i++)
                {
                    res += str[i].ToString();
                }

                //取到的十进制数据
                var resultTemp = Convert.ToInt32(res, 2);

                //十六进制数据
                var result16 = resultTemp.ToString("X2");
                var result = factor * resultTemp + offset;
                if (result < min)
                {
                    return min;
                }
                else if (result > max)
                {
                    return max;

                }
                return result;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                // 摩托罗拉MSB解析（大端序）
                if (bytes == null || start < 0 || len <= 0 || (start + len) > bytes.Length * 8)
                    return 0;

                // 计算起始字节和位偏移
                int startByte = start / 8;
                int startBit = start % 8;
    
                long value = 0;
    
                // 按位提取数据（大端序，MSB优先）
                for (int i = 0; i < len; i++)
                {
                    int currentBit = startBit + i;
                    int byteIndex = startByte + (currentBit / 8);
                    int bitIndex = 7 - (currentBit % 8); // MSB优先，所以位索引是7 - 偏移
        
                    if (byteIndex >= bytes.Length) break;
        
                    // 提取位并设置到对应位置
                    int bit = (bytes[byteIndex] >> bitIndex) & 1;
                    value |= (long)bit << (len - 1 - i); // 从高位到低位设置
                }

                

                double result = value * factor + offset;
                return Math.Max(min, Math.Min(max, result));
            }
            else
            {
                var strings = ConvertToBinary(bytes);
                //合并字节
                string str = "";
                foreach (var s in strings)
                {
                    str += s;
                }
                //计算实际开始位置
                int start1 = start / 8 * 8 + (8 - start % 8);
                string res = "";
                //往后逆序取数据
                for (int i = 1; i <= len; i++)
                {
                    res = res.Insert(0, str[start1 - i].ToString());
                }
                //取到的十进制数据
                var resultTemp = Convert.ToInt32(res, 2);

                //十六进制数据
                var result16 = resultTemp.ToString("X2");
                var result = factor * resultTemp + offset;
                if (result < min)
                {
                    return min;
                }
                else if (result > max)
                {
                    return max;

                }
                return result;
            }
                
        }

        public static string CanDataParsingStr(byte[] bytes, int start, int len, double factor = 1, int min = int.MinValue, int max = int.MaxValue, int offset = 0)
        {
            var strings = ConvertToBinary(bytes);
            //合并字节
            string str = "";
            foreach (var s in strings)
            {
                str += s;
            }
            string res = "";
            for (int i = start - 1; i < len + start - 1; i++)
            {
                res += str[i].ToString();
            }

            //取到的十进制数据
            var resultTemp = Convert.ToInt32(res, 2);

            //十六进制数据
            var result16 = resultTemp.ToString("X2");
            return result16;
        }

        public static string CanDataParsingStrXP(byte[] bytes, int start, int len, double factor = 1, int min = int.MinValue, int max = int.MaxValue, int offset = 0)
        {
            var strings = ConvertToBinary(bytes);
            //合并字节
            string str = "";
            foreach (var s in strings)
            {
                str += s;
            }
            string res = "";
            for (int i = start - 1; i < len + start - 1; i++)
            {
                res += str[i].ToString();
            }

            //取到的十进制数据
            var resultTemp = Convert.ToInt32(res, 2);

            //十六进制数据
            var result16 = resultTemp.ToString("X1");
            return result16;
        }

        public static string CanDataParsingStrMS(byte[] bytes, int start, int len, double factor = 1, int min = int.MinValue, int max = int.MaxValue, int offset = 0)
        {
            var strings = ConvertToBinary(bytes);
            //合并字节
            string str = "";
            foreach (var s in strings)
            {
                str += s;
            }
            string res = "";
            for (int i = start - 1; i < len + start - 1; i++)
            {
                res += str[i].ToString();
            }

            //取到的十进制数据
            var resultTemp = Convert.ToInt32(res, 2);

            //十六进制数据
            var result16 = resultTemp.ToString("D2");
            return result16;
        }
        public static bool CanDataParsing(byte[] bytes, int start)
        {
            var strings = ConvertToBinary(bytes);
            //合并字节
            string str = "";
            foreach (var s in strings)
            {
                str += s;
            }
            //计算实际开始位置
            int start1 = start / 8 * 8 + (8 - start % 8);
            if (str[start1] == '1')
            {
                return true;
            }
            else if (str[start1] == '1')
            {
                return false;
            }
            return false;
        }
        /// <summary>
        /// 将十六进制字节数组转成二进制字符串集合，每一项8个字节
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static string[] ConvertToBinary(byte[] data)
        {
            string[] binaryStrings = new string[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                binaryStrings[i] = Convert.ToString(data[i], 2).PadLeft(8, '0'); // 转换为二进制并补齐到8位
            }
            return binaryStrings;
        }
    }
}
