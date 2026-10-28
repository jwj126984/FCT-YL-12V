using AllData;
using Common;
using Common.Can;
using Common.SerialportHelper;
using Common.UDS;
using DetectionProcess;
using DetectionProcess.SERES;
using FCT.Dialog;
using Model;
using Models_C;
using Models_C.Dal;
using Newtonsoft.Json;
﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Shapes;

namespace FCT
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 外接设备实例类
        DeviceOperate deviceOperate = new DeviceOperate();
        CanOperate operate = new CanOperate();
        CanOperate uds = new CanOperate(1);
        BarCodeScanningGun gun;
    //    PowerHelperUSB power1;
        PowerHelperUSB power2;
        N8331 N8331;
        NXI_6400 nXI_6400;
        NXI_6400 nXI_6400_2;
        IT6322 IT6322;
        N69206H N69206H;
        N36200 N36200;
        RelayHelper relay; // 继电器控制板（共用一个串口）
        ToolControlHelper toolControl; // 治具控制卡（独立串口）
        #endregion

        bool isListen = false;
        object _lock = new object();
        Thread tr;//检测的线程

        #region 急停
        bool isStopPressDown = false;
        StopDialog stopDialog;
        #endregion

        /// <summary>
        /// 数据结果界面展示
        /// </summary>
        ObservableCollection<TestResult> results = new ObservableCollection<TestResult>();
        /// <summary>
        /// 数据后台保存以及入库数据源
        /// </summary>
        List<TestResult> testResults = new List<TestResult>();
        #region 结果弹窗
        ResultDialog dialog = null;
        bool ResultDialogIsOpen = false;
        #endregion
        bool m3905DCanUse = true;//M3905D通信是否正常


        public MainWindow()
        {
            InitializeComponent();
            LogHelper.SetMainWindow(this);
            ParSettingDialog parSettingDialog = new ParSettingDialog();
            Init();
            InitProtocolComboBox();
            userSetBtn.IsEnabled = CommonData.UserRole == Role.管理员 ? true : false;
            parSetBtn.IsEnabled = CommonData.UserRole != Role.操作员 ? true : false;
            if(CommonData.UserRole == Role.操作员)
            {
                Left = 0;
                Width = 1200;
                // 设置窗口高度为屏幕工作区域的高度
                this.Height = SystemParameters.WorkArea.Height;

                // 如果需要，可以设置窗口的垂直位置为屏幕顶部
                this.Top = 0;
            }
            else
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        }
        /// <summary>
        /// 初始化数据
        /// </summary>
        void Init()
        {

            deviceOperate = new DeviceOperate();
            operate = new CanOperate();
            uds = new CanOperate();
            passNum.Text = string.IsNullOrEmpty(ConfigHelper.PassNum) ? "0" : ConfigHelper.PassNum;
            failNum.Text = string.IsNullOrEmpty(ConfigHelper.FailNum) ? "0" : ConfigHelper.FailNum;

            var powerPort1 = ConfigHelper.IT6122BPort;
            var powerPort2 = ConfigHelper.M3905DPort;
            var N8331Port = ConfigHelper.N8331Port;
            var IT6322Port = ConfigHelper.IT6322Port;
            var N69206HPort = ConfigHelper.N69206HPort;
            var N36200Port = ConfigHelper.N36200Port;
            var gunPort = ConfigHelper.GunPort;
            var relayPort = ConfigHelper.RelayPort;
            var toolControlPort = ConfigHelper.ToolControlPort;

            var NXI_6400K_V = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400");
            var NXI_6400K_V2 = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "NXI_6400_2");


            string msg = "";
            App.OnEthernetChanged += (b) =>
            {
                if (nXI_6400 != null)
                {
                    if (b)
                    {
                        //nXI_6400.Start();
                    }
                    else
                    {
                        nXI_6400.Close();
                        nXI_6400 = null;
                    }
                }
                else
                {
                    if (b && NXI_6400K_V != null)
                    {
                        nXI_6400 = new NXI_6400(NXI_6400K_V.CommValue.Split('&')[0], NXI_6400K_V.CommValue.Split('&')[1], int.Parse(NXI_6400K_V.CommValue.Split('&')[2]));
                        CommonData.NXI_6400 = nXI_6400;
                    }
                }

                //if (nXI_6400_2 != null)
                //{
                //    if (b)
                //    {
                //        //nXI_6400_2.Start();
                //    }
                //    else
                //    {
                //        nXI_6400_2.Close();
                //        nXI_6400_2 = null;
                //    }
                //}
                //else
                //{
                //    if (b && NXI_6400K_V2 != null)
                //    {
                //        nXI_6400_2 = new NXI_6400(NXI_6400K_V2.CommValue.Split('&')[0], NXI_6400K_V2.CommValue.Split('&')[1], int.Parse(NXI_6400K_V2.CommValue.Split('&')[2]));
                //        CommonData.NXI_6400_2 = nXI_6400_2;
                //    }
                //}
            };

            try
            {
                #region 设置Can Lin
                if (deviceOperate.OpenDevice() == false)
                {
                    MessageBox.Show("设备打开失败");
                    this.Close();
                    return;
                }
                if (operate.Start(deviceOperate.GetDeviceType, deviceOperate.GetDeviceHandle, 1, 500, 500) == false)
                {
                    MessageBox.Show("工装Can打开失败");
                    this.Close();
                    return;
                }
                if (uds.Start(deviceOperate.GetDeviceType, deviceOperate.GetDeviceHandle, 0, 500, 2000) == false)
                {
                    MessageBox.Show("产品Can打开失败");
                    this.Close();
                    return;
                }

                #region 初始化公共的静态类

                CanHelper.SetCan(operate);
                // NewCanHelper.SetCan(operate);
                ZCData.SetCP(uds);
                PlayUDSHelper.SetUDS(uds, int.Parse(ConfigHelper.UdsParameters.RID, System.Globalization.NumberStyles.HexNumber));

                CommonMethod.SetProduct(uds);
                #endregion
                #endregion


                //if (string.IsNullOrEmpty(powerPort1))
                //{
                //    //MessageBox.Show("IT6122B 端口未设置");
                //    //this.Close();
                //    //return;
                //    msg += "IT6122B 端口未设置\n";
                //}
                //else
                //{
                //    try
                //    {
                //        power1 = new PowerHelperUSB(powerPort1);
                //        power1.PowerType = "IT6122B";
                //        power1.SetOUTPut(0);
                //    }
                //    catch (Exception ex)
                //    {
                //        msg += "IT6122B" + ex.Message + "\n";
                //    }

                //}
                if (string.IsNullOrEmpty(powerPort2))
                {
                    //MessageBox.Show("M3905D 端口未设置");
                    //this.Close();
                    //return;
                    msg += "M3905D 端口未设置\n";
                }
                else
                {

                    try
                    {
                        power2 = new PowerHelperUSB(powerPort2);
                        power2.PowerType = "M3905D";
                    }
                    catch (Exception ex)
                    {
                        msg += "M3905D" + ex.Message + "\n";
                    }
                }
                if (string.IsNullOrEmpty(N8331Port))
                {
                    //MessageBox.Show("N8331 端口未设置");
                    //this.Close();
                    //return;
                    msg += "N8331 端口未设置\n";
                }
                else
                {

                    try
                    {
                        N8331 = new N8331(N8331Port);
                        N8331.PowerType = "N8331";
                    }
                    catch (Exception ex)
                    {
                        msg += "N8331" + ex.Message + "\n";
                    }
                }

                if (string.IsNullOrEmpty(IT6322Port))
                {
                    msg += "IT6322 端口未设置\n";
                }
                else
                {
                    try
                    {
                        IT6322 = new IT6322(IT6322Port);
                        IT6322.PowerType = "IT6322";
                    }
                    catch (Exception ex)
                    {
                        msg += "IT6322" + ex.Message + "\n";
                    }
                }

                if (string.IsNullOrEmpty(N69206HPort))
                {
                    msg += "N62400 端口未设置\n";
                }
                else
                {
                    try
                    {
                        N69206H = new N69206H(N69206HPort);
                    }
                    catch (Exception ex)
                    {
                        msg += "N62400" + ex.Message + "\n";
                    }
                }

                if (string.IsNullOrEmpty(N36200Port))
                {
                    msg += "N36200/N36300 端口未设置\n";
                }
                else
                {
                    try
                    {
                        N36200 = new N36200(N36200Port);
                    }
                    catch (Exception ex)
                    {
                        msg += "N36200/N36300" + ex.Message + "\n";
                    }
                }




                //N8331.PowerType = "N8331";

                if (NXI_6400K_V == null)
                {
                    //MessageBox.Show("NXI_6400 参数未设置");
                    //this.Close();
                    //return;
                    msg += "NXI_6400 参数未设置\n";
                }
                else
                {
                    try
                    {
                        nXI_6400 = new NXI_6400(NXI_6400K_V.CommValue.Split('&')[0], NXI_6400K_V.CommValue.Split('&')[1], int.Parse(NXI_6400K_V.CommValue.Split('&')[2]));
                    }
                    catch (Exception ex)
                    {
                        msg += "NXI-6400" + ex.Message + "\n";
                    }
                }

                //if (NXI_6400K_V2 == null)
                //{
                //    msg += "NXI_6400_2 参数未设置\n";
                //}
                //else
                //{
                //    try
                //    {
                //        nXI_6400_2 = new NXI_6400(NXI_6400K_V2.CommValue.Split('&')[0], NXI_6400K_V2.CommValue.Split('&')[1], int.Parse(NXI_6400K_V2.CommValue.Split('&')[2]));
                //    }
                //    catch (Exception ex)
                //    {
                //        msg += "NXI-6400_2" + ex.Message + "\n";
                //    }
                //}


             //   CommonData.IT6122B = power1;
                CommonData.M3905D = power2;
                CommonData.N8331 = N8331;
                CommonData.IT6322 = IT6322;
                CommonData.N69206H = N69206H;
                CommonData.N36200 = N36200;
                CommonData.NXI_6400 = nXI_6400;
                //CommonData.NXI_6400_2 = nXI_6400_2;

                #region 设置扫码枪
                if (string.IsNullOrEmpty(gunPort))
                {
                    //MessageBox.Show("扫码枪端口未设置");
                    //this.Close();
                    //return;
                    msg += "扫码枪端口未设置\n";
                }
                else
                {

                    try
                    {
                        gun = new BarCodeScanningGun(gunPort);
                        gun.Received += (arr) =>
                        {
                            return;
                            if (isStopPressDown)
                            {
                                return;
                            }
                            if (string.IsNullOrEmpty(CommonData.BoardName))
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(this, "请配置板子类型");
                                });
                                return;
                            }
                            var temp = System.Text.Encoding.Default.GetString(arr.ToArray()).Trim();
                            if (temp.Contains("-") == false)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show(this, "无效条码");
                                });
                                return;
                            }
                            //var barV = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "条码");
                            //if (barV == null)
                            //{
                            //    MessageBox.Show("请检查数据库条码配置，并重启软件同步数据库数据");
                            //    return;
                            //}
                            //if (temp.Split('-')[0] != barV.CommValue)
                            //{
                            //    MessageBox.Show("无效条码");
                            //    return;
                            //}
                            if (temp.Length != 31)
                            {
                                return;
                            }
                            var httpResult = HttpHelper.VerBarCode(temp);
                            if (httpResult.Item1)
                            {
                                //MessageBox.Show($"{temp}验证通过");
                            }
                            else
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    MsgDialog msgDialog = new MsgDialog();
                                    msgDialog.ShowTxt($"{temp}验证失败，错误原因:{httpResult.Item2}");
                                    msgDialog.Owner = this;
                                    msgDialog.ShowDialog();
                                });

                                return;
                            }

                            this.Dispatcher.Invoke(() =>
                            {
                                if (CommonData.IsTesting)
                                {
                                    nextBoard.Text = temp;
                                }
                                else
                                {
                                    CommonData.CurrentBarCode = temp;
                                    currentBoard.Text = temp;
                                    nextBoard.Text = "";
                                }
                            });
                        };
                    }
                    catch (Exception ex)
                    {
                        msg += "扫码枪" + ex.Message + "\n";
                    }
                }

                #endregion

                #region 设置继电器控制板
                if (string.IsNullOrEmpty(relayPort))
                {
                    msg += "继电器控制板端口未设置\n";
                }
                else
                {
                    try
                    {
                        // 初始化继电器控制板（共用一个串口）
                        relay = new RelayHelper(relayPort);

                        // 初始化第一张继电器控制板（从机地址0x01）
                        //if (relay.CloseAllRelays(0x01))
                        //{
                        //    LogHelper.Debug("继电器控制板1初始化成功");
                        //}
                        //else
                        //{
                        //    LogHelper.Debug("继电器控制板1初始化失败");
                        //    msg += "继电器控制板1初始化失败\n";
                        //}

                        //// 初始化第二张继电器控制板（从机地址0x02）
                        //if (relay.CloseAllRelays(0x02))
                        //{
                        //    LogHelper.Debug("继电器控制板2初始化成功");
                        //}
                        //else
                        //{
                        //    LogHelper.Debug("继电器控制板2初始化失败");
                        //    msg += "继电器控制板2初始化失败\n";
                        //}
                    }
                    catch (Exception ex)
                    {
                        msg += "继电器控制板" + ex.Message + "\n";
                    }
                }
                CommonData.Relay = relay;
                // 初始化所有继电器（关闭所有继电器）
                //NewCanHelper.InitAllRelays();
                #endregion

                #region 设置治具控制卡
                if (string.IsNullOrEmpty(toolControlPort))
                {
                    msg += "治具控制卡端口未设置\n";
                }
                else
                {
                    try
                    {
                        toolControl = new ToolControlHelper(toolControlPort);
                        LogHelper.Debug("治具控制卡初始化成功");
                    }
                    catch (Exception ex)
                    {
                        msg += "治具控制卡" + ex.Message + "\n";
                    }
                }
                CommonData.ToolControl = toolControl;
                #endregion
                CommonData.ToolControl.SetDL1(false);
                NewCanHelper.ControlZhiJu(false);
                NewCanHelper.ControlButtonLamp(false);
                if (msg != "")
                {
                    MessageBox.Show(this, msg);
                }
                Task.Run(async () =>
                {
                    while (true)
                    {
                        if (isListen)
                        {
                            isListen = false;
                            StartListen();
                        }
                        await Task.Delay(50);
                    }
                });
                isListen = true;
                #region 检测当前是否存在检测任务
                Thread thread = new Thread(() =>
                {
                    return;
                    while (true)
                    {

                        if (string.IsNullOrEmpty(CommonData.CurrentBarCode) == false)
                        {
                            LogHelper.Debug($"检测到扫码枪扫码，内容{CommonData.CurrentBarCode}");
                            LogHelper.Debug("开始检测");
                            StartTest();

                            tr.Join();
                            if (tr != null)
                            {
                                tr.Abort();
                                tr = null;
                            }
                            //当急停按钮没有被按下 则判断是否存在下一个任务
                            if (isStopPressDown == false)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    //currentBoard.Text = "";
                                    //扫了下一块板子
                                    if (string.IsNullOrEmpty(nextBoard.Text) == false)
                                    {
                                        CommonData.CurrentBarCode = currentBoard.Text = nextBoard.Text;
                                        nextBoard.Text = "";
                                    }
                                    if (string.IsNullOrEmpty(CommonData.CurrentBarCode) == true)
                                    {
                                        CommonData.IsTesting = false;
                                    }
                                });

                            }

                        }
                        Thread.Sleep(100);
                    }
                });
                thread.IsBackground = true;
                thread.Start();
                #endregion

                if (CommonData.CurrentUser != null)
                {
                    userInfo.Text += CommonData.CurrentUser.UserName + $"({CommonData.UserRole.ToString()})";
                }
            }
            catch (Exception ex)
            {

                MessageBox.Show(this, ex.ToString());
                this.Close();
                return;
            }

            #region 急停检测
            Task.Run(async () =>
            {
                int count = 0;
                while (true)
                {
                    var jtBtnStatus = NewCanHelper.GetEmergencyStopBtnStatus();
                    if (jtBtnStatus == false && isStopPressDown == false)
                    {
                        count++;
                        if (count > 2)
                        {
                            this.Dispatcher.Invoke(() =>
                            {
                                isStopPressDown = true;
                                this.IsEnabled = false;
                                if (tr != null)
                                {
                                    tr.Abort();
                                }
                                CommonData.TestCount = 0;

                                CommonData.M3905D.SetOUTPut(0);
                                CommonData.N8331.SetOutPut4(false);
                                NewCanHelper.ControlZhiJu(false);
                                NewCanHelper.ControlButtonLamp(false);
                                CommonData.CurrentBarCode = "";
                                currentBoard.Text = "";
                                nextBoard.Text = "";
                                CommonData.IsTesting = false;
                                //右下角动画复原
                                this.Dispatcher.Invoke(() =>
                                {
                                    border2.Visibility = Visibility.Visible;
                                    border4.Visibility = Visibility.Visible;
                                    border1.Visibility = Visibility.Visible;
                                    border5.Visibility = Visibility.Visible;

                                });
                                stopDialog = new StopDialog();
                                stopDialog.Owner = this;
                                stopDialog.Show();
                                if (dialog != null && dialog.IsVisible == true)
                                {
                                    dialog.IsEnabled = false;
                                }


                            });
                            count = 0;
                        }

                    }
                    else if (jtBtnStatus == true && isStopPressDown == true)
                    {
                        this.Dispatcher.Invoke(() => {
                            if (isStopPressDown == true)
                            {
                                isStopPressDown = false;
                                stopDialog?.Close();
                                this.IsEnabled = true;
                                if (dialog != null && dialog.IsVisible == true)
                                {
                                    dialog.IsEnabled = true;
                                }
                                //StartListen();
                            }
                        });
                    }
                    await Task.Delay(500);
                }
            });
            #endregion

            #region 界面右下角动画
            int i = 0;
            CommonMethod.PlayTaskRun(() =>
            {
                if (CommonData.IsTesting)
                {
                    if (i % 3 == 0)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            border2.Visibility = Visibility.Hidden;
                            border4.Visibility = Visibility.Hidden;
                            border1.Visibility = Visibility.Hidden;
                            border5.Visibility = Visibility.Hidden;
                        });
                    }
                    else if (i % 3 == 1)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            border2.Visibility = Visibility.Visible;
                            border4.Visibility = Visibility.Visible;
                        });
                    }
                    else if (i % 3 == 2)
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            border1.Visibility = Visibility.Visible;
                            border5.Visibility = Visibility.Visible;
                        });
                    }
                    i++;
                }
                else
                {
                    i = 0;
                }
            }, 500, "动画线程");

            #endregion

            resultShowGrid.ItemsSource = results;
        }

        /// <summary>
        /// 开始测试
        /// </summary>
        void StartTest()
        {
            #region 判断设备是否初始化
           
            #endregion

            if (tr == null)
            {
                lock (_lock)
                {
                    if (tr == null)
                    {
                        tr = new Thread(() =>
                        {
                            DateTime startTime = DateTime.Now;
                            try
                            {
                                #region 准备阶段
                                NewCanHelper.InitAllRelays();
                                CommonMethod.Sleep(100);
                                NewCanHelper.MOSControlVoltageEnable(true, false,false);
                                CommonMethod.InitFrameFile(CommonData.CurrentBarCode);
                              //  NewCanHelper.SetCellTemperature(25);
                                CommonData.IsTesting = true;
                                CommonData.TestCount = 0;
                                CloseResult();
                                this.Dispatcher.Invoke(() =>
                                {
                                    msgItem1.Items.Clear();
                                    results.Clear();
                                });

                                startTime = DateTime.Now;
                                Task.Run(async () =>
                                {
                                    while (CommonData.IsTesting)
                                    {
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            useTime.Text = (DateTime.Now - startTime).TotalSeconds.ToString("0.00");
                                        });
                                        await Task.Delay(100);
                                    }
                                });
                                //CanHelper.QiTingAck();
                                #endregion
                                this.Dispatcher.Invoke(() =>
                                {
                                    prjStartTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                });
                                #region 终端电阻
                                if (CommonData.CurrentUserDetects.Where(m => m.Item1 == "CanTRTest").Count() > 0)
                                {
                                 //   if (ConfigHelper.UdsParameters.jiesuo == "VOYAH_H56C")
                                    {
                                        
                                        uds.Close();
                                        if (uds.Start(deviceOperate.GetDeviceType, deviceOperate.GetDeviceHandle, 0, 500, 2000, false) == false)
                                        {
                                            MessageBox.Show("产品Can打开失败");
                                            this.Close();
                                            return;
                                        }
                                    }
                                   
                                    CanTRTest canTRTest = new CanTRTest();
                                    canTRTest.AddModelAction += addResult;
                                    canTRTest.TestFail += testFail;
                                    TestResult model = new TestResult();
                                    LogHelper.Debug("开始终端电阻测试");
                                    var parTemp = CommonData.TestParameters.Where(m => m.MedthodName.ToLower() == canTRTest.GetType().Name.ToLower());
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        model.BoardNum = currentBoard.Text;

                                    });
                                    canTRTest.Start(parTemp);
                                    uds.Close();
                                    if (uds.Start(deviceOperate.GetDeviceType, deviceOperate.GetDeviceHandle, 0, 500, 2000, true) == false)
                                    {
                                        MessageBox.Show("产品Can打开失败");
                                        this.Close();
                                        return;
                                    }
                                }

                                #endregion

                                #region 设置电源
                                CommonData.N69206H.SetOutput(false);
                                LogHelper.Debug("设置 M3905D 远程控制");
                                power2.SetREM();

                                LogHelper.Debug("设置 M3905D 使能");
                                try
                                {
                                    //设置使能
                                    if (power2.SetOUTPut(1) == false)
                                    {
                                        throw new Exception("使能失败");
                                    }

                                }
                                catch (TimeoutException ex)
                                {
                                    MessageBox.Show("请重新拔插电源M3905D数据传输线");
                                    this.Dispatcher.Invoke(() =>
                                    {
                                        //MessageBox.Show(ex.ToString());
                                        OpenResult(false);
                                        txt1.Text = CommonData.CurrentBarCode;
                                        txt2.Visibility = Visibility.Visible;
                                        txt2.Text = ex.Message;
                                        txt3.Text = "Fail";
                                        bg1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"Red"));
                                        LogHelper.WriteLog(ex.ToString(), true);
                                    });
                                    m3905DCanUse = false;
                                    endTest();
                                    return;
                                }

                                LogHelper.Debug("设置 IT6122B 远程控制");
                            //    power1.SetREM();
                                LogHelper.Debug("设置 IT6122B 使能");
                                //设置使能

                                double mesV = double.NaN;
                                double mesC = double.NaN;
                                var task1 = Task.Run(() =>
                                {
                                    LogHelper.Debug("设置 IT-6122B电压12.8V");
                                 //   power1.SetVoltage(13.7);
                                    LogHelper.Debug($"设置模拟电芯N8331单体电压为{12.8 / 4}V");
                                    CommonData.N8331.SetVol4(12.8 / 4);
                                    CommonData.N8331.SetOutPut4(true);
                                });
                                var task2 = Task.Run(() =>
                                {
                                    LogHelper.Debug("设置 M3905D 0V350A");
                                    power2.SetVoltage(0);
                                    mesC = power2.SetCurrentAndStable(0);
                                });
                                Task.WaitAll(task1, task2);
                                LogHelper.Debug("设置NXI-6400采集卡采集电流模式");
                                CommonData.NXI_6400.Reset();
                                CommonMethod.Sleep(200);
                                CommonData.NXI_6400.SetDCAParam(3);
                                //var dl = CommonData.NXI_6400.GetCResult();
                                //TODO:杨总提出对比工装暂时去掉
                                //var gzResult = CanHelper.ReadPower(true, true);
                                //var gzResultV = gzResult.Item1;
                                //var gzResultC = gzResult.Item2;
                                //if (Math.Abs(mesV - gzResultV) > CommonData.PowerVoltageOffset)
                                //{
                                //    //LogHelper.ShowMsg("IT6122B 电压设置失败");
                                //    endTest();return;
                                //}
                                //if (Math.Abs(mesC - gzResultC) > CommonData.PowerCurrentOffset)
                                //{
                                //    LogHelper.ShowMsg("M3905D 设置失败");
                                //    endTest();return;
                                //}



                                #endregion
                            }
                            catch (ThreadAbortException e)
                            {
                                LogHelper.Debug(e.ToString());
                            }
                            catch (Exception e)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                    //MessageBox.Show(ex.ToString());
                                    OpenResult(false);
                                    txt1.Text = CommonData.CurrentBarCode;
                                    txt2.Visibility = Visibility.Visible;
                                    txt2.Text = e.Message;
                                    txt3.Text = "Fail";
                                    bg1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"Red"));
                                    LogHelper.WriteLog(e.ToString(), true);
                                });
                                endTest();
                                return;
                            }
                            #region 测试阶段
                            LogHelper.Debug("-----------------------------------------------------------------业务流程开始-----------------------------------------------------------");
                            try
                            {
                                if (!NewCanHelper.WakeOrSleep(true))
                                {
                                    LogHelper.WriteLog($"工装唤醒失败", true);
                                    endTest(); return;
                                }
                                NewCanHelper.WakeOrSleep(false);
                                if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S" )
                                {
                                    CommonMethod.Sleep(8000);
                                }
                                CommonMethod.StartSendAwaken();
                                if (CommonMethod.Unlock() == false)
                                {
                                    LogHelper.WriteLog($"解锁失败", true);
                                    endTest();
                                    return;
                                }
                                CommonMethod.StartSendUnlock();
                                

                
                          
                                this.Dispatcher.Invoke(() =>
                                {
                                    progress.Value = 0;
                                });

                                //CommonMethod.Sleep(200);
                                ////读取唤醒源
                                //var data = new List<byte>() { 0x01, 0x15 };
                                ////string huanxingFangshi = "";
                                //var temp1 = PlayUDSHelper.Send(data.ToArray(), 0x22);
                                //var t = int.Parse(temp1.Item2[2].ToString("X2")).ToString();

                                //CommonMethod.StopSendAwaken();
                                //CommonMethod.StopSendUnlock();
                                //CommonMethod.Sleep(300);
                                //NewCanHelper.WakeOrSleep(false);


                                //if (PlayUDSHelper.UdsSleep(1) == null)
                                //{
                                //    LogHelper.ShowMsg("休眠失败");
                                //}
                                //var dl = CommonMethod.GetSleep1CurrentFromPower();
                                //CommonMethod.Sleep(60000);
                                
                                    for (int i = 0; i < CommonData.CurrentUserDetects.Count; i++)
                                    {
                                        //    LogHelper.ShowMsg($"{CommonData.TestCount}");
                                        //break;
                                        var item = CommonData.CurrentUserDetects[i];
                                        this.Dispatcher.Invoke(() =>
                                        {
                                            progress.Maximum = CommonData.CurrentUserDetects.Count;
                                            progress.Value++;
                                            currentDetect.Text = item.Item2;
                                        });

                                        var temp = DetectionHelper.GetDetectInstance(item.Item1);
                                        var pars = CommonData.TestParameters.Where(m => item.Item1.Contains(m.MedthodName));
                                        temp.TestFail += testFail;
                                        temp.AddModelAction += addResult;
                                        if (item.Item1 == "CanTRTest")
                                        {
                                            continue;
                                        }

                                        temp.Start(pars);
                                    }
                                

                                    
                            }
                            catch (ThreadAbortException)
                            {

                            }
                            catch (Exception ex)
                            {
                                this.Dispatcher.Invoke(() =>
                                {
                                   
                                    addResult(new TestResult() { ChildrenItem = ex.Message, Result = false});

                                  
                                    return;
                                });

                                
                            }
                            #endregion

                            #region 结束测试

                            endTest();
                            #endregion
                        });
                        tr.IsBackground = true;
                        tr.Start();
                    }
                }
            }
        }

        private void Button11Click(object sender, RoutedEventArgs e)
        {
            //    ExcelHelper.Export();
            ControlCenter controlCenter = new ControlCenter();
            controlCenter.Owner = this;
            controlCenter.ShowDialog();
        }

        private void ClearLog_Click(object sender, RoutedEventArgs e)
        {
            msgItem1.Items.Clear();
            results.Clear();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {

        }

        private void GoToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            
        }

        private void Input_Changed(object sender, TextChangedEventArgs e)
        {
            if (totalNum != null && failNum != null && passNum != null && passRate != null)
            {
                totalNum.Text = int.Parse(failNum.Text) + int.Parse(passNum.Text) + "";
                if (double.Parse(totalNum.Text) == 0)
                {
                    passRate.Text = "0.00";
                }
                else
                {
                    passRate.Text = (double.Parse(passNum.Text) * 1.0 / double.Parse(totalNum.Text) * 100).ToString("0.00") + "";
                }

            }
        }

        private void OpenStatisticsDialog_Click(object sender, RoutedEventArgs e)
        {
            StatisticsDialog statisticsDialog = new StatisticsDialog();
            statisticsDialog.Owner = this;
            if (statisticsDialog.ShowDialog() == true)
            {
                passNum.Text = statisticsDialog.passNum.Text;
                failNum.Text = statisticsDialog.failNum.Text;
                ConfigHelper.PassNum = passNum.Text;
                ConfigHelper.FailNum = failNum.Text;
            }
        }

        private void progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //App.Current.Shutdown();
            uds?.Close();
            operate?.Close();
            deviceOperate?.CloseDevice();
            gun?.Close();
        //    power1?.Close();
            power2?.Close();
            N8331?.Close();
       //    relay?.Close();
           

            //结束所有线程
            CommonMethod.EndAllTask();

            //var urrentProcess = Process.GetCurrentProcess();
            //foreach (var thread in urrentProcess.Threads.Cast<System.Diagnostics.ProcessThread>())
            //{
            //    // 检查线程是否仍然存活
            //    if (thread.ThreadState == System.Diagnostics.ThreadState.Running)
            //    {
            //        //MessageBox.Show($"线程 {thread.Id} 仍然在运行！");
            //    }
            //}

            //关闭其他弹窗
            foreach (Window item in App.Current.Windows)
            {
                item.Close();
            }
        }
        /// <summary>
        /// 关闭结果弹窗
        /// </summary>
        void CloseResult()
        {
            this.Dispatcher.Invoke(() =>
            {
                if (dialog != null && ResultDialogIsOpen == true)
                {
                    dialog.Close();
                    ResultDialogIsOpen = false;
                    this.IsEnabled = true;
                }
            });

        }
        /// <summary>
        /// 打开结果弹窗
        /// </summary>
        /// <param name="isPass"></param>
        void OpenResult(bool isPass)
        {
            if (isStopPressDown)
            {
                return;
            }
            this.Dispatcher.Invoke(() =>
            {
                dialog = new ResultDialog(isPass);
                dialog.Closed += (s, e) =>
                {
                    this.IsEnabled = true;
                };
                dialog.Owner = this;
                ResultDialogIsOpen = true;
                dialog.Show();
                dialog.Activate();
                this.IsEnabled = false;
                if (isPass == false)
                {
                    CommonData.TestResult = false;
                }
            });
        }

        /// <summary>
        /// 结束测试
        /// </summary>
        /// <param name="startTime"></param>
        void endTest()
        {

            LogHelper.Debug("-----------------------------------------------------------------业务流程结束-----------------------------------------------------------");
            //右下角动画复原
            this.Dispatcher.Invoke(() =>
            {
                currentDetect.Text = "结束测试";
            });
            LogHelper.Debug("结束测试");
            string tempBarCode = CommonData.CurrentBarCode;
            CommonData.CurrentBarCode = "";


            //停止发送周期性报文
            //LinHelper.StopPeriodicMessage();
            //LogHelper.Debug("停止发送周期性报文");

            try
            {
                CommonMethod.StopSendAwaken();
                CommonMethod.StopSendUnlock();
               
                CommonMethod.Unlock();
                if (ConfigHelper.UdsParameters.jiesuo == "RC_PK2C")
                {
                    var temp = PlayUDSHelper.UdsSleep(1);
                    if (temp == null)
                    {
                  //      addResult(new TestResult() { ChildrenItem = "一级休眠", Result = false });
                    }
                    else
                    {
                 //       addResult(new TestResult() { ChildrenItem = "一级休眠", Result = null });
                    }
                }
                else
                {
                    var temp = PlayUDSHelper.UdsSleep(2);
                    if (temp == null)
                    {
                 //       addResult(new TestResult() { ChildrenItem = "二级休眠", Result = false });
                    }
                    else
                    {
                 //       addResult(new TestResult() { ChildrenItem = "二级休眠", Result = null });
                    }
                }



                
            }
            catch (Exception)
            {

            }

            NewCanHelper.InitAllRelays();

            #region 显示判断总结果
            if (isStopPressDown == false)
            {
                var failResult = testResults.FirstOrDefault(m => m.Result == false);
                this.Dispatcher.Invoke(() =>
                {
                    bool b = failResult == null&&CommonData.CurrentUserDetects.Count==CommonData.TestCount;//2025-5-19新增判断总项数
                    this.Dispatcher.Invoke(() =>
                    {
                        if (this.IsEnabled == true)
                        {
                            OpenResult(b);
                            txt1.Text = tempBarCode;
                            txt2.Visibility = Visibility.Collapsed;
                            txt3.Text = b ? "Pass" : "Fail";
                            bg1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"{(b ? "Green" : "Red")}"));
                        }
                    });
                    if (dialog.IsPasss)
                    {
                        passNum.Text = int.Parse(passNum.Text) + 1 + "";
                        ConfigHelper.PassNum = passNum.Text;
                    }
                    else
                    {
                        failNum.Text = int.Parse(failNum.Text) + 1 + "";
                        ConfigHelper.FailNum = failNum.Text;
                    }
                    CommonData.TestResult = b;
                });

                
                addResult(new TestResult() { ChildrenItem = "测试结束", Result = null });
            }




            #endregion

            //CanHelper.StopTest();
            NewCanHelper.EndTest();
          //  NewCanHelper.RESET(1);
         //   CommonMethod.StopSendAwaken();
          //  CommonMethod.StopSendUnlock();

           

            

            //CanHelper.AwaitQiTingUp();
            CommonData.IsTesting = false;

            LogHelper.Debug("关闭N8331 电芯1-4 输出使能");
            if (N8331.SetOutPut4(false) == false)
            {
                throw new Exception("N8331 电芯1-4 关闭输出使能失败");
            }
            //double fOutVoltage = N8331.GetVol4().Sum();




           

            DateTime endtTime = DateTime.Now;

            this.Dispatcher.Invoke(() =>
            {
                //useTime.Text = (endtTime - startTime).TotalSeconds.ToString("0.000");
            });

            this.Dispatcher.Invoke(() =>
            {
                //ExcelHelper.Export(reportList);
            });
            string useTimeStr = "";
            //右下角动画复原
            this.Dispatcher.Invoke(() =>
            {
                border2.Visibility = Visibility.Visible;
                border4.Visibility = Visibility.Visible;
                border1.Visibility = Visibility.Visible;
                border5.Visibility = Visibility.Visible;
                currentDetect.Text = "测试完成";
                useTimeStr = useTime.Text;
            });
            CommonMethod.EndWriteFrame();
            if (isStopPressDown == false)
            {
                this.Dispatcher.Invoke(() =>
                {
                    CommonData.TestResult = txt3.Text.ToLower() == "Pass".ToLower() ? true : false;
                });
                ExcelHelper.Export1(testResults, useTimeStr);
                var temp = testResults.Select(m => m).ToList();
                Task.Run(() =>
                {
                    try
                    {
                        TestResultDal testResultDal = new TestResultDal();
                        int count = testResultDal.InsertModels(temp);
                        if (count > 0)
                        {
                            LogHelper.Debug($"数据入库成功，总数据{temp.Count}条，写入{count}条");
                        }
                        else
                        {
                            LogHelper.Debug($"数据入库失败，总数据{temp.Count}条，写入{count}条");
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Debug($"数据库操作失败");
                        LogHelper.WriteLog(ex.ToString(), true);
                    }
                });
            }
            testResults.Clear();
           
            LogHelper.Init();
            isListen = true;//开始下一次监听
        }
        void addResult(TestResult model)
        {

            this.Dispatcher.Invoke(() =>
            {
                model.BoardNum = CommonData.CurrentBarCode;
                var modelTemp= JsonConvert.DeserializeObject<TestResult>(JsonConvert.SerializeObject(model));
                model.Id = results.Count + 1;
                testResults.Add(modelTemp);
                results.Add(model);
                if (resultShowGrid.Items.Count > 0)
                {
                    var border = VisualTreeHelper.GetChild(resultShowGrid, 0) as Decorator;
                    if (border != null)
                    {
                        var scroll = border.Child as ScrollViewer;
                        if (scroll != null)
                        {
                            scroll.ScrollToEnd();//最后一行
                            //scroll.ScrollToHome();//第一行
                            //scroll.ScrollToLeftEnd();//第一列
                            //scroll.ScrollToRightEnd();//最后一列
                        }
                    }
                }
            });
            if (CommonData.CurrentTestType == TestType.生产测试)
            {
                if (model.Result == false)
                {
                    LogHelper.Debug($"{results.IndexOf(model)+1}、不合格项目：{model.ChildrenItem}，合格值：{model.QualifiedValue}，实测值{model.MeasureVal}");
                    DateTime start = DateTime.Now;
                    this.Dispatcher.Invoke(() =>
                    {
                        start = DateTime.Parse(prjStartTime.Text);
                        OpenResult(false);
                        txt1.Text = CommonData.CurrentBarCode;
                        txt2.Visibility = Visibility.Visible;
                        txt2.Text = $"{results.IndexOf(model) + 1}、不合格项目：{model.ChildrenItem}";
                        txt3.Text = "Fail";
                        bg1.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString($"Red"));
                    });
                    endTest();
                    tr.Abort();
                    return;
                }
            }
        }

        void testFail(string msg)
        {

        }


        void InitProtocolComboBox()
        {
            string protocolDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "测试协议");
            if (System.IO.Directory.Exists(protocolDir))
            {
                string[] jsonFiles = System.IO.Directory.GetFiles(protocolDir, "*.json");
                foreach (string file in jsonFiles)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    protocolComboBox.Items.Add(fileName);
                }
            }

            string lastProtocol = AllData.ConfigHelper.GetValue("Protocol", "LastSelected");
            if (!string.IsNullOrEmpty(lastProtocol) && protocolComboBox.Items.Contains(lastProtocol))
            {
                protocolComboBox.SelectedItem = lastProtocol;
            }
        }

        private void ProtocolComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (protocolComboBox.SelectedItem == null)
                return;

            string protocolName = protocolComboBox.SelectedItem.ToString();
            string protocolDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "测试协议");
            string jsonFilePath = System.IO.Path.Combine(protocolDir, protocolName + ".json");

            if (System.IO.File.Exists(jsonFilePath))
            {
                try
                {
                    string json = System.IO.File.ReadAllText(jsonFilePath);
                    var config = JsonConvert.DeserializeObject<FCT.Dialog.ParSettingDialog.UdsConfig>(json);

                    FCT.Dialog.ParSettingDialog.UdsParameterMapper.UpdateStaticParameters(config);

                    SaveUdsConfigToFile(config);

                    string iniDir = System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "配置文件");
                    string iniFilePath = System.IO.Path.Combine(iniDir, protocolName + ".ini");
                    if (System.IO.File.Exists(iniFilePath))
                    {
                        LoadIniConfig(iniFilePath);
                    }

                    SelectAllTestItems();

                    AllData.ConfigHelper.SetValue("Protocol", "LastSelected", protocolName);

                    LogHelper.ShowMsg($"协议配置已成功导入: {protocolName}");
                    //  MessageBox.Show($"协议配置已成功导入: {protocolName}", "导入成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导入协议配置时出错:\n{ex.Message}", "导入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        void SaveUdsConfigToFile(FCT.Dialog.ParSettingDialog.UdsConfig config)
        {
            try
            {
                Type configType = config.GetType();
                Type staticType = typeof(AllData.ConfigHelper.UdsParameters);

                foreach (var prop in configType.GetProperties())
                {
                    var attr = prop.GetCustomAttribute<AllData.ConfigHelper.UdsParameterAttribute>();
                    if (attr != null)
                    {
                        string value = prop.GetValue(config)?.ToString() ?? string.Empty;
                        AllData.ConfigHelper.SetValue("UdsParameters", attr.TextBoxName, value);
                    }
                }

                string configPath = System.IO.Directory.GetCurrentDirectory() + "\\config.json";
                string json = JsonConvert.SerializeObject(config, Formatting.Indented);
                string configDir = System.IO.Path.GetDirectoryName(configPath);
                if (!System.IO.Directory.Exists(configDir))
                {
                    System.IO.Directory.CreateDirectory(configDir);
                }
                string tempPath = System.IO.Path.Combine(configDir, System.IO.Path.GetRandomFileName());
                System.IO.File.WriteAllText(tempPath, json, System.Text.Encoding.UTF8);
                System.IO.File.Replace(tempPath, configPath, null);
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"保存UDS配置文件失败: {ex.Message}", true);
            }
        }

        void LoadIniConfig(string iniFilePath)
        {
            try
            {
                List<TestParameter> parameters = new List<TestParameter>();
                string[] lines = System.IO.File.ReadAllLines(iniFilePath);
                TestParameter currentParam = null;

                foreach (string line in lines)
                {
                    if (line.StartsWith(";") || string.IsNullOrWhiteSpace(line))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        if (currentParam != null)
                        {
                            parameters.Add(currentParam);
                        }
                        currentParam = new TestParameter();
                        int id;
                        if (int.TryParse(line.Trim('[', ']'), out id))
                            currentParam.Id = id;
                    }
                    else if (currentParam != null && line.Contains("="))
                    {
                        int index = line.IndexOf('=');
                        string key = line.Substring(0, index).Trim();
                        string value = line.Substring(index + 1).Trim();

                        switch (key)
                        {
                            case "TestItem":
                                currentParam.TestItem = value;
                                break;
                            case "MedthodName":
                                currentParam.MedthodName = value;
                                break;
                            case "ChildrenItem":
                                currentParam.ChildrenItem = value;
                                break;
                            case "QualifiedValue":
                                double qv;
                                if (double.TryParse(value, out qv))
                                    currentParam.QualifiedValue = qv;
                                break;
                            case "MeasurementStandard":
                                currentParam.MeasurementStandard = value;
                                break;
                            case "MinVal":
                                double min;
                                if (double.TryParse(value, out min))
                                    currentParam.MinVal = min;
                                break;
                            case "MaxVal":
                                double max;
                                if (double.TryParse(value, out max))
                                    currentParam.MaxVal = max;
                                break;
                            case "Extend1":
                                currentParam.Extend1 = value;
                                break;
                            case "Extend2":
                                currentParam.Extend2 = value;
                                break;
                            case "Extend3":
                                currentParam.Extend3 = value;
                                break;
                        }
                    }
                }
                if (currentParam != null)
                {
                    parameters.Add(currentParam);
                }

                CommonData.TestParameters = parameters;

                if (TestParDal.Reset(parameters))
                {
                    CommonMethod.RefreshDatabaseData("TestParameter");
                    Key_ValDal.UpdateConfigTime(CommonData.CurrentUser.UserName);
                }
            }
            catch (Exception ex)
            {
                LogHelper.WriteLog($"加载INI配置文件失败: {ex.Message}", true);
            }
        }

        void SelectAllTestItems()
        {
            if (CommonData.CurrentUser != null)
            {
                string jiesuo = ConfigHelper.UdsParameters.jiesuo;
                string detectItems = "";

                if (jiesuo == "SERES_V32")
                {
                    detectItems = "CanTRTest,EOLClose,LBMSRead,PCBTemperature,MOSControl,VoltageTest,CurrentCalibration,CurrentCollection,CellTemperature,WorkingCurrent,BalancedTest,CurrentLimiting,SleepingCurrent,WakeSourceTest,MOSSleepKeep,EEPROMTest,ClearAH,SOCAndSOH,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "ChangAn_C390" || jiesuo == "ChangAn_B216")
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,ModuleTemperature,VoltageTest,MOSControl,BalancedTest,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeCalibration,MOSSleepKeep,WakeSourceTest,SleepingCurrent,EEPROMTest,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "AVATR_E15")
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,MOSControl,ModuleTemperature,VoltageTest,BalancedTest,MOSSleepKeep,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,EEPROMTest,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "T09_M")
                {
                    detectItems = "CanTRTest," +
               "ProduceRead," +
               "PCBTemperature," +

               "MOSControl," +
               "ModuleTemperature," +
                "Cur0A," +
               "VoltageTest," +

               "BalancedTest," +
               "CurrentLimiting," +

               "CurrentCalibration," +
               "CurrentCollection," +
               "WorkingCurrent," +
                 "MOSSleepKeep," +
               "WakeSourceTest," +
               "SleepingCurrent," +


               "ClearAH," +
               "AHClearConfirm,"+
               "SOCAndSOH," +
               "LBMSWrite," +
               "ClearDTC," +
               "BalancedAndAFE";
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "WL_F710S")
                {
                    detectItems = "CanTRTest," +
                   "ProduceRead," +
                   "PCBTemperature," +
                   "MOSControl," +
                   "ModuleTemperature," +
                   "VoltageTest," +
                   "BalancedTest," +
                   "CurrentCalibration," +
                   "CurrentCollection," +
                   "WorkingCurrent," +
                   "WakeCalibration," +
                   "MOSSleepKeep," +
                   "WakeSourceTest," +
                   "SleepingCurrent," +
                   "EEPROMTest," +
                   "ClearAH," +
                   "SOCAndSOH," +
                   "LBMSWrite," +
                   "ClearDTC," +
                   "BalancedAndAFE";
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "QR_C1068")
                {
                    detectItems = "ModuleTemperature," +
                        "PCBTemperature," +
                   "CurrentCalibration," +
                        "FenLiuR," +
                   "ProduceRead," +
                    "VoltageTest," +
                   "BalancedTest," +
                   "MOSControl," +
                   "CurrentLimiting," +
                   "WorkingCurrent," +
                   "WakeSourceTest," +
                   "SleepingCurrent," +
                   "CurrentCollection," +
                   "endcs";
                }
                else if (jiesuo == "RC_PK2C")
                {
                    detectItems = "ProduceRead,PCBTemperature,MOSControl,ModuleTemperature,VoltageTest,BalancedTest,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "RC_PK1B")
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,MOSControl,ModuleTemperature,VoltageTest,BalancedTest,CurrentLimiting,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "DF_DH1" || jiesuo == "DF_XY-A" || jiesuo == "VOYAH_H97" || jiesuo == "VOYAH_H47A")
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,MOSControl,ModuleTemperature,Discur,VoltageTest,BalancedTest,CurrentLimiting,MOSSleepKeep,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,EEPROMTest,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "WL_C1046")
                {
                    detectItems = "ProduceRead,PCBTemperature,MOSControl,ModuleTemperature,VoltageTest,BalancedTest,CurrentLimiting,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeCalibration,WakeSourceTest,SleepingCurrent,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "CHERY_E0Y" || jiesuo == "GreatWall_C03" || jiesuo == "HQ_E702" || jiesuo == "HQ_C1079")
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,MOSControl,VoltageTest,BalancedTest,CurrentLimiting,MOSSleepKeep,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,EEPROMTest,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (jiesuo == "VOYAH_H56C" || jiesuo == "XP_12V")
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,MOSControl,VoltageTest,BalancedTest,CurrentLimiting,MOSSleepKeep,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,EEPROMTest,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }
                else if (ConfigHelper.UdsParameters.jiesuo == "CN_D7")
                {
                    detectItems = "CanTRTest," +
                   "ProduceRead," +
                   "PCBTemperature," +
                   "MOSControl," +
                   "ModuleTemperature," +
                   "VoltageTest," +
                   "BalancedTest," +
                   "CurrentLimiting," +
                   "CurrentCalibration," +
                   "CurrentCollection," +
                   "WorkingCurrent," +
                   "WakeSourceTest," +
                   "SleepingCurrent," +
                   "ClearAH," +
                   "SOCAndSOH," +
                   "LBMSWrite," +
                   "ClearDTC," +
                   "BalancedAndAFE";
                }
                else
                {
                    detectItems = "CanTRTest,ProduceRead,PCBTemperature,MOSControl,ModuleTemperature,VoltageTest,BalancedTest,CurrentLimiting,MOSSleepKeep,CurrentCalibration,CurrentCollection,WorkingCurrent,WakeSourceTest,SleepingCurrent,EEPROMTest,ClearAH,SOCAndSOH,LBMSWrite,ClearDTC,BalancedAndAFE";
                }

                var tempList = detectItems.Split(',').ToList();
                bool inserted = false;
                if (!tempList.Contains("LBMSWrite"))
                {
                    inserted = true;
                }

                var newCollection = tempList
                    .SelectMany(s =>
                    {
                        if (!inserted && s == "SleepingCurrent")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s };
                        }
                        if (!inserted && s == "WakeSourceTest")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s };
                        }
                        if (!inserted && s == "EEPROMTest")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s };
                        }
                        if (!inserted && s == "SOCAndSOH")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s };
                        }
                        if (!inserted && s == "ClearDTC")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s };
                        }
                        if (!inserted && s == "LBMSWrite")
                        {
                            inserted = true;
                            return new[] { "LBMSWriteBefor", s };
                        }
                        return new[] { s };
                    })
                    .ToList();

                string finalItems = string.Join(",", newCollection);
                CommonData.CurrentUser.TestItem = finalItems;

                if (UserDal.UpdateTestItem(CommonData.CurrentUser.Id, finalItems))
                {
                    CommonMethod.RefreshDatabaseData("Sys_User");
                    Key_ValDal.UpdateConfigTime(CommonData.CurrentUser.UserName);
                }
            }
        }
        public void ShowMsg(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                msgItem1.Items.Add(msg);
                //msgItem.ItemsSource
            });
        }

        private void DeviceCheck_Click(object sender, RoutedEventArgs e)
        {
            

            if (N8331 == null) {
                MessageBox.Show("N8331电芯模拟器未连接设备");
                return;
            }
            else
            {
                var temp = N8331.SelectVol(1);
                if (temp == "")
                {
                    MessageBox.Show("N8331电芯模拟器通信失败");
                    return;
                }
            }

            if (operate == null)
            {
                MessageBox.Show("工装Can未连接设备");
                return;
            }
            else
            {
                //if (NewCanHelper.GetInternalTotalVoltage() == null)
                //{
                //    if (CanHelper.Conneted() == false)
                //    {
                //        MessageBox.Show("工装Can通信失败,请检查设备连接后重启软件");
                //        return;
                //    }
                //}
                //CanHelper.DisConted();
            }
        }

        private void ParSetting_Click(object sender, RoutedEventArgs e)
        {
            ParSettingDialog parSettingDialog = new ParSettingDialog();
            parSettingDialog.Owner = this;
            parSettingDialog.ShowDialog();
        }

        private void UserSetting_Click(object sender, RoutedEventArgs e)
        {
            UserSettingDialog userSettingDialog = new UserSettingDialog();
            userSettingDialog.Owner = this;
            userSettingDialog.ShowDialog();
        }

        /// <summary>
        /// 开始测试准备等待按钮按下
        /// </summary>
        void StartListen()
        {
            try
            {
                //死循环判断按钮按下并且扫码之后退出
                while (true)
                {
                    var btnStatus = NewCanHelper.GetButtonStatus();
                    if (btnStatus.Item1 && btnStatus.Item2)
                    {
                          var temp = gun.SendAndWaitForResponse().Split(',')[1];
                        //var  temp = "2807030309-220182D1250317023529";
                        #region 扫码枪扫码内容处理并判断
                        //if (temp.Contains("-") == false)
                        //{
                        //    //this.Dispatcher.Invoke(() =>
                        //    //{
                        //    //    MessageBox.Show(this, "无效条码");
                        //    //});
                        //    isListen = true;
                        //    return;
                        //}
                        var barV = CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "条码");
                        if (barV == null)
                        {
                            MessageBox.Show("请检查数据库条码配置，并重启软件同步数据库数据");
                            isListen = true;
                            return;
                        }
                        //if (temp.Split('-')[0] != barV.CommValue)
                        //{
                        //    MessageBox.Show($"无效条码:{temp}");
                        //    isListen = true;
                        //    return;
                        //}

                        //var httpResult = HttpHelper.VerBarCode(temp);
                        //if (httpResult.Item1)
                        //{
                        //    //MessageBox.Show($"{temp}验证通过");
                        //    LogHelper.Debug($"{temp}验证通过");
                        //}
                        //else
                        //{
                        //    this.Dispatcher.Invoke(() =>
                        //    {
                        //        MsgDialog msgDialog = new MsgDialog();
                        //        msgDialog.ShowTxt($"{temp}验证失败，错误原因:{httpResult.Item2}");
                        //        msgDialog.Owner = this;
                        //        msgDialog.ShowDialog();
                        //    });
                        //    isListen = true;
                        //    return;
                        //}
                        //if (ConfigHelper.UdsParameters.jiesuo == "HQ_E702" || ConfigHelper.UdsParameters.jiesuo == "T09_M")
                        //{

                        //}
                        else if (temp.Length < 25)
                        {
                            isListen = true;
                            return;
                        }
                        this.Dispatcher.Invoke(() =>
                        {
                            if (CommonData.IsTesting)
                            {
                                nextBoard.Text = temp;
                            }
                            else
                            {
                                CommonData.CurrentBarCode = temp;
                                currentBoard.Text = temp;
                                nextBoard.Text = "";
                            }
                        });
                        #endregion
                        break;
                    }
                    Thread.Sleep(100);
                }
                //治具下压
                NewCanHelper.ControlZhiJu(true);
                while (true)
                {
                    var status = (NewCanHelper.GetButtonStatus());

                    //如果治具到位并且按钮按下则亮灯开始测试
                    if (status.Item1 == true && status.Item2 == true)
                    {
                        if (NewCanHelper.GetZhiJuStatus() == true)
                        {
                            Thread.Sleep(100);
                            NewCanHelper.ControlButtonLamp(true);
                            this.Dispatcher.Invoke(() =>
                            {
                                msgItem1.Items.Clear();
                                results.Clear();
                            });

                            StartTest();
                            tr.Join();
                            if (tr != null)
                            {
                                tr.Abort();
                                tr = null;
                            }
                            isListen = true;
                            return;
                        }
                    }
                    //按钮松开则治具抬起，并开始下一次检测
                    else
                    {
                        
                        NewCanHelper.ControlButtonLamp(false);
                        Thread.Sleep(100);
                        NewCanHelper.ControlZhiJu(false);
                        isListen = true;
                        return;
                    }
                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                isListen = true;
            }
        }

      
    }
}
