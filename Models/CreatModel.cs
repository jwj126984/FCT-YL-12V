using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public class CreatModel
    {
        public static void Create()
        {
            string conStr = $"Server=10.144.179.147;Port=3306;Database=FCT_Database;Uid=root;Pwd=pu2210037517;";
            var ado = PlayDBHelper.ADOHelper.GetADO(conStr);
            ADOHelper.CreateDbModelAll(conStr, DBType.MySql);
            //var t = ado.SelectBySql("select * from Sys_User");
        }
    }
}
