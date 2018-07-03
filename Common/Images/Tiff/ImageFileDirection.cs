using Common.Decode;
using Common.ExtensionMethods;
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
using System.Xml;

namespace Common.Images.Tiff
{
    public class ImageFileDirection
    {
        public ImageFileDirection(Stream stream, bool isBigEndian)
        {
            IsBigEndian = isBigEndian;
            var deCountBuff = new byte[2];
            stream.Read(deCountBuff, 0, deCountBuff.Length);
            var entryCount = deCountBuff.ToInt(isBigEndian);
            Contract.Requires(entryCount > 0);
            EntryList = new List<DirectoryEntry>(entryCount);
            for (var i = 0; i < entryCount; i++)
            {
                var entry = ReadDirectoryEntry(stream);
                EntryList.Add(entry);
            }
            var nextDeIndexBuff = new byte[4];
            NextIFDIndex = nextDeIndexBuff.ToInt(isBigEndian);

            Width = TryGetIntValue(Tag.ImageWidth);
            Height = TryGetIntValue(Tag.ImageLength);
            Compression = (Compression)TryGetIntValue(Tag.Compression, 1);
            ColorType = (ColorType)TryGetIntValue(Tag.PhotometricInterpretation);
            BitsPerSample = TryGetIntValue(Tag.BitsPerSample);
            XResolution = TryGetRationalValue(Tag.XResolution);
            YResolution = TryGetRationalValue(Tag.YResolution);
            Unit = (Unit)TryGetIntValue(Tag.ResolutionUnit);
            BytesPerPixel = TryGetIntValue(Tag.SamplesPerPixel);
            RowsPerStrip = TryGetIntValue(Tag.RowsPerStrip, -1);
            PlanarConfiguration = TryGetIntValue(Tag.PlanarConfiguration);
            var xmpEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.XMP);
            if (xmpEntry != null)
            {
                throw new NotImplementedException();

                var memory = new MemoryStream(xmpEntry.Data);
                memory.Position = 0;
                var xml = new XmlDocument();
                xml.Load(memory);
                xml.Save(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\temp.xml");

            }
            //var dotRangeEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.DotRange);

            var stripCountsEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.StripByteCounts);           
            Contract.Requires(stripCountsEntry != null);

            var byteCount = stripCountsEntry.DataType == DataType.Long ? 4 : 2;
            var stripCount = (stripCountsEntry.DataByteCount + byteCount-1)/ byteCount;
            var stripSizes = new int[stripCount];
            var stripOffsets = new int[stripCount];
            for (var i = 0; i < stripCount; i++)
            {
                var startOffset = i * byteCount;
                var remain = stripCountsEntry.Data.Length - startOffset;
                if (remain >= byteCount)
                {
                    stripSizes[i] = stripCountsEntry.Data.ToInt(startOffset, byteCount, IsBigEndian);
                }
                else
                {
                    var shortBuff = new byte[byteCount];
                    Buffer.BlockCopy(stripCountsEntry.Data, startOffset, shortBuff, 0, remain);
                    stripSizes[i] = shortBuff.ToInt(0, byteCount, IsBigEndian);
                }
            }

