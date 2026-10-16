using AllData;
using Common;
using Common.Can;
using Common.SerialportHelper;
using Common.UDS;
using Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DetectionProcess.SERES
{
    /// <summary>
    /// 电流标定
    /// </summary>
    public class CurrentCalibration:DetectBase
    {
        const double threshold = 10;
        bool isSetOUTPut = false;
        double vol = double.MinValue;
        bool isDoZero = false;
        bool isDebug = false;
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------电流标定------");
            LogHelper.Debug($"解锁");
            if (CommonMethod.Unlock() == false)
            {
                LogHelper.Debug($"电流标定解锁失败");
                TestFail?.Invoke("电流标定解锁失败");
                throw new Exception($"解锁失败");
            }
            List<(double, double, double)> CalibrateTempVal1 = new List<(double, double, double)>();

            if (ConfigHelper.UdsParameters.jiesuo == "ChangAn_C390")
            {

                PlayUDSHelper.Biaodinchu();

                if (PlayUDSHelper.Reset() == false)
                {
                    //LogHelper.ShowMsg("复位失败");
                    AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                    throw new Exception($"复位失败");
                }
                if (M3905D.SetOUTPut(1) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    LogHelper.WriteLog("M3905D 开启使能失败", true);
                    throw new Exception("M3905D 开启使能失败");
                }
                isSetOUTPut = true;
                //Init();

                List<(double, double)> currS = new List<(double, double)>();//电流数据集合
                List<(double, double, double)> CalibrateTempVal = new List<(double, double, double)>();
                CalibrateTempVal.Add((0, 0, 50));
                CalibrateTempVal.Add((10, 5, 50));
                CalibrateTempVal.Add((20, 5, 50));
                CalibrateTempVal.Add((50, 5, 100));
                for (int i = 0; i < 2; i++)
                {
                    
                        if (i == 0)
                        {
                            //_canOperate.Send(data: "06 01 00 00 00 00 00 00");//正向
                            NewCanHelper.CtrolTotalCurrentCollection(false);
                        }
                        else
                        {
                            //_powerCMDM3905D.SetVoltage(0);
                            CommonData.M3905D.SetCurrent(0);
                            CommonMethod.Sleep(200);
                            //_canOperate.Send(data: "06 01 01 00 00 00 00 00");//反向
                            NewCanHelper.CtrolTotalCurrentCollection(true);
                            //_powerCMDM3905D.SetCurrent(350);
                        }

                   

                    double x0 = 0.000f;
                    double x1 = 0.000f;
                    double y0 = 0.000f;
                    double y1 = 0.000f;
                    double e0 = 0.000f;
                    double e1 = 0.000f;
                    double E0 = 0.000f;

                    for (int t = 0; t < CalibrateTempVal.Count - 1; t++)
                    {
                        if (x0 == 0 && y0 == 0)
                        {
                            M3905D.SetVoltage(CalibrateTempVal[t].Item2);
                            CommonData.M3905D.SetCurrentAndStable(CalibrateTempVal[t].Item1, CalibrateTempVal[t].Item2, CalibrateTempVal[t].Item3 / 1000.0);
                            var curall = getProduceCurrent();
                            y0 = curall.Item2;
                            x0 = curall.Item1;
                            if (i == 1)
                            {
                                y0 = -y0;
                            }
                            if (Math.Abs(x0 - y0) > threshold)
                            {
                                throw new Exception($"电流差距过大，电源输出值{y0}，产品回读值{x0}");
                            }
                        }
                        M3905D.SetVoltage(CalibrateTempVal[t + 1].Item2);
                        CommonData.M3905D.SetCurrentAndStable(CalibrateTempVal[t + 1].Item1, CalibrateTempVal[t + 1].Item2, CalibrateTempVal[t + 1].Item3 / 1000.0);
                        var curall2 = getProduceCurrent();
                        y1 = curall2.Item2;
                        x1 = curall2.Item1;
                        if (i == 1)
                        {
                            y1 = -y1;
                        }
                        if (Math.Abs(x1 - y1) > threshold)
                        {
                            throw new Exception($"电流差距过大，电源输出值{y1}，产品回读值{x1}");
                        }
                        currS.Add((x1, y1));
                    }
                }
          


                NewCanHelper.CloseTotalCurrentCollection();
                LogHelper.Debug("退出标定流程设置 M3905D 电压 0V");
                M3905D.SetVoltageAndStable(0);
                LogHelper.Debug("M3905D 关闭使能");
                M3905D.SetOUTPut(0);

                var a_b = GetAAndB(currS.Select(m => m.Item1).ToArray(), currS.Select(m => m.Item2).ToArray());
                if (a_b == null)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    throw new Exception("电流标定计算KB值失败");
                }
                
                LogHelper.Debug($"计算的K：{a_b.Value.Item1}，B{a_b.Value.Item2}");
                if (CommonMethod.Unlock() == false)
                {
                    LogHelper.Debug($"电流标定解锁失败");
                    TestFail?.Invoke("电流标定解锁失败");
                    throw new Exception($"电流标定解锁失败");
                }
                PlayUDSHelper.Biaodin(a_b.Value);
                //   Reset();

                if (PlayUDSHelper.Reset() == false)
                {
                    //LogHelper.ShowMsg("复位失败");
                    AddModelAction(new TestResult() { ChildrenItem = "SOC标定与SOH测试失败" });
                    throw new Exception($"复位失败");
                }
             //   CommonMethod.Sleep(8000);


                var modelTemp = new TestResult();
                modelTemp.ChildrenItem = $"K值";
                modelTemp.MeasureVal = a_b.Value.Item1.ToString("0.000");
                modelTemp.MeasurementStandard = "[0.92,1.09]";
                if (a_b.Value.Item1 >= 0.92 && a_b.Value.Item1 <= 1.09)
                {
                    modelTemp.Result = true;
                }
                else
                {
                    modelTemp.Result = false;
                }



                AddModelAction(modelTemp);

                modelTemp = new TestResult();
                modelTemp.ChildrenItem = $"B值";
                modelTemp.MeasureVal = a_b.Value.Item2.ToString("0.000");
                modelTemp.Result = true;
                AddModelAction(modelTemp);
                // AddModelAction(resetModel);
               
                CommonData.TestCount++;
                return true;


            }
            else
            {
                LogHelper.Debug("MOS闭合");
                //MOS闭合
                if (!PlayUDSHelper.MOSControl(MOSEnum.闭合))
                {
                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    throw new Exception("电流标定MOS闭合失败");
                    //return false;
                }

                double[,] K = new double[2, 7];
                double[,] B = new double[2, 7];
                double[,] bas = new double[2, 7];
                LogHelper.Debug("开始标定");
                //i  0正向 1反向s
                for (int i = 0; i < 2; i++)
                {
                    for (int t = 0; t < 7; t++)
                    {
                        K[i, t] = 1; B[i, t] = 0;
                        for (int j = 0; j < 2; j++)
                        {
                            ushort sid = (ushort)(Convert.ToUInt32(ConfigHelper.UdsParameters.DLB_DID.Replace(" ", ""), 16) + i * 14 + t * 2 + j);
                            int uiPara = (int)(j == 0 ? Math.Round(K[i, t] * 1000) : Math.Round((B[i, t] + 60) * 1000));
                            var temp = BitConverter.GetBytes(uiPara).Reverse();
                            List<byte> data = new List<byte>();
                            var sidArr = BitConverter.GetBytes(sid);
                            data.AddRange(sidArr.Reverse());
                            data.AddRange(temp);
                            if (PlayUDSHelper.Send(data.ToArray(), 0x2E) == null)
                            {
                                //LogHelper.ShowMsg($"标定失败");
                                AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                                throw new Exception("电流初始化标定失败");
                                //return false;
                            }
                            CommonMethod.Sleep(150);
                        }

                    }
                }

                LogHelper.Debug("初始化标定成功");
                List<(double, double, double)> CalibrateTempVal = new List<(double, double, double)>();

                CalibrateTempVal.Add((0, 0, 50));
                CalibrateTempVal.Add((10, 5, 50));
                CalibrateTempVal.Add((20, 5, 50));
                CalibrateTempVal.Add((30, 5, 50));
                CalibrateTempVal.Add((50, 5, 50));
                CalibrateTempVal.Add((100, 10, 80));
                CalibrateTempVal.Add((120, 10, 100));
                CalibrateTempVal.Add((150, 10, 100));


                LogHelper.Debug("MOS闭合");
                //MOS闭合
                if (!PlayUDSHelper.MOSControl(MOSEnum.闭合))
                {


                    //LogHelper.ShowMsg("MOS闭合失败");
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    throw new Exception("电流标定MOS闭合失败");
                    //return false;
                }
                LogHelper.Debug("M3905D 开启使能");
                if (M3905D.SetOUTPut(1) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    LogHelper.WriteLog("M3905D 开启使能失败", true);
                    throw new Exception("M3905D 开启使能失败");
                }

                CommonData.M3905D.SetCurrent(0);
                double getzerocur = 0;
                for (int i = 0; i < 2; i++)
                {
                    

                    double x0 = 0.000f;
                    double x1 = 0.000f;
                    double y0 = 0.000f;
                    double y1 = 0.000f;
                    double e0 = 0.000f;
                    double e1 = 0.000f;
                    double E0 = 0.000f;


                    if (x0 == 0 && y0 == 0)
                    {

                        M3905D.SetVoltage(0);
                        CommonData.M3905D.SetCurrent(0);
                        NewCanHelper.CloseTotalCurrentCollection();
                        if (i > 0)
                        {
                            while (true)
                            {
                                CommonMethod.Sleep(500);
                                x0 = getProduceCurrent().Item1;
                                if (Math.Abs(x0) < 0.03)
                                {
                                    break;
                                }

                            }

                        }
                        var curall = getProduceCurrent();
                        y0 = 0;
                        x0 = curall.Item1;
                        if (i == 1)
                        {
                            x0 = getzerocur;
                            y0 = -y0;
                        }
                        else
                        {
                            getzerocur = x0;
                        }
                        if (Math.Abs(x0 - y0) > threshold)
                        {
                            throw new Exception($"电流差距过大，电源输出值{y0}，产品回读值{x0}");
                        }
                    }
                    if (ConfigHelper.UdsParameters.jiesuo == "SERES_F3")
                    {
                        if (i == 0)
                        {
                            //_canOperate.Send(data: "06 01 00 00 00 00 00 00");//正向
                            NewCanHelper.CtrolTotalCurrentCollection(true);
                        }
                        else
                        {
                            //_powerCMDM3905D.SetVoltage(0);
                            CommonData.M3905D.SetCurrent(0);
                            CommonMethod.Sleep(200);
                            //_canOperate.Send(data: "06 01 01 00 00 00 00 00");//反向
                            NewCanHelper.CtrolTotalCurrentCollection(false);
                            //_powerCMDM3905D.SetCurrent(350);
                        }

                    }
                    else
                    {
                        if (i == 0)
                        {
                            //_canOperate.Send(data: "06 01 00 00 00 00 00 00");//正向
                            NewCanHelper.CtrolTotalCurrentCollection(false);
                        }
                        else
                        {
                            //_powerCMDM3905D.SetVoltage(0);
                            CommonData.M3905D.SetCurrent(0);
                            CommonMethod.Sleep(200);
                            //_canOperate.Send(data: "06 01 01 00 00 00 00 00");//反向
                            NewCanHelper.CtrolTotalCurrentCollection(true);
                            //_powerCMDM3905D.SetCurrent(350);
                        }
                    }                                                                                                  
                    for (int t = 0; t < CalibrateTempVal.Count - 1; t++)
                    {
                        

                        M3905D.SetVoltage(CalibrateTempVal[t + 1].Item2);
                        CommonData.M3905D.SetCurrentAndStable(CalibrateTempVal[t + 1].Item1, CalibrateTempVal[t + 1].Item2, CalibrateTempVal[t + 1].Item3 / 1000.0);
                        
                        var curall2 = getProduceCurrent();
                        y1 = curall2.Item2;
                        x1 = curall2.Item1;
                        if (i == 1)
                        {
                            y1 = -y1;
                        }
                        if (Math.Abs(x1 - y1) > threshold)
                        {
                            throw new Exception($"电流差距过大，电源输出值{y1}，产品回读值{x1}");
                        }
                        //计算K和B
                        e0 = y0 - x0;
                        e1 = y1 - x1;
                        E0 = (e1 - e0) / (x1 - x0);
                        K[i, t] = E0 + 1;
                        B[i, t] = e1 - E0 * x1;
                        LogHelper.Debug($"计算KB值：y0:{y0},x0:{x0},e0:{e0},y1:{y1},x1:{x1},e1:{e1},E0:{E0},K:{K[i, t]},B:{B[i, t]}");

                        bas[i, t] = (y1 - x1) / y1;
                        LogHelper.ShowMsg($"电流{y1}：{bas[i, t]}");
                        LogHelper.Debug($"电流{y1}：{bas[i, t]}");

                        //赋值下一轮
                        y0 = y1;
                        x0 = x1;
                    }
                    for (int n = 2; n < 5; n++)
                    {

                        LogHelper.ShowMsg($"{Math.Abs(bas[i, n] - bas[i, n - 1])}");
                        if (Math.Abs(bas[i, n] - bas[i, n - 1]) > 0.01)
                        {
                            throw new Exception($"K值差距过大，K1:{bas[i, n]},K2:{bas[i, n - 1]}");
                        }



                    }
                }



                LogHelper.Debug("开始标定写入");
                for (int i = 0; i < 2; i++)
                {
                    for (int t = 0; t < 7; t++)
                    {
                        for (int j = 0; j < 2; j++)
                        {
                            ushort sid = (ushort)(Convert.ToUInt32(ConfigHelper.UdsParameters.DLB_DID.Replace(" ", ""), 16) + i * 14 + t * 2 + j);
                            int uiPara = (int)(j == 0 ? K[i, t] * 1000 : (B[i, t] + 60) * 1000);
                            var temp = BitConverter.GetBytes(uiPara).Reverse();
                            List<byte> data = new List<byte>();
                            var sidArr = BitConverter.GetBytes(sid);
                            //sidArr.Reverse();
                            data.AddRange(sidArr.Reverse());
                            data.AddRange(temp);
                            if (PlayUDSHelper.Send(data.ToArray(), 0x2E) == null)
                            {
                                throw new Exception("标定写入失败");
                                //CommonMethod.Unlock();
                                //if (PlayUDSHelper.Send(data.ToArray(), 0x2E) == null)
                                //{
                                //    LogHelper.ShowMsg($"标定失败");
                                //    model.Result = false;
                                //    return false;
                                //}

                            }
                            CommonMethod.Sleep(100);
                        }
                        //正向
                        if (i == 0)
                        {
                            var modelTemp = new TestResult();
                            modelTemp.ChildrenItem = $"正向[{CalibrateTempVal[t].Item1}-{CalibrateTempVal[t + 1].Item1}]K";
                            modelTemp.MeasureVal = K[i, t].ToString("0.000");
                            modelTemp.MeasurementStandard = "[0.92,1.09]";
                            if (K[i, t] >= 0.92 && K[i, t] <= 1.09)
                            {
                                modelTemp.Result = true;
                            }
                            else
                            {
                                modelTemp.Result = false;
                            }



                            AddModelAction(modelTemp);

                            modelTemp = new TestResult();
                            modelTemp.ChildrenItem = $"正向[{CalibrateTempVal[t].Item1} - {CalibrateTempVal[t + 1].Item1}]B";
                            modelTemp.MeasureVal = B[i, t].ToString("0.000");
                            modelTemp.Result = true;
                            AddModelAction(modelTemp);
                        }
                        else
                        {
                            var modelTemp = new TestResult();
                            modelTemp.ChildrenItem = $"反向[{CalibrateTempVal[t].Item1} - {CalibrateTempVal[t + 1].Item1}]K";
                            modelTemp.MeasureVal = K[i, t].ToString("0.000");
                            modelTemp.MeasurementStandard = "[0.92,1.09]";
                            if (K[i, t] >= 0.92 && K[i, t] <= 1.09)
                            {
                                modelTemp.Result = true;
                            }
                            else
                            {
                                modelTemp.Result = false;
                            }

                            AddModelAction(modelTemp);

                            modelTemp = new TestResult();
                            modelTemp.ChildrenItem = $"反向[{CalibrateTempVal[t].Item1}  -  {CalibrateTempVal[t + 1].Item1}]B";
                            modelTemp.MeasureVal = B[i, t].ToString("0.000");
                            modelTemp.Result = true;
                            AddModelAction(modelTemp);
                        }
                    }
                }
                LogHelper.Debug("写入成功");
                CommonData.M3905D.SetCurrent(0);
                CommonData.M3905D.SetVoltage(0);

                LogHelper.Debug("MOS断开");
                //MOS断开
                if (!PlayUDSHelper.MOSControl(MOSEnum.断开))
                {
                    LogHelper.Debug("MOS断开失败");

                    return false;
                }
                else
                {
                    //LogHelper.Debug("MOS断开成功");
                }


                LogHelper.Debug($"产品断电");
                if (CommonData.N8331.SetOutPut4(false) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    throw new Exception("电芯模拟器N8331关闭使能失败");
                }
               
                CommonMethod.Sleep(1000);

                LogHelper.Debug($"产品上电");

                if (CommonData.N8331.SetOutPut4(true) == false)
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    throw new Exception("电芯模拟器N8331开启使能失败");
                }
               

                LogHelper.Debug($"关闭M3905D使能");
              
                CommonMethod.Sleep(1000);

                if (!CommonMethod.Unlock())
                {
                    AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                    throw new Exception("解锁失败");
                }

                //关闭继电器
                NewCanHelper.CloseTotalCurrentCollection();
                CommonData.TestCount++;
                return true;

            }


               
        }


       
        TestResult resetModel = new TestResult();
        public void Reset()
        {
            ////复位控制器
            //LinHelper.Reset();
            //resetModel = new TestResult() { ChildrenItem = "复位", Result = null };
            //LinHelper.StopAllSend();
            //CommonMethod.Sleep(1000);
            //LinHelper.StartPeriodicMessage();
            LogHelper.Debug($"产品断电");
            if (CommonData.N8331.SetOutPut4(false) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                throw new Exception("电芯模拟器N8331关闭使能失败");
            }
            
            CommonMethod.Sleep(1000);

            LogHelper.Debug($"产品上电");

            if (CommonData.N8331.SetOutPut4(true) == false)
            {
                AddModelAction(new TestResult() { ChildrenItem = "电流标定失败", Result = false });
                throw new Exception("电芯模拟器N8331开启使能失败");
            }
            
            CommonMethod.Sleep(500);
        }


       

       


        public static (double, double)? GetAAndB(double[] x, double[] y)
        {
            // 确保数组长度一致
            if (x.Length != y.Length)
            {
                Console.WriteLine("X和Y的数组长度必须相同！");
                return (double.NaN, double.NaN);
            }
            if (x.Distinct().Count() == 1)
            {
                Console.WriteLine("所有的 x 值相同，无法拟合直线。");
                return null;
            }

            if (y.Distinct().Count() == 1)
            {
                Console.WriteLine("所有的 y 值相同，拟合水平线。");
                double c = y[0];
                return (0, c);
            }

            int n = x.Length;

            double sX = x.Sum();
            double sY = y.Sum();
            double sXY = 0;
            for (int i = 0; i < x.Length; i++)
            {
                sXY += x[i] * y[i];
            }
            double sX2 = x.Sum(m => m * m);
            double slope = (n * sXY - sX * sY) / (n * sX2 - sX * sX);
            double intercept = (sY - slope * sX) / n;

            ushort aW = (ushort)(Math.Abs(slope) * 1000);
            var aWTemp = aW / 1000.0;
            short bW = (short)(intercept * 1000);
            var bWTemp = bW / 1000.0;


            // 计算所需的和
            double sumX = x.Sum();
            double sumY = y.Sum();
            double sumXY = x.Zip(y, (xi, yi) => xi * yi).Sum();
            double sumX2 = x.Sum(xi => xi * xi);

            // 计算斜率a和截距b
            double a = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
            double b = (sumY - a * sumX) / n;
            return (aWTemp, bWTemp);
        }
        /// <summary>
        /// 改变电源电流并采集数据
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>


        /// <summary>
        /// UDS读取电流平均值
        /// </summary>
        /// <returns></returns>
        (double, double) getProduceCurrent(int t = 0)
        {
            List<double> currents = new List<double>();
            List<double> currents2 = new List<double>();
            try
            {
                for (int i = 0; i < 6; i++)
                {
                    currents2.Add(CommonData.M3905D.GetMEASCurrent());
                    var Currlin = ZCData.GetDL;
                    currents.Add(Currlin);
                    LogHelper.Debug($"产品电流{Currlin}");
                    CommonMethod.Sleep(120);
                }
                currents.Remove(currents.Max());
                currents.Remove(currents.Min());
                currents2.Remove(currents2.Min());
                currents2.Remove(currents2.Min());
            }
            catch (Exception)
            {
                AddModelAction(new TestResult() { ChildrenItem = "电流标定失败" });
                throw new Exception("电流标定失败");
            }
            return (currents.Average(), currents2.Average());
        }
    }
}
