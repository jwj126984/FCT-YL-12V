using Model;
using Models_C.Dal;
using AllData;
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
using Models_C;
using Common;
namespace FCT.Dialog
{
    /// <summary>
    /// LoginNew.xaml 的交互逻辑
    /// </summary>
    public partial class LoginNew : Window
    {
        public LoginNew()
        {
            InitializeComponent();
            acc.Text = ConfigHelper.LastAccount;
            board.Text = ConfigHelper.SelectType;
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
           
            var user = UserDal.Login(acc.Text, pwd.Password);
            if (user != null)
            {
                try
                {
                    CommonData.CurrentUser = user;
                    //CommonData.BoardName = user.TestBoard;
                    //CommonData.UserRole = (Role)user.UserType;
                    ConfigHelper.LastAccount = acc.Text;
                    ConfigHelper.SelectType = board.Text;
                    DalBase dalBase = new DalBase();
                    CommonMethod.RefreshDatabaseData();
                    var main = new MainWindow();

                    App.Current.MainWindow = main;
                    this.Visibility = Visibility.Collapsed;
                    App.Current.MainWindow.Show();
                    this.Close();
                }
                catch (Exception ex) {
                    LogHelper.Debug(ex.ToString());
                    Environment.Exit(0);
                }
               
            }
            else
            {
                MessageBox.Show("账号密码错误");
            }
        }

        private void acc_TextChanged(object sender, TextChangedEventArgs e)
        {
            var accTemp = acc.Text;
            Task.Run(() =>
            {
                try
                {
                    var temp = UserDal.GetInfoByAcc(accTemp);
                    this.Dispatcher.Invoke(() =>
                    {
                        if (temp.Item1 != Role.None)
                        {
                            userType.Text = temp.Item1.ToString();
                            userName.Text = temp.Item3 ?? "无";
                        }
                        else
                        {
                            userType.Text = "无";
                            userName.Text = "无";
                        }
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show( ex.Message);
                }


            });

        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        

     
    }
}
