using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    public class Properties
    {
        public Properties() {
            Unit = Unit.Inch;
            XResolution = 72;
            YResolution = 72;
            ColorType = ColorType.Cmyk;
            BytesPerPixel = 4;
            BitsPerSample = 1;
            RowsPerStrip = 50;
            PlanarConfiguration = 1;
        }
        public int Width { get; set; }
        public int Height { get; set; }
        public int XResolution { get; set; }
        public int YResolution { get; set; }
        public Unit Unit { get; set; }
        public Compression Compression { get; set; }
        public ColorType ColorType { get; set; }

        /// <summary>
        /// 样点位数
        /// </summary>
        public int BitsPerSample { get; set; }

        /// <summary>
        /// 每像素的取样数
        /// 1 表示是黑白，灰度或者调色板图像
        /// 3 表示是RGB图像
        /// </summary>
        public int BytesPerPixel { get; set; }


        /// <summary>
        /// 每个Strip中的行数
        /// =StripsPerImage*SamplesPerPixel，如果PlanarConfiguration=2,其中StripsPerImage不是标签(StripsPerImage=(ImageLength+RowPerStrip-1)/RowsPerStrip
        /// </summary>
        public int RowsPerStrip { get; set; }

        /// <summary>
        /// 图像数据的平面排列方式
        ///  1 单平面格式
        ///  2 多平面格式
        /// </summary>
        public int PlanarConfiguration { get; set; }
    }
}
