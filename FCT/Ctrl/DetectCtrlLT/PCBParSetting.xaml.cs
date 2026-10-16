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
    /// PCBParSetting.xaml 的交互逻辑
    /// </summary>
    public partial class PCBParSetting : UserControl, IDetectCtrlBase
    {
        IEnumerable<TestParameter> pars = CommonData.TestParameters?.Where(m => m.MedthodName == "PCBTemperature");
        public PCBParSetting()
        {
            InitializeComponent();
            pcbBox.Text = pars?.FirstOrDefault(m=>m.Extend1=="pcb")?.MeasurementStandard.GetNumber().ToString();
            pcbStartBox.Text= pars?.FirstOrDefault(m => m.Extend1 == "pcb")?.MinVal.ToString();
            pcbEndBox.Text= pars?.FirstOrDefault(m => m.Extend1 == "pcb")?.MaxVal.ToString();
            pcbcompBox.Text = pars?.FirstOrDefault(m => m.Extend1 == "pcb")?.Extend2.GetNumber().ToString();
        }

        public bool Save()
        {
            if(double.TryParse(pcbBox.Text, out double d) == false)
            {
                MessageBox.Show("输入不是数字，保存失败");
                return false;
            }
            bool b = false;
            //if(pcbBox.Text == pars?.FirstOrDefault(m => m.Extend1 == "pcb")?.MeasurementStandard.GetNumber().ToString())
            //{
            //    return false;
            //}
            pars.FirstOrDefault(m => m.Extend1 == "pcb").MinVal = double.Parse(pcbStartBox.Text);
            pars.FirstOrDefault(m => m.Extend1 == "pcb").MaxVal = double.Parse(pcbEndBox.Text);
            pars.FirstOrDefault(m => m.Extend1 == "pcb").Extend2 = pcbcompBox.Text;
            foreach (var par in pars)
            {
                //if (par.MeasurementStandard.GetNumber().ToString() == d.ToString())
                //{
                //    continue;
                //}
                par.MeasurementStandard = $"≤±{d}℃";
                if (TestParDal.UpdatePar(par))
                {
                    b = true;
                }
            }
            return b;
            //throw new NotImplementedException();
        }
    }
}
