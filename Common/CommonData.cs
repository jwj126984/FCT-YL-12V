using AllData;
using Common.Can;
using Common.SerialportHelper;
using Model;
using Models_C.Dal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    /// <summary>
    /// 存放公共临时数据
    /// </summary>
    public static class CommonData
    {

        public static ManualResetEvent PlayThreadControlEvent { get; private set; } = new ManualResetEvent(false);
        /// <summary>
        /// 板子名称，解锁用
        /// </summary>
        public static string BoardName
        {
            get
            {
                if (CurrentUser == null)
                    return "SERES_F3";
                return ConfigHelper.UdsParameters.jiesuo;
            }
        }

        /// <summary>
        /// E171_E371和E245_3p0用 5 塞力斯用1
        /// </summary>
        public static byte SafeLevel { get; set; } = 5;
      
        /// <summary>
        /// UDS发送ID
        /// </summary>
        public static uint TID 
        { 
            get
            {
                return (uint)(ConfigHelper.SelectType == "seres" ? 0x775 : ConfigHelper.SelectType == "lt" ? 0x74E : 0x775);
            }
        }
        /// <summary>
        /// UDS读取ID
        /// </summary>
        public static uint RID
        {
            get
            {
                return (uint)(ConfigHelper.SelectType == "seres" ? 0x77D : ConfigHelper.SelectType == "lt" ? 0x7CE : 0x77D);
            }
        }
        /// <summary>
        /// 一级休眠
        /// </summary>
        public static List<byte> UDSSleep1
        {
            get
            {
                switch (ConfigHelper.SelectType)
                {
                    case "seres":
                        return new List<byte> { 0x61, 0x03, 0x03, 0x01 };
                    case "lt":
                        return new List<byte> { 0x34, 0x03, 0x03, 0x0B };
                    default:
                        return new List<byte> { 0x61, 0x03, 0x03, 0x01 };
                }
            }
        }
        /// <summary>
        /// 二级休眠
        /// </summary>
        public static List<byte> UDSSleep2
        {
            get
            {
                switch (ConfigHelper.SelectType)
                {
                    case "seres":
                        return new List<byte> { 0x61, 0x03, 0x03, 0x02 };
                    case "lt":
                        return new List<byte> { 0x34, 0x03, 0x03, 0x02 };
                    default:
                        return new List<byte> { 0x61, 0x03, 0x03, 0x02 };
                }
            }
        }
        /// <summary>
        /// 启动限流
        /// </summary>
        public static List<byte> StartLimit
        {
            get
            {
                switch (ConfigHelper.SelectType)
                {
                    case "seres":
                        return new List<byte> { 0x61, 0x01, 0x03 };
                    case "lt":
                        return new List<byte> { 0x34, 0x01, 0x03 };
                    default:
                        return new List<byte> { 0x61, 0x01, 0x03 };
                }
            }
        }
        /// <summary>
        /// 结束限流
        /// </summary>
        public static List<byte> StopLimit
        {
            get
            {
                switch (ConfigHelper.SelectType)
                {
                    case "seres":
                        return new List<byte> { 0x61, 0x01, 0x00 };
                    case "lt":
                        return new List<byte> { 0x34, 0x01, 0x00 };
                    default:
                        return new List<byte> { 0x61, 0x01, 0x00 };
                }
            }
        }

        /// <summary>
        /// 小电源电压偏差值 1V
        /// </summary>
        public static double PowerVoltageOffset { get; private set; } = 0.01;
        /// <summary>
        /// 大电源电流偏差值 1A
        /// </summary>
        public static double PowerCurrentOffset { get; private set; } = 0.01;
        /// <summary>
        /// 用户身份
        /// </summary>
        public static Role UserRole
        {
            get
            {
                if (CurrentUser == null)
                    return Role.None;
                return (Role)CurrentUser.UserType;
            }
        }
        /// <summary>
        /// 登录用户
        /// </summary>
        public static Sys_User CurrentUser { get; set; }
        /// <summary>
        /// 测试数据
        /// </summary>
        public static List<TestResult> TestDatas { get; set; }
        /// <summary>
        /// 当前测试模式
        /// </summary>
        public static TestType CurrentTestType
        {
            get
            {
                if (CurrentUser == null)
                    return TestType.None;
                return (TestType)CurrentUser.TestType;
            }
        }
        /// <summary>
        /// 当前测试了多少项 一次测试完成清空
        /// </summary>
        public static int CurrentCount { get; set; } = 0;
        #region 报告用到了的属性
        /// <summary>
        /// 当前测试板子二维码
        /// </summary>
        public static string CurrentBarCode { get; set; } = "";
        public static DateTime StartTime { get; set; }

        public static DateTime EndTime { get; set; }
        #endregion
        public static CanOperate UDS { get; set; }

        /// <summary>
        /// 超时时间
        /// </summary>
        public static int AwaitTime { get; set; } = 3;
        /// <summary>
        /// 工作电流阈值
        /// </summary>
        public static double WorkCurrentThreshold { get;private set; } = 50;

        /// <summary>
        /// 一级休眠电流阈值
        /// </summary>
        public static double Sleep1CurrentThreshold { get; private set; } = 1;
        /// <summary>
        /// 二级休眠电流阈值
        /// </summary>
        public static double Sleep2CurrentThreshold { get; private set; } = 1;

        /// <summary>
        /// 是E171 还是E245
        /// </summary>
        public static bool Is171
        {
            get
            {
                return BoardName == "E171_E371";
            }
        }

    

        /// <summary>
        /// 是否在测试中
        /// </summary>
        public static bool IsTesting { get; set; } = false;
        /// <summary>
        /// 测试了多少项
        /// </summary>
        public static int TestCount { get; set; } = 0;  
        /// <summary>
        /// 软件版本
        /// </summary>
        public static string Vision { get; private set; } = "V1.0";

        #region 数据库加载的数据
        /// <summary>
        /// 配置文件数据
        /// </summary>
        public static IEnumerable<Key_Val> Key_Vals { get; set; }
        /// <summary>
        /// 测试参数
        /// </summary>
        public static IEnumerable<TestParameter> TestParameters { get; set; }
        #endregion

        #region 当前用到的硬件的实例等
        /// <summary>
        /// IT6122B
        /// </summary>
        public static PowerHelperUSB IT6122B { get; set; }

        /// <summary>
        /// M3905D
        /// </summary>
        public static PowerHelperUSB M3905D { get; set; }
        /// <summary>
        /// 电芯模拟器 N8331
        /// </summary>
        public static N8331 N8331 { get; set; }
        /// <summary>
        /// 直流电流电压测量卡
        /// </summary>
        public static NXI_6400 NXI_6400 { get; set; }
        /// <summary>
        /// 第二块直流电流电压测量卡
        /// </summary>
        public static NXI_6400 NXI_6400_2 { get; set; }
        /// <summary>
        /// 扫码枪
        /// </summary>
        public static BarCodeScanningGun Gun { get; set; }
        /// <summary>
        /// IT6322 电源
        /// </summary>
        public static IT6322 IT6322 { get; set; }
        /// <summary>
        /// N69206H 电子负载
        /// </summary>
        public static N69206H N69206H { get; set; }
        /// <summary>
        /// N36200 系列 / N36300 系列 可编程直流电源
        /// </summary>
        public static N36200 N36200 { get; set; }
        /// <summary>
        /// 继电器控制板（共用一个串口）
        /// </summary>
        public static RelayHelper Relay { get; set; }

        /// <summary>
        /// 治具控制卡（独立串口）
        /// </summary>
        public static ToolControlHelper ToolControl { get; set; }
        #endregion

        /// <summary>
        /// 当前人员的可以使用的测试流程以及其中文显示名称
        /// </summary>
        public static List<(string, string)> CurrentUserDetects
        {
            get
            {
                List<(string, string)> result = new List<(string, string)>();
                if (CurrentUser.TestItem == null)
                    return result;
                foreach (var item in CurrentUser.TestItem.Split(','))
                {

                    result.Add((item, item.GetShowTxt()));
                }
                return result;
            }
        }

        /// <summary>
        /// 测试结果
        /// </summary>
        public static bool TestResult { get; set; } = false;

        static string DirPath = "C:\\YC\\";

        public static string FrameDirPath
        {
            get
            {
                if (System.IO.Directory.Exists(DirPath) == false)
                {
                    System.IO.Directory.CreateDirectory(DirPath);
                }
                if (System.IO.Directory.Exists($"{DirPath}{DateTime.Now.Date.ToString("yyyy年MM月dd日")}\\") == false)
                {
                    System.IO.Directory.CreateDirectory($"{DirPath}{DateTime.Now.Date.ToString("yyyy年MM月dd日")}\\");
                }
                if (System.IO.Directory.Exists($"{DirPath}{DateTime.Now.Date.ToString("yyyy年MM月dd日")}\\赛力斯\\") == false)
                {
                    System.IO.Directory.CreateDirectory($"{DirPath}{DateTime.Now.Date.ToString("yyyy年MM月dd日")}\\赛力斯\\");
                }
                return $"{DirPath}{DateTime.Now.Date.ToString("yyyy年MM月dd日")}\\赛力斯\\";
            }
        }

        public static DateTime CanStartTime { get; set; } = DateTime.MinValue;

        public static DateTime LinStartTime { get; set; } = DateTime.MinValue;
    }
}
