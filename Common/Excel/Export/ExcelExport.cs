using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Diagnostics.Contracts;
using OfficeOpenXml;
using System.IO;

namespace Common.Excel.Export
{
    public class ExcelExport
    {
        public bool Export(DataTable table, string file)
        {
            throw new NotImplementedException();
        }

        public bool Export(IEnumerable<Models.Cell> cells, ExcelWorksheet sheet)
        {
            Contract.Assert(cells != null && cells.Count() > 0);
            var layout = new VirtualLayout();
            layout.AddRange(cells);
            var size = layout.Calculate();
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