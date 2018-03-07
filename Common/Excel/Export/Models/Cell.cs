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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value">若传入null,则单元格数据格式默认为string,如有需要则强制指定Style</param>
        public Cell(object value)
        {
            SetValue(value,null);
            //FontUnit = GraphicsUnit.Point;//单位
        }
        public Cell(object value, int rowIndex, int colIndex)
            : this(value)
        {
            RowIndex = rowIndex;
            ColIndex = colIndex;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="xNo">传入Excel列号，从A开始，只接受大写字母</param>
        /// <param name="yNo">传入Excel行号，从1开始</param>
        public Cell(object value, string xNo, int yNo)
            : this(value)
        {
            Contract.Assert(!string.IsNullOrEmpty(xNo) || xNo.Length < 3);
            Contract.Assert(yNo > 0);
            int index = 0;
            for (int i = 0; i < xNo.Length; i++)
            {
                var ch = xNo[i];
                if (ch < 'A' || ch > 'Z') throw new ArgumentException();
                index *= 25;
                index += (int)(ch - 'A');
            }
            ColIndex = index;
            RowIndex = yNo + 1;
        }
        public string Style { get; private set; }
        /// <summary>
        /// 公式、表达式,需要设计表单式
        /// </summary>
        public string Formula { get; set; }
        public object Value { get; private set; }
        /// <summary>
        /// Value最终可视的效果
        /// </summary>
        public string Display { get; private set; }
        public int PixelWidth { get; set; }
        public int PixelHeight { get; set; }

        /// <summary>
        /// 单元格最小宽度,不能小于0（注：如果同一列的多个单元格MinWidth不相同，以最大的那个为准)
        /// </summary>
        public int MinWidth { get; set; }
        /// <summary>
        /// 单元格最大宽度,不能小于0（注：如果同一列的多个单元格MaxWidth不相同，以大于0的最小的那个为准)
        /// </summary>
        public int MaxWidth { get; set; }

        /// <summary>
        /// 单元格最小高度,不能小于0（注：如果同一行的多个单元格MinHeight不相同，以最大的那个为准)
        /// </summary>
        public int MinHeight { get; set; }
        /// <summary>
        /// 单元格最大高度,不能小于0（注：如果同一行的多个单元格MaxHeight不相同，以大于0的最小的那个为准)
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

        public Consts.TextAlign? TextAlign { get; set; }

        #region 字体相关属性
        public Color FontColor { get; set; }
        public Color BackgroundColor { get; set; }

        public FontFamily FontFamily { get; set; }

        public float FontSize { get; set; }
        ///// <summary>
        ///// 字体单位，默认是Point
        ///// </summary>
        //public GraphicsUnit FontUnit { get; set; }

        /// <summary>
        /// 字体加粗
        /// </summary>
        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }
        /// <summary>
        /// 文本是否自动换行
        /// </summary>
        public Consts.WhiteSpace? WhiteSpace { get; set; }
        #endregion

        public string SetValue(object value, string style)
        {
            Value = value;
            if (!string.IsNullOrEmpty(style))
            {
                Style = style;
            }
            //注:这里的Style是最终写入到Excel的，跟字符串Format无关系
            if (value != null)
            {
                string format = string.Empty;

                if (!string.IsNullOrEmpty(Style))
                {
                    //todo 根据数据类型，给出默认Style
                }
                if (!string.IsNullOrEmpty(Style))
                {
                    //todo 根据Style，给出format
                }
                else
                {
                    if (value is DateTime) format = "yyyy/MM/dd";
                }
                if (value is DateTime)
                    Display = ((DateTime)value).ToString(format);
                else if (value is int)
                    Display = ((int)value).ToString(format);
                else if (value is uint)
                    Display = ((uint)value).ToString(format);
                else if (value is long)
                    Display = ((long)value).ToString(format);
                else if (value is ulong)
                    Display = ((ulong)value).ToString(format);
                else if (value is float)
                    Display = ((float)value).ToString(format);
                else if (value is double)
                    Display = ((double)value).ToString(format);
                else if(value is decimal)
                    //货币要根据Style处理
                    throw new NotImplementedException();
                else if (value is bool)
                    //遇到需要将bool转化为 Man/Female, 是/否这种，应该额外提供FormatProvider
                    throw new NotImplementedException();
                else
                    throw new NotImplementedException();
            }
            else
            {
                Display = string.Empty;
                //todo 配置默认格式
                throw new NotImplementedException();
            }
            return Display;
        }
    }
}