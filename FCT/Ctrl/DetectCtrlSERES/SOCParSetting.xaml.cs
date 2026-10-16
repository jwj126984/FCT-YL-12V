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

namespace FCT.Ctrl.DetectCtrlSERES
{
    /// <summary>
    /// SOCParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class SOCParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter soc = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "SOCAndSOH"&&m.ChildrenItem.Contains("SOC"));
        TestParameter soh = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "SOCAndSOH"&&m.ChildrenItem.Contains("SOH"));
        public SOCParSetting()
        {
            InitializeComponent();
            if (soc != null)
            {
                socValBox.Text = soc.QualifiedValue.ToString();
                socBox.Text = soc.MeasurementStandard.GetNumber().ToString();
            }
            if (soh != null)
            {
                sohValBox.Text = soh.QualifiedValue.ToString();
            }
        }

        public bool Save()
        {
            if (socValBox.Text == soc.QualifiedValue.ToString()&& sohValBox.Text == soh.QualifiedValue.ToString())
            {
                return false;
            }
            soc.QualifiedValue = double.Parse(socValBox.Text.Trim());
            soh.QualifiedValue = double.Parse(sohValBox.Text.Trim());
            soc.MeasurementStandard = $"≥${socBox.Text}";
            bool b = false;
            if (TestParDal.UpdatePar(soc))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(soh))
            {
                b = true;
            }
            return b;
        }
    }
}
