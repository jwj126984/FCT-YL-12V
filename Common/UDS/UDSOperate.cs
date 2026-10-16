using Common.Can;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.UDS
{
    public class UDSOperate
    {
        #region 这两个参数在ZUDS中用到了
        public int protocol = 1;//1使用canfd  0 使用can发送数据
        public static IntPtr channelHandle;// 通道句柄

        #endregion
        #region Can发送
        const int CAN_MAX_DLEN = 8;
        const int CANFD_BRS = 0x01; /* bit rate switch (second bitrate for payload data) */
        const int CANFD_MAX_DLC = 15;
        const int CANFD_MAX_DLEN = 64;
        #endregion


        public IntPtr deviceHandle = IntPtr.Zero;   // 设备句柄

        public volatile bool isRunning = true;      // 线程标志
        public uint udsHandle;

        public event Action<byte, byte[]> UDSRevice;

        public event Action<uint, byte[]> ReviceData;
        //public 
        public bool Start(IntPtr _deviceHandle, int channel_index = 0, UInt32 abaud = 500, UInt32 dbaud = 2000)
        {
            deviceHandle = _deviceHandle;
            //int chn_idx = 0;    // 通道号
            uint ret;           // 返回值


            ret = ZLGCAN.ZCAN_SetValue(deviceHandle, "0/set_cn", Encoding.ASCII.GetBytes("A001"));
            IntPtr ptrSN = ZLGCAN.ZCAN_GetValue(deviceHandle, "0/get_cn/1");
            string SN = Marshal.PtrToStringAnsi(ptrSN);
            Console.WriteLine(SN);

            // 仲裁域波特率
            string path = String.Format("{0}/canfd_abit_baud_rate", channel_index);
            ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, $"{abaud * 1000}");       // 500k
            if (ret != 1)
            {
                LogHelper.WriteLog("设置仲裁域波特率失败", true);
                return false;
            }

            // 数据域波特率
            path = String.Format("{0}/canfd_dbit_baud_rate", channel_index);
            ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, $"{dbaud * 1000}");      // 2M
            if (ret != 1)
            {
                LogHelper.WriteLog("设置数据域波特率失败", true);
                return false;
            }

            // 终端电阻
            path = String.Format("{0}/initenal_resistance", channel_index);
            string resistance = "1";//1-使能 0-禁能
            ret = ZLGCAN.ZCAN_SetValue(deviceHandle, path, resistance);
            if (ret != 1)
            {
                LogHelper.WriteLog("设置终端电阻失败", true);
                return false;
            }

            // 初始化通道
            ZLGCAN.ZCAN_CHANNEL_INIT_CONFIG InitConfig = new ZLGCAN.ZCAN_CHANNEL_INIT_CONFIG(); // 结构体
            InitConfig.can_type = 1;                // 1 - CANFD (USBCANFD 只能选择CANFD模式)
            InitConfig.config.canfd.mode = 0;       // 0 - 正常模式，1 - 只听模式

            IntPtr P2InitConfig = Marshal.AllocHGlobal(Marshal.SizeOf(InitConfig));
            Marshal.StructureToPtr(InitConfig, P2InitConfig, true);                         // 转成指针
            channelHandle = ZLGCAN.ZCAN_InitCAN(deviceHandle, 0, P2InitConfig);
            Marshal.FreeHGlobal(P2InitConfig);                                              // 释放内存
            if (channelHandle == IntPtr.Zero)
            {
                LogHelper.WriteLog("初始化通道失败", true);
                return false;
            }
            Console.WriteLine("初始化通道成功");


            //打开通道
            uint Ret = ZLGCAN.ZCAN_StartCAN(channelHandle);
            if (Ret != 1)
            {
                LogHelper.WriteLog("打开通道失败", true);
                return false;
            }
            // UDS 初始化
            uint DoCan = 0;
            udsHandle = ZUDS.ZUDS_Init(DoCan);



            // 创建一个接收线程
            Thread workerThread = new Thread(ReceiveThread);
            workerThread.IsBackground = true; // 设置为后台线程
            workerThread.Start(); // 启动线程






            //// 阻塞等待
            //Console.ReadKey();
            //isRunning = false;
            //workerThread.Join();
            //ZUDS.ZUDS_Release(udsHandle);

            //// 关闭通道
            //Ret = ZLGCAN.ZCAN_ResetCAN(channelHandle);
            //if (Ret != 1)
            //{
            //    Console.WriteLine("关闭通道失败");
            //    Console.ReadKey();
            //    return false;
            //}
            //Console.WriteLine("关闭通道成功");

            //// 关闭设备
            //Ret = ZLGCAN.ZCAN_CloseDevice(deviceHandle);
            //if (Ret != 1)
            //{
            //    Console.WriteLine("关闭设备失败");
            //    Console.ReadKey();
            //    return false;
            //}
            //Console.WriteLine("关闭设备成功");
            //Console.ReadKey();      // 回车结束
            



            return true;
        }

        object _lock = new object();
        public Tuple<byte, List<byte>> Send(byte[] data, byte sid = 0x10, uint addr = 0x775, byte dataLen = 0x08, int cf = 0)
        {
            lock (_lock)
            {

            }

            // 设置会话层参数
            ZUDS._ZUDS_SESSION_PARAM zUDS_SESSION_PARAM = new ZUDS._ZUDS_SESSION_PARAM();
            IntPtr P2_ZUDS_SESSION_PARAM = Marshal.AllocHGlobal(Marshal.SizeOf(zUDS_SESSION_PARAM));
            zUDS_SESSION_PARAM.timeout = 5000;
            zUDS_SESSION_PARAM.enhanced_timeout = 2000;
            zUDS_SESSION_PARAM.reserved0 = 0;
            zUDS_SESSION_PARAM.reserved1 = 0;
            Marshal.StructureToPtr(zUDS_SESSION_PARAM, P2_ZUDS_SESSION_PARAM, true);
            ZUDS.ZUDS_SetParam(udsHandle, 0, P2_ZUDS_SESSION_PARAM);//设置会话层参数
            Marshal.FreeHGlobal(P2_ZUDS_SESSION_PARAM);

            // 设置传输层参数
            ZUDS._ZUDS_ISO15765_PARAM zUDS_ISO15765_PARAM = new ZUDS._ZUDS_ISO15765_PARAM();
            IntPtr P2_ZUDS_ISO15765_PARAM = Marshal.AllocHGlobal(Marshal.SizeOf(zUDS_ISO15765_PARAM));
            zUDS_ISO15765_PARAM.version = 1;
            zUDS_ISO15765_PARAM.max_data_len = dataLen;
            zUDS_ISO15765_PARAM.local_st_min = 0;
            zUDS_ISO15765_PARAM.block_size = 0;
            zUDS_ISO15765_PARAM.fill_byte = 0xAA;
            zUDS_ISO15765_PARAM.frame_type = 0;//0:std,1；ext
            zUDS_ISO15765_PARAM.is_modify_ecu_st_min = 0;
            zUDS_ISO15765_PARAM.remote_st_min = 20;
            zUDS_ISO15765_PARAM.fc_timeout = 1000;
            zUDS_ISO15765_PARAM.fill_mode = 2;
            Marshal.StructureToPtr(zUDS_ISO15765_PARAM, P2_ZUDS_ISO15765_PARAM, true);
            ZUDS.ZUDS_SetParam(udsHandle, 1, P2_ZUDS_ISO15765_PARAM); //设置传输层参数
            Marshal.FreeHGlobal(P2_ZUDS_ISO15765_PARAM);
            //设置回调函数
            ZUDS.OnUDSTransmitDelegate transmitCallback = new ZUDS.OnUDSTransmitDelegate(ZUDS.TransmitCallback);
            IntPtr canType = (IntPtr)1;//can 0,canfd 1
            ZUDS.ZUDS_SetTransmitHandler(udsHandle, canType, transmitCallback);//第二个参数设置can/canfd
                                                                                     // UDS 请求参数
            ZUDS._ZUDS_REQUEST request = new ZUDS._ZUDS_REQUEST();
            request.src_addr = addr;
            request.dst_addr = CommonData.RID;
            request.suppress_response = 0;
            request.sid = sid;
            byte[] paramData = { 0x03 };
            paramData = data;
            request.param_len = (uint)paramData.Length;

            request.param = Marshal.AllocHGlobal(paramData.Length);
            Marshal.Copy(paramData, 0, request.param, paramData.Length);

            // 请求结构体
            IntPtr P2request = Marshal.AllocHGlobal(Marshal.SizeOf(request));
            byte[] bytes = new byte[Marshal.SizeOf(request)];
            Marshal.Copy(bytes, 0, P2request, Marshal.SizeOf(request));
            Marshal.StructureToPtr(request, P2request, true);

            // 响应结果体
            ZUDS._ZUDS_RESPONSE response = new ZUDS._ZUDS_RESPONSE();
            IntPtr P2response = Marshal.AllocHGlobal(Marshal.SizeOf(response));

            // UDS 请求
            ZUDS.ZUDS_Request((IntPtr)udsHandle, P2request, P2response);
            Marshal.PtrToStructure(P2response, response);
            var temp = printResponse(response);                            // 打印响应结果
            List<byte> list = new List<byte>();
            Tuple<byte, List<byte>> result = null;
            if (temp.Item2 != null)
            {
                foreach (var item in temp.Item2)
                {
                    list.Add(item);
                }
                result = new Tuple<byte, List<byte>>(temp.Item1, list);
            }
            Marshal.FreeHGlobal(P2request);
            Marshal.FreeHGlobal(P2response);
            
            //if (cf < 2)
            //{
            //    Thread.Sleep(500);
            //    cf++;
            //    return Send(data, sid, addr, dataLen, cf);
            //}
            return result;


        }

        public void Close()
        {
            isRunning = false;
            ZLGCAN.ZCAN_ResetCAN(channelHandle);
            //ZUDS.ZUDS_Release(udsHandle);
        }

        public bool Send(uint id = 0x500, string data = "", bool isCanFd = true, bool canfdExp = true)
        {
            //LogHelper.WriteLog($"发送数据 id{id}-- data{data}");
            //isendJg = false;
            Thread.Sleep(10);//每次发送等10毫秒
            int frame_type_index = 0;//枕类型0标准 1扩展
            //int protocol_index = 1;//协议类型 0 can 1   canfd
            int send_type_index = 0;//发送方式0 正常发送 1 单次发送 2自发自收 3单次自发自收
            int canfd_exp_index = canfdExp ? 1 : 0;//canfd加速
            uint result; //发送的帧数
            if (isCanFd)
            {
                //canfd发送数据
                ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
                canfd_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
                canfd_data.frame.data = new byte[64];
                canfd_data.frame.len = (byte)8;
                    SplitData(data, ref canfd_data.frame.data, CANFD_MAX_DLEN);
                canfd_data.transmit_type = (uint)send_type_index;
                canfd_data.frame.flags = (byte)((canfd_exp_index != 0) ? CANFD_BRS : 0);
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_data));
                Marshal.StructureToPtr(canfd_data, ptr, true);
                result = Method.ZCAN_TransmitFD(channelHandle, ptr, 1);
                Marshal.FreeHGlobal(ptr);
                if (result != 1)
                {
                    return false;
                    //MessageBox.Show("发送数据失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //AddErr();
                }
            }
            else
            {
                ZCAN_Transmit_Data can_data = new ZCAN_Transmit_Data();
                can_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
                can_data.frame.data = new byte[8];
                can_data.frame.can_dlc = (byte)SplitData(data, ref can_data.frame.data, CAN_MAX_DLEN);
                can_data.transmit_type = (uint)send_type_index;
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
                Marshal.StructureToPtr(can_data, ptr, true);
                result = Method.ZCAN_Transmit(channelHandle, ptr, 1);
                Marshal.FreeHGlobal(ptr);
                if (result != 1)
                {
                    return false;
                    //MessageBox.Show("发送数据失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    //AddErr();
                }
            }

            return true;
        }

        // 接收线程
        void ReceiveThread()
        {
            while (isRunning)
            {
                uint RecNum = ZLGCAN.ZCAN_GetReceiveNum(channelHandle, 0);  // CAN
                if (RecNum > 0)
                {
                    ZLGCAN.ZCAN_Receive_Data zcanReceiveData = new ZLGCAN.ZCAN_Receive_Data();

                    IntPtr P2zcanReceiveData = Marshal.AllocHGlobal(Marshal.SizeOf(zcanReceiveData) * (int)RecNum);
                    byte[] zero = new byte[Marshal.SizeOf(zcanReceiveData) * (int)RecNum];
                    Marshal.Copy(zero, 0, P2zcanReceiveData, Marshal.SizeOf(zcanReceiveData) * (int)RecNum);

                    uint ReceiveNum = ZLGCAN.ZCAN_Receive(channelHandle, P2zcanReceiveData, RecNum, 10);
                    if (ReceiveNum > 0)
                    {
                        for (int i = 0; i < ReceiveNum; i++)
                        {
                            zcanReceiveData = (ZLGCAN.ZCAN_Receive_Data)Marshal.PtrToStructure(P2zcanReceiveData + i * Marshal.SizeOf(typeof(ZLGCAN.ZCAN_Receive_Data)), typeof(ZLGCAN.ZCAN_Receive_Data));
                            //Console.WriteLine("{0}:{1}", zcanReceiveData.timestamp, Program.b_array2string(zcanReceiveData.frame.data, zcanReceiveData.frame.can_dlc));
                            ReviceData?.Invoke(zcanReceiveData.frame.can_id, zcanReceiveData.frame.data);
                        }
                        ZUDS.ZUDS_OnReceive(udsHandle, ZUDS.TransferCANFrame(P2zcanReceiveData, ReceiveNum, 0));//与发送线程中得request函数相配合。
                    }
                    Marshal.FreeHGlobal(P2zcanReceiveData);
                }

                RecNum = ZLGCAN.ZCAN_GetReceiveNum(channelHandle, 1);   // CANFD
                if (RecNum > 0)
                {
                    ZLGCAN.ZCAN_ReceiveFD_Data zcanReceiveFDData = new ZLGCAN.ZCAN_ReceiveFD_Data();
                    ZLGCAN.canfd_frame canfd_Frame = new ZLGCAN.canfd_frame();
                    zcanReceiveFDData.frame = canfd_Frame;

                    IntPtr P2zcanReceiveFDData = Marshal.AllocHGlobal(80);
                    byte[] zero = new byte[80];
                    Marshal.Copy(zero, 0, P2zcanReceiveFDData, 80);

                    uint ReceiveNum = ZLGCAN.ZCAN_ReceiveFD(channelHandle, P2zcanReceiveFDData, 1, 10);
                    if (ReceiveNum > 0)
                    {
                        for (int i = 0; i < ReceiveNum; i++)
                        {
                            zcanReceiveFDData = (ZLGCAN.ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(P2zcanReceiveFDData + i * Marshal.SizeOf(typeof(ZLGCAN.ZCAN_ReceiveFD_Data)), typeof(ZLGCAN.ZCAN_ReceiveFD_Data));
                            ReviceData?.Invoke(zcanReceiveFDData.frame.can_id, zcanReceiveFDData.frame.data);
                            //Console.WriteLine("{0}:{1}", zcanReceiveFDData.timestamp, Program.b_array2string(zcanReceiveFDData.frame.data, zcanReceiveFDData.frame.len));
                        }
                        ZUDS.ZUDS_OnReceive(udsHandle, ZUDS.TransferCANFrame(P2zcanReceiveFDData, ReceiveNum, 1));
                    }
                    Marshal.FreeHGlobal(P2zcanReceiveFDData);
                }
                //Thread.Sleep(1);
                Thread.Yield();
            }
        }

        // 打印响应结果
        (byte, byte[], string) printResponse(ZUDS._ZUDS_RESPONSE response)
        {
            //Tuple<byte, byte[],string> result = new Tuple<byte, byte[], string>(0,new byte[] { },"");
            // 打印判断
            switch (response.status)
            {
                case 0:
                    if (response.type == 0)
                    {
                        Console.WriteLine("消极响应: sid = 0x{0:X},error code = 0x{1:X}", response.union.zudsNegative.sid, response.union.zudsNegative.error_code);
                        LogHelper.ShowMsg("UDS消极响应");
                        return (response.union.zudsNegative.sid, null, "UDS消极响应");
                    }
                    else if (response.type == 1)
                    {
                        byte[] ParamArray = new byte[response.union.zudsPositive.param_len];
                        if (ParamArray.Length > 0)
                        {
                            Marshal.Copy(response.union.zudsPositive.param, ParamArray, 0, (int)response.union.zudsPositive.param_len);
                        }


                        //string[] ParamStringArray = Array.ConvertAll(ParamArray, b => b.ToString("X2"));
                        //string ParamString = string.Join(" ", ParamStringArray);
                        //Console.WriteLine(@"积极响应: sid=0x{0:X} 参数长度 = {1}  参数：{2}", response.union.zudsPositive.sid, response.union.zudsPositive.param_len, ParamString);
                        //LogHelper.ShowMsg($"UDS积极响应: sid=0x{response.union.zudsPositive.sid.ToString("X")} 参数长度 = {response.union.zudsPositive.param_len}  参数：{ParamString}");
                        //UDSRevice?.Invoke(response.union.zudsPositive.sid, ParamArray);
                        return (response.union.zudsNegative.sid, ParamArray, "积极响应");
                    }
                    break;
                case 1:
                    Console.WriteLine("响应超时");
                    LogHelper.ShowMsg("UDS响应超时1");
                    return (response.union.zudsNegative.sid, null, "响应超时");
                //break;
                case 2:
                    Console.WriteLine("传输失败，请检查链路层，或请确认流控帧是否回复");
                    LogHelper.ShowMsg("UDS传输失败，请检查链路层，或请确认流控帧是否回复");
                    return (response.union.zudsNegative.sid, null, "传输失败，请检查链路层，或请确认流控帧是否回复");
                //break;
                case 3:
                    Console.WriteLine("取消请求");
                    LogHelper.ShowMsg("UDS取消请求");
                    return (response.union.zudsNegative.sid, null, "UDS取消请求");
                //break;
                case 4:
                    Console.WriteLine("抑制响应");
                    LogHelper.ShowMsg("UDS抑制响应");
                    return (response.union.zudsNegative.sid, null, "UDS抑制响应");
                //break;
                case 100:
                    Console.WriteLine("其他错误");
                    LogHelper.ShowMsg("UDS其他错误");
                    return (response.union.zudsNegative.sid, null, "UDS其他错误");
                    //break;

            }
            return (response.union.zudsNegative.sid, null, "UDS其他错误");
        }


        public static string b_array2string(byte[] ByteIn, byte LEN)
        {
            string str = "";
            for (int i = 0; i < LEN; i++)
            {
                str = str + ByteIn[i].ToString("X2") + " ";

            }
            return str;
        }

        private uint MakeCanId(uint id, int eff, int rtr, int err)//1:extend frame 0:standard frame
        {
            uint ueff = (uint)(!!(Convert.ToBoolean(eff)) ? 1 : 0);
            uint urtr = (uint)(!!(Convert.ToBoolean(rtr)) ? 1 : 0);
            uint uerr = (uint)(!!(Convert.ToBoolean(err)) ? 1 : 0);
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }
        //拆分text到发送data数组
        private int SplitData(string data, ref byte[] transData, int maxLen)
        {
            string[] dataArray = data.Split(' ');
            for (int i = 0; (i < maxLen) && (i < dataArray.Length); i++)
            {
                transData[i] = Convert.ToByte(dataArray[i].Substring(0, 2), 16);
            }

            return dataArray.Length;
        }
    }
    public class ZLGCAN
    {
        #region 函数部分
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_OpenDevice(uint device_type, uint device_index, uint reserved);
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_CloseDevice(IntPtr device_handle);
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_InitCAN(IntPtr device_handle, uint can_index, IntPtr pInitConfig);
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_SetValue(IntPtr device_handle, string path, string value);
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_SetValue(IntPtr device_handle, string path, Byte[] value);
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_GetValue(IntPtr device_handle, string path); // 返回一个指针

        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ResetCAN(IntPtr chn_handle);

        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_StartCAN(IntPtr chn_handle);

        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetIProperty(IntPtr device_handle);

        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_Transmit(IntPtr channel_handle, IntPtr pTransmit, uint len);
        /// <summary>
        /// 发送CANFD 报文
        /// </summary>
        /// <param name="channel_handle">通道句柄</param>
        /// <param name="pTransmit">结构体 ZCAN_TransmitFD_Data 结构体的首指针</param>
        /// <param name="len">报文数目</param>
        /// <returns>返回实际发送成功的报文数目。</returns>
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_TransmitFD(IntPtr channel_handle, IntPtr pTransmit, uint len);

        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_TransmitData(IntPtr device_handle, IntPtr pTransmit, uint len);
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_Receive(IntPtr channel_handle, IntPtr pReceive, uint len, int wait_time = -1);
        /// <summary>
        /// 该函数用于接收 CANFD 数据，建议使用 ZCAN_GetReceiveNum 确保缓冲区有数据再 使用。

        /// </summary>
        /// <param name="channel_handle">通道句柄值。</param>
        /// <param name="pReceive">结构体 ZCAN_ReceiveFD_Data 数组的首指针。</param>
        /// <param name="len">数组长度（本次接收的最大报文数目，实际返回值小于等于这个值）</param>
        /// <param name="wait_time">缓冲区无数据，函数阻塞等待时间，单位毫秒。若为-1 则表示无超时，一直等待，默认值为-1。(可能无效)</param>
        /// <returns>返回实际接收的报文数目。</returns>
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ReceiveFD(IntPtr channel_handle, IntPtr pReceive, uint len, int wait_time = -1);

        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_ReceiveData(IntPtr device_handle, IntPtr pReceive, uint Rcv_num);
        /// <summary>
        /// 获取缓冲区中 CAN，CANFD 或者合并接收报文数目。
        /// </summary>
        /// <param name="channel_handle">通道句柄值。</param>
        /// <param name="type">获取 CAN，CANFD 或者合并接收报文，0=CAN，1=CANFD，2=合并接收。</param>
        /// <returns>返回报文数目</returns>
        [DllImport(".\\zlgcan.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern uint ZCAN_GetReceiveNum(IntPtr channel_handle, byte type);//UINT ZCAN_Receive(CHANNEL_HANDLE channel_handle, ZCAN_Receive_Data* pReceive, UINT len, INT wait_time = -1);

        //public IntPtr Data_Transfer(IntPtr )
        //{

        //}

        #endregion

        #region 结构体部分
        public struct ZCAN_CHANNEL_INIT_CONFIG
        {
            public uint can_type;
            public _ZCAN_CHANNEL_INIT_CONFIG config;
        }

        [StructLayout(LayoutKind.Explicit, Pack = 1)]
        public struct _ZCAN_CHANNEL_INIT_CONFIG
        {
            [FieldOffset(0)]
            public _ZCAN_CHANNEL_CAN_INIT_CONFIG can;
            [FieldOffset(0)]
            public _ZCAN_CHANNEL_CANFD_INIT_CONFIG canfd;
        }

        public struct _ZCAN_CHANNEL_CAN_INIT_CONFIG
        {
            public uint acc_code;
            public uint acc_mask;
            public uint reserved;
            public byte filter;
            public byte timing0;
            public byte timing1;
            public byte mode;
        }
        public struct _ZCAN_CHANNEL_CANFD_INIT_CONFIG
        {
            public uint acc_code;
            public uint acc_mask;
            public uint abit_timing;
            public uint dbit_timing;//uint 无符号整数2^32,0-4294967295
            public uint brp;
            public byte filter;//byte无符号整数 2^8 0-255
            public byte mode;
            public ushort pad;//ushort 无符号短整数 2^16 0-65535
            public uint reserved;
        }

        public class Device_Index
        {
            public const int TYPE_CAN = 0;
            public const int TYPE_CANFD = 1;
            public const int ZCAN_USBCAN1 = 3;
            public const int ZCAN_USBCAN2 = 4;
            public const int ZCAN_CANETUDP = 12;
            public const int ZCAN_CANETTCP = 17;
            public const int ZCAN_USBCAN_E_U = 20;
            public const int ZCAN_USBCAN_2E_U = 21;
            public const int ZCAN_PCIECANFD_100U = 38;
            public const int ZCAN_PCIECANFD_200U = 39;
            public const int ZCAN_PCIECANFD_200U_EX = 62;
            public const int ZCAN_PCIECANFD_400U = 61;
            public const int ZCAN_USBCANFD_200U = 41;
            public const int ZCAN_USBCANFD_100U = 42;
            public const int ZCAN_USBCANFD_MINI = 43;
            public const int ZCAN_CLOUD = 46;
            public const int ZCAN_CANFDNET_200U_TCP = 48;
            public const int ZCAN_CANFDNET_200U_UDP = 49;
            public const int ZCAN_CANFDNET_400U_TCP = 52;
            public const int ZCAN_CANFDNET_400U_UDP = 53;
            public const int ZCAN_CANFDNET_800U_TCP = 57;
            public const int ZCAN_CANFDNET_800U_UDP = 58;
            public const int STATUS_ERR = 0;
            public const int STATUS_OK = 1;
        };
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_Transmit_Data
        {
            public can_frame Can_Frame;
            public uint transmit_type;
        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class can_frame
        {
            public uint can_id;             /* 32 bit MAKE_CAN_ID + ERR/RTR/EFF flags */
            public byte can_dlc;            // frame payload length in byte (0 .. CAN_MAX_DLEN)
            public byte __pad;             // padding
            public byte __res0;            // reserved / padding
            public byte __res1;            // reserved / padding
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] data;             // frame data
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class canfd_frame
        {
            /// <summary>
            /// /* 32 bit MAKE_CAN_ID + ERR/RTR/EFF flags */
            /// </summary>
            public uint can_id;
            /// <summary>
            ///  /* frame payload length in byte */
            /// </summary>
            public byte len;
            /// <summary>
            /// /* additional flags for CAN FD, brs-1 esi-1 _pad-6 */
            /// </summary>
            public byte flags;
            /// <summary>
            /// /* reserved / padding */
            /// </summary>
            public byte __res0;
            public byte __res1;  /* reserved / padding */
            /// <summary>
            /// data数组，64字节
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] data/* __attribute__((aligned(8)))*/;
        };

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ZCAN_Receive_Data
        {
            public can_frame frame;
            public UInt64 timestamp;

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class ZCAN_ReceiveFD_Data
        {
            public canfd_frame frame;
            public UInt64 timestamp;

        }
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCANFD_AUTO_TRANSMIT_OBJ    //CANFD定时发送帧结构体
        {
            public ushort enable;           //0-禁用，1-使能  
            public ushort index;            //定时报文索引  
            public uint interval;                  //定时周期
            public ZCAN_TransmitFD_Data obj;
        }
        /// <summary>
        /// Canfd data结构体
        /// </summary>

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCAN_TransmitFD_Data
        {
            /// <summary>
            /// 报文数据信息，详见 canfd_frame 结构说明。
            /// </summary>
            public canfd_frame Canfd_frame;
            /// <summary>
            /// 发送方式，0=正常发送，1=单次发送，2=自发自收，3=单次自发自收。
            /// </summary>
            public uint transmit_type;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCANCANFDData
        {
            public UInt64 timeStamp;
            public UInt32 flag; //class ZCANdataFlag(Structure) :
                                //    _pack_  =  1
                                //    _fields_= [("frameType", c_uint, 2),
                                //               ("txDelay",c_uint,2),
                                //   ("transmitType",c_uint,4),
                                //   ("txEchoRequest",c_uint,1),
                                //   ("txEchoed",c_uint,1),
                                //   ("reserved",c_uint,22),
                                //   ]
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] extraData;                                    //未使用  
            public canfd_frame frame;                                   //实际报文结构体
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct ZCANDataObj
        {
            public byte dataType;                                       //1-can/canfd数据。
            public byte chnl;                                           //数据通道 
            public UInt16 flag;                                         //未使用
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] extraData;                                    //未使用  
            public ZCANCANFDData zcanCANFDData;                         //报文结构体
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] reserve;                                      //用于字节对齐 
        }
        #endregion
    }

    public class ZUDS
    {
        #region 函数部分
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate uint OnUDSTransmitDelegate(IntPtr ctx, IntPtr frame, uint count);
        /// <summary>
        /// 该函数用于初始化 UDS 函数库，返回操作句柄，用于后续的操作，与 ZUDS_Release
        /// 配对使用。
        /// typedef uint32 TP_TYPE; // transport protocol
        /// #define DoCAN 0
        /// </summary>
        /// <param name="Chn_Handle"></param>
        /// <returns>操作句柄，= ZUDS_INVALID_HANDLE 为无效句柄，其他值为有效句柄。</returns>
        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern UInt32 ZUDS_Init(uint type);
        /// <summary>
        /// 该函数用于释放资源，与 ZUDS_Init 配对使用。
        /// </summary>
        /// <param name="Chn_Handle"></param>
        /// <returns></returns>
        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZUDS_Release(uint type);
        /// <summary>
        /// 该函数用于设置函数库的参数。
        /// </summary>
        /// <param name="ZUDS_HANDLE"></param>
        /// <param name="type">参数类型,= PARAM_TYPE_SESSION 0 用 于 设 置 会 话 层 参 数 ， =
        ///PARAM_TYPE_ISO15765 1 用于设置 ISO15765 的通信参数；</param>
        /// <param name="param">参数值,type =PARAM_TYPE_SESSION 0 时为 ZUDS_SESSION_PARAM，
        ///type= PARAM_TYPE_ISO15765 1 时为 ZUDS_ISO15765_PARAM。</param>
        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZUDS_SetParam(UInt32 ZUDS_HANDLE, byte type, IntPtr param);
        /// <summary>
        /// 该函数用于设置发送回调函数。函数库自身并不发送帧数据，把打包的帧数据通过回调
        ///函数传出给用户发送，用户可通过 zlgcan 函数库进行帧数据发送。
        /// </summary>
        /// <param name="ZUDS_HANDLE"></param>
        /// <param name="ctx">用户设置的参数，原封不动的传出给用户；</param>
        /// <param name="onUDSTransmit">：回调函数原型；typedef uint32 (*OnUDSTransmit)(void* ctx, const ZUDS_FRAME* frame, uint32 count);</param>
        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZUDS_SetTransmitHandler(UInt32 ZUDS_HANDLE, IntPtr canType, OnUDSTransmitDelegate onUDSTransmit);

        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZUDS_OnReceive(UInt32 ZUDS_HANDLE, IntPtr ZUDS_FRAME);

        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZUDS_Request(IntPtr ZUDS_HANDLE, IntPtr ZUDS_REQUEST, IntPtr ZUDS_RESPONSE);

        [DllImport(".\\zuds.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern void ZUDS_Stop(IntPtr ZUDS_HANDLE);

        public static uint TransmitCallback(IntPtr canType, IntPtr frame, uint count)
        {
            //uds frame tranfer to can frame
            UDSOperate program = new UDSOperate();
            if (program.protocol == 1)
            {
                ZLGCAN.canfd_frame canfd_Frame = new ZLGCAN.canfd_frame();
                ZLGCAN.ZCAN_TransmitFD_Data tranfd_data = new ZLGCAN.ZCAN_TransmitFD_Data();
                IntPtr P2canfdFrame = TransferUDSFrame(frame, count);
                IntPtr P2canfdTranFrame = Marshal.AllocHGlobal(Marshal.SizeOf(tranfd_data) * (int)count);
                for (int i = 0; i < count; i++)
                {
                    Marshal.PtrToStructure((IntPtr)(P2canfdFrame.ToInt64() + i * Marshal.SizeOf(canfd_Frame)), canfd_Frame);
                    tranfd_data.Canfd_frame = canfd_Frame;
                    tranfd_data.transmit_type = 0; // 正常发送

                    Marshal.StructureToPtr(canfd_Frame, (IntPtr)(P2canfdTranFrame.ToInt64() + i * Marshal.SizeOf(tranfd_data)), true);
                    //int k = 0;
                }
                uint Ret = ZLGCAN.ZCAN_TransmitFD(UDSOperate.channelHandle, P2canfdTranFrame, count);
                Marshal.FreeHGlobal(P2canfdFrame);
                Marshal.FreeHGlobal(P2canfdTranFrame);
                if (Ret == count) { return 0; }
                else { return 1; }
            }
            else if (program.protocol == 0)
            {
                ZLGCAN.can_frame can_Frame = new ZLGCAN.can_frame();
                ZLGCAN.ZCAN_Transmit_Data tran_data = new ZLGCAN.ZCAN_Transmit_Data();
                IntPtr P2canFrame = TransferUDSFrame(frame, count);
                IntPtr P2canTranFrame = Marshal.AllocHGlobal(Marshal.SizeOf(tran_data) * (int)count);
                for (int i = 0; i < count; i++)
                {
                    Marshal.PtrToStructure((IntPtr)(P2canFrame.ToInt64() + i * Marshal.SizeOf(can_Frame)), can_Frame);
                    tran_data.Can_Frame = can_Frame;
                    tran_data.transmit_type = 0; // 正常发送

                    Marshal.StructureToPtr(tran_data, (IntPtr)(P2canTranFrame.ToInt64() + i * Marshal.SizeOf(tran_data)), true);
                    //int k = 0;
                }
                uint Ret = ZLGCAN.ZCAN_Transmit(UDSOperate.channelHandle, P2canTranFrame, count);
                Marshal.FreeHGlobal(P2canFrame);
                Marshal.FreeHGlobal(P2canTranFrame);
                if (Ret == count) { return 0; }
                else { return 1; }
            }
            else
            {
                return 1;
            }


        }
        #region 参数定义
        public static uint udsRTR = 0x40000000;//Remote Transmission Request
        public static uint udsEFF = 0x80000000;//Extend Frame Flag 
        public static uint udsERR = 0x20000000;//Err flag
        public static bool canfdFlag = false;
        #endregion
        public static IntPtr TransferUDSFrame(IntPtr P2udsframe, uint count)
        {
            UDSOperate program = new UDSOperate();

            if (program.protocol == 1)
            {
                ZUDS._ZUDS_FRAME udsFrame = new _ZUDS_FRAME();
                ZLGCAN.canfd_frame canfd_Frame = new ZLGCAN.canfd_frame();
                IntPtr P2canfd_Frame = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_Frame) * (int)count);
                canfd_Frame.data = new byte[64];
                for (int i = 0; i < count; i++)
                {
                    Marshal.PtrToStructure((IntPtr)(P2udsframe.ToInt64() + i * Marshal.SizeOf(udsFrame)), udsFrame);
                    //判断扩展帧
                    canfd_Frame.can_id = (udsFrame.extend == 1) ? udsFrame.id | ZUDS.udsEFF : udsFrame.id;
                    //判断远程帧
                    canfd_Frame.can_id = (udsFrame.remote == 1) ? canfd_Frame.can_id | ZUDS.udsRTR : canfd_Frame.can_id;

                    canfd_Frame.len = udsFrame.data_len;

                    canfd_Frame.flags = 0x0000;

                    Array.Copy(udsFrame.data, canfd_Frame.data, udsFrame.data_len);

                    Marshal.StructureToPtr(canfd_Frame, (IntPtr)(P2canfd_Frame.ToInt64() + i * Marshal.SizeOf(canfd_Frame)), false);
                }
                return P2canfd_Frame;
            }
            else
            {
                ZUDS._ZUDS_FRAME udsFrame = new _ZUDS_FRAME();
                ZLGCAN.can_frame can_Frame = new ZLGCAN.can_frame();
                IntPtr P2can_Frame = Marshal.AllocHGlobal(Marshal.SizeOf(can_Frame) * (int)count);
                can_Frame.data = new byte[8];
                for (int i = 0; i < count; i++)
                {
                    Marshal.PtrToStructure((IntPtr)(P2udsframe.ToInt64() + i * Marshal.SizeOf(udsFrame)), udsFrame);
                    //判断扩展帧
                    can_Frame.can_id = (udsFrame.extend == 1) ? udsFrame.id | ZUDS.udsEFF : udsFrame.id;
                    //判断远程帧
                    can_Frame.can_id = (udsFrame.remote == 1) ? can_Frame.can_id | ZUDS.udsRTR : can_Frame.can_id;

                    can_Frame.can_dlc = udsFrame.data_len;

                    Array.Copy(udsFrame.data, can_Frame.data, udsFrame.data_len);

                    Marshal.StructureToPtr(can_Frame, (IntPtr)(P2can_Frame.ToInt64() + i * Marshal.SizeOf(can_Frame)), false);
                }
                return P2can_Frame;
            }




        }

        public static IntPtr TransferCANFrame(IntPtr receiveData, uint count, int isCANFD)
        {
            if (isCANFD == 1)
            {
                ZLGCAN.ZCAN_ReceiveFD_Data zcanReceiveFDData = new ZLGCAN.ZCAN_ReceiveFD_Data();
                _ZUDS_FRAME zudsFrame = new _ZUDS_FRAME();
                IntPtr P2zudsFrame = Marshal.AllocHGlobal(Marshal.SizeOf(zudsFrame) * (int)count);

                for (int i = 0; i < count; i++)
                {
                    Marshal.PtrToStructure((IntPtr)(receiveData.ToInt64() + i * Marshal.SizeOf(zcanReceiveFDData)), zcanReceiveFDData);

                    zudsFrame.id = zcanReceiveFDData.frame.can_id;
                    zudsFrame.data_len = zcanReceiveFDData.frame.len;
                    zudsFrame.data = new byte[64];

                    Array.Copy(zcanReceiveFDData.frame.data, zudsFrame.data, zudsFrame.data_len);
                    Marshal.StructureToPtr(zudsFrame, (IntPtr)(P2zudsFrame.ToInt64() + i * Marshal.SizeOf(zudsFrame)), false);
                }
                return P2zudsFrame;
            }
            else
            {
                ZLGCAN.ZCAN_Receive_Data zcanReceiveData = new ZLGCAN.ZCAN_Receive_Data();
                _ZUDS_FRAME zudsFrame = new _ZUDS_FRAME();
                IntPtr P2zudsFrame = Marshal.AllocHGlobal(Marshal.SizeOf(zudsFrame) * (int)count);

                for (int i = 0; i < count; i++)
                {
                    Marshal.PtrToStructure((IntPtr)(receiveData.ToInt64() + i * Marshal.SizeOf(zcanReceiveData)), zcanReceiveData);

                    zudsFrame.id = zcanReceiveData.frame.can_id;
                    zudsFrame.data_len = zcanReceiveData.frame.can_dlc;
                    zudsFrame.data = new byte[64];

                    Array.Copy(zcanReceiveData.frame.data, zudsFrame.data, zudsFrame.data_len);
                    Marshal.StructureToPtr(zudsFrame, (IntPtr)(P2zudsFrame.ToInt64() + i * Marshal.SizeOf(zudsFrame)), false);
                }
                return P2zudsFrame;
            }
        }
        #endregion

        #region 结构体部分
        /// <summary>
        /// 会话层面参数；即一应一答传输时的通讯参数。
        /// </summary>
        public struct _ZUDS_SESSION_PARAM
        {
            public UInt16 timeout;// ms, timeout to wait the response of the server
            public UInt16 enhanced_timeout; // ms,timeout to wait after negative response: error code 0x78
            public UInt32 reserved0; // 保留
            public UInt32 reserved1; // 保留
        }

        /// <summary>
        /// 传输数据部分的参数，例如传输时侯每帧报文的字节数。
        /// </summary>
        public struct _ZUDS_ISO15765_PARAM
        {
            public byte version; // VERSION_0, VERSION_1格式版本，为 VERSION_0 时符合 ISO15765-2 的 2004 版本格式要求；为
            //hVERSION_1 是符合 ISO15765-2 的 2016 版本新增的格式要求，如下图所示
            public byte max_data_len; // max data length, can:8, canfd:64 
            public byte local_st_min; // ms, min time between two consecutive frames
            public byte block_size;
            public byte fill_byte; // fill to invalid byte
            public byte frame_type; // 0:std 1:ext
            public byte is_modify_ecu_st_min; //是否忽略 ECU 返回流控的 STmin，强制使用本程序设置的
            //remote_st_min 参数代替

            public byte remote_st_min; //发 送 多 帧 时 用, is_ignore_ecu_st_min = 1 时 有 效 ,
            //0x00-0x7F(0ms ~127ms), 0xF1-0xF9(100us ~900us)
            public UInt16 fc_timeout; //接收流控超时时间(ms), 如发送首帧后需要等待回应流控帧。
            public byte fill_mode;//字节填充模式。FILL_MODE_NONE-不填充0；FILL_MODE_SHORT- 小于 8 字节填充至 8 字节，大于 8 字节时按 DLC 就近填充1；FILL_MODE_MAX- 始终填充至最大数据长度 (不建议)2。
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] reserved;
        }

        public struct _ZUDS_TESTER_PRESENT_PARAM
        {
            UInt32 addr;//会话保持的请求地址；
            UInt16 cycle;//发送周期，单位毫秒；
            byte suppress_response; // 1:suppress是否抑制响应，建议设置为 1；
            UInt32 reserved;//：保留，忽略即可。
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct _ZUDS_REQUEST
        {
            public uint src_addr;             // 请求地址
            public uint dst_addr;             // 响应地址
            public byte suppress_response;    // 1:抑制响应
            public byte sid;                  //service id of request
            public ushort reserve0;
            public IntPtr param;              //array,params of the service 
            public uint param_len;            //参数数组的长度
            public uint reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _ZUDS_RESPONSE
        {
            public byte status;
            public byte type; // RT_POSITIVE, RT_NEGATIVE
            public _ZUDS_Union union;
            public uint reserved;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct _ZUDS_positive
        {
            public byte sid; // service id of response
            public IntPtr param; // array, params of the service, don't free
            public uint param_len;

        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct _ZUDS_negative
        {
            public byte neg_code; // 0x7F
            public byte sid;      //service id of response
            public byte error_code;//消极响应的错误码

        }

        [StructLayout(LayoutKind.Explicit)]
        public struct _ZUDS_Union
        {
            [FieldOffset(0)]
            public _ZUDS_positive zudsPositive;

            [FieldOffset(0)]
            public _ZUDS_negative zudsNegative;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public class _ZUDS_FRAME
        {
            public uint id;
            public byte extend;
            public byte remote;
            public byte data_len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] data;
            public uint reserved;
        }
        #endregion
    }
}
