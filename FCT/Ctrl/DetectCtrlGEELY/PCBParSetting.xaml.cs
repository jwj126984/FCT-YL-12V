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
    /// PCBParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PCBParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter par = CommonData.TestParameters?.Where(m => m.MedthodName == "PCBTemperature").FirstOrDefault();
        public PCBParSetting()
        {
            InitializeComponent();

            //pcbBox.Text = par.QualifiedValue.ToString();
            pcbMeaBox.Text = par?.MeasurementStandard?.GetNumber().ToString();
        }

        public bool Save()
        {
            
            bool b = false;
            if (pcbBox.Text == par.QualifiedValue.ToString()&& pcbMeaBox.Text== par.MeasurementStandard.GetNumber().ToString())
            {
                return false;
            }
            //par.QualifiedValue = double.Parse(pcbBox.Text);
            par.MeasurementStandard = $"±{pcbMeaBox.Text}℃";

            return TestParDal.UpdatePar(par);

            //return b;
            //throw new NotImplementedException();
        }
    }
}
