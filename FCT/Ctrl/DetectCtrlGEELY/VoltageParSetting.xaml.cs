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

namespace FCT.Ctrl.DetectCtrlGEELY
{
    /// <summary>
    /// VoltageParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class VoltageParSetting : UserControl, IDetectCtrlBase
    {
        IEnumerable<TestParameter> parameters = JsonConvert.DeserializeObject<List<TestParameter>>(JsonConvert.SerializeObject(CommonData.TestParameters?.Where(m => m.MedthodName == "VoltageTest"))) ?? new List<TestParameter>();
        public VoltageParSetting()
        {
            InitializeComponent();
            foreach (var item in parameters)
            {
                var temp = _Create(item);
                volModels.Add(temp);
            }
        }
        List<VolModel> volModels = new List<VolModel>();
        VolModel _Create(TestParameter par)
        {
            VolModel volModel = new VolModel();

            StackPanel dockPanel = new StackPanel();

            DockPanel dockPanelChild = new DockPanel();
            dockPanelChild.Style = (Style)Resources["item"];
            TextBlock textBlock = new TextBlock();
            textBlock.Style = (Style)Resources["blockItem"];
            textBlock.Text = "电压点";
            dockPanelChild.Children.Add(textBlock);

            DecimalTextBox decimalTextBox = new DecimalTextBox();

            decimalTextBox.TextChanged += (s, e) =>
            {
                if (double.TryParse(decimalTextBox.Text, out double d))
                {
                    volModel.TestPoint = d;
                }
                else
                {
                    volModel.TestPoint = 0;
                }
            };
            decimalTextBox.Text = par.QualifiedValue.Value.ToString();
            volModel.Id = par.Id;
            dockPanelChild.Children.Add(decimalTextBox);

            textBlock = new TextBlock();
            textBlock.Text = "V";
            dockPanelChild.Children.Add(textBlock);

            dockPanel.Children.Add(dockPanelChild);

            #region 单体电压
            Border border = new Border();
            border.Style = (Style)Resources["border"];
            DockPanel dockPanel1 = new DockPanel();
            dockPanel1.Style = (Style)Resources["dockPanelTxt"];
            dockPanelChild = new DockPanel();
            dockPanelChild.Style = (Style)Resources["item"];

            textBlock = new TextBlock();
            textBlock.Text = "单体电压";
            dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("Text") { Source = decimalTextBox, Converter = new VolTageConvert() });
            dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = "V测量标准";
            dockPanel1.Children.Add(textBlock);

            border.Child = dockPanel1;
            dockPanelChild.Children.Add(border);

            textBlock = new TextBlock();
            textBlock.Text = "≤±";
            dockPanelChild.Children.Add(textBlock);




            DecimalTextBox decimalTextBoxChild = new DecimalTextBox();

            decimalTextBoxChild.TextChanged += (s, e) =>
            {
                volModel.Ex2 = $"≤±{(s as TextBox).Text.Trim()}V";
            };
            decimalTextBoxChild.Text = par.MeasurementStandard.Split('&')[0].GetNumber().ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "V";
            dockPanelChild.Children.Add(textBlock);


            dockPanel.Children.Add(dockPanelChild);
            #endregion

            #region 内总
            dockPanel1 = new DockPanel();
            border = new Border();
            border.Style = (Style)Resources["border"];

            dockPanel1.Style = (Style)Resources["dockPanelTxt"];
            dockPanelChild = new DockPanel();
            dockPanelChild.Style = (Style)Resources["item"];

            textBlock = new TextBlock();
            textBlock.Text = "内总电压";
            dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("Text") { Source = decimalTextBox });
            dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = "V测量标准";
            dockPanel1.Children.Add(textBlock);

            border.Child = dockPanel1;
            dockPanelChild.Children.Add(border);

            textBlock = new TextBlock();
            textBlock.Text = "≤±";
            dockPanelChild.Children.Add(textBlock);


            decimalTextBoxChild = new DecimalTextBox();

            decimalTextBoxChild.TextChanged += (s, e) =>
            {
                volModel.InVol = $"≤±{(s as TextBox).Text.Trim()}V";
            };
            decimalTextBoxChild.Text = par.MeasurementStandard.Split('&')[1].GetNumber().ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "V";
            dockPanelChild.Children.Add(textBlock);


            dockPanel.Children.Add(dockPanelChild);
            #endregion


            #region 外总
            dockPanelChild = new DockPanel();

            dockPanel1 = new DockPanel();
            dockPanel1.Style = (Style)Resources["dockPanelTxt"];
            border = new Border();
            border.Style = (Style)Resources["border"];

            textBlock = new TextBlock();
            textBlock.Text = "外总电压";
            dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            BindingOperations.SetBinding(textBlock, TextBlock.TextProperty, new Binding("Text") { Source = decimalTextBox });
            dockPanel1.Children.Add(textBlock);

            textBlock = new TextBlock();
            textBlock.Text = "V测量标准";
            dockPanel1.Children.Add(textBlock);

            border.Child = dockPanel1;
            dockPanelChild.Children.Add(border);


            textBlock = new TextBlock();
            textBlock.Text = "≤±";
            dockPanelChild.Children.Add(textBlock);


            decimalTextBoxChild = new DecimalTextBox();

            decimalTextBoxChild.TextChanged += (s, e) =>
            {
                volModel.OutVol = $"≤±{(s as TextBox).Text.Trim()}V";
            };
            decimalTextBoxChild.Text = par.MeasurementStandard.Split('&')[2].GetNumber().ToString();
            dockPanelChild.Children.Add(decimalTextBoxChild);

            textBlock = new TextBlock();
            textBlock.Text = "V";
            dockPanelChild.Children.Add(textBlock);


            dockPanel.Children.Add(dockPanelChild);
            #endregion

            panel.Children.Add(dockPanel);

            return volModel;
        }

        public bool Save()
        {
            bool b = false;
            foreach (var item in volModels)
            {
                var model = new TestParameter() { MedthodName = "VoltageTest", QualifiedValue = item.TestPoint, TestItem = "电压测试", ChildrenItem = $"单体电压?测试-{item.TestPoint / 4}V&内总电压-{item.TestPoint}V&外总电压-{item.TestPoint}V", MeasurementStandard=$"{item.Ex2}&{item.InVol}&{item.OutVol}"};
                var modelTemp = parameters.FirstOrDefault(m => m.Id == item.Id);
                if (modelTemp != null)
                {
                    if (Check(modelTemp, model))
                        continue;
                }
                if (TestParDal.Update("VoltageTest", item.Id, item.TestPoint, model))
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

    class VolModel
    {
        public double TestPoint { get; set; }
        public int Id { get; set; }
        public string Ex2 { get; set; }
        public string InVol { get; set; }
        public string OutVol { get; set; }
    }
}
