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
    /// MOSControlParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class MOSControlParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter par1 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSControl" && m.ChildrenItem == "充电MOS驱动电压（闭合MOS）(V)");
        TestParameter par2 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSControl" && m.ChildrenItem == "放电MOS驱动电压（闭合MOS）(V)");
        TestParameter par3 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSControl" && m.ChildrenItem == "充电MOS驱动电压（断开MOS）(V)");
        TestParameter par4 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSControl" && m.ChildrenItem == "放电MOS驱动电压（断开MOS）(V)");
        TestParameter par5 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSControl" && m.ChildrenItem == "充放电MOS时P+电压（断开MOS）(V)");
        TestParameter par6 = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "MOSControl" && m.ChildrenItem == "充放电MOS时P+电压（闭合MOS）(V)");

        public MOSControlParSetting()
        {
            InitializeComponent();
            box1.Text = par1?.MinVal.ToString();
            box2.Text = par1?.MaxVal.ToString();
            box3.Text = par2?.MinVal.ToString();
            box4.Text = par2?.MaxVal.ToString();
            box5.Text = par3?.MinVal.ToString();
            box6.Text = par3?.MaxVal.ToString();
            box7.Text = par4?.MinVal.ToString();
            box8.Text = par4?.MaxVal.ToString();
            box9.Text = par5?.MinVal.ToString();
            box10.Text = par5?.MaxVal.ToString();
            box11.Text = par6?.MinVal.ToString();
            box12.Text = par6?.MaxVal.ToString();
        }

        public bool Save()
        {
            bool b = false;
            if (par1.MinVal != double.Parse(box1.Text) || par1.MaxVal != double.Parse(box2.Text))
            {
                par1.MinVal = double.Parse(box1.Text);
                par1.MaxVal = double.Parse(box2.Text);
                par1.MeasurementStandard = $"[{par1.MinVal},{par1.MaxVal}]";
                if (TestParDal.UpdatePar(par1))
                {
                    b = true;
                }
            }

            if (par2.MinVal != double.Parse(box3.Text) || par2.MaxVal != double.Parse(box4.Text))
            {
                par2.MinVal = double.Parse(box3.Text);
                par2.MaxVal = double.Parse(box4.Text);
                par2.MeasurementStandard = $"[{par2.MinVal},{par2.MaxVal}]";
                if (TestParDal.UpdatePar(par2))
                {
                    b = true;
                }
            }

            if (par3.MinVal != double.Parse(box5.Text) || par3.MaxVal != double.Parse(box6.Text))
            {
                par3.MinVal = double.Parse(box5.Text);
                par3.MaxVal = double.Parse(box6.Text);
                par3.MeasurementStandard = $"[{par3.MinVal},{par3.MaxVal}]";
                if (TestParDal.UpdatePar(par3))
                {
                    b = true;
                }
            }

            if (par4.MinVal != double.Parse(box7.Text) || par4.MaxVal != double.Parse(box8.Text))
            {
                par4.MinVal = double.Parse(box7.Text);
                par4.MaxVal = double.Parse(box8.Text);
                par4.MeasurementStandard = $"[{par4.MinVal},{par4.MaxVal}]";
                if (TestParDal.UpdatePar(par4))
                {
                    b = true;
                }
            }

            if (par5.MinVal != double.Parse(box9.Text) || par5.MaxVal != double.Parse(box10.Text))
            {
                par5.MinVal = double.Parse(box9.Text);
                par5.MaxVal = double.Parse(box10.Text);
                par5.MeasurementStandard = $"[{par5.MinVal},{par5.MaxVal}]";
                if (TestParDal.UpdatePar(par5))
                {
                    b = true;
                }
            }

            if (par6.MinVal != double.Parse(box11.Text) || par6.MaxVal != double.Parse(box12.Text))
            {
                par6.MinVal = double.Parse(box11.Text);
                par6.MaxVal = double.Parse(box12.Text);
                par6.MeasurementStandard = $"[{par6.MinVal},{par6.MaxVal}]";
                if (TestParDal.UpdatePar(par6))
                {
                    b = true;
                }
            }
            return b;
        }
    }
}
