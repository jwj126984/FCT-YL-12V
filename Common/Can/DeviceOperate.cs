using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Can
{
    public class DeviceOperate
    {
        DeviceType _device_type;
        IntPtr device_handle_;
        const int NULL = 0;
        bool isOpen = false;

        public bool IsOpen
        {
            get { return isOpen; }
        }
        public DeviceType GetDeviceType
        {
            get
            {
                if (!isOpen) return DeviceType.Defult;
                return _device_type;
            }
        }
        public IntPtr GetDeviceHandle
        {
            get { if (!isOpen) return IntPtr.Zero; return device_handle_; }

        }

        /// <summary>
        /// 打开设备
        /// </summary>
        /// <returns></returns>
        public bool OpenDevice(DeviceType device_type = DeviceType.ZCAN_USBCANFD_200U, uint device_index = 0)
        {
            device_handle_ = Method.ZCAN_OpenDevice((uint)device_type, device_index, 0);
            if (NULL == (int)device_handle_)
            {
                //MessageBox.Show("打开设备失败,请检查设备类型和设备索引号是否正确", "提示",
                //        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return false;
            }
            if ((uint)device_type == 41)
            {
                string path = "0/get_cn/1";
                byte[] sn_ = new byte[30];
                IntPtr sn = Method.ZCAN_GetValue(device_handle_, path);
                Marshal.Copy(sn, sn_, 0, 30);
            }

            isOpen = true;
            _device_type = device_type;
            return true;

        }

        public bool CloseDevice()
        {

            if (isOpen)
            {
                Method.ZCAN_CloseDevice(device_handle_);
            }
            if (Method.ZCLOUD_IsConnected())
            {
                Method.ZCLOUD_DisconnectServer();
            }
            return true;
        }
    }

    public enum DeviceType
    {
        TYPE_CAN = 0,
        TYPE_CANFD = 1,
        ZCAN_USBCAN1 = 3,
        ZCAN_USBCAN2 = 4,
        ZCAN_PCI9820I = 16,
        ZCAN_CANETUDP = 12,
        ZCAN_CANETTCP = 17,
        ZCAN_CANWIFI_TCP = 25,
        ZCAN_USBCAN_E_U = 20,
        ZCAN_USBCAN_2E_U = 21,
        ZCAN_USBCAN_4E_U = 31,
        ZCAN_PCIECANFD_100U = 38,
        ZCAN_PCIECANFD_200U = 39,
        ZCAN_PCIECANFD_200U_EX = 62,
        ZCAN_PCIECANFD_400U = 61,
        ZCAN_USBCANFD_200U = 41,
        ZCAN_USBCANFD_400U = 76,
        ZCAN_USBCANFD_100U = 42,
        ZCAN_USBCANFD_MINI = 43,
        ZCAN_USBCANFD_800U = 59,
        ZCAN_CLOUD = 46,
        ZCAN_CANFDNET_200U_TCP = 48,
        ZCAN_CANFDNET_200U_UDP = 49,
        ZCAN_CANFDNET_400U_TCP = 52,
        ZCAN_CANFDNET_400U_UDP = 53,
        ZCAN_CANFDNET_800U_TCP = 57,
        ZCAN_CANFDNET_800U_UDP = 58,
        STATUS_ERR = 0,
        STATUS_OK = 1,
        Defult = -1,
    }
}
