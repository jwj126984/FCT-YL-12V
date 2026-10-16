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
    /// ProduceInfoParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ProduceInfoParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter duPar = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "GetProduceInfo" && m.Extend1.Contains("DU检测"));
        TestParameter hswdPar = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "GetProduceInfo" && m.Extend1.Contains("HSWD检测"));
        TestParameter swlmPar = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "GetProduceInfo" && m.Extend1.Contains("SWLM检测"));
        TestParameter sxblPar = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "GetProduceInfo" && m.Extend1.Contains("SXBL检测"));
        public ProduceInfoParSetting()
        {
            InitializeComponent();
            duBox.Text = duPar?.MeasurementStandard;
            hswdBox.Text = hswdPar?.MeasurementStandard;
            swlmBox.Text = swlmPar?.MeasurementStandard;
            sxblBox.Text = sxblPar?.MeasurementStandard;
        }
        public bool Save()
        {
            //throw new NotImplementedException();
            if (duPar.MeasurementStandard == duBox.Text.Trim() && hswdPar.MeasurementStandard == hswdBox.Text.Trim())
            {
                return false;
            }
            if (swlmPar.MeasurementStandard == swlmBox.Text.Trim() && sxblPar.MeasurementStandard == sxblBox.Text.Trim())
            {
                return false;
            }

            duPar.MeasurementStandard = duBox.Text.Trim();
            hswdPar.MeasurementStandard = hswdBox.Text.Trim();
            swlmPar.MeasurementStandard = swlmBox.Text.Trim();
            sxblPar.MeasurementStandard = sxblBox.Text.Trim();

            bool b = false;
            if (TestParDal.UpdatePar(duPar))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(hswdPar))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(swlmPar))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(sxblPar))
            {
                b = true;
            }

            return b;
        }
    }
}
