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
    /// PIPCParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PIPCParSetting : UserControl, IDetectCtrlBase
    {
       // TestParameter softPar = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "LBMSRead" && m.ChildrenItem.Contains("软件版本"));
        TestParameter hardPar = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "LBMSWrite" && m.ChildrenItem.Contains("硬件"));
        public PIPCParSetting()
        {
            InitializeComponent();
        //    softWare.Text = softPar?.MeasurementStandard;
            hardWare.Text = hardPar?.MeasurementStandard;

        }

        public bool Save()
        {
            //throw new NotImplementedException();
            
        //    softPar.MeasurementStandard = softWare.Text.Trim();
            hardPar.MeasurementStandard = hardWare.Text.Trim();
            return TestParDal.UpdatePar(hardPar);
        }
    }
}
