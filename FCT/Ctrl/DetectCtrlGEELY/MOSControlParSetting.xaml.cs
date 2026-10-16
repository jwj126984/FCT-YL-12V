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
    /// MOSControlParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MOSControlParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter par1 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "H_D_OAndRelayControl" && m.Extend1 == "高驱闭合");
        TestParameter par2 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "H_D_OAndRelayControl" && m.Extend1 == "高驱断开");
        TestParameter par3 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "H_D_OAndRelayControl" && m.Extend1 == "继电器闭合");
        TestParameter par4 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "H_D_OAndRelayControl" && m.Extend1 == "继电器断开");
        public MOSControlParSetting()
        {
            InitializeComponent();
            box1.Text = par1?.MeasurementStandard.GetNumber().ToString();
            box2.Text = par2?.MeasurementStandard.GetNumber().ToString();
            box3.Text = par3?.MeasurementStandard.GetNumber().ToString();
            box4.Text = par4?.MeasurementStandard.GetNumber().ToString();
        }

        public bool Save()
        {
            bool b = false;
            if (par1.MeasurementStandard.GetNumber() != decimal.Parse(box1.Text))
            {
                par1.MeasurementStandard = $"±{box1.Text}V";
                if (TestParDal.UpdatePar(par1))
                {
                    b = true;
                }
            }

            if (par2.MeasurementStandard.GetNumber() != decimal.Parse(box2.Text))
            {
                par2.MeasurementStandard = $"≤{box2.Text}V";
                if (TestParDal.UpdatePar(par2))
                {
                    b = true;
                }
            }


            if (par3.MeasurementStandard.GetNumber() != decimal.Parse(box3.Text))
            {
                par3.MeasurementStandard = $"±{box3.Text}V";
                if (TestParDal.UpdatePar(par3))
                {
                    b = true;
                }
            }


            if (par4.MeasurementStandard.GetNumber() != decimal.Parse(box4.Text))
            {
                par4.MeasurementStandard = $"≤{box4.Text}V";
                if (TestParDal.UpdatePar(par4))
                {
                    b = true;
                }
            }


            

            return b;
        }
    }
}
