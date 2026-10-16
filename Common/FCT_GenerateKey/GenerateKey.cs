using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.FCT_GenerateKey
{
    public class GenerateKey
    {
        #region 类
        /// <summary>
        /// 生成密钥参数
        /// </summary>
        public class GenerateKeyPara
        {
            /// <summary>
            /// 种子
            /// </summary>
            public byte[] ipSeedArray;

            /// <summary>
            /// 种子长度
            /// </summary>
            public uint iSeedArraySize;

            /// <summary>
            /// 请求等级
            /// </summary>
            public uint iSecurityLevel;

            /// <summary>
            /// 变量
            /// </summary>
            public byte[] ipVariant;

            /// <summary>
            /// 返回密钥
            /// </summary>
            public byte[] iopKeyArray;

            /// <summary>
            /// 密钥最大长度
            /// </summary>
            public uint iMaxKeyArraySize;

            /// <summary>
            /// 返回密钥长度
            /// </summary>
            public uint oActualKeyArraySize;

            public GenerateKeyPara(byte[] ipSeedArray, uint iSeedArraySize, uint iSecurityLevel, uint iMaxKeyArraySize)
            {
                this.ipSeedArray = ipSeedArray;
                this.iSeedArraySize = iSeedArraySize;
                this.iSecurityLevel = iSecurityLevel;
                this.ipVariant = ipSeedArray;
                this.iopKeyArray = new byte[iMaxKeyArraySize];
                this.iMaxKeyArraySize = iMaxKeyArraySize;
            }
        }
        #endregion

        #region 委托/事件  
        /// <summary>
        /// 获取密钥委托
        /// </summary>
        /// <param name="ipSeedArray"></param>
        /// <param name="iSeedArraySize"></param>
        /// <param name="iSecurityLevel"></param>
        /// <param name="ipVariant"></param>
        /// <param name="iopKeyArray"></param>
        /// <param name="iMaxKeyArraySize"></param>
        /// <param name="oActualKeyArraySize"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        private delegate int GenerateKeyEx(byte[] ipSeedArray, uint iSeedArraySize, uint iSecurityLevel, byte[] ipVariant, byte[] iopKeyArray, uint iMaxKeyArraySize, out uint oActualKeyArraySize);

        /// <summary>
        /// 获取密钥委托 (ZLGKey)
        /// </summary>
        /// <param name="iSeedArray"></param>
        /// <param name="iSeedArraySize"></param>
        /// <param name="iSecurityLevel"></param>
        /// <param name="iVariant"></param>
        /// <param name="iKeyArray"></param>
        /// <param name="iKeyArraySize"></param>
        /// <returns></returns>
        [UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        private delegate int ZLGKeyDelegate(byte[] iSeedArray, ushort iSeedArraySize, uint iSecurityLevel, string iVariant, byte[] iKeyArray, ref ushort iKeyArraySize);

        /// <summary>
        /// 获取密钥函数
        /// </summary>
        private GenerateKeyEx GenerateKeyFun;

        private ZLGKeyDelegate ZLGKeyFun;
        private bool IsZLGKey = false;
        #endregion


        #region 全局定义
        /// <summary>
        /// DLL句柄
        /// </summary>
        private DLLHelper DLL = new DLLHelper();

        /// <summary>
        /// 错误信息
        /// </summary>
        private string EorrorMsg = "";
        #endregion

        /// <summary>
        /// 加载安全算法
        /// </summary>
        /// <param name="fileName">安全算法路径</param>
        /// <param name="procName">函数名称</param>
        /// <returns></returns>
        public bool LoadAlgorithm(string fileName, string procName)
        {
            if (!DLL.LoadDll(fileName, procName, out EorrorMsg))
            {
                DLL.UnLoadDll();
                return false;
            }

            GenerateKeyFun = (GenerateKeyEx)DLL.GetFunctionAddress(typeof(GenerateKeyEx));

            return true;
        }

        /// <summary>
        /// 加载安全算法(多个函数名称)
        /// </summary>
        /// <param name="fileName">安全算法路径</param>
        /// <param name="procName">函数名称</param>
        /// <returns></returns>
        public bool LoadAlgorithm(string fileName, List<string> procName)
        {
            bool res = false;
            foreach (string name in procName)
            {
                if (DLL.LoadDll(fileName, name, out EorrorMsg))
                {
                    res = true;
                    if (name == "ZLGKey")
                    {
                        ZLGKeyFun = (ZLGKeyDelegate)DLL.GetFunctionAddress(typeof(ZLGKeyDelegate));
                        IsZLGKey = true;
                    }
                    else
                    {
                        GenerateKeyFun = (GenerateKeyEx)DLL.GetFunctionAddress(typeof(GenerateKeyEx));
                        IsZLGKey = false;
                    }
                    break;
                }
            }

            if (!res)
            {
                DLL.UnLoadDll();
            }

            return res;
        }

        /// <summary>
        /// 卸载安全算法
        /// </summary>
        public void UnLoadAlgorithm()
        {
            EorrorMsg = "";
            GenerateKeyFun = null;
            ZLGKeyFun = null;
            IsZLGKey = false;
            DLL.UnLoadDll();
        }

        /// <summary>
        /// 调用密钥生成
        /// </summary>
        /// <param name="Para">参数</param>
        /// <returns></returns>
        public int Invoke(GenerateKeyPara Para)
        {
            if (IsZLGKey)
            {
                if (ZLGKeyFun == null) { EorrorMsg = "未加载有效安全算法！"; return -1; }

                ushort keyArraySize = (ushort)Para.iMaxKeyArraySize;
                int result = ZLGKeyFun(Para.ipSeedArray, (ushort)Para.iSeedArraySize, Para.iSecurityLevel, null, Para.iopKeyArray, ref keyArraySize);
                Para.oActualKeyArraySize = keyArraySize;
                return result;
            }
            else
            {
                if (GenerateKeyFun == null) { EorrorMsg = "未加载有效安全算法！"; return -1; }

                return GenerateKeyFun(Para.ipSeedArray, Para.iSeedArraySize, Para.iSecurityLevel, Para.ipVariant, Para.iopKeyArray, Para.iMaxKeyArraySize, out Para.oActualKeyArraySize);
            }
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        public string GetErrorMsg()
        {
            return EorrorMsg;
        }
    }
}

