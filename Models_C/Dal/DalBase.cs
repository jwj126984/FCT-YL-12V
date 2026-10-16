using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AllData;

namespace Models_C.Dal
{
    public class DalBase
    {

        //static string conStr = $"Server=10.144.182.71;Port=3306;Database=seres;Uid=root;Pwd=play0726;Allow User Variables=True";

        static string conStr = $"Server=127.0.0.1;Port=3306;Database={ConfigHelper.SelectType};Uid=root;Pwd=123456;Allow User Variables=True;AllowPublicKeyRetrieval=true;SSL Mode=None";
        protected static IDataBase ado = PlayDBHelper.ADOHelper.GetADO(conStr);

        

        // 当前数据库名称
        protected static string CurrentDatabase { get; private set; }

        // 基本连接参数（可改为从配置文件读取）
        private const string Server = "127.0.0.1";
        private const int Port = 3306;
        private const string Uid = "root";
        private const string Pwd = "123456"; // 建议加密或从配置读取

        // 构造函数：默认连接 ConfigHelper.SelectType 指定的数据库
        public DalBase()
        {
            ConnectToDatabase(ConfigHelper.SelectType);
        }

        /// <summary>
        /// 动态切换到新数据库
        /// </summary>
        /// <param name="databaseName">目标数据库名称</param>
        public static void ConnectToDatabase(string databaseName)
        {
            if (string.IsNullOrEmpty(databaseName))
                throw new ArgumentException("数据库名不能为空");

            // 如果已经是目标数据库，则无需重新连接
            if (CurrentDatabase == databaseName)
                return;

            // 构造新连接字符串
            string newConStr = $"Server={Server};Port={Port};Database={databaseName};" +
                              $"Uid={Uid};Pwd={Pwd};Allow User Variables=True;" +
                              "AllowPublicKeyRetrieval=true;SSL Mode=None";

            // 创建新连接
            var newAdo = PlayDBHelper.ADOHelper.GetADO(newConStr);

            // 释放旧连接（如果存在）
            if (ado != null)
            {
                // 假设 IDataBase 实现了 IDisposable
                (ado as IDisposable)?.Dispose();
            }

            // 更新连接和当前数据库名
            ado = newAdo;
            CurrentDatabase = databaseName;
        }

    }

    

}
