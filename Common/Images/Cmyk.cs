using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images
{
    public class Cmyk
    {

        public Cmyk(byte c, byte m, byte y, byte k)
        {
            Contract.Assert(c < 101);
            Contract.Assert(m < 101);
            Contract.Assert(y < 101);
            Contract.Assert(k < 101);
            C = c;
            M = m;
            Y = y;
            K = k;
        }

        public byte C { get; private set; }
        public byte M { get; private set; }
        public byte Y { get; private set; }
        public byte K { get; private set; }

        public override string ToString()
        {
            return string.Format("Cmky:{0},{1},{2},{3}", C, M, Y, K);
        }

        public Color ToRgb()
        {
            //tRGB = { (1 - C) × (1 - K) , (1 - M) × (1 - K), (1 - Y) × (1 - K)}
            var r = (byte)Math.Round((100d - C) * (100 - K) * 255 / 10000);
            var g = (byte)Math.Round((100d - M) * (100 - K) * 255 / 10000);
            var b = (byte)Math.Round((100d - Y) * (100 - K) * 255 / 10000);
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// cmyk的取值范围是0到100的情况
        /// </summary>
        public static Color ToRgb(byte c, byte m, byte y, byte k)
        {
            //tRGB = { (1 - C) × (1 - K) , (1 - M) × (1 - K), (1 - Y) × (1 - K)}
            var r = (byte)Math.Round((100d - c) * (100 - k) * 255 / 10000);
            var g = (byte)Math.Round((100d - m) * (100 - k) * 255 / 10000);
            var b = (byte)Math.Round((100d - y) * (100 - k) * 255 / 10000);
            return Color.FromArgb(r, g, b);
        }

        /// <summary>
        /// cmyk的取值范围是0到255的情况
        /// </summary>
        /// <returns></returns>
        public static Color ToRgb256(byte c, byte m, byte y, byte k)
        {
            //tRGB = { (1 - C) × (1 - K) , (1 - M) × (1 - K), (1 - Y) × (1 - K)}
            var r = (byte)Math.Round((255d - c) * (255 - k) / 255);
            var g = (byte)Math.Round((255d - m) * (255 - k) / 255);
            var b = (byte)Math.Round((255d - y) * (255 - k) / 255);
            return Color.FromArgb(r, g, b);
        }

        public static Cmyk IntToCmky(int cmyk)
        {
            var k = (byte)(cmyk & 0xFF);
            cmyk >>= 8;
            var y = (byte)(cmyk & 0xFF);
            cmyk >>= 8;
            var m = (byte)(cmyk & 0xFF);
            cmyk >>= 8;
            var c = (byte)(cmyk & 0xFF);
            return new Cmyk(c, m, y, k);
        }

        public static int CmykToInt(byte c, byte m, byte y, byte k)
        {
            int value = c;
            value <<= 8;
            value |= m;
            value <<= 8;
            value |= y;
            value <<= 8;
            value |= k;
            return value;
        }

        public static int RgbToInt(byte r, byte g, byte b)
        {
            //最前面8位代表Alpha:255
            int value = 0xFF00 | r;
            value <<= 8;
            value |= g;
            value <<= 8;
            value |= b;
            return value;
        }
        public static Color IntToRgb(int argb)
        {
            return Color.FromArgb(argb);
        }

        public static Cmyk FromRgb(byte red, byte green, byte blue)
        {
            /*
             R, G, B ? [0, 1]
             tC'M'Y' = {1 - R, 1 - G, 1 - B}
             K = min{C', M', Y'}
             tCMYK = {0, 0, 0, 1} if K = 1
             tCMYK = { (C' - K)/(1 - K), (M' - K)/(1 - K), (Y' - K)/(1 - K), K } otherwise
             */
            double c = (double)(255 - red) / 255;
            double m = (double)(255 - green) / 255;
            double y = (double)(255 - blue) / 255;
            double k = (double)Math.Min(c, Math.Min(m, y));
            if (k == 1.0)
            {
                return new Cmyk(0, 0, 0, 100);
            }
            else
            {
                return new Cmyk((byte)Math.Round((c - k) / (1 - k) * 100),
                    (byte)Math.Round((m - k) / (1 - k) * 100),
                    (byte)Math.Round((y - k) / (1 - k) * 100),
                    (byte)Math.Round(k * 100));
            }
        }

        public static bool operator ==(Cmyk item1, Cmyk item2)
        {
            return item1.C == item2.C && item1.M == item2.M && item1.Y == item2.Y && item1.K == item2.K;
        }

        public static bool operator !=(Cmyk item1, Cmyk item2)
        {
            return item1.C != item2.C || item1.M != item2.M || item1.Y != item2.Y || item1.K != item2.K;
        }
    }
}
