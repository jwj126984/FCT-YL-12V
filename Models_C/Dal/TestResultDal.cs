using Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models_C.Dal
{
    public class TestResultDal: DalBase
    {
        public int InsertModels(List<TestResult> results)
        {
            string sql = "INSERT INTO TestResult (BoardNum,ChildrenItem,StartTime,EndTime,QualifiedValue,MeasurementStandard,MeaMinVal,MeaMaxVal,MeasureVal,DifferenceVal,Result,Extend1,Extend2,Extend3) VALUES ";
            List<string> strings = new List<string>();
            foreach (TestResult result in results) {
                string vals = $"('{result.BoardNum}','{result.ChildrenItem}',{(result.StartTime.HasValue ? ($"'{result.StartTime.ToString()}'") : "null")},{(result.EndTime.HasValue ? $"'{result.EndTime.ToString()}'" : "null")},'{result.QualifiedValue}','{result.MeasurementStandard}',{(result.MeaMinVal.HasValue ? result.MeaMinVal.ToString() : "null")},{(result.MeaMaxVal.HasValue ? result.MeaMaxVal.ToString() : "null")},'{result.MeasureVal}','{result.DifferenceVal}',{(result.Result.HasValue? (result.Result==true?"'1'":"'0'"):"null")},'{result.Extend1}','{result.Extend2}','{result.Extend3}')";
                strings.Add(vals);
            }
            sql += string.Join(",", strings);
            return ado.ExecuteSql(sql);
        }

        /// <summary>
        /// 删除指定月之前的数据
        /// </summary>
        /// <param name="month"></param>
        /// <returns></returns>
        public static int DeleteByTime(int month)
        {
            string sql = $"DELETE FROM TestResult WHERE EndTime < '{DateTime.Now.AddMonths(-month).ToString("yyyy-MM-dd HH:mm:ss")}'";
            return ado.ExecuteSql(sql);
        }
    }
}
