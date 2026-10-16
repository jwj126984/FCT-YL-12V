using Model;
using PlayDBHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models_C.Dal
{
    public class TestParDal: DalBase
    {
        /// <summary>
        /// 获取所有的测试标准
        /// </summary>
        /// <returns></returns>
        public static List<TestParameter> GetAllTestPar()
        {
            string sql = $"SELECT * from TestParameter";
            var temp = ado.SelectBySql(sql);
            if (temp == null)
            {
                throw new Exception("数据库连接失败");
            }
            return ADOHelper.DataSetToList<TestParameter>(temp);
        }
        /// <summary>
        /// 修改测试标准
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public static bool UpdatePar(TestParameter model)
        {
            //var adoTemp= PlayDBHelper.ADOHelper.GetADO()
            return ADOHelper.Update(model, model.Id, ado) > 0;
        }

        public static bool Update(string medthodName, double id, double q, TestParameter model)
        {
            var tempModel = ADOHelper.DataSetToList<TestParameter>(ado.SelectBySql($"select * from TestParameter where Id={id}")).FirstOrDefault();

            if (tempModel != null)
            {
                tempModel.QualifiedValue = model.QualifiedValue;
                tempModel.MeasurementStandard = model.MeasurementStandard;
                tempModel.ChildrenItem = model.ChildrenItem;
                if (string.IsNullOrEmpty(model.Extend1) == false)
                    tempModel.Extend1 = model.Extend1;
                tempModel.Extend2 = model.Extend2;
                //tempModel.Extend3 = model.Extend3;
                return ado.ExecuteSql($"UPDATE TestParameter SET QualifiedValue ={model.QualifiedValue},MeasurementStandard='{model.MeasurementStandard}',ChildrenItem='{model.ChildrenItem}',Extend1='{model.Extend1}',Extend2='{model.Extend2}' WHERE Id={tempModel.Id}") > 0;
                //return ADOHelper.Update(tempModel, tempModel.Id, ado) > 0;
            }
            else
            {
                //model.QualifiedValue = q;
                //model.MedthodName = medthodName;
                //return ADOHelper.Insert(model, ado) > 0;
                return false;
            }
        }


        public static bool Reset(IEnumerable<TestParameter> models)
        {
            if (ado.ExecuteSql("DELETE FROM TestParameter") <= 0)
            {
                //return false;
            }
            string sql = "INSERT INTO TestParameter (TestItem,MedthodName,ChildrenItem,QualifiedValue,MeasurementStandard,MinVal,MaxVal,Extend1,Extend2,Extend3) VALUES ";
            List<string> strings = new List<string>();
            foreach (var result in models)
            {
                string vals = $"('{result.TestItem}','{result.MedthodName}','{result.ChildrenItem}',{(result.QualifiedValue.HasValue ? ($"{result.QualifiedValue}") : "null")},'{result.MeasurementStandard}',{(result.MinVal.HasValue ? ($"{result.MinVal}") : "null")},{(result.MaxVal.HasValue ? ($"{result.MaxVal}") : "null")},'{result.Extend1}','{result.Extend2}','{result.Extend3}')";
                strings.Add(vals);
            }
            sql += string.Join(",", strings);
            return ado.ExecuteSql(sql) > 0;
        }
    }
}
