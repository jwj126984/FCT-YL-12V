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
    /// CanTRTestParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class CanTRTestParSetting :  UserControl, IDetectCtrlBase
    {
        TestParameter par = CommonData.TestParameters?.FirstOrDefault(m => m.MedthodName == "CanTRTest");
        public CanTRTestParSetting()
        {
            InitializeComponent();
            
            if (par != null)
            {
                qfVal.Text = par.MinVal.Value.ToString();
                ms.Text = par.MaxVal.Value.ToString();
            }
            //qfVal.Text=
        }

        public bool Save()
        {
            
            par.MinVal = double.Parse(qfVal.Text.Trim());
            par.MaxVal = double.Parse(ms.Text.Trim());
            return TestParDal.UpdatePar(par);
        }
    }
}
