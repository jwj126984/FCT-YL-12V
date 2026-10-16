using Model;
using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models_C.Dal
{
    public class Key_ValDal : DalBase
    {
        /// <summary>
        /// 获取所有设置值
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Key_Val> GetAll()
        {
            string sql = $"SELECT * from Key_Val";
            var temp = ado.SelectBySql(sql);
            if (temp == null)
            {
                throw new Exception("数据库连接失败");
            }
            return ADOHelper.DataSetToList<Key_Val>(temp);
        }
        public static int UpdateValue(int id, Key_Val model)
        {
            return ADOHelper.Update(model, id, ado);
        }

        public static bool UpdateValue(string key,string value)
        {
            string sql = $"select * from Key_Val where CommKey='{key}'";
            var model = ado.SelectBySql(sql);
            if (model == null)
            {
                sql = $"INSERT INTO Key_Val (CommType,CommKey,CommValue) VALUES ('Data','{key}','{value}')";
            }
            else
            {
                sql = $"UPDATE Key_Val SET CommValue ='{value}' WHERE CommKey='{key}'";
            }
            return ado.ExecuteSql(sql) > 0;
        }

        public static bool Reset(IEnumerable<Key_Val> models)
        {
            if(ado.ExecuteSql("DELETE FROM Key_Val") <= 0)
            {
                //return false;
            }
            string sql = "INSERT INTO Key_Val (CommType,CommKey,CommValue) VALUES ";
            List<string> strings = new List<string>();
            foreach (var result in models)
            {
                string vals = $"('{result.CommType}','{result.CommKey}','{result.CommValue}')";
                strings.Add(vals);
            }
            sql += string.Join(",", strings);
            return ado.ExecuteSql(sql) > 0;
        }

        public static bool UpdateConfigTime(string userName)
        {
            string sql = $"SELECT * from Key_Val";
            var temp = ado.SelectBySql(sql);
            if (temp == null)
            {
                throw new Exception("数据库连接失败");
            }
            var configTime= ADOHelper.DataSetToList<Key_Val>(temp).FirstOrDefault(m=>m.CommKey== "配置日期");
            var configUser= ADOHelper.DataSetToList<Key_Val>(temp).FirstOrDefault(m=>m.CommKey== "配置人员");

            if (configTime != null)
            {
                configTime.CommValue = DateTime.Now.ToString();
                ADOHelper.Update(configTime, configTime.Id, ado);
            }

            if (configUser != null)
            {
                configUser.CommValue = userName;
                ADOHelper.Update(configUser, configUser.Id, ado);
            }
            return true;
        }
    }
}
