using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.FCT_GenerateKey
{
    public class FCT_GenerateKey
    {

        /// <summary>
        /// 返回类型
        /// </summary>
        public enum ResultType
        {
            /// <summary>
            /// 成功
            /// </summary>
            SUCCESS,
            /// <summary>
            /// 输入参数无效
            /// </summary>
            INVALID_ENTRY_PARAMETER,
            /// <summary>
            /// DLL无效
            /// </summary>
            INVALID_DLL,
            /// <summary>
            /// 生成失败
            /// </summary>
            GENERATE_FAIL,

        }

        /// <summary>
        /// 安全算法文件夹路径
        /// </summary>
        private readonly static string SecurityAlgorithmFilePath = Directory.GetCurrentDirectory() + "\\SecurityAlgorithm";

        /// <summary>
        /// 安全算法解锁函数名称列表
        /// </summary>
        private readonly static List<string> SecurityAlgorithmFunctionNameList = new List<string>() { "GenerateKeyEx", "ZLGKey" };

        /// <summary>
        /// 项目生成密钥类
        /// </summary>
        private static GenerateKey ProjectGenerateKey = null;

        /// <summary>
        /// 获取支持项目名称
        /// </summary>
        /// <returns></returns>
        public static List<string> GetSupportProjectName()
        {
            List<string> nameList = new List<string>();

            if (Directory.Exists(SecurityAlgorithmFilePath))
            {
                string[] filePaths = Directory.GetDirectories(SecurityAlgorithmFilePath);
                foreach (string filePath in filePaths)
                {
                    nameList.Add(Path.GetFileName(filePath));
                }
            }

            return nameList;
        }


        /// <summary>
        /// 生成密钥
        /// </summary>
        /// <param name="projectName">项目名称</param>
        /// <param name="level">安全等级</param>
        /// <param name="seeds">种子</param>
        /// <param name="key">返回密钥</param>
        /// <returns></returns>
        public static ResultType GenerateKey(string projectName, byte level, List<byte> seeds, out List<byte> key)
        {
            key = new List<byte>();
            string SecurityAlgorithmDirectory = SecurityAlgorithmFilePath + "\\" + projectName;
            if (!Directory.Exists(SecurityAlgorithmDirectory)) { return ResultType.INVALID_ENTRY_PARAMETER; }
            string[] dllFiles = Directory.GetFiles(SecurityAlgorithmDirectory, "*.dll");
            if (dllFiles.Length == 0) { return ResultType.INVALID_ENTRY_PARAMETER; }
            string SecurityAlgorithmDLLPath = dllFiles[0];

            if (ProjectGenerateKey != null)
            {
                ProjectGenerateKey.UnLoadAlgorithm();
                ProjectGenerateKey = null;
            }
            ProjectGenerateKey = new GenerateKey();
            if (!ProjectGenerateKey.LoadAlgorithm(SecurityAlgorithmDLLPath, SecurityAlgorithmFunctionNameList))
            {
                return ResultType.INVALID_DLL;
            }
            GenerateKey.GenerateKeyPara generatePara = new GenerateKey.GenerateKeyPara(seeds.ToArray(), (uint)seeds.Count, level, (uint)seeds.Count);
            int res = ProjectGenerateKey.Invoke(generatePara);
            if (res != 0)
            {
                return ResultType.GENERATE_FAIL;
            }
            key = generatePara.iopKeyArray.ToList();

            ProjectGenerateKey.UnLoadAlgorithm();
            ProjectGenerateKey = null;

            return ResultType.SUCCESS;
        }
    }
}

