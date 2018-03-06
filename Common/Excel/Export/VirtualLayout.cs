using Common.Excel.Export.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Common.Excel.Export
{
    public class VirtualLayout
    {
        List<Cell> cellList;
        public VirtualLayout()
        {
            cellList = new List<Cell>();
            ExcelStyle = new ExcelStyle();
        }

        public ExcelStyle ExcelStyle { get; set; }
        public void Add(Cell cell)
        {
            Contract.Assert(cell!=null);
            cellList.Add(cell);
        }

        /// <summary>
        /// 方法不支持公式、表达式单元格的自动计算
        /// </summary>
        /// <returns></returns>
        public List<RenderCell> Calculate()
        {
            Contract.Requires(cellList.Count > 0);
            AssetExcelStyle(ExcelStyle);
            int rowCount = 0, colCount = 0;
            #region 步骤一、计算总共占据多少行、列
            foreach (var cell in cellList)
            {
                Contract.Assert(cell.RowIndex > -1);
                Contract.Assert(cell.ColIndex > -1);
                Contract.Assert(cell.Rowspan > -1);
                Contract.Assert(cell.Colspan > -1);
                var cols = cell.ColIndex + cell.Colspan;
                var rows = cell.RowIndex + cell.Rowspan;
                if (rowCount < rows) rowCount = rows;
                if (colCount < cols) colCount = cols;
            }
            //检测数据冲突,单元格覆盖；同一列要求的宽度不同；同一行要求的高度不同
            bool[,] exists = new bool[colCount, rowCount];
            foreach (var cell in cellList)
            {
                var minColIndex = cell.ColIndex;
                var maxColIndex = cell.ColIndex + cell.Colspan;
                var minRowIndex = cell.RowIndex;
                var maxRowIndex = cell.RowIndex + cell.Colspan;
                for (int x = minColIndex; x < maxColIndex; x++)
                {
                    for (int y = minRowIndex; y < maxRowIndex; y++)
                    {
                        if (exists[x, y])
                            throw new IndexOutOfRangeException("单元格重合");
                        else
                            exists[x, y] = true;
                    }
                }
            }
            #endregion

            #region 步骤二、
            //1、定义平面结构
            Cell[,] cellPlanar = new Cell[colCount, rowCount];
            foreach (var cell in cellList)
            {
                //todo 设置初始化值
                var minColIndex = cell.ColIndex;
                var maxColIndex = cell.ColIndex + cell.Colspan;
                var minRowIndex = cell.RowIndex;
                var maxRowIndex = cell.RowIndex + cell.Colspan;
                for (int x = minColIndex; x < maxColIndex; x++)
                {
                    for (int y = minRowIndex; y < maxRowIndex; y++)
                    {
                        //遇到跨行列的单元格时，用同一个单元格对象覆盖多个位置
                        cellPlanar[x, y] = cell;
                    }
                }
            }
            //2、度量所需要的各行高、各列宽
            //需要的列宽高
            int[] requireWidths = new int[colCount];
            int[] requireHeights = new int[rowCount];
            //可用的列宽高
            int[] availbleWidths = new int[colCount];
            int[] availbleHeights = new int[rowCount];
            //最终使用的列宽高
            int[] widths =new int[colCount];
            int[] heights = new int[rowCount];
            Size[,] silePlanar = new Size[colCount,rowCount];
            ;
            Graphics graphics = Graphics.FromImage(new Bitmap(
                Math.Max(cellList.Max(p => p.MaxWidth), ExcelStyle.MaxWidth),
                Math.Max(cellList.Max(p => p.MaxHeight), ExcelStyle.MaxHeight)));
            for (int y = 0; y < rowCount; y++)
            {
                for (int x = 0; x < colCount; x++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null)
                    {

                    }
                    else if (IsFormula(cell))
                    {
                        //表达式、公式单元格
                    }
                    else if (cell.Colspan > 0 || cell.Rowspan > 0)
                    {
                        //跨行、列的单元格的延伸格子
                        if (x != cell.ColIndex || y != cell.RowIndex)
                        {

                        }
                        else
                        {
                        }
                    }
                    else {
                        //普通单元格
                    }
                }
            }

            //对跨行列的首个、非首个单元格做特殊处理
            //对空的单元格做特殊处理
            //对试图类型的单元格做特殊处理
            //对表达式单元格做特殊处理

            Size size = new Size(widths.Sum(),heights.Sum());
            //3、分配空间
            #endregion
            //RenderCell[,]

            //1、统计出总行列;
            //2、每个行最大高度、每个行最大宽度统计时，先统计不跨行列的，再统计跨行列的;对于空行，自动生成多余的空格(如果连续过多的空行>10、空列>100，应该抛异常)
            //3、计算不换行时，各个文本占据的宽高；如果宽高不足以放下，应当如何处理
            //4、表达式的Size怎么计算(暂定为根据其他行列或者默认配置宽高)
            //5、图片、图表等不应该参与计算(只做冲突检测)

            //todo 输出时，应该返回整体的宽、高，用于追加图表、图片时的定位
            return null;
        }

        public void Clear()
        {
            cellList.Clear();
        }

        private Size CalcSize(Cell cell, Graphics graphics)
        {
            if (cell == null) return new Size(int.MinValue, int.MinValue);//空白，用MinValue
            if (IsFormula(cell)) return new Size(int.MaxValue, int.MaxValue);
            var text = cell.Display;
            if (string.IsNullOrEmpty(text)) return new Size(int.MinValue, int.MinValue);//空白，用MinValue
            var fontFamily = cell.FontFamily == null ? ExcelStyle.FontFamily : cell.FontFamily;
            var fontSize = cell.FontSize <= 0 ? ExcelStyle.FontSize : cell.FontSize;
            var maxWidth = cell.MaxWidth <= 0 ? ExcelStyle.MaxWidth : cell.MaxWidth;
            var size = graphics.MeasureString(text, new Font(fontFamily, fontSize), maxWidth);
            return new Size((int)size.Width, (int)size.Height);
        }
        //是否为表达式、公式单元格
        private bool IsFormula(Cell cell) {
            return !string.IsNullOrEmpty(cell.Formula);
        }

        private void AssetExcelStyle(ExcelStyle style)
        {
            Contract.Assert(style!=null);
            Contract.Assert(style.FontFamily!=null);
            Contract.Assert(style.MinHeight >= 0);
            Contract.Assert(style.MinHeight <=style.MaxHeight);
            Contract.Assert(style.MinWidth >= 0);
            Contract.Assert(style.MinWidth <= style.MaxWidth);
            //todo 晚上其他属性的确认
        }
    }
}