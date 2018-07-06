using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images
{
    public class Frame
    {
        public Frame(int width, int height, int offsetX, int offsetY, int depth = 32)
            : this(width, height, offsetX, offsetY, new byte[depth / 8 * width * height], depth)
        {
        }
        public Frame(int width, int height, int offsetX, int offsetY, byte[] pixels, int depth = 32)
        {
            if (Depth % 8 != 0)
            {
                throw new NotImplementedException();
            }
            OffsetX = offsetX;
            OffsetY = offsetY;
            InnerWidth = width;
            InnerHeight = height;
            Depth = depth;
            this.pixels = pixels;
        }

        private byte[] pixels;

        /// <summary>
        /// 位深 CMYK需要4个byte填充一个颜色，即32位(深)
        /// </summary>
        public int Depth { get; private set; }
        public int InnerWidth { get; private set; }
        public int InnerHeight { get; private set; }
        public int OffsetX { get; private set; }
        public int OffsetY { get; private set; }

        public int Stride { get { return Depth / 8 * InnerWidth; } }

        public ColorMode ColorMode { get; set; }

        public bool SetPixels(byte[] pixels)
        {
            Contract.Assert(pixels != null && pixels.Length == Depth / 8 * InnerWidth * InnerHeight);
            this.pixels = pixels;
            return true;
        }

        public byte[] GetPixels()
        {
            return pixels;
        }

        public byte[] GetPixel(int x, int y)
        {
            var bytesPerPixel = Depth / 8;
            var col = x - OffsetX;
            var row = y - OffsetY;
            var index = Stride * row + col * bytesPerPixel;
            var color = new byte[bytesPerPixel];
            for (var i = 0; i < color.Length; i++)
            {
                color[i] = pixels[index + i];
            }
            return color;
        }

        public void SetPixel(int x, int y, params byte[] color)
        {
            var bytesPerPixel = Depth / 8;
            Contract.Assert(color != null && color.Length == bytesPerPixel);
            var col = x - OffsetX;
            var row = y - OffsetY;
            var index = Stride * row + col * bytesPerPixel;

            for (var i = 0; i < color.Length; i++)
            {
                pixels[index + i] = color[i];
            }
        }

    }
}
