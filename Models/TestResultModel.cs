using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class TestResultModel
    {
        /// <summary>
        /// 展示的文本
        /// </summary>
        public string ShowTxt { get; set; }

        /// <summary>
        /// 合格值
        /// </summary>
        public double QualifiedValue { get; set; } = double.NaN;
        /// <summary>
        /// 工装板测量值
        /// </summary>
        public double ToolingBoardValue { get;set; } = double.NaN;
        /// <summary>
        /// 最小值
        /// </summary>
        public double MinValue {  get; set; }
        /// <summary>
        /// 最大值
        /// </summary>

        public double MaxValue { get; set; }
        /// <summary>
        /// 显示范围用的文本
        /// </summary>
        public string RangeShowTxt { get; set; }
        /// <summary>
        /// 实际值-合格值
        /// </summary>
        public double Difference
        {
            get
            {
                if(QualifiedValue!=double.NaN&& ToolingBoardValue != double.NaN)
                {
                    return ToolingBoardValue-QualifiedValue;
                }
                return double.NaN;
            }
        }
        /// <summary>
        /// 是否合格
        /// </summary>
        public bool IsQualified { get; set; }

        public string Remarks { get; set; }
    }
}
