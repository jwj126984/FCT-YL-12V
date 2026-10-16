using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models_C
{
    public class CanFrame
    {
        public DateTime CreateTime { get; set; }
        public double TimeStamp { get; set; }
        public int Channel { get; set; }
        public uint Id { get; set; }
        public bool IsExtended { get; set; }  // true表示扩展帧
        /// <summary>
        /// 方向
        /// </summary>
        public string Direction { get; set; }
        /// <summary>
        ///  'd'（数据帧）或 'r'（远程帧）
        /// </summary>
        public char FrameType { get; set; }
        /// <summary>
        /// 数据长度
        /// </summary>
        public int Dlc { get; set; }
        public byte[] Data { get; set; }

        public string CanType { get; set; }
    }
}
