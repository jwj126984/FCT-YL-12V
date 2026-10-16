using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AllData;
using Models_C;

namespace Common.Can
{
    public class CanOperate
    {
        const int CAN_MAX_DLEN = 8;
        const int CANFD_BRS = 0x01; /* bit rate switch (second bitrate for payload data) */
        const int CANFD_MAX_DLC = 15;
        const int CANFD_MAX_DLEN = 64;
        const uint CAN_EFF_FLAG = 
            000000U; /* EFF/SFF is set in the MSB */
        const uint CAN_RTR_FLAG = 0x40000000U; /* remote transmission request */
        const uint CAN_ERR_FLAG = 0x20000000U; /* error message frame */
        const uint CAN_ID_FLAG = 0x1FFFFFFFU; /* id */
        const int NULL = 0;
        int canmode = 0;//工作模式 0正常 1只听
        int standard = 0;//CANFD标准 ISO
        IntPtr channel_handle_;
        IntPtr _deviceHandle;
        DeviceType _deviceType;
       
        int channel_index_ = 0;
        recvdatathread recv_data_thread_;


        /// <summary>
        /// 是否启动
        /// </summary>
        bool? isStart { get; set; } = null;


        bool checkBox_merge = false;//是否合并接收
        
        public event Action<string> Resvice;
        public event Action<uint, byte[]> ReviceData;



        public CanOperate()
        {

        }
        int _type=int.MinValue;
        public CanOperate(int type)
        {
            _type=type;
        }

