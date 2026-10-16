using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dal
{
    public abstract class DalBase
    {
        //static string conStr = $"Server=10.144.179.147;Port=3306;Database=FCT_Database;Uid=root;Pwd=pu2210037517;";
        static string conStr = $"Server=127.0.0.1;Port=3306;Database=FCT_Database;Uid=root;Pwd=yc666;";
        protected IDataBase ado = PlayDBHelper.ADOHelper.GetADO(conStr);
    }
}
