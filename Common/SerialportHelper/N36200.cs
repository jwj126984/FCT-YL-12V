using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Common.CommonMethod;

namespace Common.SerialportHelper
{
    /// <summary>
    /// 恩智 NGI N36200 系列 / N36300 系列 可编程直流电源 SCPI 通讯类（LAN / RS232 / RS485）
    /// 串口出厂默认波特率 115200，命令结束符为换行符 LF
    /// </summary>
    public class N36200
    {
        public string PowerType { get; set; } = "N36200";
        private SerialPort _serialPort;
        private readonly string _endMarker = "\n";

        public N36200(string portName)
        {
            _serialPort = new SerialPort(portName);
            _serialPort.BaudRate = 115200;
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            _serialPort.Open();
        }

        public void Close()
        {
            if (_serialPort != null && _serialPort.IsOpen)
            {
                _serialPort.Close();
            }
        }

        /// <summary>
        /// 发送 SCPI 命令并等待回复
        /// </summary>
        public string SendAndWaitForResponse(string message, bool expectResponse = true, int cf = 0)
        {
            CommonMethod.Sleep(100);
            cf++;
            if (!_serialPort.IsOpen)
            {
                throw new InvalidOperationException($"{PowerType}串口未打开或者连接出现问题，请检查连接线以及数据库配置后重启软件");
            }

            string result = "";
            StringBuilder _receiveBuffer = new StringBuilder();
            ManualResetEvent responseReceivedEvent = new ManualResetEvent(false);
            responseReceivedEvent.Reset();
            _serialPort.DataReceived += serialPort_DataReceived;

            _serialPort.WriteLine(message);

            if (!expectResponse)
            {
                _serialPort.DataReceived -= serialPort_DataReceived;
                return string.Empty;
            }

            if (responseReceivedEvent.WaitOne((int)(CommonData.AwaitTime * 1000)))
            {
                _serialPort.DataReceived -= serialPort_DataReceived;
                return result.Trim();
            }
            else
            {
                if (cf > 2)
                {
                    _serialPort.DataReceived -= serialPort_DataReceived;
                    throw new TimeoutException($"{PowerType}超时，请检查");
                }
                else
                {
                    return SendAndWaitForResponse(message, expectResponse, cf);
                }
            }

            void serialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
            {
                try
                {
                    string data = _serialPort.ReadExisting();
                    _receiveBuffer.Append(data);

                    if (_receiveBuffer.ToString().Contains(_endMarker))
                    {
                        int endIndex = _receiveBuffer.ToString().IndexOf(_endMarker);
                        result = _receiveBuffer.ToString(0, endIndex + _endMarker.Length);
                        _receiveBuffer.Remove(0, endIndex + _endMarker.Length);
                        responseReceivedEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"{PowerType}Error in DataReceived: {ex.Message}");
                }
            }
        }

        #region IEEE 488.2 公共命令
        /// <summary>
        /// 恢复出厂设置（会重置电压、电流、保护、序列、充电等全部参数，耗时约10S）
        /// </summary>
        public bool Reset()
        {
            LogHelper.Debug($"{PowerType}执行 *RST 恢复出厂设置");
            SendAndWaitForResponse("*RST", false);
            return true;
        }

        /// <summary>
        /// 清除标准事件状态寄存器和错误队列
        /// </summary>
        public bool ClearErrors()
        {
            LogHelper.Debug($"{PowerType}执行 *CLS 清除错误");
            SendAndWaitForResponse("*CLS", false);
            return true;
        }

        /// <summary>
        /// 读取设备信息（制造商,型号,SN,版本号），如 NGI,N36216-80-42,N20250401,V1.62
        /// </summary>
        public string GetIdentification()
        {
            var str = SendAndWaitForResponse("*IDN?");
            LogHelper.Debug($"{PowerType}发送命令：*IDN?，回复：{str}");
            return str;
        }

        /// <summary>
        /// 检测设备连接是否正常
        /// </summary>
        public bool CheckConnect()
        {
            try
            {
                var id = GetIdentification();
                return !string.IsNullOrEmpty(id);
            }
            catch
            {
                return false;
            }
        }
        #endregion

