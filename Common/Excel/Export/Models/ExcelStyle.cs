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
            MinColWidth = 60;
            MaxColWidth = 200;
            MinRowHeight = 24;
            MaxRowHeight = 50;

            //字体
            FontColor = Color.Black;
            BackgroundColor = Color.Transparent;
            //FontFamily = new FontFamily("Arial");
            FontFamily = new FontFamily("宋体");
            //FontFamily = new FontFamily("Calibri");
            FontSize = 12;
            Bold = false;
            Italic = false;
            TextAlign = Consts.TextAlign.MiddleCenter;
            WhiteSpace = Consts.WhiteSpace.Wrap;
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
        public bool Bold { get; set; }

        public bool Italic { get; set; }
#endregion
        public Consts.TextAlign TextAlign { get; set; }
        /// <summary>
        /// 换行方式
        /// </summary>
        public Consts.WhiteSpace WhiteSpace { get; set; }
    }
}