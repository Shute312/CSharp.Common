using Common.Excel.Export.Models;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Common.Excel.Export
{
    public class Demo
    {
        public static void TestFormat()
        {
            string localFilePath = "D:\\Format.xlsx";

            ExcelPackage package = null;
            package = new ExcelPackage();
            package.Load(new FileStream(localFilePath, FileMode.Open));
            var sheet = package.Workbook.Worksheets[1];
            var rowCount = sheet.Dimension.Rows;
            var colCount = sheet.Dimension.Columns;
            for (var c = 0; c < colCount; c++)
            {
                var cell = sheet.Cells[2, c + 1];
                var format = cell.Style.Numberformat;
                var id = format.NumFmtID;
            }
        }
        public static void TestAutoLayout()
        {

            string localFilePath = "D:\\进仓记录.xlsx";
            ExcelExport export = new ExcelExport();
            ExcelStyle excelStyle = new ExcelStyle();//通常情况下，不需要修改EscelStyle
            //excelStyle.MaxColWidth = 90;
            excelStyle.MinColWidth = 80;
            excelStyle.BorderColor = Color.Black;
            excelStyle.BorderStyle = Consts.BorderStyle.Thin;

            List<Cell> list = new List<Cell>();
            list.Add(new Cell("验货室RFID进仓管控表格", "A1", "o1") { FontSize = 24, TextAlign = Consts.TextAlign.BottomCenter });
            list.Add(new Cell("进仓记录", "A2", "O2") { MinHeight = 40 });
            list.Add(new Cell("鞋型名称", 0, 2));
            list.Add(new Cell("制令号", 1, 2));
            list.Add(new Cell("ART", 2, 2));
            list.Add(new Cell("季节", 3, 2));
            list.Add(new Cell("用途", 4, 2));
            list.Add(new Cell("码数", 5, 2));
            list.Add(new Cell("样品单总数量（只）", 6, 2));
            list.Add(new Cell("鞋型负责人", 7, 2));
            list.Add(new Cell("进仓日期", 8, 2));
            list.Add(new Cell("进仓数量（只）", 9, 2));
            list.Add(new Cell("进仓时间", 10, 2));
            list.Add(new Cell("加工交付人签收条码", 11, 2));
            list.Add(new Cell("加工交付时间", 12, 2));
            list.Add(new Cell("验货室人接收人条码", 13, 2));
            list.Add(new Cell("验货室人接收时间", 14, 2));

            ExcelPackage package = null;
            try
            {
                package = new ExcelPackage();
                var sheetName = "Sheet1";
                var sheet = package.Workbook.Worksheets.Add(sheetName);
                export.Export(list, sheet, excelStyle);//不需要修改整体样式时，不需要传递excelStyle
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

        public static void TestUserSet()
        {
            string localFilePath = "D:\\工艺工段.xlsx";
            ExcelExport export = new ExcelExport();
            ExcelStyle excelStyle = new ExcelStyle();//通常情况下，不需要修改EscelStyle
            excelStyle.MaxColWidth = 100;
            excelStyle.MinColWidth = 100;
            excelStyle.BorderColor = Color.Black;
            excelStyle.BorderStyle = Consts.BorderStyle.Thin;

            List<Cell> list = new List<Cell>();
            list.Add(new Cell("工艺工段", "A1", "N1") { MaxHeight = 18, MinHeight = 18 });
            list.Add(new Cell("计划开始生产时间", 0, 1) { TextAlign = Consts.TextAlign.BottomCenter });
            list.Add(new Cell("工艺厂商", 1, 1) { BackgroundColor = Color.Yellow });
            list.Add(new Cell("工艺部件", 2, 1) { BackgroundColor = Color.Yellow });
            list.Add(new Cell("工艺发出时间", 3, 1));
            list.Add(new Cell("工艺发出数量", 4, 1));
            list.Add(new Cell("工艺完成时间", 5, 1));
            list.Add(new Cell("数量", 6, 1));
            list.Add(new Cell("欠数", 7, 1));
            list.Add(new Cell("工艺剪生产周期时间", 8, 1));
            list.Add(new Cell("异常停顿开始时间", 9, 1));
            list.Add(new Cell("异常原因", 10, 1));
            list.Add(new Cell("异常恢复生产时间", 11, 1));
            list.Add(new Cell("欠数原因", 12, 1));
            list.Add(new Cell("计划生产完成时间", 13, 1));

            ExcelPackage package = null;
            try
            {
                package = new ExcelPackage();
                var sheetName = "Sheet1";
                var sheet = package.Workbook.Worksheets.Add(sheetName);
                export.Export(list, sheet, excelStyle);//不需要修改整体样式时，不需要传递excelStyle
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
        public static void TestLayout()
        {
            Image image = new Bitmap(1000, 5000);
            Graphics graphics = Graphics.FromImage(image);
            var text = "我站在北京天安门广场观看升国旗,五星红旗随风飘扬";
            var fontSize = new ExcelStyle().FontSize;
            var fontFamily = new ExcelStyle().FontFamily;
            var sz = graphics.MeasureString(text, new Font(fontFamily, fontSize), 1000);
            graphics.DrawString(text, new Font(fontFamily, fontSize), new SolidBrush(Color.Black), 0, 0);
            image.Save("D:\\DrawString.bmp");

            ExcelExport export = new ExcelExport();
            List<Cell> list = new List<Cell>();
            list.Add(new Cell("我站在北京天安门广场观看升国旗") { RowIndex = 0, ColIndex = 0, Rowspan = 1, TextAlign = Consts.TextAlign.MiddleLeft });
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

        public static void TestEpPlus()
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
                sheet.SetValue(1, 2, "我站在北京天安门广场观看升国旗，五星红旗随风飘扬");
                sheet.Cells[1, 2].AutoFitColumns();
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

                sheet.SetValue(4, 8, "测试带有换行符的高度\r\n是否会\r\n自动撑开");//设置列宽，配置可换行后，可以撑开高度
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