        #region 设定输出电压与限流值（3.2）
        /// <summary>
        /// 设定输出电压值（0 ~ 额定电压）
        /// </summary>
        public bool SetVoltage(double voltage)
        {
            LogHelper.Debug($"{PowerType}设定输出电压: {voltage} V");
            SendAndWaitForResponse($"SOURce:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置输出限流值（0 ~ 额定电流）
        /// </summary>
        public bool SetCurrent(double current)
        {
            LogHelper.Debug($"{PowerType}设置输出限流值: {current} A");
            SendAndWaitForResponse($"SOURce:CURRent {current}", false);
            return true;
        }

        /// <summary>
        /// 设置输出内阻值（0 ~ 100000 mΩ）
        /// </summary>
        public bool SetInternalRes(double resistance)
        {
            LogHelper.Debug($"{PowerType}设置输出内阻值: {resistance} mΩ");
            SendAndWaitForResponse($"SOURce:INTErnalres {resistance}", false);
            return true;
        }

        /// <summary>
        /// 保存测试参数（1-10）
        /// </summary>
        public bool SaveFunction(int index)
        {
            LogHelper.Debug($"{PowerType}保存测试参数到: {index}");
            SendAndWaitForResponse($"SOURce:FUNCtion:SAVe {index}", false);
            return true;
        }

        /// <summary>
        /// 调用测试参数（1-10）
        /// </summary>
        public bool RecallFunction(int index)
        {
            LogHelper.Debug($"{PowerType}调用测试参数: {index}");
            SendAndWaitForResponse($"SOURce:FUNCtion:RECAll {index}", false);
            return true;
        }
        #endregion

        #region 输出控制及状态（3.3）
        /// <summary>
        /// 控制电源输出开关（ON / OFF）
        /// </summary>
        public bool SetOutput(bool enable)
        {
            LogHelper.Debug($"{PowerType}控制电源输出: {(enable ? "ON" : "OFF")}");
            SendAndWaitForResponse($"OUTPut:ONOFF {(enable ? "ON" : "OFF")}", false);
            return true;
        }

        /// <summary>
        /// 设定电源工作模式：NORMal 普通 | CHARge 电池充电 | SEQuence 序列 | CPOWer 恒功率 | CARWave 汽车测试 | APG 外部编程
        /// </summary>
        public bool SetOutputMode(string mode)
        {
            LogHelper.Debug($"{PowerType}设定电源工作模式: {mode}");
            SendAndWaitForResponse($"OUTPut:MODE {mode}", false);
            return true;
        }

        /// <summary>
        /// 设置电源进入普通模式
        /// </summary>
        public bool SetNormalMode()
        {
            return SetOutputMode("NORMal");
        }

        /// <summary>
        /// 设定电源电压上升时间（s）
        /// </summary>
        public bool SetVoltageRiseTime(double time)
        {
            LogHelper.Debug($"{PowerType}设定电压上升时间: {time} s");
            SendAndWaitForResponse($"OUTPut:VOLRisetime {time}", false);
            return true;
        }

        /// <summary>
        /// 设定电源电压下降时间（s）
        /// </summary>
        public bool SetVoltageFallTime(double time)
        {
            LogHelper.Debug($"{PowerType}设定电压下降时间: {time} s");
            SendAndWaitForResponse($"OUTPut:VOLFalltime {time}", false);
            return true;
        }

        /// <summary>
        /// 设定电源电流上升时间（s）
        /// </summary>
        public bool SetCurrentRiseTime(double time)
        {
            LogHelper.Debug($"{PowerType}设定电流上升时间: {time} s");
            SendAndWaitForResponse($"OUTPut:CURRisetime {time}", false);
            return true;
        }

        /// <summary>
        /// 设定电源电流下降时间（s）
        /// </summary>
        public bool SetCurrentFallTime(double time)
        {
            LogHelper.Debug($"{PowerType}设定电流下降时间: {time} s");
            SendAndWaitForResponse($"OUTPut:CURFalltime {time}", false);
            return true;
        }

        /// <summary>
        /// 设置电源 CV / CC 优先权（1|2|CV|CC）
        /// </summary>
        public bool SetPriority(string priority)
        {
            LogHelper.Debug($"{PowerType}设置输出优先权: {priority}");
            SendAndWaitForResponse($"OUTPut:PRIority {priority}", false);
            return true;
        }

        /// <summary>
        /// 获取电源输出状态（Bit0 ON/OFF、Bit1 告警、Bit2 CV/CC 环）
        /// </summary>
        public string GetOutputState()
        {
            var str = SendAndWaitForResponse("OUTPut:STATe?");
            LogHelper.Debug($"{PowerType}获取输出状态: {str}");
            return str;
        }

        /// <summary>
        /// 获取电源告警状态（Bit1 UVP、Bit2 OVP、Bit3 OCP、Bit4 OPP、Bit5 OTP、Bit7 FDP）
        /// </summary>
        public string GetOutputEvent()
        {
            var str = SendAndWaitForResponse("OUTPut:EVENt?");
            LogHelper.Debug($"{PowerType}获取告警状态: {str}");
            return str;
        }

        /// <summary>
        /// 清除告警
        /// </summary>
        public bool ClearOutputEvent()
        {
            LogHelper.Debug($"{PowerType}清除告警");
            SendAndWaitForResponse("OUTPut:EVENt 0", false);
            return true;
        }

        /// <summary>
        /// 设置电源收到 ON 指令后执行 ON 的延时（s）
        /// </summary>
        public bool SetDelayOn(double second)
        {
            LogHelper.Debug($"{PowerType}设置ON延时: {second} s");
            SendAndWaitForResponse($"OUTPut:DELay:ON {second}", false);
            return true;
        }

        /// <summary>
        /// 设置电源收到 OFF 指令后执行 OFF 的延时（s）
        /// </summary>
        public bool SetDelayOff(double second)
        {
            LogHelper.Debug($"{PowerType}设置OFF延时: {second} s");
            SendAndWaitForResponse($"OUTPut:DELay:OFF {second}", false);
            return true;
        }

        /// <summary>
        /// 控制泄放电路开关（ON / OFF）
        /// </summary>
        public bool SetDischarge(bool enable)
        {
            LogHelper.Debug($"{PowerType}控制泄放电路: {(enable ? "ON" : "OFF")}");
            SendAndWaitForResponse($"OUTPut:DISRes {(enable ? "ON" : "OFF")}", false);
            return true;
        }

        /// <summary>
        /// 控制上电默认状态（LASOff | LAST | RESet）
        /// </summary>
        public bool SetPowerOnState(string state)
        {
            LogHelper.Debug($"{PowerType}设置上电默认状态: {state}");
            SendAndWaitForResponse($"OUTPut:POWeron:STATe {state}", false);
            return true;
        }

        /// <summary>
        /// 选择斜率控制方式（0|1|Time|Slew）
        /// </summary>
        public bool SetSlewMode(string mode)
        {
            LogHelper.Debug($"{PowerType}选择斜率控制方式: {mode}");
            SendAndWaitForResponse($"OUTPut:SLOPemode {mode}", false);
            return true;
        }

        /// <summary>
        /// 设置电压上升斜率（斜率模式下，V/s）
        /// </summary>
        public bool SetVoltageSlewUp(double value)
        {
            LogHelper.Debug($"{PowerType}设置电压上升斜率: {value} V/s");
            SendAndWaitForResponse($"SOURce:VOLTage:SLEW:UP {value}", false);
            return true;
        }

        /// <summary>
        /// 设置电压下降斜率（斜率模式下，V/s）
        /// </summary>
        public bool SetVoltageSlewDown(double value)
        {
            LogHelper.Debug($"{PowerType}设置电压下降斜率: {value} V/s");
            SendAndWaitForResponse($"SOURce:VOLTage:SLEW:DOWN {value}", false);
            return true;
        }

        /// <summary>
        /// 设置电流上升斜率（斜率模式下，mA/s）
        /// </summary>
        public bool SetCurrentSlewUp(double value)
        {
            LogHelper.Debug($"{PowerType}设置电流上升斜率: {value} mA/s");
            SendAndWaitForResponse($"SOURce:CURRent:SLEW:UP {value}", false);
            return true;
        }

        /// <summary>
        /// 设置电流下降斜率（斜率模式下，mA/s）
        /// </summary>
        public bool SetCurrentSlewDown(double value)
        {
            LogHelper.Debug($"{PowerType}设置电流下降斜率: {value} mA/s");
            SendAndWaitForResponse($"SOURce:CURRent:SLEW:DOWN {value}", false);
            return true;
        }
        #endregion

        #region 读取输出电压电流及功率值（3.4）
        /// <summary>
        /// 回读电压（V）
        /// </summary>
        public string GetMeasureVoltage()
        {
            var str = SendAndWaitForResponse("MEASure:VOLTage?");
            LogHelper.Debug($"{PowerType}回读电压: {str} V");
            return str;
        }

        /// <summary>
        /// 回读电流（A）
        /// </summary>
        public string GetMeasureCurrent()
        {
            var str = SendAndWaitForResponse("MEASure:CURRent?");
            LogHelper.Debug($"{PowerType}回读电流: {str} A");
            return str;
        }

        /// <summary>
        /// 回读功率（W）
        /// </summary>
        public string GetMeasurePower()
        {
            var str = SendAndWaitForResponse("MEASure:POWer?");
            LogHelper.Debug($"{PowerType}回读功率: {str} W");
            return str;
        }

        /// <summary>
        /// 回读电池充电已充入容量（mAh）
        /// </summary>
        public string GetMeasureMAH()
        {
            var str = SendAndWaitForResponse("MEASure:MAH?");
            LogHelper.Debug($"{PowerType}回读容量: {str} mAh");
            return str;
        }

        /// <summary>
        /// 获取额定电压值（V）
        /// </summary>
        public string GetRatedVoltage()
        {
            var str = SendAndWaitForResponse("MEASure:VOLTage:MAXimum?");
            LogHelper.Debug($"{PowerType}额定电压: {str} V");
            return str;
        }

        /// <summary>
        /// 获取额定电流值（A）
        /// </summary>
        public string GetRatedCurrent()
        {
            var str = SendAndWaitForResponse("MEASure:CURRent:MAXimum?");
            LogHelper.Debug($"{PowerType}额定电流: {str} A");
            return str;
        }

        /// <summary>
        /// 获取额定功率值（W）
        /// </summary>
        public string GetRatedPower()
        {
            var str = SendAndWaitForResponse("MEASure:POWer:MAXimum?");
            LogHelper.Debug($"{PowerType}额定功率: {str} W");
            return str;
        }
        #endregion

        #region 保护功能（3.5）
        /// <summary>
        /// 设置欠压保护值（V）
        /// </summary>
        public bool SetUVPLevel(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置欠压保护值: {voltage} V");
            SendAndWaitForResponse($"PROTect:LESS:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置欠压保护预热时间（s）
        /// </summary>
        public bool SetUVPReady(double second)
        {
            LogHelper.Debug($"{PowerType}设置欠压保护预热时间: {second} s");
            SendAndWaitForResponse($"PROTect:UVP:READy {second}", false);
            return true;
        }

        /// <summary>
        /// 设置欠压保护延迟时间（s）
        /// </summary>
        public bool SetUVPDwell(double second)
        {
            LogHelper.Debug($"{PowerType}设置欠压保护延迟时间: {second} s");
            SendAndWaitForResponse($"PROTect:UVP:DWELl {second}", false);
            return true;
        }

        /// <summary>
        /// 设置过压保护值（V）
        /// </summary>
        public bool SetOVPLevel(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置过压保护值: {voltage} V");
            SendAndWaitForResponse($"PROTect:OVER:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置过压保护延迟时间（s）
        /// </summary>
        public bool SetOVPDwell(double second)
        {
            LogHelper.Debug($"{PowerType}设置过压保护延迟时间: {second} s");
            SendAndWaitForResponse($"PROTect:OVP:DWELl {second}", false);
            return true;
        }

        /// <summary>
        /// 设置过流保护值（A）
        /// </summary>
        public bool SetOCPLevel(double current)
        {
            LogHelper.Debug($"{PowerType}设置过流保护值: {current} A");
            SendAndWaitForResponse($"PROTect:CURRent {current}", false);
            return true;
        }

        /// <summary>
        /// 设置过流保护延迟时间（s）
        /// </summary>
        public bool SetOCPDwell(double second)
        {
            LogHelper.Debug($"{PowerType}设置过流保护延迟时间: {second} s");
            SendAndWaitForResponse($"PROTect:OCP:DWELl {second}", false);
            return true;
        }

        /// <summary>
        /// 设置过功率保护值（W）
        /// </summary>
        public bool SetOPPLevel(double power)
        {
            LogHelper.Debug($"{PowerType}设置过功率保护值: {power} W");
            SendAndWaitForResponse($"PROTect:POWer {power}", false);
            return true;
        }

        /// <summary>
        /// 设置过功率保护延迟时间（s）
        /// </summary>
        public bool SetOPPDwell(double second)
        {
            LogHelper.Debug($"{PowerType}设置过功率保护延迟时间: {second} s");
            SendAndWaitForResponse($"PROTect:OPP:DWELl {second}", false);
            return true;
        }

        /// <summary>
        /// 设置折返保护（0-OFF | 1-CV | 2-CC）
        /// </summary>
        public bool SetFDPEnable(int mode)
        {
            LogHelper.Debug($"{PowerType}设置折返保护: {mode}");
            SendAndWaitForResponse($"PROTect:FDP:ENABle {mode}", false);
            return true;
        }

        /// <summary>
        /// 设置折返保护延迟时间（s）
        /// </summary>
        public bool SetFDPDwell(double second)
        {
            LogHelper.Debug($"{PowerType}设置折返保护延迟时间: {second} s");
            SendAndWaitForResponse($"PROTect:FDP:DWELl {second}", false);
            return true;
        }

        /// <summary>
        /// 设置电压下限（V）
        /// </summary>
        public bool SetVoltageLimitLow(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置电压下限: {voltage} V");
            SendAndWaitForResponse($"SOURce:VOLTage:LEVel:LIMit:LOW {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置电压上限（V）
        /// </summary>
        public bool SetVoltageLimitHigh(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置电压上限: {voltage} V");
            SendAndWaitForResponse($"SOURce:VOLTage:LEVel:LIMit {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置电流下限（A）
        /// </summary>
        public bool SetCurrentLimitLow(double current)
        {
            LogHelper.Debug($"{PowerType}设置电流下限: {current} A");
            SendAndWaitForResponse($"SOURce:CURRent:LEVel:LIMit:LOW {current}", false);
            return true;
        }

        /// <summary>
        /// 设置电流上限（A）
        /// </summary>
        public bool SetCurrentLimitHigh(double current)
        {
            LogHelper.Debug($"{PowerType}设置电流上限: {current} A");
            SendAndWaitForResponse($"SOURce:CURRent:LEVel:LIMit {current}", false);
            return true;
        }

        /// <summary>
        /// 设置功率下限（W）
        /// </summary>
        public bool SetPowerLimitLow(double power)
        {
            LogHelper.Debug($"{PowerType}设置功率下限: {power} W");
            SendAndWaitForResponse($"SOURce:POWer:LEVel:LIMit:LOW {power}", false);
            return true;
        }

        /// <summary>
        /// 设置功率上限（W）
        /// </summary>
        public bool SetPowerLimitHigh(double power)
        {
            LogHelper.Debug($"{PowerType}设置功率上限: {power} W");
            SendAndWaitForResponse($"SOURce:POWer:LEVel:LIMit {power}", false);
            return true;
        }
        #endregion

        #region 恒功率功能（3.6）
        /// <summary>
        /// 设置电源进入恒功率模式
        /// </summary>
        public bool SetCPowerMode()
        {
            return SetOutputMode("CPOWer");
        }

        /// <summary>
        /// 设定恒功率模式电压（V）
        /// </summary>
        public bool SetCPowerVoltage(double voltage)
        {
            LogHelper.Debug($"{PowerType}设定恒功率电压: {voltage} V");
            SendAndWaitForResponse($"CPOWer:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设定恒功率模式电流（A）
        /// </summary>
        public bool SetCPowerCurrent(double current)
        {
            LogHelper.Debug($"{PowerType}设定恒功率电流: {current} A");
            SendAndWaitForResponse($"CPOWer:CURRent {current}", false);
            return true;
        }

        /// <summary>
        /// 设定恒功率模式功率（W）
        /// </summary>
        public bool SetCPowerPower(double power)
        {
            LogHelper.Debug($"{PowerType}设定恒功率: {power} W");
            SendAndWaitForResponse($"CPOWer:POWer {power}", false);
            return true;
        }
        #endregion

        #region 序列运行/编辑功能（3.7、3.8）
        /// <summary>
        /// 设置电源进入序列（SEQ）模式
        /// </summary>
        public bool SetSequenceMode()
        {
            return SetOutputMode("SEQuence");
        }

        /// <summary>
        /// 选择需运行的序列文件（1-10）
        /// </summary>
        public bool SetSequenceRunFile(int fileIndex)
        {
            LogHelper.Debug($"{PowerType}选择运行序列文件: {fileIndex}");
            SendAndWaitForResponse($"SEQuence:RUN:FILE {fileIndex}", false);
            return true;
        }

        /// <summary>
        /// 选择需编辑的序列文件编号（1-10）
        /// </summary>
        public bool SetSequenceEditFile(int fileIndex)
        {
            LogHelper.Debug($"{PowerType}选择编辑序列文件: {fileIndex}");
            SendAndWaitForResponse($"SEQuence:EDITe:FILE {fileIndex}", false);
            return true;
        }

        /// <summary>
        /// 设置序列文件总步数（1-1000）
        /// </summary>
        public bool SetSequenceLength(int length)
        {
            LogHelper.Debug($"{PowerType}设置序列总步数: {length}");
            SendAndWaitForResponse($"SEQuence:EDITe:LENGth {length}", false);
            return true;
        }

        /// <summary>
        /// 设置序列文件循环次数
        /// </summary>
        public bool SetSequenceCycle(int cycle)
        {
            LogHelper.Debug($"{PowerType}设置序列循环次数: {cycle}");
            SendAndWaitForResponse($"SEQuence:EDITe:CYCLe {cycle}", false);
            return true;
        }

        /// <summary>
        /// 设置序列文件链接文件编号（0-不链接）
        /// </summary>
        public bool SetSequenceLinkFile(int fileIndex)
        {
            LogHelper.Debug($"{PowerType}设置序列链接文件: {fileIndex}");
            SendAndWaitForResponse($"SEQuence:EDITe:LFILe {fileIndex}", false);
            return true;
        }

        /// <summary>
        /// 选择本序列文件的第 N 步
        /// </summary>
        public bool SetSequenceStep(int step)
        {
            LogHelper.Debug($"{PowerType}选择序列第 {step} 步");
            SendAndWaitForResponse($"SEQuence:EDITe:STEP {step}", false);
            return true;
        }

        /// <summary>
        /// 设置当前步的恒压值（V）
        /// </summary>
        public bool SetSequenceVoltage(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置序列当前步电压: {voltage} V");
            SendAndWaitForResponse($"SEQuence:EDITe:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置当前步的限流值（A）
        /// </summary>
        public bool SetSequenceCurrent(double current)
        {
            LogHelper.Debug($"{PowerType}设置序列当前步电流: {current} A");
            SendAndWaitForResponse($"SEQuence:EDITe:CURRent {current}", false);
            return true;
        }

        /// <summary>
        /// 设置当前步的限功率值（W）
        /// </summary>
        public bool SetSequencePower(double power)
        {
            LogHelper.Debug($"{PowerType}设置序列当前步功率: {power} W");
            SendAndWaitForResponse($"SEQuence:EDITe:POWer {power}", false);
            return true;
        }

        /// <summary>
        /// 设置当前步的运行时间
        /// </summary>
        public bool SetSequenceDwell(double dwell)
        {
            LogHelper.Debug($"{PowerType}设置序列当前步运行时间: {dwell}");
            SendAndWaitForResponse($"SEQuence:EDITe:DWELl {dwell}", false);
            return true;
        }

        /// <summary>
        /// 设置当前步运行时间单位（1-s | 2-min | 3-h）
        /// </summary>
        public bool SetSequenceDwellUnit(int unit)
        {
            LogHelper.Debug($"{PowerType}设置序列运行时间单位: {unit}");
            SendAndWaitForResponse($"SEQuence:EDITe:DWEUnit {unit}", false);
            return true;
        }

        /// <summary>
        /// 读取序列模式下当前运行的步编号
        /// </summary>
        public string GetSequenceRunStep()
        {
            var str = SendAndWaitForResponse("SEQuence:RUN:STEP?");
            LogHelper.Debug($"{PowerType}读取当前序列步编号: {str}");
            return str;
        }

        /// <summary>
        /// 读取当前序列步的运行时间
        /// </summary>
        public string GetSequenceRunTime()
        {
            var str = SendAndWaitForResponse("SEQuence:RUN:TIMe?");
            LogHelper.Debug($"{PowerType}读取当前序列步运行时间: {str}");
            return str;
        }
        #endregion

        #region 电池充电（3.9）
        /// <summary>
        /// 设置电源进入电池充电模式
        /// </summary>
        public bool SetChargeMode()
        {
            return SetOutputMode("CHARge");
        }

        /// <summary>
        /// 设置充电电压（V）
        /// </summary>
        public bool SetChargeVoltage(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置充电电压: {voltage} V");
            SendAndWaitForResponse($"BATTery:CHARge:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置充电电流（A）
        /// </summary>
        public bool SetChargeCurrent(double current)
        {
            LogHelper.Debug($"{PowerType}设置充电电流: {current} A");
            SendAndWaitForResponse($"BATTery:CHARge:CURRent {current}", false);
            return true;
        }

        /// <summary>
        /// 设置充电时间（s，0-9999）
        /// </summary>
        public bool SetChargeStopTime(double second)
        {
            LogHelper.Debug($"{PowerType}设置充电时间: {second} s");
            SendAndWaitForResponse($"BATTery:STOP:TIME {second}", false);
            return true;
        }

        /// <summary>
        /// 设置截止电压（V）
        /// </summary>
        public bool SetChargeStopVoltage(double voltage)
        {
            LogHelper.Debug($"{PowerType}设置截止电压: {voltage} V");
            SendAndWaitForResponse($"BATTery:STOP:VOLTage {voltage}", false);
            return true;
        }

        /// <summary>
        /// 设置截止电流（A）
        /// </summary>
        public bool SetChargeStopCurrent(double current)
        {
            LogHelper.Debug($"{PowerType}设置截止电流: {current} A");
            SendAndWaitForResponse($"BATTery:STOP:CURRent {current}", false);
            return true;
        }

        /// <summary>
        /// 设置充电容量（mAh，0-9999）
        /// </summary>
        public bool SetChargeStopCapacity(double capacity)
        {
            LogHelper.Debug($"{PowerType}设置充电容量: {capacity} mAh");
            SendAndWaitForResponse($"BATTery:STOP:CAPacity {capacity}", false);
            return true;
        }
        #endregion

        #region 系统设置（3.13）
        /// <summary>
        /// 设置设备 ID（1-250）
        /// </summary>
        public bool SetDeviceId(int id)
        {
            LogHelper.Debug($"{PowerType}设置设备ID: {id}");
            SendAndWaitForResponse($"SYSTem:DEVId {id}", false);
            return true;
        }

        /// <summary>
        /// 获取设备 ID
        /// </summary>
        public string GetDeviceId()
        {
            var str = SendAndWaitForResponse("SYSTem:DEVId?");
            LogHelper.Debug($"{PowerType}获取设备ID: {str}");
            return str;
        }
        #endregion
    }
}
