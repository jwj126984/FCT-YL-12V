using Common;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace FCT.Ctrl.DetectCtrlLT
{
    /// <summary>
    /// CurrentLimitingParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class CurrentLimitingParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter open = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "CurrentLimiting"&&m.ChildrenItem.Contains("闭合"));
        TestParameter close = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "CurrentLimiting"&&m.ChildrenItem.Contains("断开"));
        public CurrentLimitingParSetting()
        {
            InitializeComponent();
            if (open != null)
            {
                qValBox.Text = open.QualifiedValue.ToString();
                meaOpenValBox.Text = open.MeasurementStandard.GetNumber().ToString();
            }
            if (close != null)
            {
                meaCloseValBox.Text = close.MeasurementStandard.GetNumber().ToString();
            }
        }

        public bool Save()
        {
            
            open.QualifiedValue = double.Parse(qValBox.Text.Trim());
            open.MeasurementStandard = $"≤±{meaOpenValBox.Text}V";
            bool b = false;
            if (TestParDal.UpdatePar(open))
            {
                b = true;
            }
            close.MeasurementStandard = $"≤{meaCloseValBox.Text}V";
            close.QualifiedValue = double.Parse(meaCloseValBox.Text);
            if (TestParDal.UpdatePar(close))
            {
                b = true;
            }
            return b;
        }
    }
}
