using Common.SerialportHelper;
using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess
{
    public abstract class DetectBase
    {
        /// <summary>
        /// 添加结果
        /// </summary>
        public Action<TestResult> AddModelAction;
        /// <summary>
        /// 开始检测
        /// </summary>
        /// <param name="pars"></param>
        /// <returns></returns>
        public abstract bool Start(IEnumerable<TestParameter> pars);
        /// <summary>
        /// M3905D
        /// </summary>
        public PowerHelperUSB M3905D { get; set; }
        /// <summary>
        /// 测试不合格
        /// </summary>
        public Action<string> TestFail;
    }
}
