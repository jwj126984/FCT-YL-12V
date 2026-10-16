using Common;
using Common.Can;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    public class TotalVoltage : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {

            LogHelper.Debug("------总电压测试------");
            CommonData.TestCount++;
            return true;

            List<TestResult> result1 = new List<TestResult>();//AFE 结果
            List<TestResult> result2 = new List<TestResult>();//MCU 结果
            List<TestResult> result3 = new List<TestResult>();//外总 结果
            //根据Extend2将内总 AFE MCU分组后取
            var tempPars = pars.GroupBy(m => m.Extend2).ToList();

            var afeS = tempPars[0].ToList().OrderBy(m=>double.Parse(m.Extend3)).ToList();//AFE集合 根据Extend3测试序号排序，先测15.6
            var mcuS = tempPars[1].ToList().OrderBy(m => double.Parse(m.Extend3)).ToList();//MCU集合 根据Extend3测试序号排序，先测15.6
            var wzS = tempPars[2].ToList().OrderBy(m => double.Parse(m.Extend3)).ToList();//外总集合 根据Extend3测试序号排序，先测15.6
            //MOS 闭合
            PlayUDSHelper.MOSControl(MOSEnum.闭合);
            //他们每一个都是一样的的4条，所以用一个的参数就好了
            for (var i = 0; i < afeS.Count(); i++)
            {
                DateTime startTime=DateTime.Now;
                
               
                Thread.Sleep(400);
                var fOutVoltage= CommonData.NXI_6400.GetVResult();
                var fRetData01 = CanHelper.ReadPower(true, false).Item1;
                if(Math.Abs(fRetData01- fOutVoltage) > CommonData.PowerVoltageOffset)
                {
                    LogHelper.MessageBoxShow("电压测试电源设置失败");
                    return false;
                }

                var mcu = PlayUDSHelper.GetChargeVoltage()*0.1;
                if (mcu == double.NaN)
                {
                    LogHelper.MessageBoxShow("获取MCU失败");
                    return false;
                }
                var afe = CommonData.NXI_6400.GetVResult();
                var nAndwZDY = ZCData.NAndWZDY;
                var nz = nAndwZDY.Item1;
                var uex= nAndwZDY.Item2;
                var wdy = CommonData.NXI_6400.GetVResult();
                DateTime endTime= DateTime.Now;
                var span = (endTime - startTime).TotalSeconds / 3;
                //AFE
                TestResult model= new TestResult();
                model.StartTime = startTime;
                model.MeasurementStandard = afeS[i].MeasurementStandard;
                model.EndTime = startTime.AddSeconds(span);
                model.ChildrenItem = afeS[i].ChildrenItem;
                model.QualifiedValue = nz.ToString("0.000");
                model.MeasureVal = afe.ToString("0.000");
                model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
                double num = (double)afeS[i].MeasurementStandard.GetNumber();
                model.Result = Math.Abs((double.Parse( model.MeasureVal) - double.Parse(model.QualifiedValue))) < num;
                result1.Add(model);
                //MCU
                model = new TestResult();
                model.MeasurementStandard = mcuS[i].MeasurementStandard;
                model.StartTime = startTime.AddSeconds(span);
                model.EndTime = startTime.AddSeconds(span*2);
                model.ChildrenItem = mcuS[i].ChildrenItem;
                model.QualifiedValue = nz.ToString("0.000");
                model.MeasureVal = mcu.ToString("0.000"); 
                model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
                num = (double)mcuS[i].MeasurementStandard.GetNumber();
                model.Result = Math.Abs((double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue))) < num;
                result2.Add(model);

                //外总
                model = new TestResult();
                model.MeasurementStandard = wzS[i].MeasurementStandard;
                model.StartTime = startTime.AddSeconds(span);
                model.EndTime = startTime.AddSeconds(span * 2);
                model.ChildrenItem = wzS[i].ChildrenItem;
                model.QualifiedValue = uex.ToString("0.000");
                model.MeasureVal = wdy.ToString("0.000");
                model.DifferenceVal = (double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue)).ToString("0.000");
                num = (double)wzS[i].MeasurementStandard.GetNumber();
                model.Result = Math.Abs((double.Parse(model.MeasureVal) - double.Parse(model.QualifiedValue))) < num;
                result3.Add(model);
            }
            result1= result1.OrderBy(m => m.ChildrenItem.GetNumber()).ToList();
            result2= result2.OrderBy(m => m.ChildrenItem.GetNumber()).ToList();
            result3= result3.OrderBy(m => m.ChildrenItem.GetNumber()).ToList();
            for (int i = 0;i< result3.Count; i++)
            {
                AddModelAction(result1[i]);
                AddModelAction(result2[i]);
                AddModelAction(result3[i]);
            }
          
            return true;
        }
    }
}
