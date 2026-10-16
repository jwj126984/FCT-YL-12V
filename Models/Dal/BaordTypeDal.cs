using Model;
using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dal
{
    public class BaordTypeDal: DalBase
    {
        public List<BoardType> GetTypeByRoleId(int roleId)
        {
            string sql = $"SELECT * FROM BoardType WHERE Id IN (SELECT BoardTypeId FROM Board_Role WHERE RoleId={roleId})";
            var temp = ado.SelectBySql(sql);

            return ADOHelper.DataSetToList<BoardType>(temp);

        }
    }
}
