using Model;
using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.Dal
{
    public class TestProjectDal: DalBase
    {
        public TestProject Create(TestProject testProject)
        {
            if(ADOHelper.Insert<TestProject>(testProject, ado) > 0)
            {
                string sql = $"select * from TestProject where ProjectName='{testProject.ProjectName}' AND CodeBar='{testProject.CodeBar}' AND StartTime='{testProject.StartTime}'";
                var temp = ado.SelectBySql(sql);
                var model = ADOHelper.DataSetToList<TestProject>(temp).FirstOrDefault();
                return model;
            }
            return null;
        }
    }
}
