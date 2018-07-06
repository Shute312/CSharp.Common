using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{

    public class BitmapTiff
    {
        public BitmapTiff()
        {
            frameList = new List<Frame>();
        }
        public BitmapTiff(int width, int height, int depth = 32) : this()
        {
            XResolution = 72;
            YResolution = 72;
            Compression = Compression.LZW;
            Unit = Unit.Inch;
            var frame = new Frame(width, height, 0, 0, depth);
            AddFrameProtected(frame);
        }

        private List<Frame> frameList;
        private Frame currentFrame;
        private int frameIndex;
        public int XResolution { get; private set; }
        public int YResolution { get; private set; }

        public ColorType ColorType { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Compression Compression { get; set; }
        public Unit Unit { get; set; }
        public int BytesPerPixel { get; set; }

        public int FrameIndex
        {
            get
            {
                return frameIndex;
            }
            set
            {
                Contract.Assert(value < frameList.Count && value > -1);
                if (value != frameIndex)
                {
                    frameIndex = value;
                    if (frameIndex > -1)
                    {
                        currentFrame = frameList[frameIndex];
                    }
                    else
                    {
                        currentFrame = null;
                    }
                }
            }
        }

        public bool AddFrame(Frame frame)
        {
            return AddFrameProtected(frame);
        }

        public void SetResolution(int xResolution, int yResolution)
        {
            Contract.Assert(xResolution > 0 && yResolution > 0);
            this.XResolution = xResolution;
            this.YResolution = yResolution;
        }

        protected bool AddFrameProtected(Frame frame)
        {
            if (frameList.Contains(frame))
            {
                return false;
            }
            frameList.Add(currentFrame);
            if (frameIndex == -1)
            {
                frameIndex = 0;
            }
            return true;
        }

        public bool SetPixels(byte[] pixels)
        {
            return SetPixelsProtected(pixels, currentFrame);
        }

        public void SetPixel(int x, int y, byte[] color)
        {
            SetPixel(x, y, frameIndex, color);
        }

        public void SetPixel(int x, int y, int frame, byte[] color)
        {
            frameList[frame].SetPixel(x, y, color);
        }
        public byte[] GetPixel(int x, int y)
        {
            return GetPixel(x, y, frameIndex);
        }

        public byte[] GetPixel(int x, int y, int frame)
        {
            return frameList[frame].GetPixel(x, y);
        }

        protected bool SetPixelsProtected(byte[] pixels, Frame frame)
        {
            return frame.SetPixels(pixels);
        }

        public bool Save(Stream stream)
        {
            Contract.Requires(frameList.Count > 0);
            Contract.Requires(Unit != Unit.Unknown);

            var encode = new TiffEncode();
            var properties = new Properties();
            properties.Compression = Compression;
            properties.ColorType = ColorType;
            properties.BytesPerPixel = BytesPerPixel;
            properties.Width = Width;
            properties.Height = Height;
            properties.Unit = Unit;
            properties.XResolution = XResolution;
            properties.YResolution = YResolution;

            foreach (var frame in frameList)
            {
                var ifd = new ImageFileDirection(properties, frame.GetPixels());
                encode.AddFrame(ifd);
            }
            encode.Save(stream);
            return true;
        }

        public bool Save(string filePath)
        {
            bool isOk = false;
            using (Stream stream = File.Open(filePath, FileMode.Create, FileAccess.Write))
            {
                isOk = Save(stream);
                if (isOk)
                {
                    stream.Flush();
                }
            }
            return isOk;
        }

        public bool Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new Exception("不存在文件");
            }
            using (var stream = File.OpenRead(filePath))
            {
                return Load(stream);
            }
        }
        public bool Load(Stream stream)
        {
            TiffDecode decode = new TiffDecode();
            bool isOk = decode.Load(stream);
            if (isOk)
            {
                var firstFrame = decode.GetFrame(0);
                var properties = firstFrame.Properties;
                Compression = properties.Compression;
                BytesPerPixel = properties.BytesPerPixel;
                ColorType = properties.ColorType;
                Width = properties.Width;
                Height = properties.Height;
                Unit = properties.Unit;
                XResolution = properties.XResolution;
                YResolution = properties.YResolution;

                for (var i = 0; i < decode.FrameCount; i++)
                {
                    var ifd = decode.GetFrame(i);
                    var pixels = ifd.GetPixels();
                    frameList.Add(new Frame(Width, Height, 0, 0, pixels));
                }
                FrameIndex = 0;
            }
            return isOk;
        }

        public Bitmap GetArgbBitmap(int frameIndex = 0)
        {
            var frame = frameList[frameIndex];
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
            bitmap.SetResolution(XResolution, YResolution);
            var oldPixels = frame.GetPixels();
            var oldStride = frame.InnerWidth * BytesPerPixel;
            var newStride = frame.InnerWidth * 4;
            var newPixels = new byte[newStride * frame.InnerHeight];
            for (var h = 0; h < frame.InnerHeight; h++)
            {
                var oldRowOffset = oldStride * h;
                var newRowOffset = newStride * h;
                for (int oldColIndex = 0, newColIndex = 0; oldColIndex < oldStride; oldColIndex += BytesPerPixel, newColIndex += 4)
                {
                    var oldIndex = oldRowOffset + oldColIndex;
                    var newIndex = newRowOffset + newColIndex;
                    if (ColorType == ColorType.Cmyk)
                    {
                        var c = oldPixels[oldIndex];
                        var m = oldPixels[oldIndex + 1];
                        var y = oldPixels[oldIndex + 2];
                        var k = oldPixels[oldIndex + 3];
                        var argb = Cmyk.ToRgb256(c, m, y, k);
                        newPixels[newIndex] = argb.B;
                        newPixels[newIndex + 1] = argb.G;
                        newPixels[newIndex + 2] = argb.R;
                        newPixels[newIndex + 3] = 255;
                    }
                    else if (ColorType == ColorType.Rgb)
                    {
                        var alphaOffset = BytesPerPixel == 4 ? 1 : 0;
                        var r = oldPixels[oldIndex + alphaOffset + 2];
                        var g = oldPixels[oldIndex + alphaOffset + 1];
                        var b = oldPixels[oldIndex + alphaOffset + 0];
                        newPixels[newIndex] = b;
                        newPixels[newIndex + 1] = g;
                        newPixels[newIndex + 2] = r;
                        newPixels[newIndex + 3] = byte.MaxValue;
                    }
                }
            }
            if (frame.InnerWidth == Width)
            {
                var rect = new Rectangle(frame.OffsetX, frame.OffsetY, frame.InnerWidth, frame.InnerHeight);
                var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
                Marshal.Copy(newPixels, 0, bitmapData.Scan0, newPixels.Length);
                bitmap.UnlockBits(bitmapData);
            }
            else
            {
                for (var h = 0; h < frame.InnerHeight; h++)
                {
                    var rect = new Rectangle(frame.OffsetY + h, frame.OffsetX, frame.InnerWidth, 1);
                    var bitmapData = bitmap.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);
                    Marshal.Copy(newPixels, h * newStride, bitmapData.Scan0, newStride);
                    bitmap.UnlockBits(bitmapData);
                }
            }
            return bitmap;
        }
    }
}
