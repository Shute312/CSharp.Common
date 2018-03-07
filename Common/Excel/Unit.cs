using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Excel
{
    public class Unit
    {
        public static float Pixel2Pound(float pixel, float dpi)
        {
            //72磅＝1英寸，1英寸 = 2.54厘米
            return Pixel2Inch(pixel * 72, dpi);
        }

        public static float Pound2Pixel(float pound, float dpi)
        {
            //72磅＝1英寸，1英寸 = 2.54厘米
            return Inch2Pixel(pound, dpi) / 72;
        }
        /// <summary>
        /// 英寸转像素
        /// </summary>
        /// <param name="inch"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public static float Inch2Pixel(float inch, float dpi)
        {
            return inch * dpi;
        }
        /// <summary>
        /// 英寸转像素
        /// </summary>
        /// <param name="pixel"></param>
        /// <param name="dpi"></param>
        /// <returns></returns>
        public static float Pixel2Inch(float pixel, float dpi)
        {
            return pixel / dpi;
        }
    }
}
