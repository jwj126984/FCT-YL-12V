
using System;

namespace Model
{
    /// <summary>
    /// 
    /// </summary>
    public class TestResult
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string BoardNum { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ChildrenItem { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime? EndTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string QualifiedValue { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MeasurementStandard { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double? MeaMinVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public double? MeaMaxVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string MeasureVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string DifferenceVal { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public bool? Result { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Extend1 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Extend2 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Extend3 { get; set; }
    }
}
