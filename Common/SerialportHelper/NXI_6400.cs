using Common.Can;
using Ngi.Component.Ctrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.SerialportHelper
{
    public class NXI_6400
    {
        private NxiDMMCtrl dmm;

        private double lsSetNPLC;

        private double lsSetDelay;

        private int lsSetCount;
        private bool? isParV = null;
        public NXI_6400(string diviceType = "NXI-6400", string ip = "192.168.0.2", int id = 1)
        {
            int nRet = 0;
            try
            {
                dmm = new NxiDMMCtrl(diviceType);
                nRet = dmm.Init();
                if (nRet != NxiOperRet.NXI_OK)
                {
                    throw new Exception("错误！ 初始化失败！");
                    //throw new Exception(nRet.ToString());
                }
                if (dmm.Open(ip, id) == NxiOperRet.NXI_OK)
                {
                    //MessageBox.Show("联机成功!");
                    //groupBoxMain.Enabled = true;

                }
                else
                {
                    throw new Exception("联机失败!");
                    //return false;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
                //return false;
            }
        }

        public void Close()
        {
            if (dmm != null)
            {
                dmm.Close();
                dmm.UnInit();
            }

        }


        public void SetDCVParam(int i)
        {
            //if (isParV == true)
            //{
            //    return;
            //}
            if (dmm != null)
            {
                dmm.Abort();

            }
            LogHelper.Debug("NXI-6400 配置测量电压参数");
            try
            {
                bool useAutoRange = true;//是否使用自动量程
                bool enableAdv = true;
                //bool enableLim =  false;
                int range = i;
                //bool enableNull =  false;
                double nplc = 5;
                int sampleCount = 1;
                double delay = 0;
                lsSetNPLC = nplc;
                lsSetDelay = delay;
                lsSetCount = sampleCount;
                if (dmm.SetDCVSampingParam(useAutoRange, range, sampleCount, delay) > 0 && dmm.SetDCVNPLC(nplc) > 0)
                {
                    if (enableAdv)//是否开启平均数功能
                    {
                        dmm.EnableCalcAverParam(true);
                    }
                    //if (enableLim)//是否开启上下限检查的功能
                    //{
                    //    dmm.EnableLimitParam(true, lowVal, highVal);
                    //}
                    //if (enableNull)//电压偏置
                    //{
                    //    dmm.EnableDCVNullStatus(true, nullVal);
                    //}

                }
                else
                {
                    throw new Exception("设定参数出错！");
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            isParV = true;
        }

        public void SetDCAParam(double i)
        {
            //if (isParV == false)
            //{
            //    return;
            //}
            if (dmm != null)
            {
                dmm.Abort();
               
            }
            LogHelper.Debug("NXI-6400 配置测量电流参数");
            bool useAutoRange = false;//是否使用自动量程
            double range = i;//量程值
            //bool enableAdv = false;
            //bool enableLim = false;
            //bool enableNull = false;
            double nplc = 1;
            int sampleCount = 1;
            double delay = 0;
            lsSetNPLC = nplc;
            lsSetDelay = delay;
            lsSetCount = sampleCount;
            if (dmm.SetDCCSampingParam(useAutoRange, range, sampleCount, delay) > 0 && dmm.SetDCCNPLC(nplc) > 0)
            {
                //if (enableAdv)//是否开启平均数功能
                //{
                //    dmm.EnableCalcAverParam(true);
                //}
                //if (enableLim)//是否开启上下限检查的功能
                //{
                //    dmm.EnableLimitParam(true, lowVal, highVal);
                //}
                //if (enableNull)//电压偏置
                //{
                //    dmm.EnableDCCNullStatus(true, nullVal);
                //}
            }
            else
            {
                throw new Exception("设定参数出错！");
            }
            isParV = false;
        }
        public void Start()
        {
            LogHelper.Debug("NXI-6400 开始测量");
            dmm.StartSampling();
        }
        public void AboutDC()
        {
            dmm.Abort();
        }

        public void Reset()
        {
            if (dmm != null)
            {
                dmm.Reset();
            }
        }

        //根据nplc 延时，测量次数计算等待时间
        private void WaitForFinish(double nplc, double delay, int count)
        {
            int oneWait = (int)(nplc * 20d + (delay * 1000));//一次测量的时间(ms)

            int totalWait = oneWait * count;

            System.Threading.Thread.Sleep(totalWait);
        }
        private bool WaitResultAndRead(out double[] results)
        {
            results = null;
            WaitForFinish(lsSetNPLC, lsSetDelay, lsSetCount);
            if (dmm.QuerySampingResult(out results) > 0)
            {
                return true;
            }
            else
            {
                return false;
            }

        }
        public (List<double>, (double, double, double, double)) GetResult()
        {
            LogHelper.Debug("NXI-6400 获取结果");
            List<double> results1 = new List<double>();
            (double, double, double, double) ress = (0, 0, 0, 0);
            double[] results;

            if (WaitResultAndRead(out results))
            {
                //this.listBoxRes.Items.Add("电压(V):");
                foreach (var result in results)
                {
                    //this.listBoxRes.Items.Add(result);
                    results1.Add(result);
                }

                LogHelper.Debug("NXI-6400 获取的结果(单位A)：" + string.Join(",", results1));
                ////平均测试结果。
                //bool enableAdv = true;
                //if (enableAdv)
                //{
                //    if (dmm.QueryCalcAverResult(out results) > 0)
                //    {
                //        string res = string.Format("平均数{0}mA, 偏差{1}mA，最小值{2}mA, 最大值{3}mA", results[0], results[1], results[2], results[3]);
                //        LogHelper.Debug(res);
                //        //this.listBoxRes.Items.Add(res);
                //        ress = (results[0], results[1], results[2], results[3]);
                //    }
                //}

                ////范围检查
                //bool enableLim = radioOpenLimDCV.Checked ? true : false;
                //if (enableLim)
                //{
                //    bool isLowFail = false;
                //    bool isHighFail = false;
                //    if (dmm.QueryLimitResult(out isLowFail, out isHighFail) > 0)
                //    {
                //        string res = string.Format("超下限:{0}， 超上限{1}", isLowFail, isHighFail);
                //        this.listBoxRes.Items.Add(res);
                //    }
                //}
            }
            return (results1, ress);
        }
        /// <summary>
        /// 获取电流
        /// </summary>
        /// <returns></returns>
        public (List<double>, (double, double, double, double)) GetCResult(double dw)
        {
           
            SetDCAParam(dw);
            Start();
            return GetResult();
        }
        public (List<double>, (double, double, double, double)) GetCResult2()
        {
            SetDCAParam(0.01);
            Start();
            return GetResult();
        }
        /// <summary>
        /// 获取电压
        /// </summary>
        /// <returns></returns>
        public double GetVResult()
        {
            SetDCVParam(100);
            Start();
            return GetResult().Item1.FirstOrDefault();
        }

        public bool CheckConnect()
        {
            SetDCAParam(3);
            Start();
            return GetResult().Item1.Count>0;
        }
    }
}
