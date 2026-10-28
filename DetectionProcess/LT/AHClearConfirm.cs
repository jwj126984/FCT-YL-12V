using AllData;
using Common;
using Common.UDS;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DetectionProcess.LT
{
    /// <summary>
    /// 累积充放电AH数清除确认（T75 / T09_M 第27项）
    /// 读取总线 0x3CD 报文中的 LBMS_CumuChargeAh、LBMS_CumuDischargeAh，
    /// 判定两个物理值均小于 2。
    /// 参数约定（ini，MedthodName=AHClearConfirm）：
    ///   充电参数：ChildrenItem 含"充电"，QualifiedValue=2（上限），MeasurementStandard=<2
    ///   放电参数：ChildrenItem 含"放电"，QualifiedValue=2（上限），MeasurementStandard=<2
    /// </summary>
    public class AHClearConfirm : DetectBase
    {
        public override bool Start(IEnumerable<TestParameter> pars)
        {
            LogHelper.Debug("------累积充放电AH数清除确认------");

            var parCharge = pars.FirstOrDefault(m => m.ChildrenItem != null && m.ChildrenItem.Contains("充电"));
            var parDischarge = pars.FirstOrDefault(m => m.ChildrenItem != null && m.ChildrenItem.Contains("放电"));

            // 上限默认 2Ah（图判定标准：物理值小于2）
            double chargeLimit = parCharge != null && parCharge.QualifiedValue.HasValue ? parCharge.QualifiedValue.Value : 2.0;
            double dischargeLimit = parDischarge != null && parDischarge.QualifiedValue.HasValue ? parDischarge.QualifiedValue.Value : 2.0;

            try
            {
                CommonMethod.Sleep(500); // 等待清除后报文稳定

                // 回读累积充电AH
                double chargeAh = ZCData.CumuChargeAh;
                LogHelper.Debug($"回读累积充电AH：{chargeAh}");

                // 回读累积放电AH
                double dischargeAh = ZCData.CumuDischargeAh;
                LogHelper.Debug($"回读累积放电AH：{dischargeAh}");

                bool chargePass = chargeAh < chargeLimit;
                bool dischargePass = dischargeAh < dischargeLimit;

                AddModelAction(new TestResult()
                {
                    ChildrenItem = parCharge != null ? parCharge.ChildrenItem : "累积充电AH",
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    QualifiedValue = ("<" + chargeLimit.ToString("0.###")),
                    MeasureVal = chargeAh.ToString("0.###"),
                    MeasurementStandard = parCharge != null ? parCharge.MeasurementStandard : "<" + chargeLimit + "Ah",
                    DifferenceVal = chargeAh.ToString("0.###"),
                    Result = chargePass
                });

                AddModelAction(new TestResult()
                {
                    ChildrenItem = parDischarge != null ? parDischarge.ChildrenItem : "累积放电AH",
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    QualifiedValue = ("<" + dischargeLimit.ToString("0.###")),
                    MeasureVal = dischargeAh.ToString("0.###"),
                    MeasurementStandard = parDischarge != null ? parDischarge.MeasurementStandard : "<" + dischargeLimit + "Ah",
                    DifferenceVal = dischargeAh.ToString("0.###"),
                    Result = dischargePass
                });

                CommonData.TestCount++;
                return chargePass && dischargePass;
            }
            catch (Exception ex)
            {
                AddModelAction(new TestResult()
                {
                    ChildrenItem = "累积充放电AH数清除确认",
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    MeasureVal = ex.Message,
                    Result = false
                });
                LogHelper.WriteLog($"累积充放电AH数清除确认异常：{ex.Message}", true);
                return false;
            }
        }
    }
}
