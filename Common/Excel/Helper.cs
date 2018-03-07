using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using OfficeOpenXml;

namespace Common.Excel
{
    public class Helper
    {
        public static bool Write(string localFilePath, params DataTable[] tables)
        {
            ExcelPackage package = null;
            try
            {
                package = new ExcelPackage();
                for (var i = 0; i < tables.Length; i++)
                {
                    var table = tables[i];
                    var sheetName = string.IsNullOrEmpty(table.TableName) ? "Sheet" + (i + 1).ToString() : table.TableName;
                    ExcelWorksheet sheet = package.Workbook.Worksheets.Add(sheetName);
                    sheet.Cells["A1"].LoadFromDataTable(table, true);
                }
                FileInfo f = new FileInfo(localFilePath);
                package.SaveAs(f);
                return true;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (package != null)
                {
                    package.Dispose();
                }
            }
            return false;
        }

        public static List<DataTable> Load(string localFilePath, params string[] sheetNames)
        {
            const string GENERAL_STYLE = "General";
            ExcelPackage package = null;
            List<DataTable> tableList = null;

            //增加几种常见中文日期格式化
            Dictionary<int, string> dateFormatDic = new Dictionary<int, string>();
            dateFormatDic.Add(30, "MM/dd/yyyy");
            dateFormatDic.Add(31, "yyyy年MM月dd日");
            dateFormatDic.Add(57, "yyyy年MM月");
            //dateFormatDic.Add(58, "MM月dd日");
            dateFormatDic.Add(58, "yyyy-MM-dd");//58对应的MM月dd日会导致年丢失，根据实际需要改为yyyy-MM-dd
            try
            {
                package = new ExcelPackage();
                using (Stream stream = File.Open(localFilePath, FileMode.Open))
                {
                    package.Load(stream);
                    tableList = new List<DataTable>(package.Workbook.Worksheets.Count);
                    for (var m = 0; m < package.Workbook.Worksheets.Count; m++)
                    {
                        var worksheet = package.Workbook.Worksheets[m + 1];//第一页是1，而不是0
                        string sheetName = worksheet.Name;
                        if (string.IsNullOrEmpty(sheetName) || (sheetName.Contains("$") && !sheetName.Replace("'", "").EndsWith("$")))
                        {
                            continue;
                        }
                        //只读取名字符合的Sheet
                        sheetName = sheetName.Replace("$", "").Trim();
                        if (sheetNames != null && sheetNames.Length > 0)
                        {
                            bool fix = false;
                            foreach (var tn in sheetNames)
                            {
                                if (tn.Trim() == sheetName)
                                {
                                    fix = true;
                                    break;
                                }
                            }
                            if (!fix) continue;
                        }
                        if (worksheet.Dimension == null) continue;

                        DataTable table = null;
                        table = new DataTable(sheetName);

                        //获取worksheet的行数
                        int rows = worksheet.Dimension.End.Row;
                        //获取worksheet的列数
                        int cols = worksheet.Dimension.End.Column;
                        bool[] validCols = new bool[cols];//有效行
                        int validColCount = 0;//有效列数

                        // 恢复表头
                        for (int c = 0; c < cols; c++)
                        {
                            var cellObj = worksheet.Cells[1, c + 1].Value;//从1开始，而不是0
                            var cellStr = cellObj != null ? cellObj.ToString().Trim() : null;
                            if (!string.IsNullOrEmpty(cellStr))
                            {
                                validCols[c] = true;
                                validColCount++;
                                table.Columns.Add(cellStr);
                            }
                        }
                        if (validColCount == 0) continue;
                        for (int r = 1; r < rows; r++)
                        {
                            var tableRow = table.NewRow();
                            int emptyCount = 0;//用于移除空行
                            for (int c = 0; c < cols; c++)
                            {
                                if (validCols[c])
                                {
                                    var cell = worksheet.Cells[r + 1, c + 1];
                                    var cellObj = cell.Value;
                                    string cellStr = null;
                                    bool hasFormat = false;
                                    var numFormat = cell.Style.Numberformat;
                                    //自定义格式化机制
                                    if (GENERAL_STYLE.Equals(numFormat.Format, StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        if (numFormat.NumFmtID > 0)
                                        {
                                            if (cellObj != null && (cellObj is double || cellObj is long || cellObj is float))
                                            {
                                                try
                                                {
                                                    string format = null;
                                                    if (dateFormatDic.TryGetValue(numFormat.NumFmtID, out format))
                                                    {
                                                        cellStr = cell.GetValue<DateTime>().ToString(format);
                                                        hasFormat = true;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {

                                                }
                                            }
                                        }
                                    }
                                    if (!hasFormat)
                                    {
                                        //已有对应格式化机制的
                                        if (!string.IsNullOrEmpty(numFormat.Format) && !GENERAL_STYLE.Equals(numFormat.Format, StringComparison.CurrentCultureIgnoreCase))
                                        {
                                            hasFormat = true;
                                            cellStr = cell.Text;
                                        }
                                        else
                                        {
                                            cellStr = cellObj != null ? cellObj.ToString().Trim() : null;
                                        }
                                    }
                                    //DateTime.FromOADate(Convert.ToInt32(worksheet.Cells[2, 24].Value.ToString())).ToString("d");
                                    if (string.IsNullOrEmpty(cellStr)) emptyCount++;
                                    tableRow[c] = cellStr;
                                }
                            }
                            //全为空时，是空行，忽略掉
                            if (emptyCount < validColCount)
                            {
                                table.Rows.Add(tableRow);
                            }
                        }
                        tableList.Add(table);
                    }
                }
            }
            catch (IOException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (package != null)
                {
                    package.Dispose();
                }
            }
            return tableList;
        }
    }
}
