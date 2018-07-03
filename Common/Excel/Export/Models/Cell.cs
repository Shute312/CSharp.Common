using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Common.Excel.Export.Models
{
    public class Cell
    {
        object value;
        string format;

        public const int MAX_COL = 1000;
        public const int MAX_ROW = 10000;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">若传入null,则单元格数据格式默认为string,如有需要则强制指定Style</param>
        public Cell(object value)
        {
            Init(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rowIndex">行索引（从0开始）</param>
        /// <param name="colIndex">列索引（从0开始）</param>
        public Cell(object value, int colIndex, int rowIndex)
        {
            Init(value, colIndex, rowIndex);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="rowIndex">行索引（从0开始）</param>
        /// <param name="colIndex">列索引（从0开始）</param>
        public Cell(object value, int colIndex, int rowIndex, int colspan, int rowspan)
            : this(value, colIndex, rowIndex)
        {
            Init(value, colIndex, rowIndex, colspan,rowspan);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="xNo">传入Excel列号，从A开始</param>
        /// <param name="yNo">传入Excel行号，从1开始</param>
        public Cell(object value, string xNo, int yNo)
            : this(value)
        {
            var colIndex = ColNoToInt(xNo);
            var rowIndex = yNo - 1;
            Init(value, colIndex, rowIndex);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="xNo">传入Excel列号，从A开始，只接受大写字母</param>
        /// <param name="yNo">传入Excel行号，从1开始</param>
        public Cell(object value, string xNoFrom, int yNoFrom, string xNoTo, int yNoTo) : this(value, xNoFrom, yNoTo)
        {
            var colIndex = ColNoToInt(xNoFrom);
            var rowIndex = yNoFrom - 1;
            var rowspan = yNoTo - yNoFrom;
            var index = ColNoToInt(xNoTo);
            var colspan = index - ColIndex;
            Init(value, colIndex, rowIndex, colspan, rowspan);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="cell"></param>
        public Cell(object value, string cell)
        {
            var position = CellAddressToPosition(cell);
            var colIndex = position.X;
            var rowIndex = position.Y;
            Init(value, colIndex, rowIndex);
        }
        public Cell(object value, string start, string end)
        {
            var position = CellAddressToPosition(start);
            var positionEnd = CellAddressToPosition(end);
            var colIndex = position.X;
            var rowIndex = position.Y;
            var rowspan = positionEnd.Y - position.Y;
            var colspan = positionEnd.X - position.X;
            Init(value, colIndex, rowIndex, colspan, rowspan);
        }

        public void Init(object value)
        {
            Value = value;
            //FontUnit = GraphicsUnit.Point;//单位
        }
        public void Init(object value, int colIndex, int rowIndex)
        {
            Contract.Assert(colIndex >= 0);
            Contract.Assert(rowIndex >= 0);
            Init(value);
            if (rowIndex >= MAX_ROW) throw new ArgumentException();
            if (colIndex >= MAX_COL) throw new ArgumentException();
            RowIndex = rowIndex;
            ColIndex = colIndex;
        }
        public void Init(object value, int colIndex, int rowIndex, int colspan, int rowspan)
        {
            Contract.Assert(colspan >= 0);
            Contract.Assert(rowspan >= 0);
            Init(value, colIndex, rowIndex);
            if (rowspan >= MAX_ROW) throw new ArgumentException();
            if (colspan >= MAX_COL) throw new ArgumentException();
            Colspan = colspan;
            Rowspan = rowspan;
        }
        public string Format { get;set; }

        public string Address { get { return CellPositionToAddress(this.ColIndex, this.RowIndex); } }
        /// <summary>
        /// 公式、表达式,需要设计表单式
        /// </summary>
        public string Formula
        {
            get { return format; }
            set
            {
                format = value;
                UpdateDisplay();
            }
        }
        public object Value
        {
            get { return value; }
            set
            {
                this.value = value;
                //注:这里的Style是最终写入到Excel的，跟字符串Format无关系
                if (value != null)
                {
                    if (string.IsNullOrEmpty(Format))
                    {
                        if (value is DateTime) Format = "yyyy/MM/dd";
                    }
                }
                UpdateDisplay();
            }
        }
        /// <summary>
        /// Value最终可视的效果
        /// </summary>
        public string Display { get; private set; }
        public int Width { get;internal set; }
        public int Height { get; internal set; }

        /// <summary>
        /// [单位：像素]单元格最小宽度,不能小于0（注：如果同一列的多个单元格MinWidth不相同，以最大的那个为准)
        /// </summary>
        public int MinWidth { get; set; }
        /// <summary>
        /// [单位：像素]单元格最大宽度,不能小于0（注：如果同一列的多个单元格MaxWidth不相同，以大于0的最小的那个为准)
        /// </summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// [单位：像素]单元格最小高度,不能小于0（注：如果同一行的多个单元格MinHeight不相同，以最大的那个为准)
        /// </summary>
        public int MinHeight { get; set; }
        /// <summary>
        /// [单位：像素]单元格最大高度,不能小于0（注：如果同一行的多个单元格MaxHeight不相同，以大于0的最小的那个为准)
        /// </summary>
        public int MaxHeight { get; set; }
        public int RowIndex { get; set; }
        public int ColIndex { get; set; }

        /// <summary>
        /// 跨多少列；0表示不跨列
        /// </summary>
        public int Colspan { get; set; }
        /// <summary>
        /// 跨多少行；0表示不夸行
        /// </summary>
        public int Rowspan { get; set; }

        public Consts.BorderStyle BorderStyle { get; set; }

        public Color? BorderColor { get; set; }

        public Consts.TextAlign? TextAlign { get; set; }

        #region 字体相关属性
        public Color? FontColor { get; set; }
        public Color? BackgroundColor { get; set; }

        public FontFamily FontFamily { get; set; }

        public float? FontSize { get; set; }
        ///// <summary>
        ///// 字体单位，默认是Point
        ///// </summary>
        //public GraphicsUnit FontUnit { get; set; }

        /// <summary>
        /// 字体加粗
        /// </summary>
        public bool? Bold { get; set; }

        public bool? Italic { get; set; }
        /// <summary>
        /// 文本是否自动换行
        /// </summary>
        public Consts.WhiteSpace? WhiteSpace { get; set; }
        #endregion
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Excel中使用的高度</param>
        public void SetExcelHeight(int value)
        {
            Contract.Assert(value >= 0);
            if (value == 0) Height = 0;
            Height = GetPixelHeight(value);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">Excel中使用的宽度</param>
        public void SetExcelWidth(int value)
        {
            Contract.Assert(value >= 0);
            if (value == 0) Width = 0;
            Width = GetPixelWidth(value);
        }
        public int GetExcelHeight()
        {
            return GetExcelHeight(Height);
        }
        public int GetExcelWidth()
        {
            return GetExcelWidth(Width);
        }

        public static int GetPixelHeight(int value)
        {
            return (int)Math.Round(Unit.Pound2Pixel(value, Unit.GetDpi()));
        }

        public static int GetPixelWidth(int value)
        {
            return (int)Math.Round(Unit.Pound2Pixel(value * MultipleOfHeight(), Unit.GetDpi()));
        }

        public static int GetExcelHeight(int pixel)
        {
            return (int)Math.Round(Unit.Pixel2Pound(pixel, Unit.GetDpi()));
        }

        public static int GetExcelWidth(int pixel)
        {
            return (int)Math.Round(Unit.Pixel2Pound(pixel / MultipleOfHeight(), Unit.GetDpi()));
        }
        /// <summary>
        /// 高度是宽度的多少倍
        /// </summary>
        /// <returns></returns>
        internal static float MultipleOfHeight()
        {
            //todo 是否要根据DPI计算
            return 5.25f;
        }

        private int ColNoToInt(string xNo)
        {
            Contract.Assert(!string.IsNullOrEmpty(xNo));
            xNo = xNo.ToUpper();
            int x = 0;
            for (int i = 0; i < xNo.Length; i++)
            {
                var ch = xNo[i];
                if (ch >= 'A' && ch <= 'Z')
                {
                    x *= 26;
                    x += (ch - 'A');
                }
                else
                {
                    throw new ArgumentException();
                }
            }
            return x;
        }

        /// <summary>
        /// C1这种转为对应索引
        /// </summary>
        /// <param name="no"></param>
        /// <returns></returns>
        public static Point CellAddressToPosition(string no)
        {
            Contract.Assert(!string.IsNullOrEmpty(no) && no.Length > 1);
            no = no.ToUpper();
            int x = 0, y = 0;
            int xLength = 0;
            for (int i = 0; i < no.Length; i++)
            {
                var ch = no[i];
                if (ch >= 'A' && ch <= 'Z')
                {
                    x *= 26;
                    x += ((ch - 'A')+1);
                    xLength++;
                }
                else {
                    break;
                }
            }
            if (xLength == 0 || xLength == no.Length) throw new ArgumentException();

            if(!int.TryParse(no.Substring(xLength),out y))
            {
                throw new ArgumentException();
            }
            return new Point(x-1, y - 1);
        }

        public static string CellPositionToAddress(int xIndex, int yIndex)
        {
            var x = xIndex + 1;
            string value = string.Empty;
            while (x > 0)
            {
                if (x > 26)
                {
                    value += (char)((ushort)'A' - 1 + (x / 26));
                    x = x - (x / 26) * 26;
                }
                else
                {
                    value += (char)((ushort)'A' - 1 + x);
                    x = 0;
                }
            }
            return value + (yIndex + 1);
        }

        private void UpdateDisplay()
        {
            if (value == null)
            {
                Display = string.Empty;
            }
            else if (string.IsNullOrEmpty(format)) {
                Display = value.ToString();
            }
            else if (value is string)
                Display = (string)value;
            else if (value is DateTime)
                Display = ((DateTime)value).ToString(Format);
            else if (value is int)
                Display = ((int)value).ToString(Format);
            else if (value is uint)
                Display = ((uint)value).ToString(Format);
            else if (value is long)
                Display = ((long)value).ToString(Format);
            else if (value is ulong)
                Display = ((ulong)value).ToString(Format);
            else if (value is float)
                Display = ((float)value).ToString(Format);
            else if (value is double)
                Display = ((double)value).ToString(Format);
            else if (value is decimal)
                //货币要根据Style处理
                throw new NotImplementedException();
            else if (value is bool)
                //遇到需要将bool转化为 Man/Female, 是/否这种，应该额外提供FormatProvider
                throw new NotImplementedException();
            else
                throw new NotImplementedException();
        }
    }
}