            var stripOffsetsEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.StripOffsets);
            Contract.Requires(stripOffsetsEntry != null);
            byteCount = stripOffsetsEntry.DataType == DataType.Long ? 4 : 2;
            for (int i = 0; i < stripCount; i++)
            {
                var startOffset = i * byteCount;
                var remain = stripOffsetsEntry.Data.Length - startOffset;
                if (remain >= byteCount)
                {
                    stripOffsets[i] = stripOffsetsEntry.Data.ToInt(startOffset, byteCount, IsBigEndian);
                }
                else
                {
                    var shortBuff = new byte[byteCount];
                    Buffer.BlockCopy(stripOffsetsEntry.Data, startOffset, shortBuff, 0, remain);
                    stripOffsets[i] = shortBuff.ToInt(0, byteCount, IsBigEndian);
                }
            }
            StripList = new List<Strip>(stripCount);
            for (int i = 0; i < stripCount; i++)
            {
                var row = RowsPerStrip;
                if (row * (i + 1) > Height) row = Height - row * i;
                StripList.Add(ReadStrip(stream, stripOffsets[i], row,stripSizes[i],Compression));
            }
            var ContentLength = stream.Position;
            var StreamLength = stream.Length;
        }

        private bool IsBigEndian;
        private List<DirectoryEntry> EntryList;
        private List<Strip> StripList;

        private DirectoryEntry ReadDirectoryEntry(Stream stream)
        {
            var buff = new byte[12];
            stream.Read(buff, 0, buff.Length);
            var de = new DirectoryEntry();
            de.Tag = (Tag)buff.ToInt(0, 2, IsBigEndian);
            de.DataType = (DataType)buff.ToInt(2, 2, IsBigEndian);
            de.DataByteCount = buff.ToInt(4, 4, IsBigEndian);
            if (de.DataByteCount < 5)
            {
                de.Data = buff.Cut(8, 4);
            }
            else
            {
                var index = buff.ToInt(8, 4, IsBigEndian);
                var oldPosition = stream.Position;
                stream.Position = index;
                de.Data = new byte[de.DataByteCount];
                stream.Read(de.Data, 0, de.DataByteCount);
                stream.Position = oldPosition;
            }
            return de;
        }

        public Bitmap GetRgbImage()
        {
            Contract.Requires(Width > 0);
            Contract.Requires(Height > 0);
            Contract.Requires(BytesPerPixel > 0);
            var bitmap = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bitmap.SetResolution(XResolution, YResolution);
            var rowIndex = 0;
            var stride = BytesPerPixel * Width;//一行的数据量
            foreach (var strip in StripList)
            {
                var data = strip.Data;
                var dataLenth = data.Length;
                var row = dataLenth / BytesPerPixel / Width;
                if (row + rowIndex >= Height) row = Height - rowIndex - 1;
                var rect = new Rectangle(0, rowIndex, Width, row);
                var rgbValues = new byte[Width * row * 3];
                for (int i = 0, w = 0; i < rgbValues.Length;)
                {
                    var c = data[i++];
                    var m = data[i++];
                    var y = data[i++];
                    var k = data[i++];
                    var argb = Cmyk.ToRgb256(c, m, y, k);
                    rgbValues[w++] = argb.B;
                    rgbValues[w++] = argb.G;
                    rgbValues[w++] = argb.R;
                }
                var bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
                Marshal.Copy(rgbValues, 0, bitmapData.Scan0, rgbValues.Length);
                bitmap.UnlockBits(bitmapData);

                rowIndex += row;
            }
            return bitmap;
        }

        private Strip ReadStrip(Stream stream,int offset,int rows,int count,Compression compression)
        {
            if (stream.Position != offset)
            {
                stream.Position = offset;
            }
            return new Strip(stream,Width, rows, count, compression);
        }

        private int TryGetIntValue(Tag tag, int defaultValue = 0)
        {
            var entry = EntryList.FirstOrDefault(p => p.Tag == tag);
            if (entry != null) return GetIntValue(entry);
            else return defaultValue;
        }

        private int GetIntValue(DirectoryEntry entry)
        {
            var value = 0;
            if (entry.DataType == DataType.Short)
            {
                if (IsBigEndian)
                {
                    value = entry.Data.ToInt(2, 2, IsBigEndian);
                }
                else
                {
                    value = entry.Data.ToInt(0, 2, IsBigEndian);
                }
            }
            else if (entry.DataType == DataType.Long)
            {
                value = entry.Data.ToInt(0, 4, IsBigEndian);
            }
            else if (entry.DataType == DataType.Byte)
            {
                return entry.Data[0];
            }
            else
            {
                throw new NotImplementedException();
            }
            return value;
        }
        private int TryGetRationalValue(Tag tag, int defaultValue = 0)
        {
            var entry = EntryList.FirstOrDefault(p => p.Tag == tag);
            if (entry == null) return defaultValue;
            var value = entry.Data.ToInt(0, 2, false);
            //if (tag == Tag.XResolution || tag == Tag.YResolution)
            //{
            //    value = entry.Data[1];
            //}
            //else
            //{
            //    throw new NotImplementedException();
            //}
            return value;
        }

        public int EntryCount { get { return EntryList.Count; } }
        public int NextIFDIndex { get; private set; }

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
        public int BytesPerPixel { get;set;}


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
