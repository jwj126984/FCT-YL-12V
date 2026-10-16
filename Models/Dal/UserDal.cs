using Model;
using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dal
{
    public class UserDal: DalBase
    {
        public Sys_Role GetRoleByAcc(string acc)
        {
            List<Tuple<string, object>> pars = new List<Tuple<string, object>>();
            pars.Add(new Tuple<string, object>("Acc", acc));
            string sql = $"SELECT * from Sys_Role WHERE Id = (select RoleId from Sys_User where  Account=@Acc)";
            var temp= ado.SelectBySqlUsePars(sql, pars);
            if(temp==null)
                return null;
            return ADOHelper.DataSetToList<Sys_Role>(temp).FirstOrDefault();
        }

        public Sys_User Login(string acc ,string pwd)
        {
            List<Tuple<string, object>> pars = new List<Tuple<string, object>>();
            pars.Add(new Tuple<string, object>("Acc", acc));
            pars.Add(new Tuple<string, object>("Pwd", pwd));
            string sql = $"select RoleId from Sys_User where  Account=@Acc and Pwd=@Pwd";
            var temp = ado.SelectBySqlUsePars(sql, pars);
            if (temp == null)
                return null;
            return ADOHelper.DataSetToList<Sys_User>(temp).FirstOrDefault();
        }
    }
}
