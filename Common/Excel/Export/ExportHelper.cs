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

            //sheet.Cells.Style.ShrinkToFit = true;//单元格自动适应大小（实际效果为：填充根据单元格的Size，自动缩放到合适大小）
            //sheet.Cells.Style.WrapText = true; //单元格文字自动换行
            //sheet.DefaultColWidth = 10; //默认列宽
            //sheet.DefaultRowHeight = 30; //默认行高
            sheet.DefaultColWidth = excelStyle.MinColWidth;
            sheet.DefaultRowHeight = excelStyle.MinRowHeight;

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
                sheet.Row(rowNo).Height = GetRowDistance(cell.PixelHeight);//设置行高
                sheet.Column(colNo).Width = GetColDistance(cell.PixelWidth);//设置列宽
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
        /// <summary>
        /// Excel中行高所使用单位为磅(72磅＝1英寸，1英寸 = 2.54厘米)
        /// </summary>
        /// <returns></returns>
        public float GetRowDistance(float pixel, float dpi = 96)
        {
            return Unit.Pixel2Pound(pixel, dpi);
        }
        /// <summary>
        /// 列宽使用单位为1/12英寸(72磅＝1英寸，1英寸 = 2.54厘米)
        /// </summary>
        /// <returns></returns>
        public float GetColDistance(float pixel, float dpi = 96)
        {
            return Unit.Pixel2Pound(pixel, dpi) * 5.25f;
        }
    }
}
