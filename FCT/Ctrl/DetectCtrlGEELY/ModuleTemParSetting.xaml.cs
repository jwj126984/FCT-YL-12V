using Common;
using FCT.Ctrl.PlayCtrl;
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

namespace FCT.Ctrl.DetectCtrlGEELY
{
    /// <summary>
    /// ModuleTemParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ModuleTemParSetting : UserControl, IDetectCtrlBase
    {
        IEnumerable<TestParameter> parameters = JsonConvert.DeserializeObject<List<TestParameter>>(JsonConvert.SerializeObject(CommonData.TestParameters?.Where(m => m.MedthodName == "ModuleTemperatureTest"))) ?? new List<TestParameter>();
        public ModuleTemParSetting()
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
            textBlock.Text = "温度点";
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
            decimalTextBox.Text = par.QualifiedValue.Value.ToString();
            model.Id = par.Id;
            dockPanelChild.Children.Add(decimalTextBox);

            textBlock = new TextBlock();
            textBlock.Text = "℃";
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
                model.MeasurementStandard = $"≤±{(s as TextBox).Text.Trim()}℃";
            };
            decimalTextBoxChild.Text = par.MeasurementStandard.GetNumber().ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "℃";
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
                var model = new TestParameter() { MedthodName = "ModuleTemperatureTest", QualifiedValue = item.QualifiedValue, TestItem = "电芯温度测试", ChildrenItem = $"模组温度?-{item.QualifiedValue}℃", MeasurementStandard = item.MeasurementStandard, Extend1 = $"{item.QualifiedValue}", Extend2 = $"{item.Extend2}", Extend3 = $"{item.Extend3}" };
                var modelTemp = parameters.FirstOrDefault(m => m.QualifiedValue.ToString() == item.Extend1);
                if (modelTemp != null)
                {
                    if (Check(modelTemp, model))
                        continue;
                }


                if (TestParDal.Update("ModuleTemperatureTest", item.Id, item.QualifiedValue.Value, model))
                {
                    b = true;
                }
            }
            if (b)
            {
                parameters = JsonConvert.DeserializeObject<List<TestParameter>>(JsonConvert.SerializeObject(CommonData.TestParameters?.Where(m => m.MedthodName == "VoltageTest"))) ?? new List<TestParameter>();
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
