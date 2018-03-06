using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace Common.Excel.Export.Models
{
    public class BorderModel
    {
        public const string TYPE_SOLID = "SOLID";

        public BorderModel() {
            Color = Color.Black;
            Thinness = 1;
            Type = "Solid";
        }
        public Color Color { get; set; }

        /// <summary>
        /// 线条粗细
        /// </summary>
        public int Thinness { get; set; }

        /// <summary>
        /// 线条类型:直线、线段、等
        /// </summary>
        public string Type { get; set; }

    }
}