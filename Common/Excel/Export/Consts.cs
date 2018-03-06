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
        public enum TextAlign { TopLeft, TopCenter, TopRight, MiddleLeft, MiddleCenter, MiddleRight, BottomLeft, BottomCenter, BottomRight }
    }
}