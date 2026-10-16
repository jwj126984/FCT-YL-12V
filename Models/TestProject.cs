using System;

namespace Model
{
    /// <summary>
    /// 
    /// </summary>
    public class TestProject
    {
        /// <summary>
        /// 
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string ProjectName { get; set; }="";
        /// <summary>
        /// 
        /// </summary>
        public int? BoardId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public int? UserId { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string CodeBar { get; set; } = "";
        /// <summary>
        /// 
        /// </summary>
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public DateTime EndTime { get; set; }
    }
}
