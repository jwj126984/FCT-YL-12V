using Common;
using AllData;
using Model;
using Models_C.Dal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace FCT
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        bool isUseLogin = true;
        /// <summary>
        /// 以太网连接状态变化
        /// </summary>

        public static event Action<bool> OnEthernetChanged;
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            LogHelper.SetApp(this);
            //if (isUseLogin)
            //{
            //    this.StartupUri = new Uri("Dialog/LoginNew.xaml", UriKind.Relative);
            //}

            //UI线程未捕获异常处理事件
            this.DispatcherUnhandledException += new DispatcherUnhandledExceptionEventHandler(App_DispatcherUnhandledException);
            //Task线程内未捕获异常处理事件
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            //非UI线程未捕获异常处理事件
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            Init();
        }

        void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true; //把 Handled 属性设为true，表示此异常已处理，程序可以继续运行，不会强制退出      

                if (this.MainWindow != null)
                {
                    //此时程序出现严重异常，将强制结束退出
                    MessageBox.Show(this.MainWindow, "UI线程异常:" + e.Exception.Message);
                    this.Shutdown();
                }
                else
                {
                    //此时程序出现严重异常，将强制结束退出
                    MessageBox.Show("UI线程异常:" + e.Exception.Message);
                    this.Shutdown();
                }
            }
            catch (Exception ex)
            {
                if (this.MainWindow != null)
                {
                    //此时程序出现严重异常，将强制结束退出
                    MessageBox.Show(this.MainWindow, "UI线程发生致命错误！");
                    this.Shutdown();
                }
                else
                {
                    //此时程序出现严重异常，将强制结束退出
                    MessageBox.Show("UI线程发生致命错误！");
                    this.Shutdown();
                }
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {

            StringBuilder sbEx = new StringBuilder();
            if (e.IsTerminating)
            {
                sbEx.Append("非UI线程发生致命错误");
            }
            sbEx.Append("非UI线程异常：");
            if (e.ExceptionObject is Exception)
            {
                sbEx.Append(((Exception)e.ExceptionObject).Message);
            }
            else
            {
                sbEx.Append(e.ExceptionObject);
            }
            App.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(this.MainWindow, sbEx.ToString());
                LogHelper.Debug($"UI线程发生致命错误:{e.ToString()}");
            });

        }

        void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                //task线程内未处理捕获
                MessageBox.Show(this.MainWindow, "Task线程异常：" + e.Exception.Message);

                LogHelper.Debug($"Task线程异常:{e.ToString()}");
            });

            e.SetObserved();//设置该异常已察觉（这样处理后就不会引起程序崩溃）
        }

        public void MessageBoxShow(string str)
        {
            MessageBox.Show(str);
        }


        // 监视网络连接断开
        ManagementEventWatcher disconnectWatcher = new ManagementEventWatcher(@"root\wmi","SELECT * FROM MSNdis_StatusMediaDisconnect");
        // 监视网络连接建立
        ManagementEventWatcher connectWatcher = new ManagementEventWatcher(@"root\wmi","SELECT * FROM MSNdis_StatusMediaConnect");
        /// <summary>
        /// 程序初始化后台加载数据
        /// </summary>
        void Init()
        {
            int saveMouth = 0;
            if (string.IsNullOrEmpty(ConfigHelper.DBSaveTime))
            {
                ConfigHelper.DBSaveTime = "72";
                saveMouth = 72;
            }
            else
            {
                saveMouth=int.Parse(ConfigHelper.DBSaveTime);
            }
            Task.Run(async () =>
            {
                try
                {
                    while (true) {
                        TestResultDal.DeleteByTime(saveMouth);
                        await Task.Delay(new TimeSpan(1,0,0,0));
                    }
                }
                catch (Exception ex) {
                    MessageBox.Show(ex.ToString());
                }
            });
            //try
            //{
            //    CommonMethod.RefreshDatabaseData();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.ToString());
            //    System.Environment.Exit(0);
            //}
            
            disconnectWatcher.EventArrived += DisconnectWatcher_EventArrived;
            // 启动监视
            disconnectWatcher.Start();

           
            connectWatcher.EventArrived += ConnectWatcher_EventArrived;
            // 启动监视
            connectWatcher.Start();

            void ConnectWatcher_EventArrived(object sender,
            EventArrivedEventArgs e)
            {
                string description = (string)e.NewEvent.GetPropertyValue("InstanceName");
                //MessageBox.Show($"网络已连接：{description}");
                if(description.ToLower().Contains("ethernet")|| description.ToLower().Contains("gbe") || description.ToLower().Contains("realtek"))
                {
                    OnEthernetChanged?.Invoke(true);
                    //MessageBox.Show($"网络已连接：{description}");
                }
            }

            void DisconnectWatcher_EventArrived(object sender,
            EventArrivedEventArgs e)
            {
                string description = (string)e.NewEvent.GetPropertyValue("InstanceName");
                if (description.ToLower().Contains("ethernet") || description.ToLower().Contains("gbe") || description.ToLower().Contains("realtek"))
                {
                    OnEthernetChanged?.Invoke(false);
                    //MessageBox.Show($"网络已断开：{description}");
                }
                //MessageBox.Show($"网络已断开：{description}");
            }
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            disconnectWatcher.Stop();
            connectWatcher.Stop();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // 处理参数
            if (e.Args.Length > 0)
            {
                try
                {
                    var user = JsonConvert.DeserializeObject<Sys_User>(e.Args[0]);
                    if ((user != null))
                    {
                        try
                        {
                            CommonData.CurrentUser = user;
                        }
                        catch (Exception ex)
                        {
                            //MessageBox.Show(ex.ToString());
                            System.Environment.Exit(0);
                        }
                    }
                    else
                    {
                        MessageBox.Show("未知错误");
                        App.Current.Shutdown();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    App.Current.Shutdown();
                }

            }
            else
            {
                if (isUseLogin)
                {
                    this.StartupUri = new Uri("Dialog/LoginNew.xaml", UriKind.Relative);
                }
            }
        }
    }
}
