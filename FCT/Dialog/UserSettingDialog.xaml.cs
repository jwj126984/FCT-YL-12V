using Model;
using Models_C.Dal;
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
    /// UserSettingDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UserSettingDialog : Window
    {
        public UserSettingDialog()
        {
            InitializeComponent();
            Refresh();
            //roleCbx.ItemsSource = Role;
        }
        void Refresh()
        {
            var users = UserDal.GetAllUser();
            var list = new List<UserModel>();
            if (roleCbx.SelectedIndex != 0&& roleCbx.SelectedIndex != -1)
            {
                users = users.Where(m => m.UserType == roleCbx.SelectedIndex).ToList();
            }
            if (string.IsNullOrEmpty(nameBox.Text) == false)
            {
                users = users.Where(m => m.UserName.Contains(nameBox.Text)).ToList();
            }
            if (string.IsNullOrEmpty(accBox.Text) == false)
            {
                users = users.Where(m => m.Account.Contains(accBox.Text)).ToList();
            }
            foreach (var item in users)
            {
                list.Add(new UserModel(item));
            }
            userItemControl.ItemsSource = list;
        }

        private void Seach_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            if (roleCbx.SelectedIndex == 0 || roleCbx.SelectedIndex == -1)
            {
                MessageBox.Show(this, "请选择身份");
                return;
            }
            if (string.IsNullOrEmpty(nameBox.Text))
            {
                MessageBox.Show(this, "请输入姓名");
                return;
            }
            if (string.IsNullOrEmpty(accBox.Text))
            {
                MessageBox.Show(this, "请输入账号");
                return;
            }
            if (UserDal.AddUser(roleCbx.SelectedIndex, nameBox.Text, accBox.Text))
            {
                MessageBox.Show("添加成功");
                roleCbx.SelectedIndex = 0;
                nameBox.Text = "";
                accBox.Text = "";
                Refresh();
            }
            
        }

        private void Update_Click(object sender, RoutedEventArgs e)
        {
            var userModel = (UserModel)userItemControl.SelectedItem;
            var model = userModel.User;
            model.UserName = userModel.UserName;
            if (UserDal.Update(model))
            {
                MessageBox.Show("修改成功");
            }
        }

        private void DisEnable_Click(object sender, RoutedEventArgs e)
        {
            var userModel = (UserModel)userItemControl.SelectedItem;
        }

        private void userItemControl_CurrentCellChanged(object sender, EventArgs e)
        {
            var dataGrid = sender as DataGrid;
            dataGrid.CommitEdit(); // 提交编辑
        }

        private void ResetPwd_Click(object sender, RoutedEventArgs e)
        {
            var userModel = (UserModel)userItemControl.SelectedItem;
            if (UserDal.UpdatePwd(userModel.User.Id, "123456"))
            {
                MessageBox.Show(this, "重置成功，新密码：123456");
            }
        }
    }
    class UserModel
    {
        public UserModel()
        {

        }
        public UserModel(Sys_User model)
        {
            Id = model.Id;
            Role = (Role)model.UserType;
            UserName = model.UserName;
            Account = model.Account;
            Pwd = model.Pwd;
            _User = model;
        }

        public int Id { get; set; }

        public Role Role { get; set; }

        public string UserName { get; set; }

        public string Account { get; set; }

        public string Pwd { get; set; }

        private Sys_User _User;
        public Sys_User User { get { return _User; } }
    }
}
