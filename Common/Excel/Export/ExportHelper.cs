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
            sheet.Cells.Style.Font.Bold = excelStyle.IsBold;
            sheet.Cells.Style.Font.Color.SetColor(excelStyle.FontColor);
            sheet.Cells.Style.Font.Italic = excelStyle.IsItalic;
            sheet.Cells.Style.Font.Size = excelStyle.FontSize;
            sheet.Cells.Style.Font.Name = excelStyle.FontFamily.Name;
            foreach (var cell in cells)
            {
                var rowNo = cell.RowIndex + 1;
                var colNo = cell.ColIndex + 1;
                var excelRange = GetRange(cell, sheet);
                if (cell.Colspan > 0 || cell.Rowspan > 0) excelRange.Merge = true;//合并单元格
                //文本对齐方式
                excelRange.Style.VerticalAlignment = GetVerAlign(cell, excelStyle);
                excelRange.Style.HorizontalAlignment = GetHorAlign(cell, excelStyle);
                sheet.SetValue(rowNo, colNo, cell.Value);
                sheet.Row(rowNo).CustomHeight = true;//手动调节行高
                excelRange.Style.WrapText = true;//自动换行
                if(cell.IsItalic!=null) excelRange.Style.Font.Italic = cell.IsItalic.Value;
                if(cell.IsBold!=null) excelRange.Style.Font.Bold = cell.IsBold.Value;
                if (cell.FontColor != null) excelRange.Style.Font.Color.SetColor(cell.FontColor.Value);
                if (cell.FontSize != null) excelRange.Style.Font.Size = cell.FontSize.Value;
                if (cell.FontFamily != null) excelRange.Style.Font.Name = cell.FontFamily.Name;
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

        private ExcelHorizontalAlignment GetHorAlign(Models.Cell cell,Models.ExcelStyle excelStyle)
        {
            var align = cell.TextAlign != null ? cell.TextAlign : excelStyle.TextAlign;
            var value = ((int)align) %3;
            if (value == 0) return ExcelHorizontalAlignment.Left;
            else if (value == 1) return ExcelHorizontalAlignment.Center;
            else return ExcelHorizontalAlignment.Right;
        }
        private ExcelVerticalAlignment GetVerAlign(Models.Cell cell, Models.ExcelStyle excelStyle)
        {
            var align = cell.TextAlign != null ? cell.TextAlign : excelStyle.TextAlign;
            var value = ((int)align) / 3;
            if (value == 0) return ExcelVerticalAlignment.Top;
            else if (value == 1) return ExcelVerticalAlignment.Center;
            else return ExcelVerticalAlignment.Bottom;
        }
    }
}
