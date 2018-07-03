using Common.Excel.Export.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;

namespace Common.Excel.Export
{
    public class VirtualLayout
    {
        const int GRAPHICS_WIDTH = 1000;
        const int GRAPHICS_HEIGHT = 500;
        const int FORMULA_DEFAULT_WIDTH = -1;//表达式
        const int FORMULA_DEFAULT_HEIGHT = -1;
        const int NULL_DEFAULT_WIDTH = -2;//空白的，未指定的单元格
        const int NULL_DEFAULT_HEIGHT = -2;
        const int SLAVE_DEFAULT_WIDTH = -3;//跨行、列的单元格的延伸格
        const int SLAVE_DEFAULT_HEIGHT = -3;
        List<Cell> cellList;
        public VirtualLayout()
        {
            cellList = new List<Cell>();
            ExcelStyle = new ExcelStyle();
        }

        public ExcelStyle ExcelStyle { get; set; }
        public void Add(Cell cell)
        {
            Contract.Assert(cell != null);
            Contract.Assert(!cellList.Contains(cell));
            cellList.Add(cell);
        }

        public void AddRange(IEnumerable<Cell> cells)
        {
            Contract.Assert(cells != null && cells.Count() > 0);
            foreach (var cell in cells) Add(cell);
        }

        /// <summary>
        /// 方法不支持公式、表达式单元格的自动计算
        /// </summary>
        /// <returns></returns>
        public Size Calculate()
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
                var cols = cell.ColIndex + cell.Colspan + 1;
                var rows = cell.RowIndex + cell.Rowspan + 1;
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
                var maxRowIndex = cell.RowIndex + cell.Rowspan;
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
            Cell[,] cellPlanar = CreatePlanar<Cell>(colCount, rowCount);
            foreach (var cell in cellList)
            {
                //todo 设置初始化值
                var minColIndex = cell.ColIndex;
                var maxColIndex = cell.ColIndex + cell.Colspan;
                var minRowIndex = cell.RowIndex;
                var maxRowIndex = cell.RowIndex + cell.Rowspan;
                for (int x = minColIndex; x <= maxColIndex; x++)
                {
                    for (int y = minRowIndex; y <= maxRowIndex; y++)
                    {
                        //遇到跨行列的单元格时，用同一个单元格对象覆盖多个位置
                        cellPlanar[x, y] = cell;
                    }
                }
            }

            //可用的列宽高
            int[] availbleMaxWidths = new int[colCount];
            int[] availbleMinWidths = new int[colCount];
            int[] availbleMaxHeights = new int[rowCount];
            int[] availbleMinHeights = new int[rowCount];
            //最终使用的列宽高
            int[] widths = new int[colCount];
            int[] heights = new int[rowCount];
            Size[,] sizeTextPlanar = CreatePlanar<Size>(colCount, rowCount);//文本Size
            Graphics graphics = Graphics.FromImage(new Bitmap(GRAPHICS_WIDTH, GRAPHICS_HEIGHT));
            //graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            //graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.GammaCorrected;
            //graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            //graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            //各列可用宽度
            for (int x = 0; x < colCount; x++)
            {
                var arr = GetCol(cellPlanar, x);
                //同一列中，各个单元格分别设置MaxWidth(大于0),最小的为准
                var mw = Min(arr.Where(p => p != null && p.MaxWidth > 0).Select(p => p.MaxWidth));
                if (mw == 0) mw = ExcelStyle.MaxColWidth;
                availbleMaxWidths[x] = mw;

                //同一列中，各个单元格分别设置MinWidth(大于0),，最大的为准
                mw = Max(arr.Where(p => p != null && p.MinWidth > 0).Select(p => p.MinWidth));
                if (mw == 0) mw = ExcelStyle.MinColWidth;
                availbleMinWidths[x] = mw;
            }
            //各行可用高度
            for (int y = 0; y < rowCount; y++)
            {
                var arr = GetRow(cellPlanar, y);
                var mh = Min(arr.Where(p => p != null && p.MaxHeight > 0).Select(p => p.MaxHeight));
                if (mh == 0) mh = ExcelStyle.MaxRowHeight;
                availbleMaxHeights[y] = mh;

                //同一列中，各个单元格分别设置MinHeight(大于0),，最大的为准
                mh = Max(arr.Where(p => p != null && p.MinHeight > 0).Select(p => p.MinHeight));
                if (mh == 0) mh = ExcelStyle.MinRowHeight;
                availbleMinHeights[y] = mh;
            }
            //计算各个单元格需要的Size
            for (int x = 0; x < colCount; x++)
            {
                for (int y = 0; y < rowCount; y++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null) sizeTextPlanar[x, y] = new Size(NULL_DEFAULT_WIDTH, NULL_DEFAULT_HEIGHT);//null
                    else if (IsFormula(cell)) sizeTextPlanar[x, y] = new Size(FORMULA_DEFAULT_WIDTH, FORMULA_DEFAULT_HEIGHT);//公式，用MaxValue
                    //跨行、列的单元格的延伸格，使用0
                    else if ((cell.Rowspan > 0 || cell.Colspan > 0) && (cell.RowIndex != y || cell.ColIndex != x)) sizeTextPlanar[x, y] = new Size(SLAVE_DEFAULT_WIDTH, SLAVE_DEFAULT_HEIGHT);
                    else if (string.IsNullOrEmpty(cell.Display)) sizeTextPlanar[x, y] = new Size(0, 0);
                    else
                    {
                        var mw = availbleMaxWidths[x];
                        if (cell.Colspan > 0 && GRAPHICS_WIDTH > mw) mw = GRAPHICS_WIDTH;
                        var sz = CalcRequireSize(cellPlanar[x, y], graphics, mw);
                        sizeTextPlanar[x, y] = sz;
                    }
                }
            }

