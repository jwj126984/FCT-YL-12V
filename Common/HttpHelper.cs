using Models_C;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Common
{
    public class HttpHelper
    {
        static HttpClient client = new HttpClient();
        /// <summary>
        /// 条码验证
        /// </summary>
        /// <returns></returns>
        public static (bool,string) VerBarCode(string code)
        {
            // 构造请求的表单数据
            var formData = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("Barcode", code),
                new KeyValuePair<string, string>("StationName", CommonData.Key_Vals.FirstOrDefault(m=>m.CommKey=="StationName").CommValue),
                new KeyValuePair<string, string>("RscName", CommonData.Key_Vals.FirstOrDefault(m=>m.CommKey=="RscName").CommValue),
                new KeyValuePair<string, string>("UserName", CommonData.Key_Vals.FirstOrDefault(m=>m.CommKey=="UserName").CommValue),
                new KeyValuePair<string, string>("IsMultiPlate", "false")
            });
            try
            {
                // 发送 POST 请求
                HttpResponseMessage response = client.PostAsync(CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "MesURL").CommValue, formData).Result;

                // 确保请求成功
                response.EnsureSuccessStatusCode();

                // 获取响应内容

                string xmlData = response.Content.ReadAsStringAsync().Result;

                // 1. 加载 XML 数据
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(xmlData);

                // 2. 提取 CDATA 内容
                string cdataContent = xmlDoc.DocumentElement.InnerText;

                // 3. 解析提取的内容为 XML 文档
                XmlDocument cdataDoc = new XmlDocument();
                cdataDoc.LoadXml(cdataContent);

                // 4. 使用 JsonConvert 将 XML 转为 JSON
                string jsonResult = JsonConvert.SerializeXmlNode(cdataDoc, Newtonsoft.Json.Formatting.Indented);
                var model = JsonConvert.DeserializeObject<HttpModel>(jsonResult);
                if (model == null || model.ReturnData.ErrMsg != "OK")
                {
                    return (false, model.ReturnData.ErrMsg);
                }
                return (true,"");
                // 输出 JSON
                //Console.WriteLine(jsonResult);
            }
            catch (HttpRequestException e)
            {
                //throw new Exception(e.Message);
                LogHelper.WriteLog(e.ToString(), true);
                return (false, e.Message);
            }
            catch(WebException ex)
            {
                return (false, ex.Message);
            }
            catch(SocketException ex)
            {
                return (false, ex.Message);
            }
            catch (AggregateException ex)
            {
                return (false, ex.ExceptionGetMessage());
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }
    
}
