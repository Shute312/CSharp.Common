using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Common.Excel.Export
{
    public class Consts
    {
        public enum WhiteSpace
        {
            /// <summary>
            /// 文本不会换行
            /// </summary>
            NoWrap,
            /// <summary>
            /// 文本换行
            /// </summary>
            Wrap,
        }

        /// <summary>
        /// 文本对齐方式
        /// </summary>
        public enum TextAlign { TopLeft = 0, TopCenter = 1, TopRight = 2, MiddleLeft = 3, MiddleCenter = 4, MiddleRight = 5, BottomLeft = 6, BottomCenter = 7, BottomRight = 8 }
        public enum BorderStyle
        {
            None = 0,
            Hair = 1,
            Dotted = 2,
            DashDot = 3,
            Thin = 4,
            DashDotDot = 5,
            Dashed = 6,
            MediumDashDotDot = 7,
            MediumDashed = 8,
            MediumDashDot = 9,
            Thick = 10,
            Medium = 11,
            Double = 12
        }
    }
}