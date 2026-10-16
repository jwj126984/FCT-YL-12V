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

namespace FCT.Ctrl.DetectCtrlGEELY
{
    /// <summary>
    /// BalancedTestParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class BalancedTestParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter par = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "BalancedTest");
        public BalancedTestParSetting()
        {
            InitializeComponent();
            if (par != null)
            {
                bMeaBox.Text = par.MeasurementStandard.GetNumber().ToString();
            }
        }

        public bool Save()
        {
            if (bMeaBox.Text == par.MeasurementStandard.GetNumber().ToString())
            {
                return false;
            }
            par.MeasurementStandard = $">{bMeaBox.Text.Trim()}mA";
            return TestParDal.UpdatePar(par);
        }

    }
}
