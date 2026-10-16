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
    /// WAndSCurrParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class WAndSCurrParSetting : UserControl, IDetectCtrlBase
    {
        TestParameter work = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "WorkingCurrent");
        TestParameter sleep1 = CommonData.TestParameters?.FirstOrDefault(m => m.Extend1 == "1" && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
        TestParameter sleep2 = CommonData.TestParameters?.FirstOrDefault(m => m.Extend1 == "2" && m.MedthodName.ToLower() == "SleepingCurrent".ToLower());
        public WAndSCurrParSetting()
        {
            InitializeComponent();
            wStartBox.Text = work?.MinVal.ToString();
            wEndBox.Text = work?.MaxVal.ToString();
            s1EndBox.Text = sleep1?.MaxVal.ToString();
            s2EndBox.Text = sleep2?.MaxVal.ToString();
        }

        public bool Save()
        {
            if (work.MinVal.ToString() == wStartBox.Text && work.MaxVal.ToString() == wEndBox.Text && sleep1.MaxVal.ToString() == s2EndBox.Text &&  sleep2.MaxVal.ToString() == s2EndBox.Text)
            {
                return false;
            }
            work.MinVal = double.Parse(wStartBox.Text);
            work.MaxVal = double.Parse(wEndBox.Text);
            work.MeasurementStandard = $"[{work.MinVal},{work.MaxVal}]";


            sleep1.MaxVal = double.Parse(s1EndBox.Text);
            sleep1.MeasurementStandard = $"[{sleep1.MinVal},{sleep1.MaxVal}]";

            sleep2.MaxVal = double.Parse(s2EndBox.Text);
            sleep2.MeasurementStandard = $"[{sleep2.MinVal},{sleep2.MaxVal}]";
            bool b = false;
            if (TestParDal.UpdatePar(work))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(sleep1))
            {
                b = true;
            }
            if (TestParDal.UpdatePar(sleep2))
            {
                b = true;
            }
            return b;
        }
    }
}