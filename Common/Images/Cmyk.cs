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
        /// <summary>Pantone色，配合Adobe Illustrator调制出来的映射</summary>
        public static class Pantone
        {
            public static IEnumerable<Cmyk> GetEnumertor()
            {
                var list = new List<Cmyk>(MappingDictionary.Count);
                foreach (var item in MappingDictionary)
                {
                    list.Add(IntToCmky(item.Key));
                }
                return list;
            }

            public static Color CmykToRgb(Cmyk cmyk)
            {
                var intCmyk = CmykToInt(cmyk.C, cmyk.M, cmyk.Y, cmyk.K);
                var color = Color.Transparent;
                foreach (var item in MappingDictionary)
                {
                    if (item.Key == intCmyk)
                    {
                        return IntToRgb(item.Value);
                    }
                }
                return color;
            }

            private static Cmyk CreateCmyk(byte c, byte m, byte y, byte k, byte r, byte g, byte b)
            {
                if (MappingDictionary.ContainsKey(CmykToInt(c, m, y, k)))
                {

                }
                MappingDictionary.Add(CmykToInt(c, m, y, k), RgbToInt(r, g, b));
                return new Cmyk(c, m, y, k);
            }

            //用于映射RGB色
            private static readonly Dictionary<int, int> MappingDictionary = new Dictionary<int, int>();

            /// <summary> 青 (C:100,M:0,Y:0,K:0) => (R:0,G:160,B:232) </summary>
            public static readonly Cmyk Cyan = CreateCmyk(100, 0, 0, 0, 0, 160, 232);

            /// <summary> 洋红 (C:0,M:100,Y:0,K:0) => (R:228,G:0,B:126) </summary>
            public static readonly Cmyk Magenta = CreateCmyk(0, 100, 0, 0, 228, 0, 126);

            /// <summary> 黄 (C:0,M:0,Y:100,K:0) => (R:255,G:240,B:0) </summary>
            public static readonly Cmyk Yellow = CreateCmyk(0, 0, 100, 0, 255, 240, 0);

            /// <summary> 黑 (C:0,M:0,Y:0,K:100) => (R:34,G:24,B:20) </summary>
            public static readonly Cmyk Black = CreateCmyk(0, 0, 0, 100, 34, 24, 20);

            /// <summary> 蓝 (C:100,M:100,Y:0,K:0) => (R:28,G:31,B:135) </summary>
            public static readonly Cmyk Blue = CreateCmyk(100, 100, 0, 0, 28, 31, 135);

            /// <summary> 柔和蓝 (C:40,M:40,Y:0,K:0) => (R:165,G:154,B:201) </summary>
            public static readonly Cmyk SoftBlue = CreateCmyk(40, 40, 0, 0, 165, 154, 201);

            /// <summary> 粉蓝 (C:20,M:20,Y:0,K:0) => (R:209,G:204,B:229) </summary>
            public static readonly Cmyk PowderBlue = CreateCmyk(20, 20, 0, 0, 209, 204, 229);

            /// <summary> 幼蓝 (C:60,M:40,Y:0,K:0) => (R:113,G:140,B:198) </summary>
            public static readonly Cmyk BabyBlue = CreateCmyk(60, 40, 0, 0, 113, 140, 198);

            /// <summary> 海军蓝 (C:60,M:40,Y:0,K:40) => (R:79,G:99,B:143) </summary>
            public static readonly Cmyk NavyBlue = CreateCmyk(60, 40, 0, 40, 79, 99, 143);

            /// <summary> 冰蓝 (C:40,M:0,Y:0,K:0) => (R:158,G:216,B:246) </summary>
            public static readonly Cmyk IceBlue = CreateCmyk(40, 0, 0, 0, 158, 216, 246);

            /// <summary> 靓蓝 (C:60,M:60,Y:0,K:0) => (R:120,G:107,B:174) </summary>
            public static readonly Cmyk Indigo = CreateCmyk(60, 60, 0, 0, 120, 107, 174);

            /// <summary> 绿 (C:100,M:0,Y:100,K:0) => (R:0,G:153,B:67) </summary>
            public static readonly Cmyk Green = CreateCmyk(100, 0, 100, 0, 0, 153, 67);

            /// <summary> 浅蓝绿 (C:20,M:0,Y:0,K:20) => (R:182,G:205,B:217) </summary>
            public static readonly Cmyk LightTeal = CreateCmyk(20, 0, 0, 20, 182, 205, 217);

            /// <summary> 海洋绿 (C:20,M:0,Y:0,K:40) => (R:149,G:168,B:179) </summary>
            public static readonly Cmyk OceanGreen = CreateCmyk(20, 0, 0, 40, 149, 168, 179);

            /// <summary> 森林绿 (C:40,M:0,Y:20,K:60) => (R:83,G:115,B:114) </summary>
            public static readonly Cmyk ForestGreen = CreateCmyk(40, 0, 20, 60, 83, 115, 114);

            /// <summary> 草绿 (C:60,M:0,Y:40,K:40) => (R:66,G:138,B:123) </summary>
            public static readonly Cmyk GrassGreen = CreateCmyk(60, 0, 40, 40, 66, 138, 123);

            /// <summary> 浅绿 (C:60,M:0,Y:40,K:20) => (R:85,G:167,B:148) </summary>
            public static readonly Cmyk LightGreen = CreateCmyk(60, 0, 40, 20, 85, 167, 148);

            /// <summary> 松石绿 (C:60,M:0,Y:20,K:0) => (R:93,G:193,B:207) </summary>
            public static readonly Cmyk Turquoise = CreateCmyk(60, 0, 20, 0, 93, 193, 207);

            /// <summary> 海绿 (C:60,M:0,Y:20,K:20) => (R:79,G:168,B:181) </summary>
            public static readonly Cmyk SeaGreen = CreateCmyk(60, 0, 20, 20, 79, 168, 181);

            /// <summary> 薄荷绿 (C:40,M:0,Y:40,K:0) => (R:165,G:212,B:172) </summary>
            public static readonly Cmyk MintGreen = CreateCmyk(40, 0, 40, 0, 165, 212, 172);

            /// <summary> 军绿 (C:20,M:0,Y:20,K:40) => (R:150,G:166,B:154) </summary>
            public static readonly Cmyk ArmyGreen = CreateCmyk(20, 0, 20, 40, 150, 166, 154);

            /// <summary> 酒绿 (C:40,M:0,Y:100,K:0) => (R:170,G:205,B:3) </summary>
            public static readonly Cmyk WineGreen = CreateCmyk(40, 0, 100, 0, 170, 205, 3);

            /// <summary> 浅黄 (C:0,M:0,Y:60,K:0) => (R:255,G:245,B:127) </summary>
            public static readonly Cmyk PaleYellow = CreateCmyk(0, 0, 60, 0, 255, 245, 127);

            /// <summary> 淡黄 (C:0,M:0,Y:20,K:0) => (R:255,G:252,B:218) </summary>
            public static readonly Cmyk Yellowish = CreateCmyk(0, 0, 20, 0, 255, 252, 218);

            /// <summary> 褐 (C:0,M:20,Y:40,K:40) => (R:177,G:152,B:114) </summary>
            public static readonly Cmyk Brown = CreateCmyk(0, 20, 40, 40, 177, 152, 114);

            /// <summary> 褐 (C:0,M:20,Y:60,K:20) => (R:217,G:183,B:101) </summary>
            public static readonly Cmyk Glod = CreateCmyk(0, 20, 60, 20, 217, 183, 101);

            /// <summary> 深黄 (C:0,M:20,Y:100,K:0) => (R:253,G:208,B:0) </summary>
            public static readonly Cmyk DarkYellow = CreateCmyk(0, 20, 100, 0, 253, 208, 0);

            /// <summary> 桔红(橙) (C:0,M:60,Y:100,K:0) => (R:239,G:130,B:0) </summary>
            public static readonly Cmyk Orange = CreateCmyk(0, 60, 100, 0, 239, 130, 0);

            /// <summary> 秋橘红 (C:0,M:60,Y:80,K:0) => (R:239,G:131,B:54) </summary>
            public static readonly Cmyk AutumnOrange = CreateCmyk(0, 60, 80, 0, 239, 131, 54);

            /// <summary> 浅橘红 (C:0,M:40,Y:80,K:0) => (R:246,G:172,B:59) </summary>
            public static readonly Cmyk LightOrange = CreateCmyk(0, 40, 80, 0, 246, 172, 59);

            /// <summary> 沙黄 (C:0,M:20,Y:40,K:0) => (R:251,G:215,B:161) </summary>
            public static readonly Cmyk SandYellow = CreateCmyk(0, 20, 40, 0, 251, 215, 161);

            /// <summary> 栗 (C:0,M:20,Y:40,K:60) => (R:134,G:114,B:84) </summary>
            public static readonly Cmyk Maroon = CreateCmyk(0, 20, 40, 60, 134, 114, 84);

            /// <summary> 黄卡其 (C:0,M:0,Y:20,K:40) => (R:180,G:178,B:155) </summary>
            public static readonly Cmyk Khaki = CreateCmyk(0, 0, 20, 40, 180, 178, 155);

            /// <summary> 红 (C:0,M:100,Y:100,K:0) => (R:229,G:0,B:17) </summary>
            public static readonly Cmyk Red = CreateCmyk(0, 100, 100, 0, 229, 0, 17);

            /// <summary> 霓虹粉 (C:0,M:100,Y:60,K:0) => (R:229,G:0,B:68) </summary>
            public static readonly Cmyk NeonPink = CreateCmyk(0, 100, 60, 0, 229, 0, 68);

            /// <summary> 深粉 (C:0,M:60,Y:40,K:0) => (R:238,G:132,B:125) </summary>
            public static readonly Cmyk DarkPink = CreateCmyk(0, 60, 40, 0, 238, 132, 125);

            /// <summary> 热粉 (C:0,M:80,Y:40,K:0) => (R:233,G:83,B:106) </summary>
            public static readonly Cmyk HotPink = CreateCmyk(0, 80, 40, 0, 233, 83, 106);

            /// <summary> 深玫瑰红 (C:0,M:60,Y:20,K:20) => (R:206,G:115,B:134) </summary>
            public static readonly Cmyk DarkRose = CreateCmyk(0, 60, 20, 20, 206, 115, 134);

            /// <summary> 灰玫瑰红 (C:0,M:40,Y:20,K:20) => (R:211,G:154,B:155) </summary>
            public static readonly Cmyk GreyRose = CreateCmyk(0, 40, 20, 20, 211, 154, 155);

            /// <summary> 紫红 (C:0,M:40,Y:0,K:60) => (R:132,G:94,B:111) </summary>
            public static readonly Cmyk PurpleRed = CreateCmyk(0, 40, 0, 60, 132, 94, 111);

            /// <summary> 紫 (C:0,M:80,Y:0,K:20) => (R:174,G:65,B:131) </summary>
            public static readonly Cmyk Purple = CreateCmyk(0, 80, 0, 20, 174, 65, 131);

            /// <summary> 深蓝光紫 (C:0,M:60,Y:0,K:40) => (R:169,G:93,B:128) </summary>
            public static readonly Cmyk DarkBluePurple = CreateCmyk(0, 60, 0, 40, 169, 93, 128);

            /// <summary> 浅蓝光紫 (C:0,M:40,Y:0,K:0) => (R:244,G:180,B:207) </summary>
            public static readonly Cmyk LightBluePurple = CreateCmyk(0, 40, 0, 0, 244, 180, 207);

            /// <summary> 浅紫 (C:20,M:60,Y:0,K:20) => (R:204,G:125,B:177) </summary>
            public static readonly Cmyk LightPurple = CreateCmyk(20, 60, 0, 0, 204, 125, 177);

            /// <summary> 华贵紫 (C:20,M:60,Y:0,K:20) => (R:207,G:167,B:204) </summary>
            public static readonly Cmyk ElegantPurple = CreateCmyk(20, 60, 0, 20, 177, 108, 154);

            /// <summary> 蓝紫 (C:40,M:100,Y:0,K:0) => (R:164,G:0,B:130) </summary>
            public static readonly Cmyk RoyalPurple = CreateCmyk(40, 100, 0, 0, 164, 0, 130);

            /// <summary> 蓝紫 (C:60,M:80,Y:0,K:0) => (R:125,G:70,B:151) </summary>
            public static readonly Cmyk DarkBlue = CreateCmyk(60, 80, 0, 0, 125, 70, 151);

            /*
            /// <summary> 黑10 (C:0,M:0,Y:0,K:10) => (R:238,G:239,B:239) </summary>
            public static readonly Cmyk Black10 = CreateCmyk(0, 0, 0, 10, 238, 239, 239);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:20) => (R:220,G:220,B:221) </summary>
            public static readonly Cmyk Black20 = CreateCmyk(0, 0, 0, 20, 220, 220, 221);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:30) => (R:201,G:201,B:202) </summary>
            public static readonly Cmyk Black30 = CreateCmyk(0, 0, 0, 30, 201, 201, 202);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:40) => (R:180,G:181,B:181) </summary>
            public static readonly Cmyk Black40 = CreateCmyk(0, 0, 0, 40, 180, 181, 181);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:50) => (R:159,G:159,B:160) </summary>
            public static readonly Cmyk Black50 = CreateCmyk(0, 0, 0, 50, 159, 159, 160);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:60) => (R:136,G:136,B:137) </summary>
            public static readonly Cmyk Black60 = CreateCmyk(0, 0, 0, 60, 136, 136, 137);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:70) => (R:113,G:112,B:113) </summary>
            public static readonly Cmyk Black70 = CreateCmyk(0, 0, 0, 70, 113, 112, 113);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:80) => (R:89,G:87,B:87) </summary>
            public static readonly Cmyk Black80 = CreateCmyk(0, 0, 0, 80, 89, 87, 87);

            /// <summary> 黑10 (C:0,M:0,Y:0,K:90) => (R:62,G:58,B:57) </summary>
            public static readonly Cmyk Black90 = CreateCmyk(0, 0, 0, 90, 62, 58, 57);

            */

            /// <summary> C100M50 (C:100,M:50,Y:0,K:0) => (R:0,G:104,B:182) </summary>
            public static readonly Cmyk Cyan100Magenta50 = CreateCmyk(100, 50, 0, 0, 0, 104, 182);

            /// <summary> C100M50 (C:50,M:100,Y:0,K:0) => (R:146,G:6,B:131) </summary>
            public static readonly Cmyk Cyan50Magenta100 = CreateCmyk(50, 100, 0, 0, 146, 6, 131);

            /// <summary> C100Y50 (C:100,M:0,Y:50,K:0) => (R:0,G:157,B:150) </summary>
            public static readonly Cmyk Cyan100Yellow50 = CreateCmyk(0, 100, 50, 0, 0, 157, 150);

            /// <summary> C100Y50 (C:100,M:0,Y:50,K:0) => (R:142,G:0,B:30) </summary>
            public static readonly Cmyk Cyan50Yellow100 = CreateCmyk(50, 0, 100, 0, 142, 195, 30);

            /// <summary> C100Y50 (C:50,M:0,Y:100,K:0) => (R:229,G:0,B:79) </summary>
            public static readonly Cmyk Magenta100Yellow50 = CreateCmyk(100, 0, 50, 0, 229, 0, 79);

            /// <summary> C100Y50 (C:50,M:0,Y:100,K:0) => (R:243,G:151,B:0) </summary>
            public static readonly Cmyk Magenta50Yellow100 = CreateCmyk(0, 50, 100, 0, 243, 151, 0);

            ///// <summary> 蓝 C100M50 (C:100,M:100,Y:0,K:0) => (R:28,G:31,B:135) </summary>
            //public static readonly Cmyk Cyan100Magenta100 = CreateCmyk(100, 100, 0, 0, 28, 31, 135);

            ///// <summary> 绿 C100M50 (C:100,M:0,Y:100,K:0) => (R:0,G:153,B:67) </summary>
            //public static readonly Cmyk Cyan100Yellow100 = CreateCmyk(100, 0, 100, 0, 0, 153, 67);

            ///// <summary> 红 C100M50 (C:0,M:100,Y:100,K:0) => (R:229,G:0,B:17) </summary>
            //public static readonly Cmyk Magenta100Yellow100 = CreateCmyk(0, 100, 100, 0, 229, 0, 17);

            /// <summary> 白 (C:0,M:0,Y:0,K:0) => (R:254,G:254,B:254) </summary>
            //public static readonly Cmyk White = CreateCmyk(0, 0, 0, 0, 254, 254, 254);

        }
    }
}
