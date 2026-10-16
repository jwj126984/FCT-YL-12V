using Common;
using Common.Can;
using Common.UDS;
using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestDemo
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        DeviceOperate deviceOperate = new DeviceOperate();
        CanOperate operate = new CanOperate();
        UDSOperate product = new UDSOperate();

        CanOperate uds = new CanOperate();
        public MainWindow()
        {
            InitializeComponent();
            LogHelper.SetMainWindow(this);

            if (deviceOperate.OpenDevice() == false)
            {
                MessageBox.Show("设备打开失败");
            }
            //// 产品的Can
            //if (product.Start(deviceOperate.GetDeviceHandle, 0, 500, 2000) == false)
            //{
            //    MessageBox.Show("产品打开失败");
            //    return;
            //}
            if (operate.Start(deviceOperate.GetDeviceType, deviceOperate.GetDeviceHandle, 1, 500, 500) == false)
            {
                MessageBox.Show("工装板打开失败");
                return;
            }

            if (uds.Start(deviceOperate.GetDeviceType, deviceOperate.GetDeviceHandle, 0, 500, 2000) == false)
            {
                MessageBox.Show("产品打开失败");
                return;
            }

            #region 初始化公共的静态类
            CanHelper.SetCan(operate);
            //UdsHelper.SetUds(product);
            ZCData.SetCP(uds);
            CommonMethod.SetProduct(uds);
            CommonMethod.SetWork(operate);

            PlayUDSHelper.SetUDS(uds);
            #endregion
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            msgItem.ItemsSource = null;
            
        }
        public void ShowMsg(string msg)
        {
            this.Dispatcher.Invoke(() =>
            {
                msgItem.Items.Add(msg);
                //msgItem.ItemsSource
            });
        }
    }
}
