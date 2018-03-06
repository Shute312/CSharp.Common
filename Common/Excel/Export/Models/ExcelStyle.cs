using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Common.Excel.Export.Models
{
    public class ExcelStyle
    {
        public ExcelStyle()
        {
            MaxWidth = 200;
            MaxHeight = 100;
            MinWidth = 20;
            MinHeight = 24;
            
            //字体
            FontColor = Color.Black;
            BackgroundColor = Color.Transparent;
            FontFamily = new FontFamily("Arial");
            FontSize = 10;
            IsBold = false;
            IsItalic = false;
            TextAlign = Consts.TextAlign.MiddleCenter;
            WhiteSpace = Consts.WhiteSpace.NoWrap;
        }
        /// <summary>
        /// 单元格最大宽度
        /// </summary>
        public int MaxWidth { get; set; }
        /// <summary>
        /// 单元格最大高度
        /// </summary>
        public int MaxHeight { get; set; }
        /// <summary>
        /// 单元格最小宽度
        /// </summary>
        public int MinWidth { get; set; }
        /// <summary>
        /// 单元格最小高度
        /// </summary>
        public int MinHeight { get; set; }

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
        public Consts.TextAlign TextAlign { get; set; }
        /// <summary>
        /// 换行方式
        /// </summary>
        public Consts.WhiteSpace WhiteSpace { get; set; }
    }
}