            #region 先满足列宽,再处理行高
            for (int x = 0; x < colCount; x++)
            {
                //排除跨列的单元格，计算出要多大的宽度
                var iw = 0;
                for (int y = 0; y < rowCount; y++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null) continue;
                    if (cell.Colspan > 0) continue;
                    var size = sizeTextPlanar[x, y];
                    iw = Math.Max(iw, size.Width);
                    if (availbleMinWidths[x] > 0)
                        iw = Math.Max(iw, availbleMinWidths[x]);
                    if (availbleMaxWidths[x] > 0)
                        iw = Math.Min(iw, availbleMaxWidths[x]);
                }
                widths[x] = iw;
            }
            for (int x = 0; x < colCount; x++)
            {
                //对于跨列的，将所需要的内容进行分摊（格则：尽可能保持这几个列一样的宽度，依旧不够时，由可以撑开的某个列来承担）
                var list = new List<Tuple<Cell, Size, int>>();
                for (int y = 0; y < rowCount; y++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null) continue;
                    if (cell.Colspan == 0) continue;
                    if (cell.RowIndex != y) continue;//跨列单元格的延伸单元格
                    if (cell.ColIndex != x) continue;
                    var size = sizeTextPlanar[x, y];
                    if (size.Width == 0) continue;
                    if (size.Width <= availbleMaxWidths[x]) continue;
                    list.Add(new Tuple<Cell, Size, int>(cell, size, y));
                }
                if (list.Count > 0)
                {
                    var maxWidth = list.Max(p => p.Item2.Width);
                    var tuple = list.Find(p => p.Item2.Width == maxWidth);
                    var cell = tuple.Item1;
                    var aws = availbleMaxWidths.Skip(x).Take(cell.Colspan + 1).ToArray();//单元格所处的多个列的有效宽度是多少
                    var sumWidth = aws.Sum();
                    //如果跨越的多个列的总宽度小于等于这个单元格要求的宽度
                    if (sumWidth <= maxWidth)
                    {
                        for (int c = 0; c <= cell.Colspan; c++)
                        {
                            widths[cell.ColIndex + c] = aws[c];
                        }
                    }
                    else
                    {
                        //要将多出来的这些宽度，分配到这个单元格跨越的列中去
                        float remain = maxWidth - widths.Skip(x).Take(cell.Colspan + 1).Sum();
                        while (remain > 0)
                        {
                            for (int c = 0; c <= cell.Colspan; c++)
                            {
                                var xc = x + c;
                                var w = widths[xc];
                                var aw = availbleMaxWidths[xc];
                                if (w < aw) w += (int)Math.Ceiling(remain / (cell.Colspan + 1));
                                if (w > aw) w = aw;
                                widths[xc] = w;
                            }
                            remain = maxWidth - widths.Skip(x).Take(cell.Colspan + 1).Sum();//再次计算还有多少空间未分配(widths.Skip(x).Take(cell.Colspan+1).Sum()是可变值)
                            if (remain < 0) widths[x + cell.Colspan] -= (int)remain;//Ceiling的向上取整，导致会多几个宽度
                        }
                    }
                }
            }
            #endregion

            #region 列宽计算完毕后，计算行高
            //重新获取一次跨列单元格的Size(因为列宽度可能缩小，导致文本需要换行)
            for (int x = 0; x < colCount; x++)
            {
                for (int y = 0; y < rowCount; y++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null) continue;
                    else if (IsFormula(cell)) continue;
                    //跨行、列的单元格的延伸格，使用0
                    else if (string.IsNullOrEmpty(cell.Display)) continue;
                    else
                    {
                        var size = sizeTextPlanar[x, y];
                        if (size.Height == 0) continue;
                        var mw = widths.Skip(x).Take(cell.Colspan + 1).Sum();
                        //重新获取一次尺寸
                        if (size.Width > mw)
                        {
                            //不够放时，如果文本允许换行，要重新计算Size
                            if ((cell.WhiteSpace != null ? cell.WhiteSpace.Value : ExcelStyle.WhiteSpace) == Consts.WhiteSpace.Wrap)
                            {
                                size = CalcRequireSize(cellPlanar[x, y], graphics, mw);
                                sizeTextPlanar[x, y] = size;
                            }
                        }
                    }
                }
            }

            for (int y = 0; y < rowCount; y++)
            {
                //排除跨行的单元格，计算出要多大的高度
                var ih = 0;
                for (int x = 0; x < colCount; x++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null) continue;
                    if (cell.Rowspan > 0) continue;
                    var size = sizeTextPlanar[x, y];
                    ih = Math.Max(ih, size.Height);
                    if (availbleMinHeights[y] > 0)
                        ih = Math.Max(ih, availbleMinHeights[y]);
                    if (availbleMaxHeights[y] > 0)
                        ih = Math.Min(ih, availbleMaxHeights[y]);
                }
                heights[y] = ih;
            }
            for (int y = 0; y < rowCount; y++)
            {
                //对于跨行的，将所需要的内容进行分摊（格则：尽可能保持这几个行一样的高度，依旧不够时，由可以撑开的某个列来承担）
                var list = new List<Tuple<Cell, Size, int>>();
                for (int x = 0; x < colCount; x++)
                {
                    var cell = cellPlanar[x, y];
                    if (cell == null) continue;
                    if (cell.Rowspan == 0) continue;
                    if (cell.RowIndex != y) continue;//跨行单元格的延伸单元格
                    if (cell.ColIndex != x) continue;
                    var size = sizeTextPlanar[x, y];
                    if (size.Height == 0) continue;
                    if (size.Height <= availbleMaxHeights[y]) continue;
                    list.Add(new Tuple<Cell, Size, int>(cell, size, x));
                }
                if (list.Count > 0)
                {
                    var maxHeight = list.Max(p => p.Item2.Height);//取出最大的高度
                    var tuple = list.Find(p => p.Item2.Height == maxHeight);
                    var cell = tuple.Item1;
                    var ahs = availbleMaxHeights.Skip(y).Take(cell.Rowspan + 1).ToArray();//单元格所处的多个行的有效高度是多少
                    var sumHeight = ahs.Sum();
                    //如果跨越的多个列的总宽度小于等于这个单元格要求的宽度
                    if (sumHeight <= maxHeight)
                    {
                        for (int r = 0; r <= cell.Rowspan; r++)
                        {
                            heights[cell.RowIndex + r] = ahs[r];
                        }
                    }
                    else
                    {
                        //要将多出来的这些高度，分配到这个单元格跨越的列中去
                        float remain = maxHeight - heights.Skip(y).Take(cell.Rowspan + 1).Sum();
                        while (remain > 0)
                        {
                            for (int r = 0; r <= cell.Rowspan; r++)
                            {
                                var yr = y + r;
                                var h = heights[yr];
                                var ah = availbleMaxHeights[yr];
                                if (h < ah) h += (int)Math.Ceiling(remain / (cell.Rowspan + 1));
                                if (h > ah) h = ah;
                                heights[yr] = h;
                            }
                            remain = maxHeight - heights.Skip(y).Take(cell.Rowspan + 1).Sum();//再次计算还有多少空间未分配(heights.Skip(y).Take(cell.Rowspan+1)是可变值)
                            if (remain < 0) heights[y + cell.Rowspan] -= (int)remain;//Ceiling的向上取整，导致会多几个高度
                        }
                    }
                }
            }
            #endregion

            graphics.Dispose();

            //将列宽设置为符合Width设定的值
            for (int x = 0; x < widths.Length; x++)
            {
                if (widths[x] == 0) widths[x] = availbleMinWidths[x];
            }
            //将行高设置为符合Height设定的值
            for (int y = 0; y < heights.Length; y++)
            {
                if (heights[y] == 0) heights[y] = availbleMinHeights[y];
            }

            //3、分配空间
            #endregion
            foreach (var cell in cellList)
            {
                if (cell.Colspan > 0) cell.Width = widths.Skip(cell.ColIndex).Take(cell.Colspan + 1).Sum();
                else cell.Width = widths[cell.ColIndex];

                if (cell.Rowspan > 0) cell.Height = heights.Skip(cell.RowIndex).Take(cell.Rowspan + 1).Sum();
                else cell.Height = heights[cell.RowIndex];
            }
            Size totalSize = new Size(widths.Sum(), heights.Sum());
            return totalSize;
        }

        public void Clear()
        {
            cellList.Clear();
        }
        /// <summary>
        /// 度量需要占用的尺码
        /// </summary>
        private Size CalcRequireSize(Cell cell, Graphics graphics, int maxWidth)
        {
            //todo 支持文字旋转(存在旋转时，maxWidth要做相应的变换)
            var text = cell.Display;
            if (string.IsNullOrEmpty(text)) return new Size(0, 0);//string.Empty，用0
            var fontFamily = cell.FontFamily == null ? ExcelStyle.FontFamily : cell.FontFamily;
            var fontSize = cell.FontSize == null ? ExcelStyle.FontSize : cell.FontSize.Value;
            bool bold = cell.Bold == null ? ExcelStyle.Bold : cell.Bold.Value;
            bool italic = cell.Italic == null ? ExcelStyle.Italic : cell.Italic.Value;
            var fontStyle = FontStyle.Regular;
            if (italic) fontStyle |= FontStyle.Italic;
            if (bold) fontStyle |= FontStyle.Bold;
            var font = new Font(fontFamily, fontSize, fontStyle, GraphicsUnit.Point);
            var sz = graphics.MeasureString(text, font, maxWidth);
            //graphics.PageUnit = cell.FontUnit;
            var fontHeight = font.GetHeight(graphics);
            int lineCount =(int) Math.Round(sz.Height / fontHeight);
            var horPadding = (int)(fontHeight / 10);
            int verPadding = (int)(fontHeight / 10) + 1;//加点偏距
            var size = new Size((int)sz.Width + horPadding * 2, (int)sz.Height + lineCount * verPadding);
            return size;
        }
        //是否为表达式、公式单元格
        private bool IsFormula(Cell cell)
        {
            return !string.IsNullOrEmpty(cell.Formula);
        }

        private void AssetExcelStyle(ExcelStyle style)
        {
            Contract.Assert(style != null);
            Contract.Assert(style.FontFamily != null);
            Contract.Assert(style.MinRowHeight >= 0);
            Contract.Assert(style.MaxRowHeight > 0);
            Contract.Assert(style.MinRowHeight <= style.MaxRowHeight);
            Contract.Assert(style.MinColWidth >= 0);
            Contract.Assert(style.MaxColWidth > 0);
            Contract.Assert(style.MinColWidth <= style.MaxColWidth);
            //todo 晚上其他属性的确认
        }

        private T[,] CreatePlanar<T>(int width, int height)
        {
            return new T[width, height];
        }
        private T[] GetRow<T>(T[,] planar, int rowIndex)
        {
            int width = planar.GetLength(0), height = planar.GetLength(1);
            var arr = new T[width];
            for (int x = 0; x < width; x++)
            {
                arr[x] = planar[x, rowIndex];
            }
            return arr;
        }
        private T[] GetCol<T>(T[,] planar, int colIndex)
        {
            int width = planar.GetLength(0), height = planar.GetLength(1);
            var arr = new T[height];
            for (int y = 0; y < height; y++)
            {
                arr[y] = planar[colIndex, y];
            }
            return arr;
        }
        public static TSource Min<TSource>(IEnumerable<TSource> source)
        {
            if (source == null || source.Count() == 0) return default(TSource);
            return source.Min();
        }
        public static TSource Max<TSource>(IEnumerable<TSource> source)
        {
            if (source == null || source.Count() == 0) return default(TSource);
            return source.Max();
        }
        public static int Sum(IEnumerable<int> source)
        {
            if (source == null || source.Count() == 0) return default(int);
            return source.Sum();
        }
    }
}