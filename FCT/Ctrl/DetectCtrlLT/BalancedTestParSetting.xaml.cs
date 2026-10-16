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
                bStartBox.Text = par.MinVal.ToString();
                bEndBox.Text = par.MaxVal.ToString();
            }
        }

        public bool Save()
        {
           
            par.MinVal = double.Parse(bStartBox.Text.Trim());
            par.MaxVal= double.Parse(bEndBox.Text.Trim());
            par.MeasurementStandard = $"[{par.MinVal},{par.MaxVal}]";
            return TestParDal.UpdatePar(par);
        }
    }
}
