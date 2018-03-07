using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            ExcelPackage package = null;
            try
            {
                var localFilePath = "E:\\text.xlsx";
                package = new ExcelPackage();
                var sheetName = "Sheet1";
                var sheet = package.Workbook.Worksheets.Add(sheetName);

                //sheet.Cells.Style.ShrinkToFit = true;
                //sheet.Row(1).CustomHeight = true;
                //sheet.Column(1).Width = 100;
                //sheet.Row(1).Height = 100;
                sheet.Cells[1, 1].Style.WrapText = true;
                sheet.SetValue(1, 1, "我站在北京天安门广场观看升国旗");
                sheet.Cells[1, 1, 2, 1].Merge = true;
                sheet.SetValue(1, 2, "五星红旗随风飘扬");
                sheet.Cells[1, 2, 1, 4].Merge = true;
                sheet.SetValue(1, 5, DateTime.Now);
                sheet.SetValue(2, 2, "2,2");
                sheet.SetValue(2, 3, "2,3");
                sheet.SetValue(2, 4, "2,3");
                sheet.SetValue(2, 5, "2,5");
                sheet.Row(3).Height = 60;
                sheet.Column(6).Width = 60;
                sheet.SetValue(3, 6, string.Empty);

                FileInfo f = new FileInfo(localFilePath);
                package.SaveAs(f);
                System.Diagnostics.Process.Start(localFilePath);
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
        }
    }
}
