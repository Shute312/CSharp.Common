using Common.Excel.Export;
using Common.Excel.Export.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
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
            Image image = new Bitmap(1000,5000);
            Graphics graphics = Graphics.FromImage(image);
            var text = "我站在北京天安门广场观看升国旗,五星红旗随风飘扬";
            var fontSize = new ExcelStyle().FontSize;
            var fontFamily = new ExcelStyle().FontFamily;
            var sz = graphics.MeasureString(text, new Font(fontFamily, fontSize), 1000);
            graphics.DrawString(text, new Font(fontFamily, fontSize) ,new SolidBrush(Color.Black),0,0);
            image.Save("D:\\DrawString.bmp");

            ExcelExport export = new ExcelExport();
            List<Cell> list = new List<Cell>();
            list.Add(new Cell("我站在北京天安门广场观看升国旗") { RowIndex = 0, ColIndex = 0, Rowspan = 1 ,TextAlign = Consts.TextAlign.MiddleLeft});
            list.Add(new Cell("我站在北京天安门广场观看升国旗,五星红旗随风飘扬") { RowIndex = 0, ColIndex = 1, Colspan = 2 });
            list.Add(new Cell(DateTime.Now) { RowIndex = 0, ColIndex = 4 });
            list.Add(new Cell("2,2") { RowIndex = 1, ColIndex = 1 });
            list.Add(new Cell("2,3") { RowIndex = 1, ColIndex = 2 });
            list.Add(new Cell("2,4") { RowIndex = 1, ColIndex = 3 });
            list.Add(new Cell("2,5") { RowIndex = 1, ColIndex = 4 });
            list.Add(new Cell(string.Empty) { RowIndex = 2, ColIndex = 5, MinWidth = Cell.GetPixelWidth(60), MinHeight = Cell.GetPixelHeight(60) });//测试空内容撑开
            list.Add(new Cell("测试宽度是否会自动撑开") { RowIndex = 2, ColIndex = 6, MinWidth = Cell.GetPixelWidth(15) });
            list.Add(new Cell("测试高度是否会自动撑开") { RowIndex = 3, ColIndex = 7, MaxWidth = 60 });

            ExcelPackage package = null;
            try
            {
                var localFilePath = "D:\\Text.xlsx";
                package = new ExcelPackage();
                var sheetName = "Sheet1";
                var sheet = package.Workbook.Worksheets.Add(sheetName);
                export.Export(list, sheet);
                FileInfo f = new FileInfo(localFilePath);
                package.SaveAs(f);
                System.Diagnostics.Process.Start(localFilePath);
            }
            //catch (Exception ex)
            //{
            //}
            finally
            {
                if (package != null)
                {
                    package.Dispose();
                }
            }
        }

        

        static void TestEpPlus()
        {
            ExcelPackage package = null;
            try
            {
                var localFilePath = "D:\\TestEpPlus.xlsx";
                package = new ExcelPackage();
                var sheetName = "Sheet1";
                var sheet = package.Workbook.Worksheets.Add(sheetName);

                //sheet.Cells.Style.ShrinkToFit = true;//实际效果是：单元格Size不变，但里边的内容按比例缩放了
                //sheet.Row(1).CustomHeight = true;//(默认false)单手动设置过行高时，就自动变为true
                //sheet.Column(1).Width = 100;
                //sheet.Row(1).Height = 100;
                sheet.Cells[1, 1].Style.WrapText = true;
                sheet.SetValue(1, 1, "我站在北京天安门广场观看升国旗");
                sheet.Cells[1, 1, 2, 1].Merge = true;
                sheet.SetValue(1, 2, "我站在北京天安门广场观看升国旗,五星红旗随风飘扬");
                sheet.Cells[1, 2, 1, 4].Merge = true;
                sheet.SetValue(1, 5, DateTime.Now);
                sheet.SetValue(2, 2, "2,2");
                sheet.SetValue(2, 3, "2,3");
                sheet.SetValue(2, 4, "2,3");
                sheet.SetValue(2, 5, "2,5");
                sheet.Row(3).Height = 60;
                sheet.Column(6).Width = 60;
                sheet.SetValue(3, 6, string.Empty);
                sheet.SetValue(3, 7, "测试宽度是否会自动撑开");
                sheet.Column(8).Width = 15;

                sheet.SetValue(4, 8, "测试高度是否会自动撑开");//设置列宽，配置可换行后，可以撑开高度
                sheet.Cells[4, 8].Style.WrapText = true;

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
