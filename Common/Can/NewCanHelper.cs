using AllData;
using Common.SerialportHelper;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Common.Can
{
    public static class NewCanHelper
    {
        static RelayHelper _relay => CommonData.Relay;
        static ToolControlHelper _toolControl => CommonData.ToolControl;

        #region 基本硬件控制

        public static void RESET(int chance)
        {
            try
            {
                _toolControl.SetDL2(false);
                System.Threading.Thread.Sleep(100);
                LogHelper.Debug("工装板复位成功");
            }
            catch (Exception ex)
            {
                LogHelper.ShowMsg($"工装板复位失败: {ex.Message}");
            }
        }

        public static float? GetCanTR()
        {
            try
            {
                uint rawValue = _relay.ReadSW_Rx2();
                float resistanceKohm = rawValue ;
                return resistanceKohm ;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"获取TR电阻失败: {ex.Message}");
                return null;
            }
        }

        public static (bool, bool) GetButtonStatus()
        {
            try
            {
                bool di1 = _toolControl.ReadDIL1();
                bool di2 = _toolControl.ReadDIL2();
                return (di1, di2);
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"获取按钮状态失败: {ex.Message}");
                return (false, false);
            }
        }

        public static bool ControlZhiJu(bool isPresss)
        {
            try
            {
                
                return _toolControl.SetDL2(isPresss);
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"控制治具失败: {ex.Message}");
                return false;
            }
        }

        public static bool GetZhiJuStatus()
        {
            try
            {
                return _toolControl.ReadDIL3();
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"获取治具状态失败: {ex.Message}");
                return false;
            }
        }

        public static bool ControlButtonLamp(bool b)
        {
            try
            {
                bool result1 = _toolControl.SetDL3(b);
                bool result2 = _toolControl.SetDL4(b);
                return result1 && result2;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"控制按钮灯失败: {ex.Message}");
                return false;
            }
        }

        public static bool WakeOrSleep(bool b)
        {
            try
            {
                return _relay.SetRelayCard1(3, b);
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"唤醒/睡眠控制失败: {ex.Message}");
                return false;
            }
        }

        public static bool GetEmergencyStopBtnStatus()
        {
            try
            {
                return !_toolControl.ReadDIH2();
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"获取急停按钮状态失败: {ex.Message}");
                return false;
            }
        }

        public static double? GetInternalTotalVoltage()
        {
            try
            {
                ushort rawValue = _relay.Read0_30V_IN1();
                double voltage = rawValue / 100.0;
                return voltage;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"获取内总电压失败: {ex.Message}");
                return null;
            }
        }

        public static double? GetExternalTotalVoltage()
        {
            try
            {
                ushort rawValue = _relay.Read0_30V_IN2();
                double voltage = rawValue / 100.0;
                return voltage;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"获取外总电压失败: {ex.Message}");
                return null;
            }
        }

        public static bool MOSControlVoltageEnable(bool _within, bool _outside, bool threeside = true, bool fourside = false,bool gd = true)
        {
            try
            {
                bool result = true;

                
                result &= _relay.SetRelayCard1(1, _within);

                result &= _relay.SetRelayCard2(1, _outside);
                result &= _relay.SetRelayCard2(24, _outside);

                if (gd == false)
                {

                    if (threeside)
                    {
                        result &= _relay.SetRelayCard1(2, false);
                        if (fourside)
                        {
                            result &= _relay.SetRelayCard1(11, true);
                            result &= _relay.SetRelayCard1(9, false);

                        }
                        else
                        {
                            result &= _relay.SetRelayCard1(9, true);
                            result &= _relay.SetRelayCard1(11, false);

                        }
                    }
                    else
                    {
                        result &= _relay.SetRelayCard1(2, true);
                        result &= _relay.SetRelayCard1(1, false);
                        result &= _relay.SetRelayCard1(9, false);
                        result &= _relay.SetRelayCard1(11, false);
                    }
                }
                else
                {
                    result &= _relay.SetRelayCard1(2, false);
                    result &= _relay.SetRelayCard1(9, false);
                    result &= _relay.SetRelayCard1(11, false);
                }


                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"MOS控制失败: {ex.Message}");
                return false;
            }
        }

        public static bool CtrolTotalCurrentCollection(bool isJust)
        {
            try
            {
                if(isJust)//b-  p-:29 31
                {
                    CommonData.Relay.SetRelayCard2(29, false);
                    CommonData.Relay.SetRelayCard2(31, false);
                    CommonData.Relay.SetRelayCard2(30, true);
                    CommonData.Relay.SetRelayCard2(32, true);
                    
                
                }
                else//p-  b-:30 32
                {
                    CommonData.Relay.SetRelayCard2(30, false);
                    CommonData.Relay.SetRelayCard2(32, false);
                    CommonData.Relay.SetRelayCard2(29, true);
                    CommonData.Relay.SetRelayCard2(31, true);

                }
                
                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"电流采集控制失败: {ex.Message}");
                return false;
            }
        }

        public static bool CloseTotalCurrentCollection()
        {

            try
            {
                CommonData.Relay.SetRelayCard2(29, false);
                CommonData.Relay.SetRelayCard2(30, false);
                CommonData.Relay.SetRelayCard2(31, false);
                CommonData.Relay.SetRelayCard2(32, false);

                return true;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"关闭电流采集失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 初始化所有继电器（关闭所有继电器）
        /// </summary>
        public static bool InitAllRelays()
        {
            try
            {
                bool result = true;
                result &= _relay.CloseAllRelayCard1();
                CommonMethod.Sleep(40);
                result &= _relay.CloseAllRelayCard2();
                LogHelper.Debug($"初始化所有继电器: {(result ? "成功" : "失败")}");
                return result;
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"初始化所有继电器失败: {ex.Message}");
                return false;
            }
        }

        public static void SetCellTemperature(double d)
        {
            try
            {
                int resistanceKohm = ConvertTemperatureToResistance(d);
                for (int i = 0; i < 3; i++)
                {
                    _relay.SetResistance(i + 1, resistanceKohm);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"设置电池温度失败: {ex.Message}");
            }
        }

        private static int ConvertTemperatureToResistance(double temperature)
        {
            switch (temperature)
            {
                case -35: return 1000;
                case -30: return 1000;
                case -20: return 992;
                case -8: return 1000;
                case 25: return 100;
                case 40: return 1000;
                case 44: return 1000;
                case 45: return 44;
                case 56: return 1000;
                case 85: return 11;
                case 86: return 1000;
                default: return 100;
            }
        }

        public static void EndTest()
        {
            try
            {
                CommonMethod.Sleep(100);
                ControlButtonLamp(false);
                CommonMethod.Sleep(100);
                ControlZhiJu(false);
                DateTime now = DateTime.Now;
                CommonData.M3905D.SetCurrent(0);
                CommonData.M3905D.SetVoltage(0);
                CommonData.M3905D.SetOUTPut(0);
                NewCanHelper.CloseTotalCurrentCollection();
                // 初始化所有继电器（关闭所有继电器）
                //NewCanHelper.InitAllRelays();

              //  var cur = CommonData.M3905D.SetCurrentAndStable(0, 0, 5 / 1000.0);
                //while (true)
                //{
                //    LogHelper.ShowMsg($"当前电流：{cur}");
                //    CommonMethod.Sleep(500);
                //    if (cur < 1)
                //    {
                //        ControlButtonLamp(false);
                //        ControlZhiJu(false);
                //        break;
                //    }
                //    else if ((DateTime.Now - now).TotalSeconds > 5)
                //    {
                //        MessageBox.Show("请手动关闭电流源之后按下急停");
                //    }
                //    cur = CommonData.M3905D.SetCurrentAndStable(0, 0, 5 / 1000.0);
                //}
            }
            catch (Exception ex)
            {
                LogHelper.Debug($"结束测试失败: {ex.Message}");
            }
        }

        #endregion
    }
}
