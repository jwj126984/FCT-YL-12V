using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.FCT_GenerateKey
{
    public class DLLHelper
    {

        /// </summary>
        /// <param name="lpFileName">DLL 文件名 </param>
        /// <returns> 函数库模块的句柄 </returns>
        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);
        /// <summary>
        /// 原型是 : FARPROC GetProcAddress(HMODULE hModule, LPCWSTR lpProcName);
        /// </summary>
        /// <param name="hModule"> 包含需调用函数的函数库模块的句柄 </param>
        /// <param name="lpProcName"> 调用函数的名称 </param>
        /// <returns> 函数指针 </returns>
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
        /// <summary>
        /// 原型是 : BOOL FreeLibrary(HMODULE hModule);
        /// </summary>
        /// <param name="hModule"> 需释放的函数库模块的句柄 </param>
        /// <returns> 是否已释放指定的 Dll</returns>
        [DllImport("kernel32", EntryPoint = "FreeLibrary", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        /// <summary>
        /// Loadlibrary 返回的函数库模块的句柄
        /// </summary>
        private IntPtr hModule = IntPtr.Zero;
        /// <summary>
        /// GetProcAddress 返回的函数指针
        /// </summary>
        private IntPtr farProc = IntPtr.Zero;


        /// <summary>
        /// 参数传递方式枚举 ,ByValue 表示值传递 ,ByRef 表示址传递
        /// </summary>
        public enum ModePass
        {
            ByValue = 0x0001,
            ByRef = 0x0002
        }


        /// <summary>
        /// 加载DLL
        /// </summary>
        /// <param name="FileName">文件路径</param>
        /// <param name="ProcName">函数名称</param>
        /// <param name="errorMsg">返回错误消息</param>
        /// <returns>加载结果</returns>
        public bool LoadDll(string FileName, string ProcName, out string errorMsg)
        {
            errorMsg = "";

            // 取得函数库模块的句柄
            hModule = LoadLibrary(FileName);
            // 若函数库模块的句柄为空，则抛出异常
            if (hModule == IntPtr.Zero) { errorMsg = "未找到文件 :" + FileName; return false; }
            // 取得函数指针
            farProc = GetProcAddress(hModule, ProcName);
            // 若函数指针，则抛出异常
            if (farProc == IntPtr.Zero) { errorMsg = "没有找到 :" + ProcName + "函数的入口！"; return false; }

            return true;
        }

        /// <summary>
        /// 获取函数地址
        /// </summary>[UnmanagedFunctionPointerAttribute(CallingConvention.Cdecl)]
        /// <param name="t">类型</param>
        /// <returns></returns>
        public Delegate GetFunctionAddress(Type t)
        {
            return Marshal.GetDelegateForFunctionPointer(farProc, t);
        }

        /// <summary>
        /// 卸载 Dll
        /// </summary>
        public void UnLoadDll()
        {
            FreeLibrary(hModule);
            hModule = IntPtr.Zero;
            farProc = IntPtr.Zero;
        }

        /// <summary>
        /// 调用所设定的函数
        /// </summary>
        /// <param name="ObjArray_Parameter"> 实参 </param>
        /// <param name="TypeArray_ParameterType"> 实参类型 </param>
        /// <param name="ModePassArray_Parameter"> 实参传送方式 </param>
        /// <param name="Type_Return"> 返回类型 </param>
        /// <returns> 返回所调用函数的 object</returns>
        public bool Invoke(out object Result, object[] ObjArray_Parameter, Type[] TypeArray_ParameterType, ModePass[] ModePassArray_Parameter, Type Type_Return, out string errorMsg)
        {
            Result = -1;
            errorMsg = "";
            if (hModule == IntPtr.Zero) { errorMsg = "函数库模块的句柄为空，请确保已进行 LoadDll 操作！"; return false; }
            if (farProc == IntPtr.Zero) { errorMsg = "函数指针为空，请确保已进行 LoadDll 操作！"; return false; }
            if (ObjArray_Parameter.Length != ModePassArray_Parameter.Length) { errorMsg = "参数个数及其传递方式的个数不匹配！"; return false; }

            try
            {
                // 下面是创建 MyAssemblyName 对象并设置其 Name 属性
                AssemblyName MyAssemblyName = new AssemblyName();
                MyAssemblyName.Name = "InvokeFun";
                // 生成单模块配件
                //AssemblyBuilder MyAssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(MyAssemblyName, AssemblyBuilderAccess.Run);
                AssemblyBuilder MyAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(MyAssemblyName, AssemblyBuilderAccess.Run);
                ModuleBuilder MyModuleBuilder = MyAssemblyBuilder.DefineDynamicModule("InvokeDll");
                // 定义要调用的方法 , 方法名为“ MyFun ”，返回类型是“ Type_Return ”参数类型是“ TypeArray_ParameterType ”
                MethodBuilder MyMethodBuilder = MyModuleBuilder.DefineGlobalMethod("MyFun", MethodAttributes.Public | MethodAttributes.Static, Type_Return, TypeArray_ParameterType);
                // 获取一个 ILGenerator ，用于发送所需的 IL
                ILGenerator IL = MyMethodBuilder.GetILGenerator();
                int i;
                for (i = 0; i < ObjArray_Parameter.Length; i++)
                {// 用循环将参数依次压入堆栈
                    switch (ModePassArray_Parameter[i])
                    {
                        case ModePass.ByValue:
                            IL.Emit(OpCodes.Ldarg, i);
                            break;
                        case ModePass.ByRef:
                            IL.Emit(OpCodes.Ldarga, i);
                            break;
                        default:
                            errorMsg = "第 " + (i + 1).ToString() + " 个参数没有给定正确的传递方式！";
                            return false;
                    }
                }
                // 判断处理器类型
                if (IntPtr.Size == 4)
                {
                    IL.Emit(OpCodes.Ldc_I4, farProc.ToInt32());
                }
                else if (IntPtr.Size == 8)
                {
                    IL.Emit(OpCodes.Ldc_I8, farProc.ToInt64());
                }
                else
                {
                    throw new PlatformNotSupportedException();
                }
                IL.EmitCalli(OpCodes.Calli, CallingConvention.Cdecl, Type_Return, TypeArray_ParameterType);
                IL.Emit(OpCodes.Ret); // 返回值
                MyModuleBuilder.CreateGlobalFunctions();
                // 取得方法信息
                MethodInfo MyMethodInfo = MyModuleBuilder.GetMethod("MyFun");
                Result = MyMethodInfo.Invoke(null, ObjArray_Parameter);
            }
            catch (Exception ex)
            {
                errorMsg = "执行函数失败，原因：" + ex.Message.ToString();
                return false;
            }

            return true;
        }



    }
}
