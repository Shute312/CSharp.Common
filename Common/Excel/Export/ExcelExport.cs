using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics.Contracts;
using OfficeOpenXml;
using System.IO;
using System.Diagnostics;

namespace Common.Excel.Export
{
    public class ExcelExport
    {
        /// <summary>
        /// 读取Excel模板，然后将table数据黏贴到指定区域
        /// </summary>
        /// <param name="templateAbsPath">模板路径</param>
        /// <param name="tables"></param>
        /// <param name="tablesAndCellPositions">Item1传入table；Item2传入字符串，如：C1，表示位于第一行第三列的单元格；Item3传入Sheet的索引</param>
        /// <returns>网络相对路径（~/App_Data/..）</returns>
        public static bool Export(string templateAbsPath, string outputAbsPath,params Tuple<DataTable, string, int>[] tablesAndCellPositions)
        {
            if (!File.Exists(templateAbsPath))
            {
                throw new Exception("未发现Excel模板文件");
            }
            ExcelPackage package = null;
            bool isOk = false;
            try
            {
                package = new ExcelPackage();
                using (Stream stream = File.Open(templateAbsPath, FileMode.Open))
                {
                    package.Load(stream);
                }
                foreach (Tuple<DataTable, string, int> tuple in tablesAndCellPositions)
                {
                    var table = tuple.Item1;
                    var sheetNumber = tuple.Item3;
                    var position = Models.Cell.CellAddressToPosition(tuple.Item2);
                    var sheet = package.Workbook.Worksheets[sheetNumber + 1];
                    sheet.Cells[position.Y + 1, position.X + 1].LoadFromDataTable(table, false);
                }
                FileInfo fileInfo = new FileInfo(outputAbsPath);
                package.SaveAs(fileInfo);
                isOk = true;
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
            return isOk;
        }
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

        public bool Export(IEnumerable<Models.Cell> cells, ExcelWorksheet sheet,Models.ExcelStyle excelStyle = null)
        {
            Contract.Assert(cells != null && cells.Count() > 0);
            var layout = new VirtualLayout();
            layout.AddRange(cells);
            Stopwatch stopwatch = new Stopwatch();
            if (excelStyle != null) layout.ExcelStyle = excelStyle;
            stopwatch.Start();
            var size = layout.Calculate();
            stopwatch.Stop();
            var time = stopwatch.Elapsed;
            layout.Clear();
            ExportHelper exportHelper = new ExportHelper();
            return exportHelper.Export(cells, sheet,layout.ExcelStyle);
        }

        public bool Export(IEnumerable<Models.Cell> cells, string localFilePath)
        {
            ExcelPackage package = null;
            try
            {
                package = new ExcelPackage();
                var sheetName = "Sheet1";
                var sheet = package.Workbook.Worksheets.Add(sheetName);
                if (Export(cells, sheet))
                {
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
    }
}