        /// <summary>
        /// 设置CANFD标准
        /// </summary>
        /// <param name="channel_index">通道号</param>
        /// <param name="canfd_standard">默认0是ISO</param>
        /// 
        /// <returns></returns>
        private bool SetISO(IntPtr handle, int channel_index, int canfd_standard)
        {
            string path = channel_index + "/canfd_standard";
            string value = canfd_standard.ToString();
            uint ret = Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
            return (ret == 1);
        }
        /// <summary>
        /// 设置开启合并接收
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="channel_index"></param>
        /// <param name="checkBox_merge"></param>
        /// <returns></returns>
        private bool SetDataMerge(IntPtr handle, int channel_index = 0, bool checkBox_merge = false)
        {
            byte merge_ = 0;
            if (checkBox_merge) merge_ = 1;
            string path = channel_index + "/set_device_recv_merge";
            string value = merge_.ToString();
            return 1 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="channel_index"></param>
        /// <param name="abaud">500kps</param>
        /// <param name="dbaud">500kps</param>
        /// <returns></returns>
        private bool SetFdBaudrate(IntPtr handle, int channel_index = 0, UInt32 abaud = 500, UInt32 dbaud = 500)
        {
            string path = channel_index + "/canfd_abit_baud_rate";
            string value = (abaud * 1000).ToString();
            if (1 != Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            path = channel_index + "/canfd_dbit_baud_rate";
            value = (dbaud * 1000).ToString();
            if (1 != Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 设置终端电阻使能
        /// </summary>
        /// <param name="checkBox_resistance"></param>
        /// <returns></returns>
        private bool SetResistanceEnable(IntPtr handle, int channel_index = 0, bool checkBox_resistance = true)
        {
           
            string path = channel_index + "/initenal_resistance";
            string value = (checkBox_resistance ? "1" : "0");
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            return 1 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
        }

        /// <summary>
        /// 设置滤波
        /// </summary>
        /// <param name="handle"></param>
        /// <param name="channel_index"></param>
        /// <param name="lbms">滤波模式 默认2禁能不会执行</param>
        /// <returns></returns>
        private bool SetFilter(IntPtr handle, int channel_index = 0, string lbms = "2", string startId = "", string endId = "")
        {
            string path = channel_index + "/filter_clear";//清除滤波
            string value = "0";

            if (0 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = channel_index + "/filter_mode";
            value = lbms;
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (value == "2")
            {
                //path = channel_index + "/filter_mode";
                //value ="0";
                //Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
                //path = channel_index + "/filter_start";
                //value ="0";
                //Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
                //path = channel_index + "/filter_end";
                //value = "0x7ff";
                //Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
                //path = channel_index + "/filter_ack";//滤波生效
                //value = "0";
                //Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value));
                return true;
            }
            if (0 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = channel_index + "/filter_start";
            value = startId;
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = channel_index + "/filter_end";
            value = endId;
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            path = channel_index + "/filter_ack";//滤波生效
            value = "0";
            //char* pathCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(path).ToPointer();
            //char* valueCh = (char*)System.Runtime.InteropServices.Marshal.StringToHGlobalAnsi(value).ToPointer();
            if (0 == Method.ZCAN_SetValue(handle, path, Encoding.ASCII.GetBytes(value)))
            {
                return false;
            }

            //如果要设置多条滤波，在清除滤波和滤波生效之间设置多条滤波即可
            return true;
        }


        public bool Start(DeviceType deviceType, IntPtr deviceHandle, int channel_index, UInt32 abaud = 500, UInt32 dbaud = 500, bool checkBox_resistance = true)
        {
            _deviceType = deviceType;
            _deviceHandle = deviceHandle;
            ////未打开
            //if (!m_bOpen)
            //{
            //    return false;
            //}
            channel_index_ = channel_index;
            uint type = (uint)deviceType;
            //false
            bool netDevice = type == Define.ZCAN_CANETTCP || type == Define.ZCAN_CANETUDP || type == Define.ZCAN_CANWIFI_TCP ||
               type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
               type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
               type == Define.ZCAN_CANFDNET_800U_UDP;
            //false
            bool canfdnetDevice = type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
                type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
                type == Define.ZCAN_CANFDNET_800U_UDP;
            //false
            bool pcieCanfd = type == Define.ZCAN_PCIECANFD_100U ||
                type == Define.ZCAN_PCIECANFD_200U ||
                type == Define.ZCAN_PCIECANFD_400U ||
                type == Define.ZCAN_PCIECANFD_200U_EX;
            //true
            bool usbCanfd = type == Define.ZCAN_USBCANFD_100U ||
                type == Define.ZCAN_USBCANFD_200U ||
                type == Define.ZCAN_USBCANFD_400U ||
                type == Define.ZCAN_USBCANFD_MINI ||
                type == Define.ZCAN_USBCANFD_800U;
            //true
            bool canfdDevice = usbCanfd || pcieCanfd;

            //设置ISO标准
            if (usbCanfd && (type != Define.ZCAN_USBCANFD_800U))
            {
                if (!SetISO(deviceHandle, channel_index, standard)) //设置CANFD标准
                {

                    return false;
                }
            }
            bool result = true;
            if (usbCanfd)
            {
                if (type == Define.ZCAN_USBCANFD_200U || type == Define.ZCAN_USBCANFD_400U || type == Define.ZCAN_USBCANFD_800U)
                {

                    if (!SetDataMerge(deviceHandle, channel_index_))
                    {
                        //MessageBox.Show("合并设置失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }
                result = SetFdBaudrate(deviceHandle, channel_index_, abaud, dbaud);
            }

            if (!result)
            {
                //MessageBox.Show("设置波特率失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            ZCAN_CHANNEL_INIT_CONFIG config_ = new ZCAN_CHANNEL_INIT_CONFIG();
            if (!netDevice)
            {
                config_.canfd.mode = (byte)canmode;
                if (usbCanfd)
                {
                    config_.can_type = Define.TYPE_CANFD;
                    config_.canfd.mode = 0;
                }
                else if (pcieCanfd)
                {
                    config_.can_type = Define.TYPE_CANFD;
                    config_.canfd.filter = 0;
                    config_.canfd.acc_code = 0;
                    config_.canfd.acc_mask = 0xFFFFFFFF;
                    config_.canfd.mode = 0;
                }
                else
                {
                    config_.can_type = Define.TYPE_CAN;
                    config_.can.filter = 0;
                    config_.can.acc_code = 0;
                    config_.can.acc_mask = 0xFFFFFFFF;
                    config_.can.mode = 0;
                }
            }
            IntPtr pConfig = Marshal.AllocHGlobal(Marshal.SizeOf(config_));
            Marshal.StructureToPtr(config_, pConfig, true);


            channel_handle_ = Method.ZCAN_InitCAN(deviceHandle, (uint)channel_index_, pConfig);
            Marshal.FreeHGlobal(pConfig);

            //if (canfdnetDevice)  //CANFDNET设备开启合并接收
            //{

            //    if (!setDataMerge())
            //    {
            //        MessageBox.Show("合并设置失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            //        return;
            //    }



            //}




            if (NULL == (int)channel_handle_)
            {
                //MessageBox.Show("初始化CAN失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            if (!netDevice)
            {
                if (usbCanfd)
                {
                    if (!SetResistanceEnable(deviceHandle, channel_index_, checkBox_resistance))
                    {
                        //MessageBox.Show("使能终端电阻失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                        return false;
                    }
                }

                if (usbCanfd && !SetFilter(deviceHandle, channel_index_))
                {
                    //MessageBox.Show("设置滤波失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    return false;
                }
                //if (type == Define.ZCAN_USBCAN_2E_U || type == Define.ZCAN_USBCAN_E_U)
                //{
                //    if (!setFilter())
                //    {
                //        MessageBox.Show("设置滤波失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                //        return;
                //    }

                //}

            }

            if (Method.ZCAN_StartCAN(channel_handle_) != Define.STATUS_OK)
            {
                //MessageBox.Show("启动CAN失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
             type = (uint)_deviceType;   //增加CANFDNET滤波,CANFDNET滤波必须在startcan之后进行。
             canfdnetDevice = type == Define.ZCAN_CANFDNET_400U_TCP || type == Define.ZCAN_CANFDNET_400U_UDP ||
                             type == Define.ZCAN_CANFDNET_200U_TCP || type == Define.ZCAN_CANFDNET_200U_UDP || type == Define.ZCAN_CANFDNET_800U_TCP ||
                             type == Define.ZCAN_CANFDNET_800U_UDP;
            if (canfdnetDevice && !SetFilter(_deviceHandle, channel_index_))
            {
                //MessageBox.Show("设置滤波失败", "提示", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }

            //button_start.Enabled = false;
            bool m_bStart = true;
            if (null == recv_data_thread_)
            {
                recv_data_thread_ = new recvdatathread();
                recv_data_thread_.setChannelHandle(channel_handle_);
                recv_data_thread_.setStart(m_bStart);
                recv_data_thread_.RecvCANData += this.AddData;
                recv_data_thread_.RecvFDData += this.AddData;
                recv_data_thread_.RecvData += this.AddData;  //合并接收
                recv_data_thread_.RecvLINData += this.AddData;
                recv_data_thread_.setDeviceHandle(_deviceHandle);

            }
            else
            {
                recv_data_thread_.setChannelHandle(channel_handle_);
                recv_data_thread_.setDeviceHandle(_deviceHandle);
            }


            if (checkBox_merge)
            {
                recv_data_thread_.setMerge_flag(1);//通知接收业务，开启合并接收 
            }
            else
            {
                recv_data_thread_.setMerge_flag(0);//通知接收业务，开启合并接收  
            }
            isStart = true;
            //listBox.Items.Clear();
            return true;
            //return true;
        }

       
        public void Close()
        {
            var temp = Method.ZCAN_ResetCAN(channel_handle_);
        }


        private void AddData(ZCAN_Receive_Data[] data, uint len)
        {
            string text = "";
            for (uint i = 0; i < len; ++i)
            {
                ZCAN_Receive_Data can = data[i];
                uint id = data[i].frame.can_id;
                string eff = IsEFF(id) ? "扩展帧" : "标准帧";
                string rtr = IsRTR(id) ? "远程帧" : "数据帧";
                text = String.Format("接收到CAN ID:0x{0:X8} {1:G} {2:G} 长度:{3:D1} 数据:", GetId(id), eff, rtr, can.frame.can_dlc);

                for (uint j = 0; j < can.frame.can_dlc; ++j)
                {
                    text = String.Format("{0:G}{1:X2} ", text, can.frame.data[j]);
                }
                Resvice?.Invoke(text);

                if(channel_index_== 0)
                {
                    CanFrame model = new CanFrame();
                    model.CreateTime = DateTime.Now;
                    model.IsExtended = IsEFF(id);
                    model.FrameType = IsRTR(id) ? 'r' : 'd';
                    model.Id = GetId(id);
                    model.Direction = "Rx";
                    model.Dlc = can.frame.can_dlc;
                    model.Channel = channel_index_ + 1;
                    model.Data = can.frame.data.Select(m => m).ToArray();
                    model.CanType = "CAN";
                    CommonMethod.AddFrameToFile(model);
                }
                


                ReviceData?.Invoke(GetId(id), can.frame.data);
                //lock (lock_obj)
                //{
                //    list_box_data_.Add(text);
                //}
                //TODO:得到数据
            }

            //Object[] list = { this, System.EventArgs.Empty };
            //this.listBox.BeginInvoke(new EventHandler(SetListBox), list);
        }

        private void AddData(ZCAN_ReceiveFD_Data[] data, uint len)
        {
            string text = "";
            for (uint i = 0; i < len; ++i)
            {
                ZCAN_ReceiveFD_Data canfd = data[i];
                uint id = data[i].frame.can_id;
                string eff = IsEFF(id) ? "扩展帧" : "标准帧";
                string rtr = IsRTR(id) ? "远程帧" : "数据帧";
                text = String.Format("接收到CANFD ID:0x{0:X8} {1:G} {2:G} 长度:{3:D1} 数据:", GetId(id), eff, rtr, canfd.frame.len);
                for (uint j = 0; j < canfd.frame.len; ++j)
                {
                    text = String.Format("{0:G}{1:X2} ", text, canfd.frame.data[j]);
                }
                var temp = BitConverter.ToString(canfd.frame.data).Replace("-", " ");

                Resvice?.Invoke(text);

                if (channel_index_ == 0)
                {
                    CanFrame model = new CanFrame();
                    model.CreateTime = DateTime.Now;
                    model.IsExtended = IsEFF(id);
                    model.FrameType = IsRTR(id) ? 'r' : 'd';
                    model.Id = GetId(id);
                    model.Direction = "Rx";
                    model.Dlc = canfd.frame.len;
                    model.Channel = channel_index_ + 1;
                    model.Data = canfd.frame.data.Select(m => m).ToArray();
                    model.CanType = "CANFD";
                    CommonMethod.AddFrameToFile(model);
                }
                

                ReviceData?.Invoke(GetId(id), canfd.frame.data);
                //LogHelper.WriteLog($"收到数据 {temp}");
                //if (temp.Contains("12 01 01 00 00 00 00 00 00 00 00 00"))
                //{
                //    Send(data:"12 A1 01 00 00 00 00 00 00 00 00 00 00 00 00 00");
                //    Send(data: "13 01 01 00 00 00 00 00 00 00 00 00 00 00 00 01");
                //    lock (this)
                //    {
                //        isendJg = false;
                //    }
                //}
                //lock (lock_obj)
                //{
                //    list_box_data_.Add(text);
                //}
                //TODO:得到数据
            }

            //Object[] list = { this, System.EventArgs.Empty };
            //this.listBox.BeginInvoke(new EventHandler(SetListBox), list);
        }

        private void AddData(ZCANDataObj[] data, uint len)
        {
            string text = "";
            for (uint i = 0; i < len; ++i)
            {
                ZCANDataObj data_obj = data[i];
                if (data_obj.dataType != 1)
                {
                    if (data_obj.dataType == 2)
                    {
                        text = "收到错误帧"; //如果不是CAN/CANFD报文就直接忽略 
                    }
                    if (data_obj.dataType == 5)
                    {
                        continue;
                        text = "收到总线利用率包";
                    }

                }
                else
                {
                    uint id = data[i].zcanCANFDData.frame.can_id;
                    string eff = IsEFF(id) ? "扩展帧" : "标准帧";
                    string rtr = IsRTR(id) ? "远程帧" : "数据帧";
                    string is_fd = Convert.ToBoolean(data[i].zcanCANFDData.flag & flag.CANFD_FLAG) ? "CANFD帧" : "CAN帧"; //按位与CANFD标志位
                    string is_send = Convert.ToBoolean(data[i].zcanCANFDData.flag & flag.TXECHOED_FLAG) ? "发送" : "接收到";//判断是否发送回显报文
                    text = String.Format("通道{0:D1} {1:G} {2:G} ID:0x{3:X8} {4:G} {5:G} 长度:{6:D1} 数据:", data_obj.chnl, is_send, is_fd, GetId(id), eff, rtr, data_obj.zcanCANFDData.frame.len);
                    for (uint j = 0; j < data_obj.zcanCANFDData.frame.len; ++j)
                    {
                        text = String.Format("{0:G}{1:X2} ", text, data_obj.zcanCANFDData.frame.data[j]);
                    }

                    ReviceData?.Invoke(GetId(id), data_obj.zcanCANFDData.frame.data);
                }
                Resvice?.Invoke(text);

                //lock (lock_obj)
                //{
                //    list_box_data_.Add(text);
                //}
            }

            //Object[] list = { this, System.EventArgs.Empty };
            //this.listBox.BeginInvoke(new EventHandler(SetListBox), list);
        }

        private void AddData(ZCAN_LIN_MSG[] data, uint len)
        {
            string text = "";
            for (uint i = 0; i < len; ++i)
            {
                ZCAN_LIN_MSG lin_data = data[i];
                byte chnl = lin_data.chnl;
                byte dataType = lin_data.dataType;
                byte id = (byte)(lin_data.zcanLINData.pid.rawVal & 0x3f); //去掉校验位，获取真实ID
                string dir_ = lin_data.zcanLINData.rxData.dir == 0 ? "接收" : "发送";
                byte datalen = lin_data.zcanLINData.rxData.datalen;
                if (dataType == 1)
                {
                    text = String.Format("LIN通道:{0:X1} 接收到LIN错误帧，可能当前报文头没有响应", lin_data.chnl);
                    Resvice?.Invoke(text);

                    //lock (lock_obj)
                    //{
                    //    list_box_data_.Add(text);
                    //}
                    continue;
                }
                else if (dataType == 2)
                {
                    ZCANLINEventData lin_event;
                    int size = Marshal.SizeOf(typeof(ZCAN_LIN_MSG));
                    IntPtr ptr = Marshal.AllocHGlobal(size);
                    Marshal.StructureToPtr(lin_data.zcanLINData, ptr, false);
                    lin_event = (ZCANLINEventData)Marshal.PtrToStructure(ptr, (typeof(ZCANLINEventData)));
                    text = String.Format("LIN通道:{0} 接收到唤醒报文 时间戳：{1}  事件类型：{2}", lin_data.chnl, lin_event.timeStamp, lin_event.type);
                    Resvice?.Invoke(text);
                    //lock (lock_obj)
                    //{
                    //    list_box_data_.Add(text);
                    //}
                    continue;
                }

                text = String.Format("LIN通道:{0:X1}  {1:G}LIN报文: ID:0x{2:X8}  长度:{3:D1} 数据:", lin_data.chnl, dir_, id, datalen);
                for (uint j = 0; j < datalen; ++j)
                {
                    text = String.Format("{0:G}{1:X2} ", text, lin_data.zcanLINData.rxData.data[j]);
                }
                Resvice?.Invoke(text);

                ReviceData?.Invoke(id, lin_data.zcanLINData.rxData.data);
                //lock (lock_obj)
                //{
                //    list_box_data_.Add(text);
                //}
            }

            //Object[] list = { this, System.EventArgs.Empty };
            //this.listBox.BeginInvoke(new EventHandler(SetListBox), list);
        }
        public bool IsEFF(uint id)//1:extend frame 0:standard frame
        {
            return !!Convert.ToBoolean((id & CAN_EFF_FLAG));
        }

        public bool IsRTR(uint id)//1:remote frame 0:data frame
        {
            return !!Convert.ToBoolean((id & CAN_RTR_FLAG));
        }

        public bool IsERR(uint id)//1:error frame 0:normal frame
        {
            return !!Convert.ToBoolean((id & CAN_ERR_FLAG));
        }

        public uint GetId(uint id)
        {
            return id & CAN_ID_FLAG;
        }

        public bool Send(uint id = 0x500, string data = "", bool isCanFd = true, bool canfdExp = true, int len = 16,bool lentype = false,int zhentype = 0)
        {
            if (isStart == false)
            {
                return false;
            }
            //LogHelper.WriteLog($"发送数据 id{id}-- data{data}");
            //isendJg = false;
            //Thread.Sleep(10);//每次发送等10毫秒
            if ((ConfigHelper.UdsParameters.jiesuo == "RC_PK2C" || ConfigHelper.UdsParameters.jiesuo == "RC_PK1B") && id > 0x7FF)
            {
                zhentype = 1;
            }
            int frame_type_index = zhentype;//枕类型0标准 1扩展
            //int protocol_index = 1;//协议类型 0 can 1   canfd
            int send_type_index = 0;//发送方式0 正常发送 1 单次发送 2自发自收 3单次自发自收
            int canfd_exp_index = canfdExp ? 1 : 0;//canfd加速
            uint result; //发送的帧数
            if (ConfigHelper.Cantype && lentype && isCanFd)
            {
                //canfd发送数据
                ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
                canfd_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
                canfd_data.frame.data = new byte[64];
                  canfd_data.frame.len = (byte)len;
                SplitData(data, ref canfd_data.frame.data, CANFD_MAX_DLEN);
                canfd_data.transmit_type = (uint)send_type_index;
                canfd_data.frame.flags = (byte)((canfd_exp_index != 0) ? CANFD_BRS : 0);
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_data));
                Marshal.StructureToPtr(canfd_data, ptr, true);
                result = Method.ZCAN_TransmitFD(channel_handle_, ptr, 1);

                if (channel_index_ == 0)
                {
                    CanFrame model = new CanFrame();
                    model.CreateTime = DateTime.Now;
                    model.IsExtended = IsEFF(id);
                    model.FrameType = IsRTR(id) ? 'r' : 'd';
                    model.Id = GetId(id);
                    model.Direction = "Tx";
                    model.Dlc = canfd_data.frame.len;
                    model.Channel = channel_index_ + 1;
                    model.Data = canfd_data.frame.data.Select(m => m).ToArray();
                    model.CanType = "CANFD";
                    CommonMethod.AddFrameToFile(model);
                }
                


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
                if(lentype)
                {
                    can_data.frame.can_dlc = (byte)len;
                    SplitData(data, ref can_data.frame.data, CAN_MAX_DLEN);
                }
                else
                {
                    can_data.frame.can_dlc = (byte)SplitData(data, ref can_data.frame.data, CAN_MAX_DLEN);
                }
                    
                can_data.transmit_type = (uint)send_type_index;
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
                Marshal.StructureToPtr(can_data, ptr, true);
                result = Method.ZCAN_Transmit(channel_handle_, ptr, 1);

                if(channel_index_== 0)
                {
                    CanFrame model = new CanFrame();
                    model.CreateTime = DateTime.Now;
                    model.IsExtended = IsEFF(id);
                    model.FrameType = IsRTR(id) ? 'r' : 'd';
                    model.Id = GetId(id);
                    model.Direction = "Tx";
                    model.Dlc = can_data.frame.can_dlc;
                    model.Channel = channel_index_ + 1;
                    model.Data = can_data.frame.data.Select(m => m).ToArray();
                    model.CanType = "CAN";
                    CommonMethod.AddFrameToFile(model);
                }
               

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

        public bool SendLBMS(uint id = 0x500, string data = "", bool isCanFd = true, bool canfdExp = true, int len = 16, bool lentype = false)
        {
            if (isStart == false)
            {
                return false;
            }
            //LogHelper.WriteLog($"发送数据 id{id}-- data{data}");
            //isendJg = false;
            //Thread.Sleep(10);//每次发送等10毫秒
            int frame_type_index = 0;//枕类型0标准 1扩展
            //int protocol_index = 1;//协议类型 0 can 1   canfd
            int send_type_index = 0;//发送方式0 正常发送 1 单次发送 2自发自收 3单次自发自收
            int canfd_exp_index = canfdExp ? 1 : 0;//canfd加速
            uint result; //发送的帧数
            if (ConfigHelper.Cantype && lentype && isCanFd)
            {
                //canfd发送数据
                ZCAN_TransmitFD_Data canfd_data = new ZCAN_TransmitFD_Data();
                canfd_data.frame.can_id = MakeCanId(id, frame_type_index, 0, 0);
                canfd_data.frame.data = new byte[64];
                canfd_data.frame.len = (byte)len;
                SplitData1(data, ref canfd_data.frame.data, CANFD_MAX_DLEN);
                canfd_data.transmit_type = (uint)send_type_index;
                canfd_data.frame.flags = (byte)((canfd_exp_index != 0) ? CANFD_BRS : 0);
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(canfd_data));
                Marshal.StructureToPtr(canfd_data, ptr, true);
                result = Method.ZCAN_TransmitFD(channel_handle_, ptr, 1);

                if (channel_index_ == 0)
                {
                    CanFrame model = new CanFrame();
                    model.CreateTime = DateTime.Now;
                    model.IsExtended = IsEFF(id);
                    model.FrameType = IsRTR(id) ? 'r' : 'd';
                    model.Id = GetId(id);
                    model.Direction = "Tx";
                    model.Dlc = canfd_data.frame.len;
                    model.Channel = channel_index_ + 1;
                    model.Data = canfd_data.frame.data.Select(m => m).ToArray();
                    model.CanType = "CANFD";
                    CommonMethod.AddFrameToFile(model);
                }



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
                if (lentype)
                {
                    can_data.frame.can_dlc = (byte)len;
                    SplitData(data, ref can_data.frame.data, CAN_MAX_DLEN);
                }
                else
                {
                    can_data.frame.can_dlc = (byte)SplitData(data, ref can_data.frame.data, CAN_MAX_DLEN);
                }

                can_data.transmit_type = (uint)send_type_index;
                IntPtr ptr = Marshal.AllocHGlobal(Marshal.SizeOf(can_data));
                Marshal.StructureToPtr(can_data, ptr, true);
                result = Method.ZCAN_Transmit(channel_handle_, ptr, 1);

                if (channel_index_ == 0)
                {
                    CanFrame model = new CanFrame();
                    model.CreateTime = DateTime.Now;
                    model.IsExtended = IsEFF(id);
                    model.FrameType = IsRTR(id) ? 'r' : 'd';
                    model.Id = GetId(id);
                    model.Direction = "Tx";
                    model.Dlc = can_data.frame.can_dlc;
                    model.Channel = channel_index_ + 1;
                    model.Data = can_data.frame.data.Select(m => m).ToArray();
                    model.CanType = "CAN";
                    CommonMethod.AddFrameToFile(model);
                }


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
        private uint MakeCanId(uint id, int eff, int rtr, int err)//1:extend frame 0:standard frame
        {
            uint ueff = (uint)(!!(Convert.ToBoolean(eff)) ? 1 : 0);
            uint urtr = (uint)(!!(Convert.ToBoolean(rtr)) ? 1 : 0);
            uint uerr = (uint)(!!(Convert.ToBoolean(err)) ? 1 : 0);
            return id | ueff << 31 | urtr << 30 | uerr << 29;
        }

        private int SplitData1(string data, ref byte[] transData, int maxLen)
        {
            string[] dataArray = data.Split(' ');
            for (int i = 0; (i < maxLen) && (i < dataArray.Length); i++)
            {
               

                
                  if (!string.IsNullOrEmpty(dataArray[i]))
                        transData[i] = Convert.ToByte(dataArray[i].Substring(0, 2), 16);
                
            }

            return dataArray.Length;
        }
        //拆分text到发送data数组
        private int SplitData(string data, ref byte[] transData, int maxLen)
        {
            string[] dataArray = data.Split(' ');
            for (int i = 0; (i < maxLen) && (i < dataArray.Length); i++)
            {
                if(!string.IsNullOrEmpty(dataArray[i]))
                    transData[i] = Convert.ToByte(dataArray[i].Substring(0, 2), 16);

               
            }

            return dataArray.Length;
        }
    }

    public class recvdatathread
    {
        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvCANDataEventHandler(ZCAN_Receive_Data[] data, uint len);//CAN数据接收事件委托

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvFDDataEventHandler(ZCAN_ReceiveFD_Data[] data, uint len);//CANFD数据接收事件委托

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvDataEventHandler(ZCANDataObj[] data, uint len);//CANFD数据接收事件委托

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RecvLINDataEventHandler(ZCAN_LIN_MSG[] data, uint len);//CANFD数据接收事件委托

        uint? uds_handle;
        const int TYPE_CAN = 0;
        const int TYPE_CANFD = 1;

        bool m_bStart;
        IntPtr lin_channel_handle_;
        IntPtr channel_handle_;
        IntPtr device_handle_;
        byte merge_ = 0;//初始不开启合并
        Thread recv_thread_;
        static object locker = new object();
        public RecvCANDataEventHandler OnRecvCANDataEvent;
        public RecvFDDataEventHandler OnRecvFDDataEvent;
        public RecvDataEventHandler OnRecvDataEvent;
        public RecvLINDataEventHandler OnRecvLINDataEvent;
        public recvdatathread()
        {
        }

        public event RecvCANDataEventHandler RecvCANData
        {
            add { OnRecvCANDataEvent += new RecvCANDataEventHandler(value); }
            remove { OnRecvCANDataEvent -= new RecvCANDataEventHandler(value); }
        }

        public event RecvFDDataEventHandler RecvFDData
        {
            add { OnRecvFDDataEvent += new RecvFDDataEventHandler(value); }
            remove { OnRecvFDDataEvent -= new RecvFDDataEventHandler(value); }
        }

        public event RecvDataEventHandler RecvData
        {
            add { OnRecvDataEvent += new RecvDataEventHandler(value); }
            remove { OnRecvDataEvent -= new RecvDataEventHandler(value); }
        }


        public event RecvLINDataEventHandler RecvLINData
        {
            add { OnRecvLINDataEvent += new RecvLINDataEventHandler(value); }
            remove { OnRecvLINDataEvent -= new RecvLINDataEventHandler(value); }
        }

        public void setStart(bool start)
        {

            m_bStart = start;
            if (start)
            {
                recv_thread_ = new Thread(RecvDataFunc);
                recv_thread_.IsBackground = true;
                recv_thread_.Start();
            }
            else
            {
                recv_thread_.Join();
                recv_thread_ = null;
            }
        }

        public void SetUDS_Handle(uint? _uds_handle)
        {
            uds_handle = _uds_handle;
        }

        public void setChannelHandle(IntPtr channel_handle)
        {
            lock (locker)
            {
                channel_handle_ = channel_handle;
            }
        }

        public void setDeviceHandle(IntPtr device_handle)
        {
            lock (locker)
            {
                device_handle_ = device_handle;
            }
        }


        public void setMerge_flag(Byte merge)
        {
            lock (locker)
            {
                merge_ = merge;
            }
        }


        public void setLINChannelHandle(IntPtr lin_channel_handle)
        {
            lock (locker)
            {
                lin_channel_handle_ = lin_channel_handle;
            }
        }


        //数据接收函数
        protected void RecvDataFunc()
        {
            ZCAN_Receive_Data[] can_data = new ZCAN_Receive_Data[10000];
            ZCAN_ReceiveFD_Data[] canfd_data = new ZCAN_ReceiveFD_Data[10000];
            ZCANDataObj[] data_obj = new ZCANDataObj[10000];
            ZCAN_LIN_MSG[] lin_data = new ZCAN_LIN_MSG[10000];
            uint len = 0;
            while (m_bStart)
            {
                lock (locker)
                {
                    if (merge_ != 1)
                    { //分开接收

                        len = Method.ZCAN_GetReceiveNum(channel_handle_, TYPE_CAN);

                        if (len > 0)
                        {
                            int size = Marshal.SizeOf(typeof(ZCAN_Receive_Data));
                            IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                            len = Method.ZCAN_Receive(channel_handle_, ptr, 100, 50);
                            for (int i = 0; i < len; ++i)
                            {
                                can_data[i] = (ZCAN_Receive_Data)Marshal.PtrToStructure(
                                    (IntPtr)((Int64)ptr + i * size), typeof(ZCAN_Receive_Data));
                            }
                            OnRecvCANDataEvent(can_data, len);



                            if (uds_handle != null)
                            {
                                ZCAN_Receive_Data zcanReceiveData = new ZCAN_Receive_Data();
                                var RecNum = len;
                                IntPtr P2zcanReceiveData = Marshal.AllocHGlobal(Marshal.SizeOf(zcanReceiveData) * (int)RecNum);
                                byte[] zero = new byte[Marshal.SizeOf(zcanReceiveData) * (int)RecNum];
                                Marshal.Copy(zero, 0, P2zcanReceiveData, Marshal.SizeOf(zcanReceiveData) * (int)RecNum);

                                uint ReceiveNum = Method.ZCAN_Receive(channel_handle_, P2zcanReceiveData, RecNum, 10);
                                if (ReceiveNum > 0)
                                {
                                    for (int i = 0; i < ReceiveNum; i++)
                                    {
                                        zcanReceiveData = (ZCAN_Receive_Data)Marshal.PtrToStructure(P2zcanReceiveData + i * Marshal.SizeOf(typeof(ZCAN_Receive_Data)), typeof(ZCAN_Receive_Data));
                                        //Console.WriteLine("{0}:{1}", zcanReceiveData.timestamp, b_array2string(zcanReceiveData.frame.data, zcanReceiveData.frame.can_dlc));
                                    }

                                    //ZLGUDSMethod.ZUDS_OnReceive(uds_handle.Value, ZLGUDSMethod.TransferCANFrame(P2zcanReceiveData, ReceiveNum, 0));//与发送线程中得request函数相配合。
                                }
                                Marshal.FreeHGlobal(P2zcanReceiveData);
                            }
                            Marshal.FreeHGlobal(ptr);
                        }

                        len = Method.ZCAN_GetReceiveNum(channel_handle_, TYPE_CANFD);
                        if (len > 0)
                        {
                            int size = Marshal.SizeOf(typeof(ZCAN_ReceiveFD_Data));
                            IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                            len = Method.ZCAN_ReceiveFD(channel_handle_, ptr, 100, 50);
                            for (int i = 0; i < len; ++i)
                            {
                                canfd_data[i] = (ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(
                                    (IntPtr)((Int64)ptr + i * size), typeof(ZCAN_ReceiveFD_Data));
                            }
                            OnRecvFDDataEvent(canfd_data, len);

                            if (uds_handle != null)
                            {
                                ZCAN_ReceiveFD_Data zcanReceiveFDData = new ZCAN_ReceiveFD_Data();
                                canfd_frame canfd_Frame = new canfd_frame();
                                zcanReceiveFDData.frame = canfd_Frame;

                                IntPtr P2zcanReceiveFDData = Marshal.AllocHGlobal(80);
                                byte[] zero = new byte[80];
                                Marshal.Copy(zero, 0, P2zcanReceiveFDData, 80);

                                uint ReceiveNum = Method.ZCAN_ReceiveFD(channel_handle_, P2zcanReceiveFDData, 1, 10);
                                if (ReceiveNum > 0)
                                {
                                    for (int i = 0; i < ReceiveNum; i++)
                                    {
                                        zcanReceiveFDData = (ZCAN_ReceiveFD_Data)Marshal.PtrToStructure(P2zcanReceiveFDData + i * Marshal.SizeOf(typeof(ZCAN_ReceiveFD_Data)), typeof(ZCAN_ReceiveFD_Data));

                                    }
                                    //ZLGUDSMethod.ZUDS_OnReceive(uds_handle.Value, ZLGUDSMethod.TransferCANFrame(P2zcanReceiveFDData, ReceiveNum, 1));
                                }
                                Marshal.FreeHGlobal(P2zcanReceiveFDData);
                            }

                            Marshal.FreeHGlobal(ptr);
                        }

                        {
                            int size = Marshal.SizeOf(typeof(ZCAN_LIN_MSG));
                            IntPtr ptr = Marshal.AllocHGlobal(50 * size);
                            len = Method.ZCAN_ReceiveLIN(lin_channel_handle_, ptr, 50, 0);
                            if (len > 0)
                            {
                                for (int i = 0; i < len; ++i)
                                {
                                    lin_data[i] = (ZCAN_LIN_MSG)Marshal.PtrToStructure(
                                        (IntPtr)((Int64)ptr + i * size), typeof(ZCAN_LIN_MSG));
                                }
                                OnRecvLINDataEvent(lin_data, len);

                            }
                            Marshal.FreeHGlobal(ptr);
                        }
                    }
                    else
                    { //合并接收
                        len = Method.ZCAN_GetReceiveNum(channel_handle_, 2); //合并接收类型type为2
                        if (len > 0)
                        {
                            int size = Marshal.SizeOf(typeof(ZCANDataObj));
                            IntPtr ptr = Marshal.AllocHGlobal((int)100 * size);
                            len = Method.ZCAN_ReceiveData(device_handle_, ptr, 100, 50);         //传设备的句柄
                            for (int i = 0; i < len; ++i)
                            {
                                data_obj[i] = (ZCANDataObj)Marshal.PtrToStructure(
                                    (IntPtr)((Int64)ptr + i * size), typeof(ZCANDataObj));
                            }
                            OnRecvDataEvent(data_obj, len);
                            Marshal.FreeHGlobal(ptr);
                        }

                    }
                }


                Thread.Sleep(1);
            }
        }
    }
}
