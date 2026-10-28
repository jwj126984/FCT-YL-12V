using AllData;
using Common.Can;
using NationalInstruments.Visa;
using NPOI.POIFS.Crypt.Dsig;
using NPOI.SS.Formula.Functions;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Common.UDS
{
    public class PlayUDSHelper
    {
        static CanOperate _uds;
        static byte[] _data;
        static List<byte> _list = new List<byte>();
        private static ManualResetEvent _responseReceivedEvent = new ManualResetEvent(false);
        public static void SetUDS(CanOperate uds, int sid = 0x77d)
        {
            _uds = uds;
            _uds.ReviceData += (id, arr) =>
            {
                if (sid == id)
                {
                    if (_list.Count > 1000000)
                    {
                        _list.Clear();
                    }
                    _list.AddRange(arr);
                    _data = arr;
                    // 通知主线程响应已接收
                    _responseReceivedEvent.Set();
                }
            };
        }
        static object _lock = new object();


        public static byte[] SendUDS(string data, uint sid = 0x775, int len = 8, int sendCount = 0)
        {
            sid = uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber);
            ManualResetEvent r = new ManualResetEvent(false);
            byte[] response = null;

            _uds.Send(sid, data, ConfigHelper.Cantype, true, len, true);
            _uds.ReviceData += reviceData;

            r.Reset();
            if (r.WaitOne((int)(CommonData.AwaitTime * 1000)))
            {
                _uds.ReviceData -= reviceData;
                return response;
            }
            else
            {
                _uds.ReviceData -= reviceData;
                if (sendCount < 2)
                {
                    sendCount++;
                    return SendUDS(data, sid, len, sendCount);
                }
                throw new Exception($"{data} 产品UDS请求超时");
            }

            _data = null;
            int list_len1 = _list.Count;
            _uds.Send(sid, data, true, true, len);
            DateTime now = DateTime.Now;

            while (true)
            {
                int list_len2 = _list.Count;
                if (list_len1 != list_len2)
                {
                    if (list_len2 > list_len1)
                    {
                        lock (_lock)
                        {
                            var temp = _list.Skip(list_len1).ToArray();
                            return temp;
                        }
                    }
                    else
                    {
                        return _list.ToArray();
                    }
                }
                if (_data != null)
                {
                    return _data;
                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("UDS 请求超时");
                    return null;
                }
            }

            void reviceData(uint id, byte[] arr)
            {
                if (id == uint.Parse(ConfigHelper.UdsParameters.RID, System.Globalization.NumberStyles.HexNumber))
                {
                    response = arr.Select(m => m).ToArray();
                    r.Set();
                }
            }
        }

        public static List<byte[]> SendUDS2(string data, uint sid = 0x775, int len = 8, int sendCount = 0)
        {
            sid = uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber);
            uint responseId = uint.Parse(ConfigHelper.UdsParameters.RID, System.Globalization.NumberStyles.HexNumber);

            // 用于存储所有接收到的响应
            List<byte[]> responses = new List<byte[]>();
            ManualResetEvent responseReceived = new ManualResetEvent(false);
            bool isTimeout = false;

            // 设置接收事件处理程序
            void reviceDataHandler(uint id, byte[] arr)
            {
                if (id == responseId)
                {
                    lock (responses)
                    {
                        responses.Add(arr.ToArray()); // 保存每条响应

                        // 如果这是第一条响应，设置事件信号
                        //  if (responses.Count == 1)
                        {
                            responseReceived.Set();
                        }
                    }
                }
            }



            try
            {
                // 发送请求
                _uds.Send(sid, data, true, true, len, true);
                _uds.ReviceData += reviceDataHandler;

                // 等待第一条响应
                if (!responseReceived.WaitOne((int)(CommonData.AwaitTime * 1000)))
                {
                    isTimeout = true;
                }

                // 即使收到第一条响应，也等待可能的后续响应
                if (!isTimeout)
                {
                    // 额外等待时间用于接收可能的后续响应
                    DateTime startTime = DateTime.Now;
                    while ((DateTime.Now - startTime).TotalMilliseconds < 500)
                    {
                        Thread.Sleep(5); // 小间隔检查
                    }
                }

                // 处理超时或重试
                if (isTimeout)
                {
                    _uds.ReviceData -= reviceDataHandler;

                    if (sendCount < 2)
                    {
                        sendCount++;
                        return SendUDS2(data, sid, len, sendCount);
                    }
                    throw new Exception($"{data} 产品UDS请求超时");
                }

                return responses;
            }
            finally
            {
                _uds.ReviceData -= reviceDataHandler;
            }
        }

        /// <summary> 
        /// 字节数组转16进制字符串 
        /// </summary> 
        /// <param name="bytes"></param> 
        /// <returns></returns> 
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr.Substring(0, returnStr.Length - 1);
        }
        public static Tuple<byte, List<byte>> Send(byte[] data, byte sid, uint addr = 0x775, byte dataLen = 0x08, int cf = 0, bool isAwaitAck = true, bool isudslen = true)
        {
            List<byte> list = new List<byte>();
            addr = uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber);
            byte firstByte = (byte)(data.Length + 1);
            list.Add(firstByte);
            list.Add(sid);
            list.AddRange(data);
            if (dataLen != 8)
            {
                list.Insert(0, 0x00);
            }
            var temp = byteToHexStr(list.ToArray());
            LogHelper.Debug($"UDS发送数据:{temp} 长度:{dataLen}");

            var result = SendUDS(temp, addr, dataLen);

            LogHelper.Debug($"UDS接收数据：{string.Join(" ", result.Select(m => m.ToString("X2")))}");
            if (isAwaitAck == false)
                return null;
            if (result == null)
            {
                return null;
            }
            int lenIndex = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] != 0)
                {
                    lenIndex = i;
                    break;
                }
            }
            if (isudslen == true)
            {
                int len = result[lenIndex];
                if (!ConfigHelper.Cantype)
                {
                    len = 7;
                }
                var id = result[lenIndex + 1];
                if (id == 0x7f)
                {
                    LogHelper.Debug($"UDS 回复7F");
                    return null;
                }
                
                list = new List<byte>();
                for (int i = lenIndex + 3; i < len + lenIndex + 1; i++)
                {
                    list.Add(result[i]);
                }
                list.Insert(0, result[lenIndex + 2]);
                return new Tuple<byte, List<byte>>(id, list);
            }
            else
            {
                var id = result[lenIndex + 1];
                int len = result.Length;
                list = new List<byte>();
                for (int i = 0; i < len; i++)
                {
                    list.Add(result[i]);
                }
                return new Tuple<byte, List<byte>>(id, list);

            }




        }
        /// <summary>
        /// 进入拓展
        /// </summary>
        /// <returns></returns>
        public static bool GoToExtencion()
        {
            LogHelper.Debug("进入拓展");
            var temp = Send(new byte[] { 0x03 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            return temp != null;
        }

        public static bool GoToGC()
        {
            LogHelper.Debug("进入拓展");
            var temp = Send(new byte[] { 0x63,0x00 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));


            temp = Send(new byte[] { 0x63, 0x03, 0x04, 0x0C, 0x08 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));

            return temp != null;
        }

        public static bool GoToAPP()
        {
            LogHelper.Debug("进入拓展");
            var temp = Send(new byte[] { 0x03 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));


            temp = Send(new byte[] { 0x01, 0x02, 0x03 }, 0x31, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            CommonMethod.Sleep(20);


            temp = Send(new byte[] { 0x02 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            CommonMethod.Sleep(500);
            temp = Send(new byte[] { 0x01, 0xF1, 0x84, 0xF1, 0x1F, 0xF2, 0x2F }, 0x31, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber),16);
            CommonMethod.Sleep(20);
            temp = Send(new byte[] { 0x01 }, 0x11, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            return temp != null;
        }
        public static bool GoToAPP_V57()
        {
            LogHelper.Debug("进入拓展");
            var temp = Send(new byte[] { 0x03 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));


            temp = Send(new byte[] { 0x01, 0x02, 0x03 }, 0x31, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            CommonMethod.Sleep(20);

            temp = Send(new byte[] { 0x02 }, 0x85, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            CommonMethod.Sleep(500);
            temp = Send(new byte[] { 0x03, 0x01 }, 0x28, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            CommonMethod.Sleep(500);

            temp = Send(new byte[] { 0x02 }, 0x10, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            CommonMethod.Sleep(500);
            temp = Send(new byte[] { 0x01, 0xF1, 0x84, 0xF1, 0x1F, 0xF2, 0x2F }, 0x31, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), 16);
            CommonMethod.Sleep(20);
            temp = Send(new byte[] { 0x01 }, 0x11, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            return temp != null;
        }
        /// <summary>
        /// 获取种子
        /// </summary>
        /// <returns></returns>
        public static List<byte> GetSeed()
        {
            LogHelper.Debug("获取种子");
            var temp = Send(new byte[] { 0x01 }, 0x27, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            if (temp == null)
            {
                return null;
            }



            if (ConfigHelper.SelectType != "seres" && ConfigHelper.UdsParameters.jiesuo != "JAC_X6" && ConfigHelper.UdsParameters.jiesuo != "DF_XY-A" &&
                ConfigHelper.UdsParameters.jiesuo != "DF_DH1" && ConfigHelper.UdsParameters.jiesuo != "ChangAn_C390" && ConfigHelper.UdsParameters.jiesuo != "HQ_E702" && ConfigHelper.UdsParameters.jiesuo != "HQ_C1079" &&
                ConfigHelper.UdsParameters.jiesuo != "ChangAn_B216" && ConfigHelper.UdsParameters.jiesuo != "QR_C1068" && ConfigHelper.UdsParameters.jiesuo != "WL_F710S" && ConfigHelper.UdsParameters.jiesuo != "RC_PK2C" && ConfigHelper.UdsParameters.jiesuo != "RC_PK1B")

            {
                return new List<byte>() { temp.Item2[1], temp.Item2[2], temp.Item2[3], temp.Item2[4], };
            }


            if (ConfigHelper.UdsParameters.xieyi == "CAN" || ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
            {

                var lists = new List<byte>();
                for (int i = 2; i < 6; i++)
                {
                    lists.Add(temp.Item2[i]);
                }

                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {

                        for (int i = 1; i < 8; i++)
                        {
                            lists.Add(respenson[i]);
                        }
                    }
                    if (respenson[0] == 0x22)
                    {

                        for (int i = 1; i < 6; i++)
                        {
                            lists.Add(respenson[i]);
                        }
                    }
                }

                return lists;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
            {

                var lists = new List<byte>();
                for (int i = 2; i < 6; i++)
                {
                    lists.Add(temp.Item2[i]);
                }

                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {

                        for (int i = 1; i < 8; i++)
                        {
                            lists.Add(respenson[i]);
                        }
                    }
                    if (respenson[0] == 0x22)
                    {

                        for (int i = 1; i < 8; i++)
                        {
                            lists.Add(respenson[i]);
                        }
                    }
                    if (respenson[0] == 0x23)
                    {

                        for (int i = 1; i < 8; i++)
                        {
                            lists.Add(respenson[i]);
                        }
                    }
                    if (respenson[0] == 0x24)
                    {

                        for (int i = 1; i < 8; i++)
                        {
                            lists.Add(respenson[i]);
                        }
                    }
                }

                return lists;
            }

            else
            {
                var list = new List<byte>();
                int startIndex = 1;
                for (int i = startIndex; i < 16 + startIndex; i++)
                {
                    list.Add(temp.Item2[i]);
                }
                return list;

            }


        }

        public static List<byte> GetSeedWL()
        {
            LogHelper.Debug("获取种子");
            var temp = Send(new byte[] { 0x03 }, 0x27, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
            if (temp == null)
            {
                return null;
            }

            return new List<byte>() { temp.Item2[1], temp.Item2[2], temp.Item2[3], temp.Item2[4], };
            
        }

        public static bool Biaodin1068(List<double> a_b)
        {
            List<byte> bytes = new List<byte>();
            bytes.Add(0x73);
            bytes.Add(0xb6);
            for (int i = 0; i < a_b.Count; i++)
            {
                double value = a_b[i] * 1000 + 1000000;
                int intValue = (int)value;
                bytes.Add((byte)((intValue >> 24) & 0xFF));
                bytes.Add((byte)((intValue >> 16) & 0xFF));
                bytes.Add((byte)((intValue >> 8) & 0xFF));
                bytes.Add((byte)(intValue & 0xFF));
            }

            string data = "00 2B 2E";
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            DateTime now = DateTime.Now;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x30, true);

            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("标定失败");
                    return false;

                }
            }
        }

        /// <summary>
        /// 标定
        /// </summary>
        /// <param name="seeds"></param>
        /// <returns></returns>
        public static bool Biaodin((double, double) a_b)
        {
            string data = $"10 17 2E FD 13 {((ushort)(a_b.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b.Item2 * 1000)) & 0xff).ToString("X2")} 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        public static bool BiaodinHQ((double, double) a_b)
        {
            string data = $"10 17 2E 70 9E {((ushort)(a_b.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b.Item2 * 1000)) & 0xff).ToString("X2")} 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }


        public static bool Biaodinwl((double, double) a_b)
        {
            string data = $"10 17 2E 30 26 {((ushort)(a_b.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b.Item2 * 1000)) & 0xff).ToString("X2")} 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool Biaodinchuwldy()
        {
            string data = $"10 13 2E 30 31 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

           
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool Biaodinwldy((double, double) a_b1, (double, double) a_b2, (double, double) a_b3, (double, double) a_b4)
        {
            string data = $"10 13 2E 30 31 {((ushort)(a_b1.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b1.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b1.Item2 * 1000)) & 0xff).ToString("X2")} {((ushort)(a_b2.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b2.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b3.Item1 * 1000)).ConvertToFormattedHex(2)}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 {((ushort)(a_b3.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item2 * 1000)).ConvertToFormattedHex(2)} 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

          
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }


        public static bool BiaodinchuHQdy()
        {
            string data = $"10 13 2E 70 99 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);


            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool BiaodinHQdy((double, double) a_b1, (double, double) a_b2, (double, double) a_b3, (double, double) a_b4)
        {
            string data = $"10 13 2E 70 99 {((ushort)(a_b1.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b1.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b1.Item2 * 1000)) & 0xff).ToString("X2")} {((ushort)(a_b2.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b2.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b3.Item1 * 1000)).ConvertToFormattedHex(2)}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 {((ushort)(a_b3.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item2 * 1000)).ConvertToFormattedHex(2)} 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);


            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        public static bool BiaodinchudyCANFD(string str)
        {
            string data = $"00 13 2E {str} 03 E8 00 00 03 E8 00 00 03 E8 00 00 03 E8 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 24, true);
            _data = null;
          
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        public static bool BiaodindyCANFD((double, double) a_b1, (double, double) a_b2, (double, double) a_b3, (double, double) a_b4, string str)
        {
            string data = $"00 13 2E {str} {((ushort)(a_b1.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b1.Item2 * 1000) >> 8).ToString("X2")} " +
                $"{(((uint)(a_b1.Item2 * 1000)) & 0xff).ToString("X2")} {((ushort)(a_b2.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b2.Item2 * 1000)).ConvertToFormattedHex(2)} " +
                $"{((ushort)(a_b3.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b3.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item2 * 1000)).ConvertToFormattedHex(2)}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
           

            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        public static bool Biaodinchudy(string str)
        {
            string data = $"10 13 2E {str} 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);


            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool Biaodindy((double, double) a_b1, (double, double) a_b2, (double, double) a_b3, (double, double) a_b4,string str)
        {
            string data = $"10 13 2E {str} {((ushort)(a_b1.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b1.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b1.Item2 * 1000)) & 0xff).ToString("X2")} {((ushort)(a_b2.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b2.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b3.Item1 * 1000)).ConvertToFormattedHex(2)}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 {((ushort)(a_b3.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item2 * 1000)).ConvertToFormattedHex(2)} 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);


            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        public static bool BiaodinchuAVATRdy()
        {
            string data = $"10 13 2E FD 17 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);


            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool BiaodinAVATRdy((double, double) a_b1, (double, double) a_b2, (double, double) a_b3, (double, double) a_b4)
        {
            string data = $"10 13 2E FD 17 {((ushort)(a_b1.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b1.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b1.Item2 * 1000)) & 0xff).ToString("X2")} {((ushort)(a_b2.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b2.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b3.Item1 * 1000)).ConvertToFormattedHex(2)}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 {((ushort)(a_b3.Item2 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item1 * 1000)).ConvertToFormattedHex(2)} {((ushort)(a_b4.Item2 * 1000)).ConvertToFormattedHex(2)} 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);


            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool GOTOGCPK()
        {
            string data = $"03 10 63 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(100);
            data = $"06 10 63 03 04 0C 08";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
          
            //   _data = null;
            DateTime now = DateTime.Now;
            return true;
        }
        public static bool keepcur()
        {
            LogHelper.Debug("保存电流");
            List<byte> bytes1 = new List<byte> { 0x01, 0x74, 0x15 };
            var temp = Send(bytes1.ToArray(), 0x31);
            if (temp != null)
            {
                return true;
            }
            return false;
        }
        public static bool Biaodinchu1068()
        {
            LogHelper.Debug("初始化标定");
            List<byte> bytes1 = new List<byte> { 0x01, 0x74, 0x16 };
            var temp = Send(bytes1.ToArray(), 0x31);
            if (temp != null)
            {
                return true;
            }
            return false;
        }
        public static bool Biaodinchu()
        {
            string data = $"10 17 2E FD 13 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        public static bool BiaodinchuC1046()
        {
            string data = $"00 17 2E 4B 40 03 E8 00 00 03 E8 00 00 03 E8 00 00 03 E8 00 00 03 E8 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 32, true);
            _data = null;
           
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }


        public static bool BiaodinchuHQ()
        {
            string data = $"10 17 2E 70 9E 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool Biaodinchuwl()
        {
            string data = $"10 17 2E 30 26 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //  CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }


        public static bool BiaodinRC((double, double) a_b)
        {
            string data = $"10 17 2E 13 00 {((ushort)(a_b.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b.Item2 * 1000) >> 8).ToString("X2")}";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {(((uint)(a_b.Item2 * 1000)) & 0xff).ToString("X2")} 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
              CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool BiaodinC1046((double, double) a_b)
        {
            string data = $"00 17 2E 4B 40 {((ushort)(a_b.Item1 * 1000)).ConvertToFormattedHex(2)} {((uint)(a_b.Item2 * 1000) >> 8).ToString("X2")} {(((uint)(a_b.Item2 * 1000)) & 0xff).ToString("X2")} 03 E8 00 00 03 E8" +
                $"00 00 03 E8 00 00 03 E8 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 32, true);
            _data = null;
            
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }
        public static bool BiaodinchuRC()
        {
            string data = $"10 17 2E 13 00 03 E8 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 00 03 E8 00 00 03 E8";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
              CommonMethod.Sleep(20);
            data = $"22 00 00 03 E8 00 00 03";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"23 E8 00 00 00 00 00 00";
            LogHelper.Debug($"key{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            //   _data = null;
            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null)
                {
                    //  LogHelper.ShowMsg($"{_data[1]}");
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }

                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    LogHelper.ShowMsg("解锁失败");
                    return false;

                }
            }
        }

        /// <summary>
        /// 解锁
        /// </summary>
        /// <param name="seeds">种子</param>
        /// <returns></returns>
        public static bool JieSuo(List<byte> seeds)
        {
            LogHelper.Debug($"开始解锁，seeks:{string.Join(" ", seeds.Select(m => m.ToString("X2")))}");
            var prjName = CommonData.BoardName;
            List<byte> key;


            FCT_GenerateKey.FCT_GenerateKey.ResultType res = FCT_GenerateKey.FCT_GenerateKey.GenerateKey(prjName, 0x01, seeds, out key);

            //     LogHelper.Debug($"密钥{BitConverter.ToString(key.ToArray())}");
            if (res != FCT_GenerateKey.FCT_GenerateKey.ResultType.SUCCESS)
            {
                LogHelper.Debug("种子转换失败");
                return false;
            }

            var bytes = new List<byte>();


            var bytes1 = new List<byte>() { 0x02 };
            bytes.AddRange(key);
            bytes1.AddRange(key);
            byte len = (byte)(ConfigHelper.SelectType != "seres" ? 0x08 : 0x40);

            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" ||
                ConfigHelper.UdsParameters.jiesuo == "JAC_X6" || ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || 
                ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                len = 0x40;
            }

            if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B")
            {
                len = 0x30;
            }

            if( ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
            {
                len = 0x14;
            }

            if ((!ConfigHelper.Cantype && len == 0x40 )|| ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A" || ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {

                string data = $"10 12 27 02 {((byte)bytes[0]).ToString("X2")} {((byte)bytes[1]).ToString("X2")} {((byte)bytes[2]).ToString("X2")} {((byte)bytes[3]).ToString("X2")}";
                LogHelper.Debug($"key{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
                _data = null;
                CommonMethod.Sleep(20);
                data = $"21 {((byte)bytes[4]).ToString("X2")} {((byte)bytes[5]).ToString("X2")} {((byte)bytes[6]).ToString("X2")} {((byte)bytes[7]).ToString("X2")} {((byte)bytes[8]).ToString("X2")} {((byte)bytes[9]).ToString("X2")} {((byte)bytes[10]).ToString("X2")}";
                LogHelper.Debug($"key{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
                //  CommonMethod.Sleep(20);
                data = $"22 {((byte)bytes[11]).ToString("X2")} {((byte)bytes[12]).ToString("X2")} {((byte)bytes[13]).ToString("X2")} {((byte)bytes[14]).ToString("X2")} {((byte)bytes[15]).ToString("X2")} 00 00";
                LogHelper.Debug($"key{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
                //   _data = null;
                DateTime now = DateTime.Now;

                while (true)
                {
                    if (_data != null)
                    {
                        //  LogHelper.ShowMsg($"{_data[1]}");
                        if (_data[1] == 0x67)
                        {
                            return true;
                        }

                    }

                    if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                    {
                        LogHelper.ShowMsg("解锁失败");
                        return false;

                    }
                }



            }
            else
            {
                var temp = Send(bytes1.ToArray(), sid: 0x27, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), dataLen: len)?.Item2;
                if (ConfigHelper.SelectType != "seres")
                {
                    if (temp == null || temp[0] != 0x02)
                    {
                        LogHelper.ShowMsg("解锁失败");
                        return false;

                    }
                    return true;
                }
                else
                {
                    if (temp == null)
                    {
                        return false;
                    }
                    //if (temp == null ||  temp[0] != 0x14)
                    //{
                    //    LogHelper.ShowMsg("解锁失败");
                    //    return false;
                    //}
                    return true;
                }
            }


        }


        public static bool JieSuoWL(List<byte> seeds)
        {
            LogHelper.Debug($"开始解锁，seeks:{string.Join(" ", seeds.Select(m => m.ToString("X2")))}");
            var prjName = CommonData.BoardName;
            List<byte> key;


            FCT_GenerateKey.FCT_GenerateKey.ResultType res = FCT_GenerateKey.FCT_GenerateKey.GenerateKey(prjName, 0x03, seeds, out key);

            //     LogHelper.Debug($"密钥{BitConverter.ToString(key.ToArray())}");
            if (res != FCT_GenerateKey.FCT_GenerateKey.ResultType.SUCCESS)
            {
                LogHelper.Debug("种子转换失败");
                return false;
            }

            var bytes = new List<byte>();


            var bytes1 = new List<byte>() { 0x04 };
            bytes.AddRange(key);
            bytes1.AddRange(key);
            byte len = 0x08;


            var temp = Send(bytes1.ToArray(), sid: 0x27, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), dataLen: len)?.Item2;

            if (temp == null || temp[0] != 0x04)
            {
                LogHelper.ShowMsg("解锁失败");
                return false;

            }
            return true;

        }


        public static bool JieSuo1068(List<byte> seeds)
        {
            LogHelper.Debug($"开始解锁，seeks:{string.Join(" ", seeds.Select(m => m.ToString("X2")))}");
            var prjName = CommonData.BoardName;
            List<byte> key;


            FCT_GenerateKey.FCT_GenerateKey.ResultType res = FCT_GenerateKey.FCT_GenerateKey.GenerateKey(prjName, 0x01, seeds, out key);

            //     LogHelper.Debug($"密钥{BitConverter.ToString(key.ToArray())}");
            if (res != FCT_GenerateKey.FCT_GenerateKey.ResultType.SUCCESS)
            {
                LogHelper.Debug("种子转换失败");
                return false;
            }

            var bytes = new List<byte>();


            var bytes1 = new List<byte>() { 0x02 };
            bytes.AddRange(key);
            bytes1.AddRange(key);
            byte len = 0x08;


            var temp = Send(bytes1.ToArray(), sid: 0x27, uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), dataLen: len)?.Item2;

            if (temp == null || temp[0] != 0x04)
            {
                LogHelper.ShowMsg("解锁失败");
                return false;

            }
            return true;

        }



        /// <summary>
        /// 休眠
        /// </summary>
        /// <param name="level">休眠等级</param>
        /// <returns></returns>
        public static List<byte> UdsSleep(int level)
        {
            LogHelper.Debug("UDS休眠");
            List<byte> data = new List<byte>();
            if (level == 1)
                data = HexChange(ConfigHelper.UdsParameters.XM1_DID);
            else if (level == 2)
            {
                data = HexChange(ConfigHelper.UdsParameters.XM2_DID);
            }
            return Send(data.ToArray(), byte.Parse(ConfigHelper.UdsParameters.XM1_SID, System.Globalization.NumberStyles.HexNumber), uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber))?.Item2;
        }


        /// <summary>
        /// 启动限流
        /// </summary>
        /// <param name="isStart">true 开启 false 断开</param>
        /// <returns></returns>
        public static bool StartCurrentLimiting(bool isStart)
        {
            var bytes = HexChange(ConfigHelper.UdsParameters.XLOpen_DID);
            if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                isStart = !isStart;
            }
                byte b = (byte)(isStart ? 0x01 : 0x00);
            bytes.Add(b);
            LogHelper.Debug($"{(isStart ? "开启" : "关闭")}限流模块");
            var temp = Send(bytes.ToArray(), byte.Parse(ConfigHelper.UdsParameters.XLOpen_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                if (temp.Item1 == 0x6f || temp.Item1 == 0x6E)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 结束限流模块
        /// </summary>
        /// <returns></returns>
        public static bool StopCurrentLimiting()
        {
            LogHelper.Debug("结束限流模块");
            var bytes = HexChange(ConfigHelper.UdsParameters.XLEnd_DID);
            var temp = Send(bytes.ToArray(), 0x2F);
            if (temp != null)
            {
                if (temp.Item1 == 0x6f)
                {
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 开启通道均衡
        /// </summary>
        /// <param name="isc1AndC3">true 开启1 3通道，false的话开启2 4 通道</param>
        /// <returns></returns>
        public static bool StartChannelBalance(bool? isc1AndC3 = null,int index = -1)
        {
            LogHelper.Debug("开启均衡");
            List<byte> arr = HexChange(ConfigHelper.UdsParameters.JHStart_DID);
            byte b;
            if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                if (isc1AndC3.Value)
                {
                    arr.Add(0x01);
                    arr.Add(0x00);
                    arr.Add(0x01);
                    arr.Add(0x00);
                }
                else
                {
                    arr.Add(0x00);
                    arr.Add(0x01);
                    arr.Add(0x00);
                    arr.Add(0x01);
                }

                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.JHStart_SID, System.Globalization.NumberStyles.HexNumber), uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), 12);
                if (temp != null)
                {
                    if (temp.Item1 == 0x6f || temp.Item1 == 0x71)
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (isc1AndC3.HasValue)
                {
                    b = (byte)(isc1AndC3.Value ? 0x05 : 0x0A);
                }
                else if (index > -1)
                {
                    b = (byte)(1 << index);
                }
                else
                {
                    b = 0x0f;
                }
                arr.Add(b);
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.JHStart_SID, System.Globalization.NumberStyles.HexNumber), uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber));
                if (temp != null)
                {
                    if (temp.Item1 == 0x6f || temp.Item1 == 0x71)
                    {
                        return true;
                    }
                }
            }
                return false;
            
        }

        public static bool StartChannelBalance1068(bool? isc1AndC3 = null, int index = -1)
        {
            LogHelper.Debug("开启均衡");
            List<byte> arr = new List<byte> { 0x73, 0xBB, 0x03 };
            byte b;
            if (isc1AndC3.HasValue)
            {
                b = (byte)(isc1AndC3.Value ? 0x05 : 0x0A);
            }
            else if (index == 0)
            {
                b = 0x00;
            }
            else
            {
                b = 0x0f;
            }
            arr.Add(b);
            arr.Add(0x0f);
            var temp = Send(arr.ToArray(), 0x2f);
            if (temp != null)
            {
                if (temp.Item1 == 0x6f && temp.Item2[3] == b)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 等待开启通道均衡
        /// </summary>
        /// <param name="isc1AndC3"></param>
        /// <returns></returns>
        public static bool AwaitStartChannelBalance()
        {
            List<byte> arr = new List<byte> { 0x01, 0x10 };
            var temp = Send(arr.ToArray(), 0x22);
            if (temp != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 关闭通道均衡
        /// </summary>
        /// <returns></returns>
        public static bool CloseChannelBalance()
        {
            LogHelper.Debug("关闭均衡通道");
            byte[] arr = new byte[] { 0x61, 0x01, 0x00 };
            var temp = Send(arr, 0x2F);
            if (temp != null && temp.Item1 == 0x6F)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 结束通道均衡
        /// </summary>
        /// <returns></returns>
        public static bool StopChannelBalance()
        {
            LogHelper.Debug("结束均衡控制");
            List<byte> arr = HexChange(ConfigHelper.UdsParameters.JHEnd_DID);
            if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.JHEnd_SID, System.Globalization.NumberStyles.HexNumber), uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), 12);
                if (temp != null && temp.Item1 == 0x6F)
                {
                    return true;
                }
            }
            else
            {
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.JHEnd_SID, System.Globalization.NumberStyles.HexNumber));
                if (temp != null && temp.Item1 == 0x6F)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool GETBiaodin()
        {
            List<byte> bytes1 = new List<byte> { 0x73, 0xb6 };
            var temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                return true;
            }
            return false;
        }
        public static double Getcur()
        {
            LogHelper.Debug("初始化标定");
            List<byte> bytes1 = new List<byte> { 0x73, 0x96 };
            var temp = Send(bytes1.ToArray(), 0x22);
            LogHelper.ShowMsg($"{temp.Item2[2]},{temp.Item2[3]},{temp.Item2[4]},{temp.Item2[5]}");
            if (temp != null)
            {
                int value = (temp.Item2[2] << 24) | (temp.Item2[3] << 16) | (temp.Item2[4] << 8) | temp.Item2[5];
                return value * 0.001 - 2000;

            }
            return 0;
        }
        public static bool Setcurtype(byte level)
        {
            LogHelper.Debug("初始化标定");
            List<byte> bytes1 = new List<byte> { 0x73, 0xb5 };
            bytes1.Add(level);
            var temp = Send(bytes1.ToArray(), 0x2e);
            if (temp != null)
            {
                return true;
            }
            return false;
        }

        public static byte Getcurtype()
        {
            LogHelper.Debug("初始化标定");
            List<byte> bytes1 = new List<byte> { 0x73, 0xb5 };
            var temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return 0;
        }

        public static double Getcur1068()
        {
            LogHelper.Debug("初始化标定");
            List<byte> bytes1 = new List<byte> { 0x73, 0x96 };
            var temp = Send(bytes1.ToArray(), 0x22);
            LogHelper.ShowMsg($"{temp.Item2[2]},{temp.Item2[3]},{temp.Item2[4]},{temp.Item2[5]}");
            if (temp != null)
            {
                int value = (temp.Item2[2] << 24) | (temp.Item2[3] << 16) | (temp.Item2[4] << 8) | temp.Item2[5];
                return value * 0.001 - 2000;

            }
            return 0;
        }

        public static bool StopChannelBalance1068()
        {
            LogHelper.Debug("结束均衡控制");
            List<byte> arr = new List<byte> { 0x73, 0xBB, 0x03, 0x00, 0x0f };
            var temp = Send(arr.ToArray(), 0x2f);
            if (temp != null && temp.Item1 == 0x6F)
            {
                return true;
            }
            return false;
        }
        public static bool ResetEEPROM()
        {
            LogHelper.Debug("重置EEPROM");
            List<byte> bytes1 = new List<byte>() { 0x01, 0x74, 0x14, 0xFF };
            var temp = Send(bytes1.ToArray(), 0x31);
            if (temp != null)
            {
                return true;
            }
            return false;
        }

        public static bool ClearECU()
        {
            LogHelper.Debug("重置ECU");
            List<byte> bytes1 = new List<byte>() { 0x01, 0x74, 0x14, 0x14 };
            var temp = Send(bytes1.ToArray(), 0x31);
            if (temp != null)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// MOS控制
        /// </summary>
        /// <param name="MOSEnum"></param>
        /// <returns></returns>
        public static bool MOSControl(MOSEnum MOSEnum)
        {

            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216")
            {
                LogHelper.Debug($"MOS控制{MOSEnum}");
                List<byte> arr = new List<byte>() { 0xFD, 0x00, 0x03 };
                if ((int)MOSEnum == 3)
                {
                    arr.Add(0x01);
                }
                else if ((int)MOSEnum == 0)
                {
                    arr.Add(0x00);
                }
                else if ((int)MOSEnum == 1)
                {
                    arr.Add(0x00);
                }
                else if ((int)MOSEnum == 2)
                {
                    arr.Add(0x01);
                }


                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSControl_SID, System.Globalization.NumberStyles.HexNumber));

                CommonMethod.Sleep(200);

                arr = new List<byte>() { 0xFD, 0x01, 0x03 };
                if ((int)MOSEnum == 3)
                {
                    arr.Add(0x01);
                }
                else if ((int)MOSEnum == 0)
                {
                    arr.Add(0x00);
                }
                else if ((int)MOSEnum == 1)
                {
                    arr.Add(0x01);
                }
                else if ((int)MOSEnum == 2)
                {
                    arr.Add(0x00);
                }

                temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSControl_SID, System.Globalization.NumberStyles.HexNumber));

                if (temp != null && temp.Item1 == 0x6F)
                {
                    return true;
                }
                return false;

            }
            else if(ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                LogHelper.Debug($"MOS控制{MOSEnum}");
                List<byte> arr = HexChange(ConfigHelper.UdsParameters.MOSControl_DID);
                
                if(MOSEnum == 0)
                {
                    arr.Add(0x60);
                    arr.Add(0x01);
                }
                else
                {
                    
                    arr.Add(0x61);
                    arr.Add(0x01);
                    
                }
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSControl_SID, System.Globalization.NumberStyles.HexNumber));

                if (temp != null && temp.Item1 == 0x6e)
                {
                    return true;
                }
                return false;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
            {
                LogHelper.Debug($"MOS控制{MOSEnum}");
                List<byte> arr = HexChange(ConfigHelper.UdsParameters.MOSControl_DID);

                if (MOSEnum == MOSEnum.断开)
                {
                    arr.Add(0x00);
                    arr.Add(0x00);
                }
                else if (MOSEnum == MOSEnum.充电闭合)
                {

                    arr.Add(0x01);
                    arr.Add(0x00);

                }
                else if (MOSEnum == MOSEnum.放电闭合)
                {

                    arr.Add(0x00);
                    arr.Add(0x01);

                }
                else
                {

                    arr.Add(0x01);
                    arr.Add(0x01);

                }
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSControl_SID, System.Globalization.NumberStyles.HexNumber));

                if (temp != null && temp.Item1 == 0x6F)
                {
                    return true;
                }
                return false;
            }
            else
            {
                LogHelper.Debug($"MOS控制{MOSEnum}");
                List<byte> arr = HexChange(ConfigHelper.UdsParameters.MOSControl_DID);
                byte b = (byte)MOSEnum;
                arr.Add(b);
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSControl_SID, System.Globalization.NumberStyles.HexNumber));

                if (temp != null && (temp.Item1 == 0x6F || temp.Item1 == 0x71))
                {
                    return true;
                }
                return false;
            }

        }

        public static bool MOSControl2510(MOSEnum2510 MOSEnum)
        {
            
                LogHelper.Debug($"MOS控制{MOSEnum}");
                List<byte> arr = new List<byte>() { 0x73, 0xBA, 0x03 };

                if ((int)MOSEnum == 0)
                {

                    List<byte> arr1 = new List<byte>() { 0x73, 0xBA, 0x00 };


                    var temp = Send(arr1.ToArray(), 0x2f);

                    if (temp != null && temp.Item1 == 0x6F)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    if ((int)MOSEnum == 1)
                    {
                        arr.Add(0x01);
                    }
                    else if ((int)MOSEnum == 2)
                    {
                        arr.Add(0x02);
                    }
                    else if ((int)MOSEnum == 3)
                    {
                        arr.Add(0x03);
                    }
                    else if ((int)MOSEnum == 4)
                    {
                        arr.Add(0x04);
                    }
                    else if ((int)MOSEnum == 5)
                    {
                        arr.Add(0x05);
                    }
                    else if ((int)MOSEnum == 6)
                    {
                        arr.Add(0x06);
                    }
                    else if ((int)MOSEnum == 7)
                    {
                        arr.Add(0x07);
                    }

                    arr.Add(0x07);
                    var temp = Send(arr.ToArray(), 0x2f);

                    if (temp != null && temp.Item1 == 0x6F)
                    {
                        return true;
                    }
                    return false;
                }


           

        }
        

        public static bool StopMOSControl()
        {
            LogHelper.Debug("停止MOS控制");
            byte[] arr = new byte[] { 0x61, 0x00, 0x00 };
            var temp = Send(arr, 0x2F);
            if (temp != null && temp.Item1 == 0x6F)
            {
                return true;
            }
            return false;
        }

        public static bool SetSOC1068(int soc)
        {
            LogHelper.Debug($"SOC标定{soc}");
            List<byte> arr = HexChange(ConfigHelper.UdsParameters.SOCT_DID);
            byte b = (byte)(soc);
            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390")
            {
                b = (byte)(soc);
            }

            arr.Add(b);
            var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.SOCT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null && temp.Item1 == 0x71)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// SOC 标定
        /// </summary>
        /// <param name="soc"></param>
        /// <returns></returns>
        public static bool SetSOC(int soc)
        {
            LogHelper.Debug($"SOC标定{soc}");
            List<byte> arr = HexChange(ConfigHelper.UdsParameters.SOCT_DID);
            byte b = (byte)(soc * 2);
            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                b = (byte)(soc);
            }

            arr.Add(b);
            var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.SOCT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null && (temp.Item1 == 0x71 || temp.Item1 == 0x6F || temp.Item1 == 0x6E))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// 复位
        /// </summary>
        /// <returns></returns>
        public static bool Reset()
        {
            LogHelper.Debug("复位");
            byte[] arr = new byte[] { 0x01 };
            var temp = Send(arr, 0x11);
            if (temp != null)
            {
                return true;
            }
            return false;
        }


        public static List<byte> GETEEPROM()
        {
            LogHelper.Debug("复位");
            byte[] arr = new byte[] { 0x02,0xff };
            var temp = Send(arr, 0x19, isudslen: false);
            if (temp != null)
            {
                return temp?.Item2;
            }
            return temp?.Item2;
        }

        public static List<byte> SETEEPROM(byte[] arr)
        {
            LogHelper.Debug("复位"); 
           
            var temp = Send(arr, 0x19, isudslen: false);
            if (temp != null)
            {
                return temp?.Item2;
            }
            return temp?.Item2;
        }
        /// <summary>
        /// 读取EEPROM快照
        /// </summary>
        /// <returns></returns>
        public static List<byte> EEPROM()
        {
            LogHelper.Debug("读取EEPROM快照");
            List<byte> arr = HexChange(ConfigHelper.UdsParameters.EEPROM_DID);
            var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.EEPROM_SID, System.Globalization.NumberStyles.HexNumber), isudslen: false);
            return temp?.Item2;
        }

        public static List<byte> EEPROM2()
        {
            LogHelper.Debug("读取EEPROM快照2");
            List<byte> arr = HexChange("11");
            var temp = Send(arr.ToArray(), byte.Parse("27", System.Globalization.NumberStyles.HexNumber), isudslen: false);
            return temp?.Item2;
        }

        public static byte EEPROMHQ()
        {
            LogHelper.Debug("读取EEPROM快照");
            List<byte> arr = HexChange(ConfigHelper.UdsParameters.EEPROM_DID);
            byte RESULT = 0;
            var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.EEPROM_SID, System.Globalization.NumberStyles.HexNumber));

            if(ConfigHelper.UdsParameters.jiesuo == "AVATR_E15")
            {
                return temp.Item2[52];
            }
            

            CommonMethod.Sleep(20);
            var respensons = SendUDS2("30 00 00 00 00 00 00 00");

            foreach (var respenson in respensons)
            {

                if (respenson[0] == 0x27)
                {
                    RESULT = respenson[7];

                }

            }
            return RESULT;
        }

        public static byte EEPROMWL(byte[] arr)
        {
            LogHelper.Debug("读取EEPROM快照");
           
            byte RESULT = 0;
            

            if(ConfigHelper.UdsParameters.jiesuo == "AVATR_E15")
            {
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.EEPROM_SID, System.Globalization.NumberStyles.HexNumber), isudslen: false);

                CommonMethod.Sleep(20);
                return temp.Item2[52];
            }
            else
            {
                var temp = Send(arr.ToArray(), byte.Parse(ConfigHelper.UdsParameters.EEPROM_SID, System.Globalization.NumberStyles.HexNumber), isudslen: false);

                CommonMethod.Sleep(20);
                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x25)
                    {
                        RESULT = respenson[1];

                    }

                }
                return RESULT;
            }
                
            
        }
        /// <summary>
        /// UDS获取均衡结果
        /// </summary>
        /// <returns></returns>
        public static byte[] GetBalancedAndAFE()
        {
            LogHelper.Debug("UDS获取均衡结果");
            List<byte> bytes1 = new List<byte>() { 0x02, 0xff };
            LogHelper.Debug("03 19 02 FF");
            var temp = SendUDS("03 19 02 FF");
            LogHelper.Debug(string.Join(" ", temp.Select(m => m.ToString("X2"))));
            return temp;
            //var temp = Send(bytes1.ToArray(), 0x19);
            //if (temp != null)
            //{
            //    if (temp.Item2.Count == 2)
            //    {
            //        return true;
            //    }
            //    else { return false; }
            //    return true;
            //}
            //return false;
        }
        /// <summary>
        /// 清故障
        /// </summary>
        /// <returns></returns>
        public static bool ClearErrorE0Y()
        {
            LogHelper.Debug("清故障");
            List<byte> bytes1 = new List<byte>() { 0xff, 0xff, 0xff };
            var temp = Send(bytes1.ToArray(), 0x14);
            if (temp != null)
            {
                return true;
            }
            return false;
        }

        public static bool ClearError()
        {
            LogHelper.Debug("清故障");
            List<byte> bytes1 = new List<byte>() { 0xff, 0xff, 0xff };
            var temp = Send(bytes1.ToArray(), 0x14);
            if (temp != null)
            {
                return true;
            }
            return false;
        }


        public static bool CloseEOL()
        {
            LogHelper.Debug("关闭EOL");
            List<byte> bytes1 = new List<byte>() { 0x37, 0xC1, 0x03, 0x98};
            var temp = Send(bytes1.ToArray(), 0x2F);

            
            CommonMethod.Sleep(50);
            bytes1 = new List<byte>() { 0x37, 0xC1, 0x03, 0xA8 };
            temp = Send(bytes1.ToArray(), 0x2F);
               
            return true;
        }

        public static bool OpenEOL()
        {
            LogHelper.Debug("关闭EOL");
            List<byte> bytes1 = new List<byte>() { 0x37, 0xC1, 0x03, 0x98 };
            var temp = Send(bytes1.ToArray(), 0x2F);
           
                CommonMethod.Sleep(50);
                bytes1 = new List<byte>() { 0x37, 0xC1, 0x03, 0x58 };
                temp = Send(bytes1.ToArray(), 0x2F);
              
            return true;
        }
        /// <summary>
        /// 清除累积充放电AH
        /// </summary>
        /// <returns></returns>
        public static bool ClearAH()
        {
            LogHelper.Debug("清除累积充放电AH");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.ClearAH_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.ClearAH_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// MCU充电电压
        /// </summary>
        /// <returns></returns>
        public static double GetChargeVoltage()
        {
            LogHelper.Debug("MCU充电电压");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MCUCV_DID);
            var temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }
        /// <summary>
        ///  MCU放电电压
        /// </summary>
        /// <returns></returns>
        public static double GetDischargeVoltage()
        {

            LogHelper.Debug("MCU放电电压");
            List<byte> bytes1 = new List<byte>() { 0x01, 0x0A };
            var temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;

        }
        /// <summary>
        /// MOS充电电压
        /// </summary>
        /// <returns></returns>
        public static double GetMOSChargeVoltage()
        {

            if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR" ||
                ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || 
                ConfigHelper.UdsParameters.jiesuo == "T09_M" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046")

            {
                return ZCData.ChargeMOSVolt;
            }
            else
            {
                LogHelper.Debug("MOS充电电压");
                List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MOSCV_DID);
                var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSCV_SID, System.Globalization.NumberStyles.HexNumber));
                if (temp != null)
                {
                    return temp.Item2[2];
                }

                return double.NaN;
            }

        }
        public static List<byte> GetcurCA()
        {
            {
                LogHelper.Debug("唤醒电流阈值");
                List<byte> bytes1 = new List<byte> {};
                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {
                    bytes1 = HexChange("30 29");
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
                {
                    bytes1 = HexChange("4b 43");
                }
                else
                {
                    bytes1 = HexChange("FD 11");
                }
                     
                var temp = Send(bytes1.ToArray(), 0x22);
                if (temp != null)
                {
                    return temp.Item2;
                }

                return null;
            }

        }

        

        public static List<byte> GetcurCA2()
        {
            {
                LogHelper.Debug("唤醒电流阈值2");
                List<byte> bytes1 = new List<byte> { };
                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {
                    bytes1 = HexChange("30 30");
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
                {
                    bytes1 = HexChange("4b 44");
                }
                else
                {
                    bytes1 = HexChange("FD 12");
                }
                var temp = Send(bytes1.ToArray(), 0x22);
                if (temp != null)
                {
                    return temp.Item2;
                }

                return null;
            }

        }

       

        public static bool CurrCalibrate(List<byte> cur)
        {

            LogHelper.Debug("电流阈值标定");
            string data = "";
            if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
            {
                data = $"05 2E 30 30 {(cur[2]).ToString("X2")} {(cur[3]).ToString("X2")} AA AA";
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "WL_C1046")
            {
                data = $"05 2E 4B 44 {(cur[2]).ToString("X2")} {(cur[3]).ToString("X2")} AA AA";
            }
            else
            {
                data = $"05 2E FD 12 {(cur[2]).ToString("X2")} {(cur[3]).ToString("X2")} AA AA";
            }
             
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
           
            DateTime now = DateTime.Now;
            _data = null;
            while (true)
            {
                if (_data != null && _data.Length > 3)
                {
                    if (_data[1] == 0x6E)
                    {
                        return true;
                    }
                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    return false;
                }
            }
        }

        public static double GetPCBTemperature1068()
        {
            LogHelper.Debug("PCB温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.PCBT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.PCBT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }

        public static double GetMOSCTemperature1068()
        {
            LogHelper.Debug("充电MOS温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MOSCT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSCT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }

        public static double GetMOSFTemperature1068()
        {
            LogHelper.Debug("放电MOS温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MOSFT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSFT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }

        public static double GetXLTemperature1068()
        {
            LogHelper.Debug("限流模块温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.XLT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.XLT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }

        public static double GetPCBTemperature()
        {
            LogHelper.Debug("PCB温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.PCBT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.PCBT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[3];
            }
            return double.NaN;
        }

        public static double GetMOSCTemperature()
        {
            LogHelper.Debug("充电MOS温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MOSCT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSCT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[3];
            }
            return double.NaN;
        }

        public static double GetMOSFTemperature()
        {
            LogHelper.Debug("放电MOS温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MOSFT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSFT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[3];
            }
            return double.NaN;
        }

        public static double GetXLTemperature()
        {
            LogHelper.Debug("限流模块温度");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.XLT_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.XLT_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[3];
            }
            return double.NaN;
        }
        public static double GetXLTemperaturec1046()
        {
            LogHelper.Debug("限流模块温度");
            List<byte> bytes1 = new List<byte> { 0x4b, 0x47 };
            var temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }

        public static double GetDXTemperature11068()
        {
            LogHelper.Debug("电芯温度1");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.DXT1_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.DXT1_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }
        public static double GetDXTemperature21068()
        {
            LogHelper.Debug("电芯温度2");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.DXT2_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.DXT2_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return double.NaN;
        }

        public static double GetDXTemperature1()
        {
            LogHelper.Debug("电芯温度1");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.DXT1_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.DXT1_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2] << 8 | temp.Item2[3];
            }
            return double.NaN;
        }
        public static double GetDXTemperature2()
        {
            LogHelper.Debug("电芯温度2");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.DXT2_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.DXT2_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2] << 8 | temp.Item2[3];
            }
            return double.NaN;
        }
        public static double GetDXTemperature3()
        {
            LogHelper.Debug("电芯温度3");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.DXT3_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.DXT3_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2] << 8 | temp.Item2[3];
            }
            return double.NaN;
        }
        /// <summary>
        /// MOS放电电压
        /// </summary>
        /// <returns></returns>
        public static double GetMOSDischargeVoltage()
        {

            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_B216" || ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390" || ConfigHelper.UdsParameters.jiesuo.Substring(0, 5) == "AVATR" || 
                ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B" || ConfigHelper.UdsParameters.jiesuo == "WL_C1046" || ConfigHelper.UdsParameters.jiesuo == "WL_F710S" || ConfigHelper.UdsParameters.jiesuo == "T09_M")

            {
                return ZCData.DischargeMOSVolt;
            }
            else
            {
                LogHelper.Debug("MOS放电电压");
                List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.MOSFV_DID);
                var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.MOSFV_SID, System.Globalization.NumberStyles.HexNumber));
                if (temp != null)
                {
                    return temp.Item2[2];
                }
                return double.NaN;
            }
        }
        /// <summary>
        /// 获取唤醒源
        /// </summary>
        /// <returns></returns>
        public static byte GetAwakenSource()
        {
            LogHelper.Debug("获取唤醒源");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.GetHX_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.GetHX_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return temp.Item2[2];
            }
            return 0xFF;
        }
        /// <summary>
        /// LBMS写入
        /// </summary>
        /// <returns></returns>
        public static bool LBMWrite(string barCode)
        {
            LogHelper.Debug("LBMS写入");
            int barYear = int.Parse(barCode.Substring(0, 2));
            int barMonth = int.Parse(barCode.Substring(2, 2));
            int barDay = int.Parse(barCode.Substring(4, 2));
            string barValue = BarCodeYear.Substring(barYear - 1, 1) + BarCodeMonth.Substring(barMonth - 1, 1) + BarCodeDay.Substring(barDay - 1, 1) + barCode.Substring(8, 4);
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < barValue.Length; i++)
            {
                if (i == 3)
                {
                    bytes.Add(0x30);
                }
                bytes.Add((byte)barValue[i]);
            }

            string data = "00 0B 2E F1 8C";
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }

            _data = null;
            _responseReceivedEvent.Reset();


            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x40);
            // 等待接收到完整响应
            if (_responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000))) // 超时时间：5秒
            {
                return true;
            }
            else
            {
                LogHelper.ShowMsg("超时");
                return false;
            }

        }
        /// <summary>
        /// 硬件版本标定
        /// </summary>
        /// <param name="vision"></param>
        /// <returns></returns>
        public static bool VersionCalibate(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x93);
            for (int i = 0; i < 17; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 40, true);
            LogHelper.ShowMsg($"{_uds}");
            return true;
            //DateTime now = DateTime.Now;
            //while (true)
            //{
            //    if (_data != null && _data.Length > 3)
            //    {
            //        if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x93)
            //        {
            //            return true;
            //        }
            //    }

            //    if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
            //    {
            //        return false;
            //    }
            //}
        }

        public static bool VersionCalibateRC(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0x13);
            bytes.Add(0x1D);
            for (int i = 0; i < 9; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 16, true);
            LogHelper.ShowMsg($"{_uds}");
            return true;

        }

        public static bool BarCodeCalibrateRC1B(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
         
           
           
            bytes.Add(0x00);
            bytes.Add(0x00);
            bytes.Add(0x00);
            bytes.Add(Convert.ToByte(vision.Substring(6, 2)) );
            bytes.Add(Convert.ToByte(vision.Substring(8, 2)));
            string data = ConfigHelper.UdsParameters.PCH_Date + " 13" + " 1C" + " " + vision.Substring(0, 2) + " " + vision.Substring(2, 2)+" " + vision.Substring(4, 2);
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 16, true);
            LogHelper.ShowMsg($"{_uds}");
            return true;

        }

        public static bool VersionCalibateJH(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0x29);
            bytes.Add(0x4D);
            for (int i = 0; i < 17; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 40, true);
            LogHelper.ShowMsg($"{_uds}");
            return true;

        }


        public static bool VersionCalibateRJJH1068(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();

            bytes.Add(0x73);
            bytes.Add(0xB4);
            for (int i = 0; i < 6; i++)
            {

                if (i < vision.Length)
                {
                    if (vision[i] == '.')
                    {
                        continue;
                    }
                    bytes.Add(((byte)vision[i]));
                }
                else
                {

                }
            }
            string data = "06 2E";
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            LogHelper.ShowMsg($"{_data}");
            return true;

        }
        public static bool VersionCalibateRJJH(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF0);
            bytes.Add(0xFA);
            for (int i = 0; i < 9; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = ConfigHelper.UdsParameters.RJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 40, true);
            LogHelper.ShowMsg($"{_uds}");
            return true;

        }
        public static bool VersionCalibateLT(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 9; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = $"10 13 2E F1 93 {bytes[0].ToString("X2")} {bytes[1].ToString("X2")} {bytes[2].ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            //_uds.Send(0x74E, data, true, true, 8);
            var temp = SendUDS(data);
            CommonMethod.Sleep(20);
            if (temp != null && temp[0] == 0x30)
            {
                data = $"21 {bytes[3].ToString("X2")} {bytes[4].ToString("X2")} {bytes[5].ToString("X2")} {bytes[6].ToString("X2")} {bytes[7].ToString("X2")} {bytes[8].ToString("X2")} 20";
                LogHelper.ShowMsg($"{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
                data = $"22 20 20 20 20 20 20 CC";
                LogHelper.ShowMsg($"{data}");
                temp = SendUDS(data);
                DateTime now = DateTime.Now;
                return true;
                //while (true)
                //{
                //    if (_data != null && _data.Length > 3)
                //    {
                //        if (_data[1] == 0x6E && _data[2] == 0xF1)
                //        {
                //            return true;
                //        }
                //    }

                //    if ((DateTime.Now - now).TotalMilliseconds > 5 * 1000)
                //    {
                //        return false;
                //    }
                //    Thread.Sleep(100);
                //}
            }
            return false;
        }

        public static bool VersionCalibateVOYAH_H56C(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 9; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = $"10 0B 2E F1 93 {bytes[0].ToString("X2")} {bytes[1].ToString("X2")} {bytes[2].ToString("X2")}";
            //_uds.Send(0x74E, data, true, true, 8);
            var temp = SendUDS(data);
            CommonMethod.Sleep(20);
            if (temp != null && temp[0] == 0x30)
            {
                data = $"21 {bytes[3].ToString("X2")} {bytes[4].ToString("X2")} {bytes[5].ToString("X2")} {bytes[6].ToString("X2")} {bytes[7].ToString("X2")} {bytes[8].ToString("X2")} CC";

                temp = SendUDS(data);
                DateTime now = DateTime.Now;
                while (true)
                {
                    if (_data != null && _data.Length > 3)
                    {
                        if (_data[1] == 0x6E && _data[2] == 0xF1)
                        {
                            return true;
                        }
                    }

                    if ((DateTime.Now - now).TotalMilliseconds > 5 * 1000)
                    {
                        return false;
                    }
                    Thread.Sleep(100);
                }
            }
            return false;
        }

        public static bool VersionCalibateMS(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x93);
            for (int i = 0; i < 8; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x00);
                }
            }
            string data = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 40, true);
           
            return true;
        }

        public static bool VersionCalibateC1046(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x93);
            for (int i = 0; i < 3; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                
            }
            string data = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            return true;
        }

        public static bool VersionCalibateC1046JX(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x11);
            for (int i = 0; i < 2; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }

            }
            string data = "05 2E";
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            return true;
        }

        public static bool VersionCalibateCA(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 9; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }

            }
            string data = $"10 12 2E F0 89 {bytes[0].ToString("X2")} {bytes[1].ToString("X2")} {bytes[2].ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
           
            var temp = SendUDS(data);
            CommonMethod.Sleep(20);
            if (temp != null && temp[0] == 0x30)
            {
                data = $"21 {bytes[3].ToString("X2")} {bytes[4].ToString("X2")} {bytes[5].ToString("X2")} {bytes[6].ToString("X2")} 00 00 00";

                LogHelper.ShowMsg($"{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
                data = $"22 00 00 00 00 00 AA AA";
                LogHelper.ShowMsg($"{data}");
                temp = SendUDS(data);
                DateTime now = DateTime.Now;
                return true;
                
            }
            return false;
        }

        

        public static bool VersionCalibateWL(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 5; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }

            }
            string data = $"10 08 2E F1 93 {bytes[0].ToString("X2")} {bytes[1].ToString("X2")} {bytes[2].ToString("X2")}";
            LogHelper.ShowMsg($"{data}");

            var temp = SendUDS(data);
            CommonMethod.Sleep(20);
            if (temp != null && temp[0] == 0x30)
            {
                data = $"21 {bytes[3].ToString("X2")} {bytes[4].ToString("X2")} 00 00 00 00 00";

                LogHelper.ShowMsg($"{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
           
                DateTime now = DateTime.Now;
                return true;

            }
            return false;
        }

        public static bool VersionCalibateDF(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            for (int i = 0; i < 5; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }

            }
            string data = $"10 0B 2E F1 93 {bytes[0].ToString("X2")} {bytes[1].ToString("X2")} {bytes[2].ToString("X2")}";
            LogHelper.ShowMsg($"{data}");

            var temp = SendUDS(data);
            CommonMethod.Sleep(20);
            if (temp != null && temp[0] == 0x30)
            {
                data = $"21 {bytes[3].ToString("X2")} 00 00 00 00 00 00";

                LogHelper.ShowMsg($"{data}");
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

                DateTime now = DateTime.Now;
                return true;

            }
            return false;
        }

        public static bool VersionCalibateCan(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x93);
            for (int i = 0; i < 24; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data1 = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data1 += " " + item.ToString("X2");
            }
            List<byte> data = HexChange(data1);
            string senddata = $"{((byte)data[0]).ToString("X2")} {((byte)data[1]).ToString("X2")} {((byte)data[2]).ToString("X2")} {((byte)data[3]).ToString("X2")} {((byte)data[4]).ToString("X2")} {((byte)data[5]).ToString("X2")} {((byte)data[6]).ToString("X2")} {((byte)data[7]).ToString("X2")}";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), senddata, true, true, 8, true);
            LogHelper.ShowMsg($"key1:{senddata}");
            CommonMethod.Sleep(20);
            for (int i = 1; i < 4; i++)
            {
                senddata = $"{i + 20} {((byte)data[i * 7 + 1]).ToString("X2")} {((byte)data[i * 7 + 2]).ToString("X2")} {((byte)data[i * 7 + 3]).ToString("X2")} {((byte)data[i * 7 + 4]).ToString("X2")} {((byte)data[i * 7 + 5]).ToString("X2")} {((byte)data[i * 7 + 6]).ToString("X2")} {((byte)data[i * 7 + 7]).ToString("X2")}";
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), senddata, true, true, 8, true);
                LogHelper.ShowMsg($"key{i + 1}:{senddata}");
                CommonMethod.Sleep(20);
            }

            return true;
        }
        public static string GetHardVersion()
        {
            string data = $"03 22 F1 93 CC CC CC CC";
            var temp = SendUDS(data);
            data = $"30 00 00 CC CC CC CC CC";
            var temnp1 = SendUDS(data);
            return Encoding.Default.GetString(new byte[] { temp[5], temp[6], temp[7], temnp1[1], temnp1[2], temnp1[3], temnp1[4], temnp1[5] });
        }

        public static string GetHardVersionMS()
        {
            string data = $"03 22 F1 93 00 00 00 00";
            var temp = SendUDS(data);
            CommonMethod.Sleep(20);
            data = $"30 00 00 CC CC CC CC CC";
            var temnp1 = SendUDS(data);
            return Encoding.Default.GetString(new byte[] { temp[5], temp[6], temp[7], temnp1[1] });
        }

        public static bool BarCodeCalibrate1068(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);
            for (int i = 0; i < barValue.Length; i++)
            {
                bytes.Add((byte)barValue[i]);
            }

            string data = "00 27 2E";
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x24, true);
            _data = null;

            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null && _data.Length > 3)
                {
                    if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
                    {
                        return true;
                    }
                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 序列号标定
        /// </summary>
        /// <param name="barCode">扫码结果</param>
        /// <returns></returns>
        public static bool BarCodeCalibrate(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);
            for (int i = 0; i < barValue.Length; i++)
            {
                if (i == 3)
                {
                    bytes.Add(0x30);
                }
                bytes.Add((byte)barValue[i]);
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x40, true);
            _data = null;

            DateTime now = DateTime.Now;
            while (true)
            {
                if (_data != null && _data.Length > 3)
                {
                    if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
                    {
                        return true;
                    }
                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    return false;
                }
            }
        }

        public static bool BarCodeCalibrateLT(string barValue)
        {

            LogHelper.Debug("批次号标定");

            string data = $"10 13 {ConfigHelper.UdsParameters.PCH_Date} {((byte)barValue[0]).ToString("X2")} {((byte)barValue[1]).ToString("X2")} {((byte)barValue[2]).ToString("X2")}";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            LogHelper.ShowMsg($"{data}");
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {((byte)barValue[3]).ToString("X2")} {((byte)barValue[4]).ToString("X2")} {((byte)barValue[5]).ToString("X2")} 30 {((byte)barValue[8]).ToString("X2")} {((byte)barValue[9]).ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"22 {((byte)barValue[10]).ToString("X2")} {((byte)barValue[11]).ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            DateTime now = DateTime.Now;
            _data = null;
            return true;
            //while (true)
            //{
            //    if (_data != null && _data.Length > 3)
            //    {
            //        if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
            //        {
            //            return true;
            //        }
            //    }

            //    if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
            //    {
            //        return false;
            //    }
            //}
        }
        public static bool BarCodeCalibrateVOYAH_H56C(string barValue)
        {

            LogHelper.Debug("批次号标定");

            string data = $"10 0F 2E F1 8C {((byte)barValue[0]).ToString("X2")} {((byte)barValue[1]).ToString("X2")} {((byte)barValue[2]).ToString("X2")}";
            _uds.Send(0x74E, data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {((byte)barValue[3]).ToString("X2")} {((byte)barValue[4]).ToString("X2")} {((byte)barValue[5]).ToString("X2")} 30 {((byte)barValue[8]).ToString("X2")} {((byte)barValue[9]).ToString("X2")}";
            _uds.Send(0x74E, data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"22 {((byte)barValue[10]).ToString("X2")} {((byte)barValue[11]).ToString("X2")}";
            _uds.Send(0x74E, data, true, true, 8, true);
            DateTime now = DateTime.Now;
            _data = null;
            while (true)
            {
                if (_data != null && _data.Length > 3)
                {
                    if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
                    {
                        return true;
                    }
                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    return false;
                }
            }
        }

        public static bool BarCodeCalibrateVOYAH_H97(string barValue)
        {

            LogHelper.Debug("批次号标定");

            string data = $"10 0F 2E F1 8C {((byte)barValue[0]).ToString("X2")} {((byte)barValue[1]).ToString("X2")} {((byte)barValue[2]).ToString("X2")}";
            _uds.Send(0x74E, data, true, true, 8, true);
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {((byte)barValue[3]).ToString("X2")} {((byte)barValue[4]).ToString("X2")} {((byte)barValue[5]).ToString("X2")} {((byte)barValue[6]).ToString("X2")} {((byte)barValue[7]).ToString("X2")} {((byte)barValue[8]).ToString("X2")} {((byte)barValue[9]).ToString("X2")}";
            _uds.Send(0x74E, data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"22 {((byte)barValue[10]).ToString("X2")} {((byte)barValue[11]).ToString("X2")}";
            _uds.Send(0x74E, data, true, true, 8, true);
            DateTime now = DateTime.Now;
            _data = null;
            while (true)
            {
                if (_data != null && _data.Length > 3)
                {
                    if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
                    {
                        return true;
                    }
                }

                if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
                {
                    return false;
                }
            }
        }
        public static bool BarCodeCalibrateCN(string barValue)
        {

            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);


            for (int i = 0; i < 12; i++)
            {
                if (i < barValue.Length)
                {
                    bytes.Add(((byte)barValue[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, false, 20, true);
            LogHelper.ShowMsg(data);

            DateTime now = DateTime.Now;
            return true;

        }

        public static bool VersionCalibateCN(string vision)
        {
            LogHelper.Debug("硬件版本标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0x14);
            bytes.Add(0xB6);
            for (int i = 0; i < 11; i++)
            {
                if (i < vision.Length)
                {
                    bytes.Add(((byte)vision[i]));
                }
                else
                {
                    bytes.Add(0x20);
                }
            }
            string data = ConfigHelper.UdsParameters.YJ_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 16, true);
            LogHelper.ShowMsg($"{_uds}");
            return true;

        }




        public static bool BarCodeCalibrateMS(string barValue)
        {

            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);

            for (int i = 0; i < 10; i++)
            {
                bytes.Add(0x00);
            }
            for (int i = 0; i < barValue.Length; i++)
            {
                bytes.Add((byte)barValue[i]);
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, false, 0x20, true);
            LogHelper.ShowMsg(data);

            DateTime now = DateTime.Now;
            return true;

        }
        public static bool BarCodeCalibrateXP(string barValue)
        {

            LogHelper.Debug("批次号标定");

            string data = $"{ConfigHelper.UdsParameters.PCH_Date} F1 8C {((byte)barValue[0]).ToString("X2")} {((byte)barValue[1]).ToString("X2")} {((byte)barValue[2]).ToString("X2")}";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            LogHelper.ShowMsg($"{data}");
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {((byte)barValue[3]).ToString("X2")} {((byte)barValue[4]).ToString("X2")} {((byte)barValue[5]).ToString("X2")} {((byte)barValue[6]).ToString("X2")} {((byte)barValue[7]).ToString("X2")} {((byte)barValue[8]).ToString("X2")} {((byte)barValue[9]).ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"22 {((byte)barValue[10]).ToString("X2")} {((byte)barValue[11]).ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            DateTime now = DateTime.Now;
            _data = null;
            return true;
            //while (true)
            //{
            //    if (_data != null && _data.Length > 3)
            //    {
            //        if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
            //        {
            //            return true;
            //        }
            //    }

            //    if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
            //    {
            //        return false;
            //    }
            //}
        }

        public static bool BarCodeCalibrateRC(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);
            for (int i = 0; i < 20; i++)
            {
                if(i < barValue.Length)
                {
                    bytes.Add((byte)barValue[i]);
                }
                else
                {
                    bytes.Add(0x20);
                }
              
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 32, true);


            DateTime now = DateTime.Now;
            return true;

        }

        public static bool BarCodeCalibrateJH(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0x29);
            bytes.Add(0x4C);
            for (int i = 0; i < barValue.Length; i++)
            {
                if (i == 3)
                {
                    bytes.Add(0x30);
                }
                bytes.Add((byte)barValue[i]);
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x40, true);


            DateTime now = DateTime.Now;
            return true;
           
        }

        public static bool BarCodeCalibrateGQ(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);
            for (int i = 0; i < barValue.Length; i++)
            {
                
                bytes.Add((byte)barValue[i]);
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x1a, true);


            DateTime now = DateTime.Now;
            return true;

        }

        public static bool BarCodeCalibrateC1046(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);

            byte[] bcdbuf = new byte[barValue.Length/2];
            bytes.Add(0x20);
            for (int i = 0; i < bcdbuf.Length-2; i++)
            {
                byte high = (byte)(barValue[i*2] - '0');
                byte low = (byte)(barValue[i * 2+1] - '0');
                bytes.Add((byte)((high << 4) | low));
            }
            bytes.Add(0x00);
            bytes.Add(0x00);
            for (int i = bcdbuf.Length - 2; i < bcdbuf.Length; i++)
            {
                byte high = (byte)(barValue[i * 2] - '0');
                byte low = (byte)(barValue[i * 2 + 1] - '0');
                bytes.Add((byte)((high << 4) | low));
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 16, true);


            DateTime now = DateTime.Now;
            return true;

        }

        public static bool BarCodeCalibrateCA(string barValue)
        {

            LogHelper.Debug("批次号标定");

            string data = $"10 10 {ConfigHelper.UdsParameters.PCH_Date} {int.Parse(barValue[8].ToString()).ToString("X2")} {int.Parse(barValue[9].ToString()).ToString("X2")} {int.Parse(barValue[10].ToString()).ToString("X2")}";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            LogHelper.ShowMsg($"{data}");
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {int.Parse(barValue[11].ToString()).ToString("X2")} 20 20 20 20 20 {((byte)barValue[7]).ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"22 {int.Parse(barValue.Substring(0, 2)):X2} {int.Parse(barValue.Substring(2, 2)):X2} {int.Parse(barValue.Substring(4, 2)):X2}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            DateTime now = DateTime.Now;
            _data = null;
            return true;
         
        }

        public static bool BarCodeCalibrateDF(string barValue)
        {

            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>();
            bytes.Add(0xF1);
            bytes.Add(0x8C);

            
            for (int i = 0; i < barValue.Length; i++)
            {
                bytes.Add((byte)barValue[i]);
            }

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            _data = null;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, false, 0x20, true);
            LogHelper.ShowMsg(data);

            DateTime now = DateTime.Now;
            return true;

        }
        public static bool BarCodeCalibrateWL(string barValue)
        {

            LogHelper.Debug("批次号标定");

            string data = $"10 1B {ConfigHelper.UdsParameters.PCH_Date} {int.Parse(barValue[8].ToString()).ToString("X2")} {int.Parse(barValue[9].ToString()).ToString("X2")} {int.Parse(barValue[10].ToString()).ToString("X2")}";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            LogHelper.ShowMsg($"{data}");
            _data = null;
            CommonMethod.Sleep(20);
            data = $"21 {int.Parse(barValue[11].ToString()).ToString("X2")} 20 20 20 20 20 {((byte)barValue[7]).ToString("X2")}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            CommonMethod.Sleep(20);
            data = $"22 {int.Parse(barValue.Substring(0, 2)):X2} {int.Parse(barValue.Substring(2, 2)):X2} {int.Parse(barValue.Substring(4, 2)):X2}";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);

            data = $"23 ";
            LogHelper.ShowMsg($"{data}");
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 8, true);
            DateTime now = DateTime.Now;
            _data = null;
            return true;

        }

        public static bool SCCalibrateWL(string barValue)
        {

            byte[] arr = new byte[] { 0xF1, 0x8B, 0x20, Convert.ToByte(barValue.Substring(0, 2),16), Convert.ToByte(barValue.Substring(2, 2),16), Convert.ToByte(barValue.Substring(4, 2),16) };
            var temp = Send(arr, 0x2E);
            if (temp != null && temp.Item1 == 0x6E)
            {
                return true;
            }
            return false;

        }



        public static bool BarCodeCalibrateEH3(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>() { 0xF1, 0x8C, 0x31, 0x30, 0x5f, 0x38, 0x30, 0x37, 0x30, 0x30, 0x30, 0x36, 0x34, 0x31, 0x41, 0x41, 0x5f, 0x31, 0x31, 0x5f, 0x39, 0x44, 0x51, 0x5f, 0x31, 0x32, 0x5f };
            ;


            for (int i = 0; i < barValue.Length - 5; i++)
            {
                bytes.Add((byte)barValue[i]);
            }
            bytes.Add(0);
            bytes.Add(0);
            bytes.Add(0);

            string data = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data += " " + item.ToString("X2");
            }
            int number = int.Parse(barValue.Substring(7, 4));
            string HexString = number.ToString("X4");
            string HexString1 = string.Join(" ", Enumerable.Range(0, HexString.Length / 2).Select(i => HexString.Substring(i * 2, 2)));
            data = data + " " + HexString1;

            _uds.SendLBMS(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, true, true, 0x30, true);
            _data = null;

            return true;
        }

        public static bool BarCodeCalibrateE0Y(string barValue)
        {
            LogHelper.Debug("序列号标定");
            List<byte> bytes = new List<byte>() { 0xF1, 0x8C, 0x31, 0x30, 0x5f, 0x38, 0x30, 0x37, 0x30, 0x30, 0x30, 0x36, 0x34, 0x31, 0x41, 0x41, 0x5f, 0x31, 0x31, 0x5f, 0x39, 0x44, 0x51, 0x5f, 0x31, 0x32, 0x5f };
            ;


            for (int i = 0; i < barValue.Length; i++)
            {
                bytes.Add((byte)barValue[i]);
            }

            string data1 = ConfigHelper.UdsParameters.PCH_Date;
            foreach (var item in bytes)
            {
                data1 += " " + item.ToString("X2");
            }
            List<byte> data = HexChange(data1);
            string senddata = $"{((byte)data[0]).ToString("X2")} {((byte)data[1]).ToString("X2")} {((byte)data[2]).ToString("X2")} {((byte)data[3]).ToString("X2")} {((byte)data[4]).ToString("X2")} {((byte)data[5]).ToString("X2")} {((byte)data[6]).ToString("X2")} {((byte)data[7]).ToString("X2")}";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), senddata, true, true, 8, true);
            LogHelper.ShowMsg($"key1:{senddata}");
            CommonMethod.Sleep(20);
            for (int i = 1; i < 5; i++)
            {
                senddata = $"{i + 20} {((byte)data[i * 7 + 1]).ToString("X2")} {((byte)data[i * 7 + 2]).ToString("X2")} {((byte)data[i * 7 + 3]).ToString("X2")} {((byte)data[i * 7 + 4]).ToString("X2")} {((byte)data[i * 7 + 5]).ToString("X2")} {((byte)data[i * 7 + 6]).ToString("X2")} {((byte)data[i * 7 + 7]).ToString("X2")}";
                _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), senddata, true, true, 8, true);
                LogHelper.ShowMsg($"key{i + 1}:{senddata}");
                //   CommonMethod.Sleep(20);
            }
            senddata = $"25 {((byte)data[36]).ToString("X2")} {((byte)data[37]).ToString("X2")} {((byte)data[38]).ToString("X2")} {((byte)data[39]).ToString("X2")} {((byte)data[40]).ToString("X2")} AA AA";
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), senddata, true, true, 8, true);
            LogHelper.ShowMsg($"key6:{senddata}");
            return true;
            //_data = null;
            //DateTime now = DateTime.Now;
            //while (true)
            //{
            //    if (_data != null && _data.Length > 3)
            //    {
            //        if (_data[1] == 0x6E && _data[2] == 0xF1 && _data[3] == 0x8C)
            //        {
            //            return true;
            //        }
            //    }

            //    if ((DateTime.Now - now).TotalMilliseconds > CommonData.AwaitTime * 1000)
            //    {
            //        return false;
            //    }
            //}
        }

        public static (double, double, double, double) GetDYS()
        {
            (double, double, double, double) volt;

            volt.Item1 = 0;
            volt.Item2 = 0;
            volt.Item3 = 0;
            volt.Item4 = 0;

            LogHelper.Debug("获取单体电压");
            List<byte> bytes1 = new List<byte> { 0x73, 0x97 };
            var temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                volt.Item1 = (double)((temp.Item2[2] * 256 + temp.Item2[3]) * 0.001);
            }

            LogHelper.Debug("获取单体电压");
            bytes1 = new List<byte> { 0x73, 0x98 };
            temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                volt.Item2 = (double)((temp.Item2[2] * 256 + temp.Item2[3]) * 0.001);
            }

            LogHelper.Debug("获取单体电压");
            bytes1 = new List<byte> { 0x73, 0x99 };
            temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                volt.Item3 = (double)((temp.Item2[2] * 256 + temp.Item2[3]) * 0.001);
            }

            LogHelper.Debug("获取单体电压");
            bytes1 = new List<byte> { 0x73, 0x9a };
            temp = Send(bytes1.ToArray(), 0x22);
            if (temp != null)
            {
                volt.Item4 = (double)((temp.Item2[2] * 256 + temp.Item2[3]) * 0.001);
            }


            return volt;
        }

        public static string GetVersion1068(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1;
            if (level == 2)
            {
                bytes1 = new List<byte>() { 0xf0 };
            }
            else
            {
                bytes1 = new List<byte>() { 0x73 };
            }


            switch (level)
            {
                case 0:
                    bytes1.Add(0xB3);
                    break;
                case 1:
                    bytes1.Add(0xB2);
                    break;
                case 2:
                    bytes1.Add(0x89);
                    break;
            }
            //CommonMethod.Sleep(100);
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";
            if (ConfigHelper.UdsParameters.xieyi == "CAN" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
            {
                var lists = new List<byte>();
                if (level == 1 || level == 0)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }

                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (level == 1 || level == 0)
                    {


                        if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702")
                        {
                            for (int i = 1; i < 6; i++)
                            {
                                version += (char)respenson[i];
                            }


                        }

                        else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                        {
                            if (respenson[0] == 0x21)
                            {
                                for (int i = 1; i < 3; i++)
                                {
                                    version += (char)respenson[i];
                                }

                            }



                        }

                        else
                        {
                            for (int i = 1; i < 8; i++)
                            {
                                version += (char)respenson[i];
                            }

                            if (respenson[0] == 0x25)
                            {

                                for (int i = 1; i < 6; i++)
                                {
                                    version += (char)respenson[i]; ;
                                }
                            }
                        }


                    }
                    else
                    {
                        if (respenson[0] == 0x24)
                        {
                            for (int i = 2; i < 8; i++)
                            {
                                version += (char)respenson[i];
                            }

                        }
                        if (respenson[0] == 0x25)
                        {
                            for (int i = 1; i < 6; i++)
                            {
                                version += (char)respenson[i]; ;
                            }

                        }


                    }

                }

                return version;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03")
            {
                var lists = new List<byte>();
                if (level == 1 || level == 0)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }

                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (level == 1 || level == 0)
                    {
                        if (respenson[0] == 0x21)
                        {
                            for (int i = 1; i < 5; i++)
                            {
                                version += (char)respenson[i];
                            }

                        }


                    }

                }
                return version;
            }
            else
            {
                if (temp != null)
                {


                    for (int i = 5; i < 22; i++)
                    {
                        version += (char)_data[i];
                    }
                    return version;


                }
                return "";
            }
        }



        /// <summary>
        /// 获取版本 0 获取硬件版本 1 获取软件版本 2 获取扫码序号
        /// </summary>
        /// <param name="level">0 获取硬件版本 1 获取软件版本 2 获取扫码序号</param>
        /// <returns></returns>
        public static string GetVersion(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>() { 0xF1 };
            switch (level)
            {
                case 0:
                    bytes1.Add(0x93);
                    break;
                case 1:
                    bytes1.Add(0x95);
                    break;
                case 2:
                    bytes1.Add(0x8C);
                    break;
                case 3:
                    bytes1.Add(0x8B);
                    break;
            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";
            if (ConfigHelper.UdsParameters.xieyi == "CAN" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
            {
                var lists = new List<byte>();
                if (level == 1 || level == 0)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }
                if(level == 2 && ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {
                    for (int i = 3; i < 6; i++)
                    {
                        version += temp.Item2[i].ToString("");

                    }
                }
                if (level == 3)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        version += temp.Item2[i].ToString("X2");

                    }
                    return version;
                }

                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (level == 2 && ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                    {
                        if (respenson[0] == 0x21)
                        {
                            
                             version += respenson[1].ToString("");
                            
                             version += respenson[7].ToString("");

                        }

                        if (respenson[0] == 0x22)
                        {
                            for (int i = 1; i < 4; i++)
                            {
                                version += respenson[i].ToString("X2");
                            }

                        }
                        
                    }

                    if (level == 1 || level == 0)
                    {


                        if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "HQ_C1079")
                        {
                            for (int i = 1; i < 6; i++)
                            {
                                version += (char)respenson[i];
                            }

                            
                        }
                        else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                        {
                            for (int i = 1; i < 3; i++)
                            {
                                version += (char)respenson[i];
                            }


                        }

                        else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                        {
                            if (respenson[0] == 0x21)
                            {
                                for (int i = 1; i < 3; i++)
                                {
                                    version += (char)respenson[i];
                                }

                            }



                        }

                        else
                        {
                            for (int i = 1; i < 8; i++)
                            {
                                version += (char)respenson[i];
                            }

                            if (respenson[0] == 0x25)
                            {

                                for (int i = 1; i < 6; i++)
                                {
                                    version += (char)respenson[i]; ;
                                }
                            }
                        }
                            

                    }
                    else
                    {
                        if (respenson[0] == 0x24)
                        {
                            for (int i = 2; i < 8; i++)
                            {
                                version += (char)respenson[i];
                            }

                        }
                        if (respenson[0] == 0x25)
                        {
                            for (int i = 1; i < 6; i++)
                            {
                                version += (char)respenson[i]; ;
                            }

                        }


                    }

                }

                return version;
            }
            else if (ConfigHelper.UdsParameters.jiesuo == "GreatWall_C03")
            {
                var lists = new List<byte>();
                if (level == 1 || level == 0)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }

                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (level == 1 || level == 0)
                    {
                        if (respenson[0] == 0x21)
                        {
                            for (int i = 1; i < 5; i++)
                            {
                                version += (char)respenson[i];
                            }

                        }


                    }

                }
                return version;
            }
            else
            {
                if (temp != null)
                {
                    if (level == 1 || level == 0)
                    {

                        for (int i = 5; i < 22; i++)
                        {
                            version += (char)_data[i];
                        }
                        return version;
                    }
                    else if (level == 2)
                    {
                        if (ConfigHelper.UdsParameters.jiesuo == "CHERY_EH3" || ConfigHelper.UdsParameters.jiesuo == "QR_S5D")
                        {
                            for (int i = 30; i < 36; i++)
                            {

                                version += (char)_data[i];
                            }

                            return version;

                        }
                        else if (ConfigHelper.UdsParameters.jiesuo == "SERES_F3")
                        {
                            for (int i = 5; i < 13; i++)
                            {
                                if (i == 8)
                                    continue;
                                version += (char)_data[i];
                            }
                            return version;
                        }
                        else
                        {
                            for (int i = 30; i < 41; i++)
                            {

                                version += (char)_data[i];
                            }
                            return version;

                        }

                    }
                }
                return "";
            }
        }

        public static string GetVersionKH(int level)
        {
            LogHelper.Debug("获取版本");

            List<byte> bytes1 = new List<byte>() { 0xF1 };
            if (level == 1)
            {
                bytes1 = new List<byte>() { 0xF0 };
            }

            bytes1.Add(0x89);

            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            if (temp != null)
            {

                if (ConfigHelper.UdsParameters.xieyi == "CAN")
                {
                    var lists = new List<byte>();
                    
                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                    

                    var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                    foreach (var respenson in respensons)
                    {
                        
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                {
                    var lists = new List<byte>();

                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }


                    var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                    foreach (var respenson in respensons)
                    {
                        if(respenson[0] == 0x21)
                        {
                            for (int i = 1; i < 8; i++)
                            {
                                version += (char)respenson[i];
                            }
                        }

                        if (respenson[0] == 0x22)
                        {
                            for (int i = 1; i < 4; i++)
                            {
                                version += (char)respenson[i];
                            }
                        }


                    }
                }
                else
                {
                    for (int i = 5; i < 22; i++)
                    {
                        version += (char)_data[i];
                    }
                }

                        
                return version;
                
                
            }
            return "";
            
        }

        public static string GetVersionMS(int level)
        {
            LogHelper.Debug("获取版本");

            List<byte> bytes1 = new List<byte>() { 0xF1 };
            switch (level)
            {
                case 0:
                    bytes1.Add(0x89);
                    break;
                case 1:
                    bytes1.Add(0x79);
                    break;
                case 2:
                    bytes1.Add(0x71);
                    break;
                case 3:
                    bytes1.Add(0x95);
                    break;
            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();


            for (int i = 2; i < 4; i++)
            {
                version += temp.Item2[i].ToString("X2");

            }






            return version;


        }

        public static string GetVersionV32(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x87);
                    break;
                case 1:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x8c);
                    break;
                case 2:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x80);
                    break;
                case 3:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x89);
                    break;

                case 4:
                    bytes1.Add(0x02);
                    bytes1.Add(0x16);
                    break;
            }

           
           
                
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();

            for (int i = 2; i < temp.Item2.Count; i++)
            {
                version += (char)temp.Item2[i];

            }



            return version;


        }
        public static string GetVersionCN(int level)
        {
            LogHelper.Debug("获取版本");


            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF0);
                    bytes1.Add(0x89);
                    break;
                case 1:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x89);
                    break;
                case 2:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x95);
                    break;
                case 3:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x93);
                    break;
                case 4:
                    bytes1.Add(0x14);
                    bytes1.Add(0xB8);
                    break;
                case 5:
                    bytes1.Add(0x14);
                    bytes1.Add(0xB6);
                    break;
                case 6:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x8C);
                    break;
                case 7:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x80);
                    break;

            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();


            for (int i = 2; i < temp.Item2.Count; i++)
            {
                version += (char)temp.Item2[i];

            }





            return version;


        }

        public static string GetVersionGQ(int level)
        {
            LogHelper.Debug("获取版本");

            
            List<byte> bytes1 = new List<byte>() { 0xF1 };
            switch (level)
            {
                case 0:
                    bytes1.Add(0x95);
                    break;
                case 1:
                    bytes1.Add(0x93);
                    break;
                case 2:
                    bytes1.Add(0x89);
                    break;
                case 3:
                    bytes1.Add(0x7F);
                    break;
                case 4:
                    bytes1.Add(0x87);
                    break;

            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();

            if (level == 0)
            {
                for (int i = 3; i < 10; i++)
                {
                    version += (char)temp.Item2[i];

                }
            }
            else if(level == 1)
            {
                for (int i = 2; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }
            }
            else
            {
                for (int i = 2; i < temp.Item2.Count; i++)
                {
                    version += (char)temp.Item2[i];

                }
            }
                



            return version;


        }

        

        public static string GetVersionH97(int level)
        {
            LogHelper.Debug("获取版本");

            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x89);
                    break;
                case 1:
                    bytes1.Add(0xF0);
                    bytes1.Add(0x89);
                    break;
                case 2:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x80);
                    break;
            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();

            for (int i = 3; i < 6; i++)
            {
                version += (char)temp.Item2[i];

            }
            if(level == 2)
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }
                    



                }
            }
            else 
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }



                }
            }


            return version;


        }

        public static string GetVersionC1046(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x18);
                    break;
                case 1:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x95);
                    break;
                case 2:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x8C);
                    break;
                case 3:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x93);
                    break;

                case 4:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x11);
                    break;
            }




            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();
            if(level == 2)
            {
                for (int i = 2; i < temp.Item2.Count; i++)
                {
                    version += temp.Item2[i].ToString("X2");

                }
            }
            else
            {
                for (int i = 2; i < temp.Item2.Count; i++)
                {
                    version += (char)temp.Item2[i];

                }
            }
            



            return version;


        }

        public static string GetVersionHQ(int level)
        {
            LogHelper.Debug("获取版本");

            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x95);
                    break;
                case 1:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x93);
                    break;
                case 2:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x89);
                    break;
                case 3:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x91);
                    break;
                case 4:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x87);
                    break;
                case 5:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x80);
                    break;
            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();

            for (int i = 3; i < 6; i++)
            {
                version += (char)temp.Item2[i];

            }
            if (level == 0 || level == 1 || level == 2)
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 7; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }





                }
            }

            else if (level == 3 )
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }





                }
            }
            else if (level == 5)
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }
                





                }
            }
            else
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {
                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 5; i++)
                        {
                            version += (char)respenson[i];
                        }
                    }



                }
            }


            return version;


        }
        public static string GetVersionXP(int level)
        {
            LogHelper.Debug("获取版本");
            
            List<byte> bytes1 = new List<byte>() { 0x02 };
            switch (level)
            {
                case 0:
                    bytes1.Add(0x40);
                    break;
                case 1:
                    bytes1.Add(0x41);
                    break;
                
            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";
           
            var lists = new List<byte>();
           
            for (int i = 3; i < 6; i++)
            {
                version += (char)temp.Item2[i];

            }
            if (level == 0)
            {
                var respensons = SendUDS2("30 00 00 AA AA AA AA AA");

                foreach (var respenson in respensons)
                {

                    for (int i = 1; i < 8; i++)
                    {
                        version += (char)respenson[i];
                    }


                }
            }
            

            return version;
            
            
        }

        public static string GetVersionWL(int level)
        {
            LogHelper.Debug("获取版本");

            List<byte> bytes1 = new List<byte>() { 0xF1 };
            switch (level)
            {
                case 0:
                    bytes1.Add(0x89);
                    break;
                case 1:
                    bytes1.Add(0x80);
                    break;
                case 2:
                    bytes1.Add(0x95);
                    break;

            }
            var temp = Send(bytes1.ToArray(), 0x22);
            if(temp == null)
            {
                return "";
            }
            string version = "";

            var lists = new List<byte>();

            if (level == 0)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }




                }
            }

            if (level == 1)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 3; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }




                }
            }

            if (level == 2)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                   

                }
            }



            return version;


        }

        public static string GetVersionPK(int level)
        {
            LogHelper.Debug("获取版本");

            List<byte> bytes1 = new List<byte> { };

            if (level == 3)
            {
                bytes1.Add(0x13);
                bytes1.Add(0x1D);
            }
            else if (level == 4)
            {
                bytes1.Add(0x13);
                bytes1.Add(0x1C);
            }
            else
            {
                bytes1.Add(0xF1);
            }
            switch (level)
            {
                case 0:
                    bytes1.Add(0x94);
                    break;
                case 1:
                    bytes1.Add(0x95);
                    break;
                case 2:
                    bytes1.Add(0x8C);
                    break;
               

            }

            
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";

            var lists = new List<byte>();

            if (level == 0)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x23)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x24)
                    {
                        for (int i = 1; i < 2; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }




                }
            }

            if (level == 1)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                  


                }
            }

            if (level == 2)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x22)
                    {
                        for (int i = 1; i < 8; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }
                    if (respenson[0] == 0x23)
                    {
                        for (int i = 1; i < 4; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }



                }
                
            }
            if (level == 3)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += (char)temp.Item2[i];

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 1; i < 6; i++)
                        {
                            version += (char)respenson[i];
                        }

                    }



                }
            }
            if (level == 4)
            {
                for (int i = 3; i < 6; i++)
                {
                    version += temp.Item2[i].ToString("X2");

                }

                var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                foreach (var respenson in respensons)
                {

                    if (respenson[0] == 0x21)
                    {
                        for (int i = 4; i < 6; i++)
                        {
                            version += respenson[i].ToString("D2");
                        }

                    }



                }
            }


            return version;


        }

        

        public static string 
            GetYJRJ(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x89);
                    break;
                case 1:
                    bytes1.Add(0xF0);
                    bytes1.Add(0x89);
                    break;
                case 2:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x70);
                    break;
                case 3:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x8C);
                    break;
                case 4:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x71);
                    break;
                case 5:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x80);
                    break;
            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";


            if (ConfigHelper.UdsParameters.xieyi == "CANFD" && ConfigHelper.UdsParameters.jiesuo != "M18_3" && ConfigHelper.UdsParameters.jiesuo != "DF_DH1" && ConfigHelper.UdsParameters.jiesuo != "DF_XY-A")
            {
                if (level == 1 || level == 0)
                {
                    for (int i = 2; i < 9; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }
                if (level == 2)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }
                if (level == 3)
                {
                    string version1 = "";
                    string version2 = "";
                    string version3 = "";
                    for (int i = 2; i < 6; i++)
                    {
                        version1 += ((int)temp.Item2[i]).ToString();

                    }

                    version2 += (char)temp.Item2[11];


                    for (int i = 12; i < 15; i++)
                    {
                        version3 += temp.Item2[i].ToString("D2");

                    }
                    
                    version = version3 + "0" + version2 + version1;
                }


                return version;
            }
            else
            {
                var lists = new List<byte>();
                if (level == 1 || level == 0)
                {
                    for (int i = 3; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }

                    var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                    foreach (var respenson in respensons)
                    {
                        if (level == 1 || level == 0)
                        {
                            if (respenson[0] == 0x21)
                            {
                                for (int i = 1; i < 5; i++)
                                {
                                    version += (char)respenson[i];
                                }

                            }


                        }

                    }
                }
                else if (level == 2)
                {
                    for (int i = 2; i < 6; i++)
                    {
                        version += (char)temp.Item2[i];

                    }
                }
                else if (level == 4)
                {
                    for (int i = 2; i < 4; i++)
                    {
                        version += temp.Item2[i].ToString("X2");

                    }
                }
                else if (level == 3)
                {
                    
                    if (ConfigHelper.UdsParameters.jiesuo == "DF_DH1" || ConfigHelper.UdsParameters.jiesuo == "DF_XY-A")
                    {
                        for (int i = 3; i < 6; i++)
                        {
                            version += (char)temp.Item2[i];

                        }
                        var respensons = SendUDS2("30 00 00 00 00 00 00 00");
                        foreach (var respenson in respensons)
                        {

                            if (respenson[0] == 0x21)
                            {

                                for (int i = 1; i < 8; i++)
                                {
                                    version += (char)respenson[i];
                                }


                            }

                            if (respenson[0] == 0x22)
                            {
                                for (int i = 1; i < 6; i++)
                                {
                                    version += (char)respenson[i];
                                }

                            }
                            

                          
                        }
                    }
                    else
                    {
                        string version1 = "";
                        string version2 = "";
                        string version3 = "";
                        for (int i = 3; i < 6; i++)
                        {
                            version1 += ((int)temp.Item2[i]).ToString();

                        }
                        var respensons = SendUDS2("30 00 00 00 00 00 00 00");

                        if (ConfigHelper.UdsParameters.jiesuo == "M18_3")
                        {
                            foreach (var respenson in respensons)
                            {

                                if (respenson[0] == 0x22)
                                {

                                    for (int i = 1; i < 8; i++)
                                    {
                                        version += (char)respenson[i];
                                    }


                                }

                                if (respenson[0] == 0x23)
                                {
                                    for (int i = 1; i < 4; i++)
                                    {
                                        version += (char)respenson[i];
                                    }

                                }


                            }
                        }
                        else
                        {
                            foreach (var respenson in respensons)
                            {

                                if (respenson[0] == 0x21)
                                {

                                    version1 += ((int)respenson[1]).ToString();
                                    version2 += (char)respenson[7];


                                }

                                if (respenson[0] == 0x22)
                                {
                                    for (int i = 1; i < 4; i++)
                                    {
                                        version3 += respenson[i].ToString("D2");
                                    }

                                }


                            }
                            version = version3 + "0" + version2 + version1;
                        }
                    }
                    


                }


                return version;
            }

            



        }

        public static string GetYRGQ(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x84);
                    break;
                case 1:
                    bytes1.Add(0x02);
                    bytes1.Add(0x00);
                    break;
                case 2:
                    bytes1.Add(0x02);
                    bytes1.Add(0x01);
                    break;
                case 3:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x99);
                    break;
                case 4:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x80);
                    break;
                case 5:
                    bytes1.Add(0xF1);
                    bytes1.Add(0x8C);
                    break;


            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";




            var lists = new List<byte>();
            if (level == 0)
            {
                for (int i = 2; i < 12; i++)
                {
                    version += temp.Item2[i].ToString("X2");

                }

                return version;


            }
            else if(level == 1 || level == 2)
            {
                for (int i = 2; i < 4; i++)
                {
                    version += temp.Item2[i].ToString("X2");

                }

                return version;


            }
            else if (level == 3)
            {
                for (int i = 2; i < 6; i++)
                {
                    version += temp.Item2[i].ToString("X2");

                }

                return version;


            }
            else if (level == 4)
            {
                for (int i = 2; i < 19; i++)
                {
                    version += (char)temp.Item2[i];

                }

                return version;


            }
            else
            {
                for (int i = 2; i < 23; i++)
                {
                    version += (char)temp.Item2[i];

                }
                return version;
            }


            return version;



        }

        public static string GetYRJH(int level)
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>();
            switch (level)
            {
                case 0:
                    bytes1.Add(0xF0);
                    bytes1.Add(0xFA);
                    break;
                case 1:
                    bytes1.Add(0x29);
                    bytes1.Add(0x4D);
                    break;
                case 2:
                    bytes1.Add(0x29);
                    bytes1.Add(0x4C);
                    break;
               

            }
            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";




            var lists = new List<byte>();
            if (level == 1 || level == 0)
            {
                for (int i = 5; i < 14; i++)
                {
                    version += (char)_data[i];
                }
                return version;


            }
            else if (level == 2)
            {
                for (int i = 5; i < 13; i++)
                {
                    if (i == 8)
                        continue;
                    version += (char)_data[i];
                }
                return version;
            }
            

            return version;



        }
        /// <summary>
        /// 获取批次号
        /// </summary>
        /// <returns></returns>
        public static string GetBarCode()
        {
            byte[] b1 = null;
            byte[] b2 = null;
            string str = "";
            LogHelper.Debug("获取批次号");
            string data = $"03 22 F1 8C";

            var temp = SendUDS(data);
            for (int i = 5; i < 8; i++)
            {
                str += (char)temp[i];
            }
            CommonMethod.Sleep(20);
            //_uds.Send(0x74E, data, isCanFd: true, len: 8);
            data = $"30";
            ManualResetEvent r = new ManualResetEvent(false);
            List<byte> response = new List<byte>();
            int count = 0;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, isCanFd: true, len: 8, lentype:true);
            _uds.ReviceData += reviceData;
            r.Reset();
            if (r.WaitOne((int)(CommonData.AwaitTime * 1000)))
            {
                _uds.ReviceData -= reviceData;
                if (ConfigHelper.UdsParameters.jiesuo == "XP_12V" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H97" || ConfigHelper.UdsParameters.jiesuo == "VOYAH_H47A")
                {
                    for (int i = 1; i < 8; i++)
                    {
                        str += (char)b1[i];
                    }
                }
                else
                {
                    for (int i = 1; i < 7; i++)
                    {
                        str += (char)b1[i];
                    }
                }    
                    
                for (int i = 1; i < 3; i++)
                {
                    str += (char)b2[i];
                }
            }
            else
            {
                _uds.ReviceData -= reviceData;
                throw new Exception($"{data} 产品UDS请求超时");
            }

            void reviceData(uint id, byte[] arr)
            {
                if (id == uint.Parse(ConfigHelper.UdsParameters.RID, System.Globalization.NumberStyles.HexNumber))
                {
                    if (arr[0] == 0x21)
                    {
                        b1 = arr;

                    }
                    if (arr[0] == 0x22)
                    {
                        b2 = arr;

                    }
                    count++;

                    if (b1 != null & b2 != null)
                    {
                        r.Set();
                    }
                }
            }
            return str;
        }

        public static string GetBarCodeCA()
        {
            byte[] b1 = null;
            byte[] b2 = null;
            string str = "";
            LogHelper.Debug("获取批次号");
            string data = $"03 22 F1 8C 00 00 00 00";

            var temp = SendUDS(data);
            for (int i = 5; i < 8; i++)
            {
                str += (char)temp[i];
            }
            //_uds.Send(0x74E, data, isCanFd: true, len: 8);
            data = $"30";
            ManualResetEvent r = new ManualResetEvent(false);
            List<byte> response = new List<byte>();
            int count = 0;
            _uds.Send(uint.Parse(ConfigHelper.UdsParameters.TID, System.Globalization.NumberStyles.HexNumber), data, isCanFd: true, len: 8);
            _uds.ReviceData += reviceData;
            r.Reset();
            if (r.WaitOne((int)(CommonData.AwaitTime * 1000)))
            {
                _uds.ReviceData -= reviceData;
                for (int i = 1; i < 7; i++)
                {
                    str += (char)b1[i];
                }
                for (int i = 1; i < 3; i++)
                {
                    str += (char)b2[i];
                }
            }
            else
            {
                _uds.ReviceData -= reviceData;
                throw new Exception($"{data} 产品UDS请求超时");
            }

            void reviceData(uint id, byte[] arr)
            {
                if (id == uint.Parse(ConfigHelper.UdsParameters.RID, System.Globalization.NumberStyles.HexNumber))
                {
                    if (arr[0] == 0x21)
                    {
                        b1 = arr;

                    }
                    if (arr[0] == 0x22)
                    {
                        b2 = arr;

                    }
                    count++;

                    if (b1 != null & b2 != null)
                    {
                        r.Set();
                    }
                }
            }
            return str;
        }
        /// <summary>
        /// 获取电流
        /// </summary>
        /// <returns></returns>
        public static double GetCurrent()
        {
            LogHelper.Debug("获取电流");
            List<byte> bytes1 = HexChange(ConfigHelper.UdsParameters.DL_DID);
            var temp = Send(bytes1.ToArray(), byte.Parse(ConfigHelper.UdsParameters.DL_SID, System.Globalization.NumberStyles.HexNumber));
            if (temp != null)
            {
                return (double)(BitConverter.ToUInt32(new byte[] { temp.Item2[5], temp.Item2[4], temp.Item2[3], temp.Item2[2] }, 0) * 0.001) - 1000;
            }
            return 0xFF;
        }

        public static List<byte> HexChange(string hexStr)
        {
            string[] hexParts = hexStr.Split(' ');
            List<byte> byteList = new List<byte>();

            foreach (string part in hexParts)
            {
                if (byte.TryParse(part, System.Globalization.NumberStyles.HexNumber, null, out byte result))
                {
                    byteList.Add(result);
                }
                else
                {
                    Console.WriteLine($"无效的十六进制数: {part}");
                }
            }
            return byteList;

        }


        public static string GetECU()
        {
            LogHelper.Debug("获取版本");
            List<byte> bytes1 = new List<byte>();

            bytes1.Add(0xF1);
            bytes1.Add(0x8C);

            var temp = Send(bytes1.ToArray(), 0x22);
            string version = "";






            for (int i = 5; i < 41; i++)
            {
                version += (char)_data[i];
            }





            return version;



        }
       

        
        public static int GetSOC()
        {

            List<byte> arr = new List<byte> { 0x73, 0x91 };

            var temp = Send(arr.ToArray(), 0x22);
            if (temp != null && temp.Item1 == 0x62)
            {
                return (int)((temp.Item2[2]) & 0x7f);
            }
            return 0x00;
        }

        public static int GetMOS()
        {

            List<byte> arr = new List<byte> { 0x73, 0x8f };

            var temp = Send(arr.ToArray(), 0x22);
            if (temp != null && temp.Item1 == 0x62)
            {
                return temp.Item2[2];
            }
            return 0;
        }

        public static int GetSB()
        {

            List<byte> arr = new List<byte> { 0x73, 0xB0 };

            var temp = Send(arr.ToArray(), 0x22);
            if (temp != null && temp.Item1 == 0x62)
            {
                return temp.Item2[2];
            }
            return 0;
        }
        static string BarCodeYear = "123456789ABCDEFGHJKLMNPRSTVWXY";
        static string BarCodeMonth = "123456789ABC";
        static string BarCodeDay = "123456789ABCDEFGHJKLMNPRSTVWXYZ";
    }

    
    public enum MOSEnum
    {
        断开 = 0x00,
        充电闭合 = 0x01,
        放电闭合 = 0x02,
        闭合 = 0x03
    }

    public enum MOSEnum2510
    {
        取消控制 = 0x00,
        充电断 = 0x01,
        放电断 = 0x02,
        充电放电断 = 0x03,
        BUCK断 = 0x04,
        BUCK充电断 = 0x05,
        BUCK放电断 = 0x06,
        全断 = 0x07
    }
}
