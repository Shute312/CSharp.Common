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
        public string Style { get; set; }
        /// <summary>
        /// 公式、表达式,需要设计表单式
        /// </summary>
        public string Formula { get; set; }
        public object Value
        {
            get;
            set;
        }
        /// <summary>
        /// Value最终可视的效果
        /// </summary>
        public string Display {
            get;
        }
        public int Width { get; set; }
        public int Height { get; set; }

        public int MinWidth { get; set; }
        public int MaxWidth { get; set; }

        public int MinHeight { get; set; }

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

        public Consts.TextAlign TextAlign { get; set; }

        #region 字体相关属性
        public Color FontColor { get; set; }
        public Color BackgroundColor { get; set; }

        public FontFamily FontFamily { get; set; }

        public float FontSize { get; set; }

        /// <summary>
        /// 字体加粗
        /// </summary>
        public bool IsBold { get; set; }

        public bool IsItalic { get; set; }
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
                string format;
                //todo 判断显示样式
                if (!string.IsNullOrEmpty(style))
                {

                }
                else
                {
                }

                if (value is DateTime)
                {
                }
                else if (value is int)
                {
                }
                else if (value is long)
                {
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else
            {
                //todo 配置默认格式
                throw new NotImplementedException();
            }
            return Display;
        }
    }
}