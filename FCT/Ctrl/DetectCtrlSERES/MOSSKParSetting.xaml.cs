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
    /// MOSSKParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MOSSKParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter par = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSSleepKeep");
        public MOSSKParSetting()
        {
            InitializeComponent();

            if (par != null)
            {
                qValBox.Text = par.QualifiedValue.ToString();
                meaValBox.Text = par.MeasurementStandard.GetNumber().ToString();
            }
        }

        public bool Save()
        {
            
            par.QualifiedValue = double.Parse(qValBox.Text.Trim());
            par.MeasurementStandard = $"≤±{meaValBox.Text}V";
            return TestParDal.UpdatePar(par);
        }
    }
}
