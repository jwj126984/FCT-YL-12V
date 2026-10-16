using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FCT.Dialog
{
    /// <summary>
    /// ResultDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ResultDialog : Window
    {
        public bool _IsPasss;
        public bool IsPasss
        {
            get
            {
                return _IsPasss;
            }
        }
        public ResultDialog(bool isPass)
        {
            InitializeComponent();
            if (isPass)
            {
                txt.Text = "PASS";
                txt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Green"));
            }
            else
            {
                txt.Text = "FAIL";
                txt.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("Red"));
            }
            _IsPasss = isPass;
        }

        private void bg_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.Close();
        }
    }
}
