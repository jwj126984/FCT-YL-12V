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
    /// ErrorParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class ErrorParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter ErrRespBMS = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "BusMalfunction" && m.ChildrenItem.Contains("ErrRespBMS"));
        TestParameter BattSnsrHwFltRaw = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "BusMalfunction" && m.ChildrenItem.Contains("BattSnsrHwFltRaw"));
        TestParameter BattRlyStsPrimSwtSts = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "BusMalfunction" && m.ChildrenItem.Contains("BattRlyStsPrimSwtSts"));
        TestParameter BattHwFltStsPrim = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "BusMalfunction" && m.ChildrenItem.Contains("BattHwFltStsPrim"));
        TestParameter BattFltStsPrim = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "BusMalfunction" && m.ChildrenItem.Contains("BattFltStsPrim"));
        public ErrorParSetting()
        {
            InitializeComponent();
            ErrRespBMSBox.Text = ErrRespBMS?.MeasurementStandard.GetNumber().ToString();
            BattSnsrHwFltRawBox.Text = BattSnsrHwFltRaw?.MeasurementStandard.GetNumber().ToString();
            BattRlyStsPrimSwtStsBox.Text = BattRlyStsPrimSwtSts?.MeasurementStandard.GetNumber().ToString();
            BattHwFltStsPrimBox.Text = BattHwFltStsPrim?.MeasurementStandard.GetNumber().ToString();
            BattFltStsPrimBox.Text = BattFltStsPrim?.MeasurementStandard.GetNumber().ToString();
        }

        public bool Save()
        {
            //throw new NotImplementedException();
            if(ErrRespBMS.MeasurementStandard == $"={double.Parse(ErrRespBMSBox.Text)}"&&
                BattSnsrHwFltRaw.MeasurementStandard == $"={double.Parse(BattSnsrHwFltRawBox.Text)}"&&
                BattRlyStsPrimSwtSts.MeasurementStandard == $"<{double.Parse(BattRlyStsPrimSwtStsBox.Text)}"&&
                 BattHwFltStsPrim.MeasurementStandard == $"={double.Parse(BattHwFltStsPrimBox.Text)}"&&
                 BattFltStsPrim.MeasurementStandard == $"={double.Parse(BattFltStsPrimBox.Text)}"
                )
            {
                return false;
            }
            ErrRespBMS.MeasurementStandard = $"={double.Parse(ErrRespBMSBox.Text)}";
            BattSnsrHwFltRaw.MeasurementStandard = $"={double.Parse(BattSnsrHwFltRawBox.Text)}";
            BattRlyStsPrimSwtSts.MeasurementStandard = $"<{double.Parse(BattRlyStsPrimSwtStsBox.Text)}";
            BattHwFltStsPrim.MeasurementStandard = $"={double.Parse(BattHwFltStsPrimBox.Text)}";
            BattFltStsPrim.MeasurementStandard = $"={double.Parse(BattFltStsPrimBox.Text)}";
            bool b = false;
            if (TestParDal.UpdatePar(ErrRespBMS))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(BattSnsrHwFltRaw))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(BattRlyStsPrimSwtSts))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(BattHwFltStsPrim))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(BattFltStsPrim))
            {
                b = true;
            }
            return b;
        }
    }
}
