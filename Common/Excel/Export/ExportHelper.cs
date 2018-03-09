using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Excel.Export
{
    public class ExportHelper
    {
        public bool Export(IEnumerable<Models.Cell> cells, ExcelWorksheet sheet, Models.ExcelStyle excelStyle = null)
        {
            if (excelStyle == null) excelStyle = new Models.ExcelStyle();
            sheet.DefaultColWidth = Models.Cell.GetExcelWidth(excelStyle.MinColWidth);
            sheet.DefaultRowHeight = Models.Cell.GetExcelHeight(excelStyle.MinRowHeight);
            sheet.Cells.Style.Font.Bold = excelStyle.Bold;
            sheet.Cells.Style.Font.Color.SetColor(excelStyle.FontColor);
            sheet.Cells.Style.Font.Italic = excelStyle.Italic;
            sheet.Cells.Style.Font.Size = excelStyle.FontSize;
            sheet.Cells.Style.Font.Name = excelStyle.FontFamily.Name;
            sheet.Cells.Style.VerticalAlignment = GetVerAlign(excelStyle.TextAlign);
            sheet.Cells.Style.HorizontalAlignment = GetHorAlign(excelStyle.TextAlign);
            //背景色一次性设置会有点问题
            //sheet.Cells.Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
            //sheet.Cells.Style.Fill.BackgroundColor.SetColor(excelStyle.BackgroundColor);
            //不能一次性设置
            //if (excelStyle.BorderStyle != Consts.BorderStyle.None && excelStyle.BorderColor != null)
            //{
            //    sheet.Cells.Style.Border.BorderAround((ExcelBorderStyle)((int)excelStyle.BorderStyle), excelStyle.BorderColor.Value);
            //}
            foreach (var cell in cells)
            {
                var rowNo = cell.RowIndex + 1;
                var colNo = cell.ColIndex + 1;
                var excelRange = GetRange(cell, sheet);
                if (cell.Colspan > 0 || cell.Rowspan > 0) excelRange.Merge = true;//合并单元格
                //文本对齐方式
                if (cell.TextAlign != null)
                {
                    excelRange.Style.VerticalAlignment = GetVerAlign(cell.TextAlign.Value);
                    excelRange.Style.HorizontalAlignment = GetHorAlign(cell.TextAlign.Value);
                }
                sheet.SetValue(rowNo, colNo, cell.Value);
                sheet.Row(rowNo).CustomHeight = true;//手动调节行高
                excelRange.Style.WrapText = true;//自动换行
                //是否泄题
                if(cell.Italic!=null) excelRange.Style.Font.Italic = cell.Italic.Value;
                //是否粗体
                if(cell.Bold!=null) excelRange.Style.Font.Bold = cell.Bold.Value;
                //文本颜色
                if (cell.FontColor != null) excelRange.Style.Font.Color.SetColor(cell.FontColor.Value);
                //字号
                if (cell.FontSize != null) excelRange.Style.Font.Size = cell.FontSize.Value;
                //字体
                if (cell.FontFamily != null) excelRange.Style.Font.Name = cell.FontFamily.Name;
                //数值显示格式
                if (!string.IsNullOrEmpty(cell.Format)) excelRange.Style.Numberformat.Format = cell.Format;
                //公式
                if (!string.IsNullOrEmpty(cell.Formula)) excelRange.Formula = cell.Formula;
                //背景色
                if (cell.BackgroundColor != null)
                {
                    excelRange.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    excelRange.Style.Fill.BackgroundColor.SetColor(cell.BackgroundColor.Value);
                }
                else if (excelStyle.BackgroundColor != null && excelStyle.BackgroundColor.Value != System.Drawing.Color.White)
                {
                    excelRange.Style.Fill.PatternType = ExcelFillStyle.DarkGrid;
                    excelRange.Style.Fill.BackgroundColor.SetColor(excelStyle.BackgroundColor.Value);
                }
                //边框
                if (cell.BorderStyle != Consts.BorderStyle.None && cell.BorderColor != null)
                {
                    excelRange.Style.Border.BorderAround((ExcelBorderStyle)((int)cell.BorderStyle), cell.BorderColor.Value);
                }
                else if (excelStyle.BorderStyle != Consts.BorderStyle.None && excelStyle.BorderColor != null)
                {
                    excelRange.Style.Border.BorderAround((ExcelBorderStyle)((int)excelStyle.BorderStyle), excelStyle.BorderColor.Value);
                }

                sheet.Row(rowNo).Height = cell.GetExcelHeight();//设置行高
                sheet.Column(colNo).Width = cell.GetExcelWidth();//设置列宽
            }
            return false;
        }

        private ExcelRange GetRange(Models.Cell cell, ExcelWorksheet sheet) {
            var rowNo = cell.RowIndex + 1;
            var colNo = cell.ColIndex + 1;
            if (cell.Colspan > 0 || cell.Rowspan > 0)
                return sheet.Cells[rowNo, colNo, rowNo + cell.Rowspan, colNo + cell.Colspan];
            else
                return sheet.Cells[rowNo, colNo];
        }

        private ExcelHorizontalAlignment GetHorAlign(Consts.TextAlign align)
        {
            var value = ((int)align) %3;
            if (value == 0) return ExcelHorizontalAlignment.Left;
            else if (value == 1) return ExcelHorizontalAlignment.Center;
            else return ExcelHorizontalAlignment.Right;
        }
        private ExcelVerticalAlignment GetVerAlign(Consts.TextAlign align)
        {
            var value = ((int)align) / 3;
            if (value == 0) return ExcelVerticalAlignment.Top;
            else if (value == 1) return ExcelVerticalAlignment.Center;
            else return ExcelVerticalAlignment.Bottom;
        }
    }
}
