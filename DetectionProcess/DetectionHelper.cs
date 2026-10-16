using AllData;
using Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DetectionProcess
{
    public static class DetectionHelper
    {
        /// <summary>
        /// 根据类名称创建实例
        /// </summary>
        /// <param name="instanceName">类名称</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static DetectBase GetDetectInstance(string instanceName)
        {
            string Detect = "";
            if (ConfigHelper.SelectType == "seres")
            {
                Detect = "DetectionProcess.SERES.";
            }
            else if (ConfigHelper.SelectType == "lt")
            {
                Detect = "DetectionProcess.LT.";
            }

            if (string.IsNullOrEmpty(instanceName))
            {
                throw new ArgumentNullException("实例名称不能为空");
            }
            var type = Type.GetType(Detect + instanceName);
            if (type == null)
            {
                throw new ArgumentNullException("该实例不存在");
            }
            var instance = Activator.CreateInstance(type) as DetectBase;
            if (instance == null)
            {
                throw new Exception("创建实例失败");
            }
            instance.M3905D = CommonData.M3905D;
            return instance;
        }
    }
}

