using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    internal class PlayEnum
    {
    }
    /// <summary>
    /// 字典类型
    /// </summary>
    public enum KeyType
    {
        None = 0,
        /// <summary>
        /// 串口类型
        /// </summary>
        SerialPort = 1,
        /// <summary>
        /// CAN类型
        /// </summary>
        Can = 2,
    }
    public enum SaveTimeEnum
    {
        一个月=1,
        三个月=3,
        六个月=6,
        一年=12,
        两年=24,
        三年=36,
        六年=72
    }
}
