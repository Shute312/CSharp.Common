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
#if DEBUG
            MaxColWidth = 40;
            MaxRowHeight = 20;
            MinColWidth = 5;
            MinRowHeight = 5;
#else
            MaxWidth = 200;
            MaxHeight = 100;
            MinWidth = 20;
            MinHeight = 24;
#endif

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
        /// 最大列度
        /// </summary>
        public int MaxColWidth { get; set; }
        /// <summary>
        /// 最大行度
        /// </summary>
        public int MaxRowHeight { get; set; }
        /// <summary>
        /// 最小列度
        /// </summary>
        public int MinColWidth { get; set; }
        /// <summary>
        /// 最小行度
        /// </summary>
        public int MinRowHeight { get; set; }

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