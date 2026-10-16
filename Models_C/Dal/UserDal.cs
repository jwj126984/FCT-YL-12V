using Model;
using Mysqlx.Crud;
using MySqlX.XDevAPI.Common;
using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models_C.Dal
{
    public class UserDal: DalBase
    {
        /// <summary>
        /// 根据账号查询身份以及板子名称
        /// </summary>
        /// <param name="acc"></param>
        /// <returns></returns>
        public static (Role,string,string) GetInfoByAcc(string acc)
        {
            
            List<Tuple<string, object>> pars = new List<Tuple<string, object>>();
            pars.Add(new Tuple<string, object>("Acc", acc));
            string sql = $"SELECT * from Sys_User WHERE Account=@Acc";
            var temp = ado.SelectBySqlUsePars(sql, pars);
            if (temp == null)
            {
                return (Role.None, "", "");
            }
            var user = ADOHelper.DataSetToList<Sys_User>(temp).FirstOrDefault();
            if (user != null)
            {
                return ((Role)user.UserType, user.TestBoard,user.UserName);
            }
            return (Role.None,"","");
        }
        /// <summary>
        /// 登录
        /// </summary>
        /// <param name="acc"></param>
        /// <param name="pwd"></param>
        /// <returns></returns>
        public static Sys_User Login(string acc,string pwd)
        {
            List<Tuple<string, object>> pars = new List<Tuple<string, object>>();
            pars.Add(new Tuple<string, object>("Acc", acc));
            pars.Add(new Tuple<string, object>("Pwd", pwd));
            string sql = $"SELECT * from Sys_User WHERE Account=@Acc and Pwd=@Pwd";
            var temp = ado.SelectBySqlUsePars(sql, pars);
            return ADOHelper.DataSetToList<Sys_User>(temp).FirstOrDefault();
        }
        /// <summary>
        /// 修改指定用户的测试流程
        /// </summary>
        /// <param name="id"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        public static bool UpdateTestItem(int id,string items)
        {
            string sql = $"UPDATE Sys_User SET TestItem='{items}' WHERE Id={id}";
            return ado.ExecuteSql(sql) > 0;
        }

        public static bool UpdateTestBoard(string user, string board)
        {
            string sql = $"UPDATE Sys_User SET TestBoard='{board}' WHERE UserName='{user}'";
            return ado.ExecuteSql(sql) > 0;
        }
        public static List<Sys_User> GetAllUser()
        {
            return ADOHelper.DataSetToList<Sys_User>(ado.SelectBySql("Select * from Sys_User where UserType!=-1"));
        }
        

        public static bool UpdatePwd(int id,string pwd)
        {
            string sql = $"UPDATE Sys_User SET Pwd=@pwd WHERE Id={id}";
            return ado.ExecuteSqlUsePars(sql, new List<Tuple<string, object>>() { new Tuple<string, object>("@pwd", pwd) })>0;
        }

        public static bool AddUser(int roleId,string name,string acc)
        {
            var model = new Sys_User();
            model.Pwd = "123";
            model.Account = acc;model.UserName = name;
            model.UserType = roleId;
            model.TestType = 1;
            return ADOHelper.Insert(model, ado) > 0;
        }


        public static bool Update(Sys_User model)
        {
            return ADOHelper.Update(model, model.Id, ado) > 0;
        }



        public static bool Reset(IEnumerable<Sys_User> models)
        {
            if (ado.ExecuteSql("DELETE FROM Sys_User") <= 0)
            {
                //return false;
            }
            string sql = "INSERT INTO Sys_User (TestBoard,UserType,UserName,Account,Pwd,TestType,TestItem) VALUES ";
            List<string> strings = new List<string>();
            foreach (var result in models)
            {
                string vals = $"('{result.TestBoard}','{result.UserType}','{result.UserName}','{result.Account}','{result.Pwd}','{result.TestType}','{result.TestItem}')";
                strings.Add(vals);
            }
            sql += string.Join(",", strings);
            return ado.ExecuteSql(sql)>0;
        }

    }

    

    public enum Role
    {
        None = 0,
        管理员=1,
        操作员=2,
        技术员=3,
        质量员=4
    }
    public enum TestType
    {
        None=0,
        生产测试=1,
        研发测试=2,
    }
}
