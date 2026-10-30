using AllData;
using Microsoft.Win32;
using Model;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace Common
{
    public class ExcelHelper
    {
        static string dirPath = System.IO.Directory.GetCurrentDirectory() + "\\Report\\";
        static string mesDirPath =  "C:\\YC\\MESReport\\";
        static ExcelHelper()
        {
            if (Directory.Exists(dirPath) == false)
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            if (Directory.Exists(mesDirPath) == false)
            {
                System.IO.Directory.CreateDirectory(mesDirPath);
            }
        }
        public static void Export1(List<TestResult> list, string useTime)
        {
            if (list.Count == 0)
                return;
            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.Filter = "Excel(2007-2016)|*.xlsx";
            //if (saveFileDialog.ShowDialog() != DialogResult.OK)
            //{
            //    return;
            //}
            //if (string.IsNullOrEmpty(saveFileDialog.FileName))
            //{
            //    throw new Exception("文件名为空");

            //    //return;//文件名为空或取消不作为
            //}
            string reportname = CommonData.BoardName;
            if (CommonData.BoardName == "T09_M")
            {
                reportname = ConfigHelper.UdsParameters.REPORT;
            }

            if (CommonData.BoardName == "WL_F710S")
            {
                reportname = ConfigHelper.UdsParameters.REPORT;
            }

            string filePath = dirPath + $"\\{reportname}_{list.FirstOrDefault().BoardNum}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{(CommonData.TestResult ? "PASS" : "FAIL")}.xlsx";
            var extension = System.IO.Path.GetExtension(filePath);

            if (extension.Equals(".xlsx"))
            {
                // 创建工作簿和工作表
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Sheet1");
                // 创建单元格样式（居中）
                // 创建字体样式
                IFont font = sheet.Workbook.CreateFont();
                font.FontHeightInPoints = 28; // 设置字体大小
                font.Color = IndexedColors.Blue.Index; // 设置字体颜色
                font.IsBold = true;
                // 创建单元格样式并应用字体
                ICellStyle cellStyle = sheet.Workbook.CreateCellStyle();
                ICellStyle cellStyle1 = sheet.Workbook.CreateCellStyle();
                cellStyle.SetFont(font);
                cellStyle1.SetFont(font);
                for (int i = 0; i < 6; i++)
                {
                    sheet.SetColumnWidth(i, 30 * 256);
                }
                cellStyle1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellStyle1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                cellStyle1.WrapText = true;
                bool isPass = list.FirstOrDefault(m => m.Result == false) == null;
                MergeCells(sheet, 0, 0, 6, $"FCT测试报告-{(CommonData.TestResult ? "PASS" : "FAIL")}", cellStyle1, 3);
                //string str = "";
                //str += $"产品名称:{CommonData.BoardName}-通用程序       ";
                //str += $"测试人员:{CommonData.CurrentUser.UserName}\n";
                //str += $"工装编号:{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "工装编号").CommValue}       ";
                //str += $"测试时间:{DateTime.Now.ToString()}(用时:{useTime})\n";
                //str += $"产品条码:{list.FirstOrDefault().BoardNum}";
                //MergeCells(sheet, 1, 0, 6, str, cellStyle1, 10);

                MergeCells(sheet, 1, 0, 0, $"产品名称:", cellStyle, 3);
                //MergeCells(sheet, 1, 4, 6, $"{}", cellStyle, 3);

                ////GL - E245 - C1011(2807030260)
                //MergeCells(sheet, 1, 1, 2, $"SERES-C1011(2807030260)", cellStyle, 3);
                //MergeCells(sheet, 1, 1, 2, $"SERES_F3-通用程序", cellStyle, 3);

                //数据库数据 SERES_F3 - 通用程序
                var produceName = ConfigHelper.UdsParameters.chanpin;
                if (produceName.Contains("?"))
                {
                    //produceName.Replace("?", (CommonData.Is171 ? "E171" : "E245"));
                }
                MergeCells(sheet, 1, 1, 2, produceName, cellStyle, 3);



                MergeCells(sheet, 1, 3, 3, $"测试人员:", cellStyle, 3);
                MergeCells(sheet, 1, 4, 6, $"{CommonData.CurrentUser.UserName}", cellStyle, 3);
                MergeCells(sheet, 2, 0, 0, $"配置人员:", cellStyle, 3);
                MergeCells(sheet, 2, 1, 2, $"李超", cellStyle, 3);
                MergeCells(sheet, 2, 3, 3, $"配置时间:", cellStyle, 3);
                MergeCells(sheet, 2, 4, 6, $"{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "配置日期").CommValue}", cellStyle, 3);

                MergeCells(sheet, 3, 0, 0, $"工装编号:", cellStyle, 3);
                MergeCells(sheet, 3, 1, 2, $"{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "工装编号").CommValue}", cellStyle, 3);
                MergeCells(sheet, 3, 3, 3, $"测试时间:", cellStyle, 3);
                MergeCells(sheet, 3, 4, 6, $"{DateTime.Now.ToString()}(用时:{useTime})", cellStyle, 3);
                MergeCells(sheet, 4, 0, 0, $"产品条码:", cellStyle, 3);
                MergeCells(sheet, 4, 1, 6, $"{list.FirstOrDefault().BoardNum}", cellStyle, 3);

                font = sheet.Workbook.CreateFont();
                font.IsBold = true;
                ICellStyle cellStyleTemp = sheet.Workbook.CreateCellStyle();
                cellStyleTemp.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellStyleTemp.SetFont(font);
                MergeCells(sheet, 5, 0, 0, "测试项", cellStyleTemp);
                MergeCells(sheet, 5, 1, 1, "合格值", cellStyleTemp);
                MergeCells(sheet, 5, 2, 2, "实测值", cellStyleTemp);
                MergeCells(sheet, 5, 3, 3, "差值", cellStyleTemp);
                MergeCells(sheet, 5, 4, 4, "判断标准", cellStyleTemp);
                MergeCells(sheet, 5, 5, 5, "用时（S）", cellStyleTemp);
                MergeCells(sheet, 5, 6, 6, "结果", cellStyleTemp);

                int rowIndex = 6;
                foreach (var item in list)
                {
                    //MergeCells(sheet, rowIndex, 0, 0, item.Id.ToString());
                    MergeCells(sheet, rowIndex, 0, 0, item.ChildrenItem);
                    MergeCells(sheet, rowIndex, 1, 1, string.IsNullOrEmpty(item.QualifiedValue) ? "-" : item.QualifiedValue.ToString());
                    MergeCells(sheet, rowIndex, 2, 2, string.IsNullOrEmpty(item.MeasureVal) ? "-" : item.MeasureVal.ToString());
                    MergeCells(sheet, rowIndex, 3, 3, string.IsNullOrEmpty(item.DifferenceVal) ? "-" : item.DifferenceVal.ToString());
                    MergeCells(sheet, rowIndex, 4, 4, string.IsNullOrEmpty(item.MeasurementStandard) ? "-" : item.MeasurementStandard.ToString());
                    if (item.EndTime == null || item.StartTime == null)
                    {
                        MergeCells(sheet, rowIndex, 5, 5, "-");
                    }
                    else
                    {
                        MergeCells(sheet, rowIndex, 5, 5, (item.EndTime - item.StartTime).Value.TotalSeconds.TruncateDouble(3).ToString());
                    }
                    if (item.Result == null)
                    {
                        MergeCells(sheet, rowIndex, 6, 6, "-");
                    }
                    else
                    {
                        MergeCells(sheet, rowIndex, 6, 6, item.Result.BoolToStr2());
                    }

                    rowIndex++;

                }

                using (FileStream stream = File.OpenWrite(filePath))
                {
                    workbook.Write(stream);
                    stream.Close();
                    LogHelper.ShowMsg("数据保存成功");
                }
                System.IO.File.Copy(filePath, mesDirPath + System.IO.Path.GetFileName(filePath), true);
                workbook.Close();
            }
        }

        public static void Export(List<TestResult> list, string useTime)
        {
            if (list.Count == 0)
                return;
            //SaveFileDialog saveFileDialog = new SaveFileDialog();
            //saveFileDialog.Filter = "Excel(2007-2016)|*.xlsx";
            //if (saveFileDialog.ShowDialog() != DialogResult.OK)
            //{
            //    return;
            //}
            //if (string.IsNullOrEmpty(saveFileDialog.FileName))
            //{
            //    throw new Exception("文件名为空");

            //    //return;//文件名为空或取消不作为
            //}
            var b = list.FirstOrDefault(m => m.Result == false);
            string filePath = dirPath + $"\\SERES_F2_{list.FirstOrDefault().BoardNum}_{DateTime.Now.ToString("yyyyMMddHHmmss")}_{(b == null ? "PASS" : "FAIL")}.xlsx";
            var extension = System.IO.Path.GetExtension(filePath);

            if (extension.Equals(".xlsx"))
            {
                // 创建工作簿和工作表
                IWorkbook workbook = new XSSFWorkbook();
                ISheet sheet = workbook.CreateSheet("Sheet1");
                // 创建单元格样式（居中）
                // 创建字体样式
                IFont font = sheet.Workbook.CreateFont();
                font.FontHeightInPoints = 28; // 设置字体大小
                font.Color = IndexedColors.Blue.Index; // 设置字体颜色
                font.IsBold = true;
                // 创建单元格样式并应用字体
                ICellStyle cellStyle = sheet.Workbook.CreateCellStyle();
                ICellStyle cellStyle1 = sheet.Workbook.CreateCellStyle();
                cellStyle.SetFont(font);
                cellStyle1.SetFont(font);
                for (int i = 0; i < 6; i++)
                {
                    sheet.SetColumnWidth(i, 30 * 256);
                }
                cellStyle1.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellStyle1.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
                cellStyle1.WrapText = true;
                bool isPass = list.FirstOrDefault(m => m.Result == false) == null;
                MergeCells(sheet, 0, 0, 6, $"FCT测试报告-{(isPass ? "PASS" : "FAIL")}", cellStyle1, 3);
                //string str = "";
                //str += $"产品名称:GEELY_{(CommonData.Is171 ? "E171" : "E245")}-通用程序       ";
                //str += $"测试人员:{CommonData.CurrentUser.UserName}\n";
                //str += $"工装编号:{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "工装编号").CommValue}       ";
                //str += $"测试时间:{DateTime.Now.ToString()}(用时:{useTime})\n";
                //str += $"产品条码:{list.FirstOrDefault().BoardNum}";
                //MergeCells(sheet, 1, 0, 6, str, cellStyle1, 10);

                MergeCells(sheet, 1, 0, 0, $"产品名称:", cellStyle, 3);
                MergeCells(sheet, 1, 1, 2, $"产品名称:SERES_F2-通用程序", cellStyle, 3);
                MergeCells(sheet, 1, 3, 3, $"测试人员:", cellStyle, 3);
                MergeCells(sheet, 1, 4, 6, $"{CommonData.CurrentUser.UserName}", cellStyle, 3);

                MergeCells(sheet, 2, 0, 0, $"配置人员:", cellStyle, 3);
                MergeCells(sheet, 2, 1, 2, $"{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "配置人员").CommValue}", cellStyle, 3);
                MergeCells(sheet, 2, 3, 3, $"配置时间:", cellStyle, 3);
                MergeCells(sheet, 2, 4, 6, $"{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "配置日期").CommValue}", cellStyle, 3);

                MergeCells(sheet, 3, 0, 0, $"工装编号:", cellStyle, 3);
                MergeCells(sheet, 3, 1, 2, $"{CommonData.Key_Vals.FirstOrDefault(m => m.CommKey == "工装编号").CommValue}", cellStyle, 3);
                MergeCells(sheet, 3, 3, 3, $"测试时间:", cellStyle, 3);
                MergeCells(sheet, 3, 4, 6, $"{DateTime.Now.ToString()}(用时:{useTime})", cellStyle, 3);
                MergeCells(sheet, 4, 0, 0, $"产品条码:", cellStyle, 3);
                MergeCells(sheet, 4, 1, 6, $"{list.FirstOrDefault().BoardNum}", cellStyle, 3);

                font = sheet.Workbook.CreateFont();
                font.IsBold = true;
                ICellStyle cellStyleTemp = sheet.Workbook.CreateCellStyle();
                cellStyleTemp.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
                cellStyleTemp.SetFont(font);
                MergeCells(sheet, 5, 0, 0, "测试项", cellStyleTemp);
                MergeCells(sheet, 5, 1, 1, "合格值", cellStyleTemp);
                MergeCells(sheet, 5, 2, 2, "实测值", cellStyleTemp);
                MergeCells(sheet, 5, 3, 3, "差值", cellStyleTemp);
                MergeCells(sheet, 5, 4, 4, "判断标准", cellStyleTemp);
                MergeCells(sheet, 5, 5, 5, "用时（S）", cellStyleTemp);
                MergeCells(sheet, 5, 6, 6, "结果", cellStyleTemp);

                int rowIndex = 6;
                foreach (var item in list)
                {
                    //MergeCells(sheet, rowIndex, 0, 0, item.Id.ToString());
                    MergeCells(sheet, rowIndex, 0, 0, item.ChildrenItem);
                    MergeCells(sheet, rowIndex, 1, 1, string.IsNullOrEmpty(item.QualifiedValue) ? "-" : item.QualifiedValue.ToString());
                    MergeCells(sheet, rowIndex, 2, 2, string.IsNullOrEmpty(item.MeasureVal) ? "-" : item.MeasureVal.ToString());
                    MergeCells(sheet, rowIndex, 3, 3, string.IsNullOrEmpty(item.DifferenceVal) ? "-" : item.DifferenceVal.ToString());
                    MergeCells(sheet, rowIndex, 4, 4, string.IsNullOrEmpty(item.MeasurementStandard) ? "-" : item.MeasurementStandard.ToString());
                    if (item.EndTime == null || item.StartTime == null)
                    {
                        MergeCells(sheet, rowIndex, 5, 5, "-");
                    }
                    else
                    {
                        MergeCells(sheet, rowIndex, 5, 5, (item.EndTime - item.StartTime).Value.TotalSeconds.TruncateDouble(3).ToString());
                    }
                    if (item.Result == null)
                    {
                        MergeCells(sheet, rowIndex, 6, 6, "-");
                    }
                    else
                    {
                        MergeCells(sheet, rowIndex, 6, 6, item.Result.BoolToStr2());
                    }

                    rowIndex++;

                }

                using (FileStream stream = File.OpenWrite(filePath))
                {
                    workbook.Write(stream);
                    stream.Close();
                    LogHelper.ShowMsg("数据保存成功");
                }
                workbook.Close();
            }
        }
        /// <summary>
        /// 合并单元格并设置内容
        /// </summary>
        /// <param name="sheet">工作表对象</param>
        /// <param name="rowIndex">行索引</param>
        /// <param name="startCol">起始列索引</param>
        /// <param name="endCol">结束列索引</param>
        /// <param name="content">单元格内容</param>
        static void MergeCells(ISheet sheet, int rowIndex, int startCol, int endCol, string content, ICellStyle _cellStyle = null, short height = 1)
        {
            // 创建行
            IRow row = sheet.GetRow(rowIndex) ?? sheet.CreateRow(rowIndex);
            row.Height = (short)(height * 1.17 * 256);
            // 创建单元格并设置内容
            ICell cell = row.CreateCell(startCol);
            cell.SetCellValue(content);

            // 设置单元格样式（居中）
            ICellStyle cellStyle = sheet.Workbook.CreateCellStyle();
            cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center;
            cellStyle.VerticalAlignment = NPOI.SS.UserModel.VerticalAlignment.Center;
            if (_cellStyle != null)
            {
                cell.CellStyle = _cellStyle;
            }
            else
            {
                cell.CellStyle = cellStyle;
            }


            if (startCol < endCol)
                // 合并单元格
                sheet.AddMergedRegion(new CellRangeAddress(rowIndex, rowIndex, startCol, endCol));
        }
    }
}
