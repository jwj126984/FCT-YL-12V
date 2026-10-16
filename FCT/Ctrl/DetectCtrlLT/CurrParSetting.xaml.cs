using Common;
using FCT.Ctrl.PlayCtrl;
using FCT.PConvert;
using Model;
using Models_C.Dal;
using Newtonsoft.Json;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FCT.Ctrl.DetectCtrlLT
{
    /// <summary>
    /// CurrParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class CurrParSetting : UserControl,IDetectCtrlBase
    {
        IEnumerable<TestParameter> parameters = JsonConvert.DeserializeObject<List<TestParameter>>(JsonConvert.SerializeObject(CommonData.TestParameters?.Where(m => m.MedthodName == "CurrentCollection"))) ?? new List<TestParameter>();
        public CurrParSetting()
        {
            InitializeComponent();
            foreach (var item in parameters)
            {
                pars.Add(_Create(item));
            }
        }
        List<TestParameter> pars = new List<TestParameter>();
        TestParameter _Create(TestParameter par)
        {
            TestParameter model = new TestParameter();

            DockPanel dockPanel = new DockPanel();

            DockPanel dockPanelChild = new DockPanel();
            dockPanelChild.Style = (Style)Resources["item"];
            TextBlock textBlock = new TextBlock();
            textBlock.Style = (Style)Resources["blockItem"];
            textBlock.Text = "电流点";
            dockPanelChild.Children.Add(textBlock);

            DecimalTextBox decimalTextBox = new DecimalTextBox();

            decimalTextBox.TextChanged += (s, e) =>
            {
                if (double.TryParse(decimalTextBox.Text, out double d))
                {
                    model.QualifiedValue = d;
                }
                else
                {
                    model.QualifiedValue = 0;
                }
            };
            model.Id = par.Id;
            decimalTextBox.Text = par.QualifiedValue.Value.ToString();
            dockPanelChild.Children.Add(decimalTextBox);

            textBlock = new TextBlock();
            textBlock.Text = "A";
            dockPanelChild.Children.Add(textBlock);

            dockPanel.Children.Add(dockPanelChild);

           
            #region 
            dockPanelChild = new DockPanel();

            var dockPanel1 = new DockPanel();
            dockPanel1.Style = (Style)Resources["dockPanelTxt"];
            var border = new Border();
            border.Style = (Style)Resources["border"];

            //textBlock = new TextBlock();
            //textBlock.Text = "电流";
            //dockPanel1.Children.Add(textBlock);

            //textBlock = new TextBlock();
            //BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("Text") { Source = decimalTextBox });
            //dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = "，测量标准";
            dockPanel1.Children.Add(textBlock);

            border.Child = dockPanel1;
            dockPanelChild.Children.Add(border);


            textBlock = new TextBlock();
            textBlock.Text = "≤±";
            dockPanelChild.Children.Add(textBlock);


            var decimalTextBoxChild = new DecimalTextBox();

            decimalTextBoxChild.TextChanged += (s, e) =>
            {
                model.MeasurementStandard = $"≤±{(s as TextBox).Text.Trim()}A";
            };
            decimalTextBoxChild.Text = par.MeasurementStandard.GetNumber().ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "A";
            dockPanelChild.Children.Add(textBlock);


            dockPanel.Children.Add(dockPanelChild);
            #endregion

            #region 电源电压
            dockPanelChild = new DockPanel();

            dockPanel1 = new DockPanel();
            dockPanel1.Style = (Style)Resources["dockPanelTxt"];
            border = new Border();
            border.Style = (Style)Resources["border"];

            //textBlock = new TextBlock();
            //textBlock.Text = "电流";
            //dockPanel1.Children.Add(textBlock);

            //textBlock = new TextBlock();
            //BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("Text") { Source = decimalTextBox });
            //dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = "，设置电源";
            dockPanel1.Children.Add(textBlock);

            border.Child = dockPanel1;
            dockPanelChild.Children.Add(border);




            decimalTextBoxChild = new DecimalTextBox();

            decimalTextBoxChild.TextChanged += (s, e) =>
            {
                model.Extend1= $"{(s as TextBox).Text.Trim()}";
            };
            decimalTextBoxChild.Text = par.Extend1.ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "V";
            dockPanelChild.Children.Add(textBlock);


            dockPanel.Children.Add(dockPanelChild);
            #endregion

            #region 电源误差范围
            dockPanelChild = new DockPanel();

            dockPanel1 = new DockPanel();
            dockPanel1.Style = (Style)Resources["dockPanelTxt"];
            border = new Border();
            border.Style = (Style)Resources["border"];

            //textBlock = new TextBlock();
            //textBlock.Text = "电流";
            //dockPanel1.Children.Add(textBlock);

            //textBlock = new TextBlock();
            //BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("Text") { Source = decimalTextBox });
            //dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = "，电源误差范围";
            dockPanel1.Children.Add(textBlock);

            border.Child = dockPanel1;
            dockPanelChild.Children.Add(border);


            textBlock = new TextBlock();
            textBlock.Text = "≤";
            dockPanelChild.Children.Add(textBlock);


            decimalTextBoxChild = new DecimalTextBox();

            decimalTextBoxChild.TextChanged += (s, e) =>
            {
                model.Extend2 = $"{(s as TextBox).Text.Trim()}";
            };
            decimalTextBoxChild.Text = par.Extend2.GetNumber().ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "mA";
            dockPanelChild.Children.Add(textBlock);


            dockPanel.Children.Add(dockPanelChild);
            #endregion

            panel.Children.Add(dockPanel);

            return model;
        }

        public bool Save()
        {
            bool b = false;
            foreach (var item in pars)
            {
                var model = new TestParameter() { MedthodName = "CurrentCollection", QualifiedValue = item.QualifiedValue, TestItem = "电流测试", ChildrenItem = $"电流测试点{item.QualifiedValue}A", MeasurementStandard = item.MeasurementStandard, Extend1 = $"{item.Extend1}", Extend2 = $"{item.Extend2}", Extend3 = $"{item.Extend3}" };
                var modelTemp = parameters.FirstOrDefault(m => m.QualifiedValue == model.QualifiedValue);
             
                
                if (TestParDal.Update("CurrentCollection",item.Id, item.QualifiedValue.Value, model))
                {
                    b = true;
                }
            }
            return b;
        }

        /// <summary>
        /// 检查是否发生变化
        /// </summary>
        /// <returns></returns>
        bool Check(TestParameter oldP, TestParameter newP)
        {
            return oldP.QualifiedValue == newP.QualifiedValue && oldP.MeasurementStandard == newP.MeasurementStandard && oldP.Extend1 == newP.Extend1 && oldP.Extend2 == newP.Extend2 && oldP.Extend3 == newP.Extend3;
        }

        
    }
}
