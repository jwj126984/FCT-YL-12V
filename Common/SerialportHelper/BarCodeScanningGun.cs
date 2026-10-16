using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.SerialportHelper
{
    public class BarCodeScanningGun
    {
        private readonly string _endMarker = "\r"; // 结束标志
        public event Action<List<byte>> Received;
        SerialPort serialPort = new SerialPort();
        public BarCodeScanningGun(string portName)
        {
            //if (CommonData.IsDebug)
            //{
            //    return;
            //}
            try
            {
                //serialPort.DtrEnable = true;
                //serialPort.RtsEnable = true;
                //serialPort.ReceivedBytesThreshold = 0;
                serialPort.PortName = portName;
                serialPort.BaudRate = 115200;//波特率
                //serialPort.DataBits = int.Parse("8");//设置数据位
                serialPort.Encoding = Encoding.Default;
                serialPort.Open();

                int lenTemp = serialPort.BytesToRead;//获取可以读取的字节数
                if (lenTemp > 0)
                {
                    byte[] buff = new byte[lenTemp];//创建缓存数据数组
                    serialPort.Read(buff, 0, lenTemp);//把数据读取到buff数组
                }

                Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            await Task.Delay(100);
                            int len = serialPort.BytesToRead;//获取可以读取的字节数
                            if (len > 0)
                            {
                                byte[] buff = new byte[len];//创建缓存数据数组
                                serialPort.Read(buff, 0, len);//把数据读取到buff数组
                                Received?.Invoke(buff.ToList());
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.ShowMsg(ex.ToString());
                    }

                });
            }
            catch (Exception err)
            {
                LogHelper.ShowMsg($"打开扫码枪失败" + err.ToString());
                throw new Exception(err.Message);
            }
        }

        public void Close()
        {
            if (serialPort != null && serialPort.IsOpen)
            {
                serialPort.Close();
            }

        }

        /// <summary>
        /// 发送消息并等待回复
        /// </summary>
        /// <param name="message">发送的消息</param>
        /// <param name="expectResponse">是否等待回复</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <exception cref="TimeoutException"></exception>
        public string SendAndWaitForResponse()
        {
            if (!serialPort.IsOpen)
            {
                throw new InvalidOperationException("串口未打开");
            }

            string result = "";
            StringBuilder _receiveBuffer = new StringBuilder();
            ManualResetEvent responseReceivedEvent = new ManualResetEvent(false);
            responseReceivedEvent.Reset();
            serialPort.DataReceived += serialPort_DataReceived;


            var sendData = new List<byte>() { 0x04, 0xE4, 0x04, 0x00, 0xFF, 0x14 };
            // 发送数据
            serialPort.Write(sendData.ToArray(), 0, sendData.Count);
            // 等待接收到完整响应
            if (responseReceivedEvent.WaitOne(CommonData.AwaitTime * 1000))
            {
                // 等待接收到完整响应
                if (responseReceivedEvent.WaitOne(CommonData.AwaitTime * 1000))
                    serialPort.DataReceived -= serialPort_DataReceived;
                return result.Trim();
            }
            else
            {
                serialPort.DataReceived -= serialPort_DataReceived;
                throw new TimeoutException("超时");
            }

            void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                try
                {
                    // 读取串口数据
                    string data = serialPort.ReadExisting();
                    _receiveBuffer.Append(data);

                    // 检查是否包含结束标志
                    if (_receiveBuffer.ToString().Contains(_endMarker))
                    {
                        // 提取完整数据
                        int endIndex = _receiveBuffer.ToString().IndexOf(_endMarker);
                        result = _receiveBuffer.ToString(0, endIndex + _endMarker.Length);

                        // 清理缓冲区已处理部分
                        _receiveBuffer.Remove(0, endIndex + _endMarker.Length);

                        // 通知主线程响应已接收
                        responseReceivedEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in DataReceived: {ex.Message}");
                }
            }
        }
    }
}
