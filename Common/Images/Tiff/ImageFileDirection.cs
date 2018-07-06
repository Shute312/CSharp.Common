using Common.Decode;
using Common.Encode;
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
    internal class ImageFileDirection
    {
        public ImageFileDirection(Properties properties = null, byte[] pixels = null)
        {
            Properties = properties != null ? properties : new Properties();
            EntryList = new List<DirectoryEntry>();
            CountBuff = new byte[2];
            Pixels = pixels;
        }
        private byte[] CountBuff;
        private List<DirectoryEntry> EntryList;
        private List<byte[]> StripList;
        private long StartPosition = -1;
        private byte[] Pixels;

        public int EntryCount { get; private set; }
        public int StripCount { get; private set; }
        public int NextIfdIndex { get; private set; }

        public Properties Properties { get; }

        /// <summary>
        /// 只解析IFD的头信息，用于找到所有IFD
        /// LoadHeader为只加载头信息，配合LoadContent使用，能有效提高性能
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="isBigEndian"></param>
        /// <returns></returns>
        public bool LoadHeader(Stream stream, bool isBigEndian)
        {
            if (StartPosition != -1) throw new InvalidOperationException("LoadHeader为只加载头信息，配合LoadContent使用；不能跟Load混合使用");
            Pixels = null;
            StartPosition = stream.Position;
            stream.Read(CountBuff, 0, CountBuff.Length);
            EntryCount = CountBuff.ToInt(isBigEndian);
            Contract.Requires(EntryCount > 0);
            stream.Position = StartPosition + CountBuff.Length + EntryCount * 12;
            var nextDeIndexBuff = new byte[4];
            stream.Read(nextDeIndexBuff, 0, nextDeIndexBuff.Length);
            NextIfdIndex = nextDeIndexBuff.ToInt(isBigEndian);
            return true;
        }
        /// <summary>
        /// 加载除头信息外的内容
        /// LoadHeader为只加载头信息，配合LoadContent使用，能有效提高性能
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="isBigEndian"></param>
        /// <returns></returns>
        public bool LoadContent(Stream stream, bool isBigEndian)
        {
            if (EntryList.Count == EntryCount) return true;

            //加载其他配置属性
            while (EntryList.Count < EntryCount)
            {
                var entry = ReadDirectoryEntry(stream, isBigEndian);
                EntryList.Add(entry);
            }

            Properties.Width = TryGetIntValue(Tag.ImageWidth, isBigEndian);
            Properties.Height = TryGetIntValue(Tag.ImageLength, isBigEndian);
            Properties.Compression = (Compression)TryGetIntValue(Tag.Compression, isBigEndian, 1);
            Properties.ColorType = (ColorType)TryGetIntValue(Tag.PhotometricInterpretation, isBigEndian);
            Properties.BitsPerSample = TryGetIntValue(Tag.BitsPerSample, isBigEndian);
            Properties.XResolution = TryGetRationalValue(Tag.XResolution, isBigEndian);
            Properties.YResolution = TryGetRationalValue(Tag.YResolution, isBigEndian);
            Properties.Unit = (Unit)TryGetIntValue(Tag.ResolutionUnit, isBigEndian);
            Properties.BytesPerPixel = TryGetIntValue(Tag.SamplesPerPixel, isBigEndian);
            Properties.RowsPerStrip = TryGetIntValue(Tag.RowsPerStrip, isBigEndian, -1);
            Properties.PlanarConfiguration = TryGetIntValue(Tag.PlanarConfiguration, isBigEndian);
            var xmpEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.XMP);
            if (xmpEntry != null)
            {
                //throw new NotImplementedException();

                //XMP是XML数据，里边可能存着其他图片资源
#if DEBUG
                var memory = new MemoryStream(xmpEntry.Data);
                memory.Position = 0;
                var xml = new XmlDocument();
                xml.Load(memory);
                xml.Save(AppDomain.CurrentDomain.BaseDirectory + "Tiff_InnerXmp.xml");
#endif
            }
            //var dotRangeEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.DotRange);

            var stripCountsEntry = EntryList.FirstOrDefault(p => p.Tag == Tag.StripByteCounts);
            Contract.Requires(stripCountsEntry != null);

            var byteCount = GetByteCountByDataType(stripCountsEntry.DataType);
            //var stripCount = (stripCountsEntry.DataByteCount + byteCount - 1) / byteCount;
            var stripCount = stripCountsEntry.DataCount;
            var stripSizes = new int[stripCount];
            var stripOffsets = new int[stripCount];
            for (var i = 0; i < stripCount; i++)
            {
                var startOffset = i * byteCount;
                var remain = stripCountsEntry.Data.Length - startOffset;
                if (remain >= byteCount)
                {
                    stripSizes[i] = stripCountsEntry.Data.ToInt(startOffset, byteCount, isBigEndian);
                }
                else
                {
                    var shortBuff = new byte[byteCount];
                    Buffer.BlockCopy(stripCountsEntry.Data, startOffset, shortBuff, 0, remain);
                    stripSizes[i] = shortBuff.ToInt(0, byteCount, isBigEndian);
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
                    stripOffsets[i] = stripOffsetsEntry.Data.ToInt(startOffset, byteCount, isBigEndian);
                }
                else
                {
                    var shortBuff = new byte[byteCount];
                    Buffer.BlockCopy(stripOffsetsEntry.Data, startOffset, shortBuff, 0, remain);
                    stripOffsets[i] = shortBuff.ToInt(0, byteCount, isBigEndian);
                }
            }
            StripList = new List<byte[]>(stripCount);
            for (int i = 0; i < stripCount; i++)
            {
                var row = Properties.RowsPerStrip;
                if (row * (i + 1) > Properties.Height) row = Properties.Height - row * i;
                StripList.Add(ReadStrip(stream, stripOffsets[i], row, stripSizes[i], Properties.Compression));
            }
            GetPixels();//生成完整的Pexels
            return true;
        }

        public bool Load(Stream stream, bool isBigEndian)
        {
            Contract.Requires(StartPosition == -1);
            LoadHeader(stream, isBigEndian);
            LoadContent(stream, isBigEndian);
            return true;
        }

        private DirectoryEntry ReadDirectoryEntry(Stream stream, bool isBigEndian)
        {
            var newPostion = StartPosition + CountBuff.Length + EntryList.Count * 12;
            if (stream.Position != newPostion)
            {
                stream.Position = newPostion;
            }
            var buff = new byte[12];
            stream.Read(buff, 0, buff.Length);
            var de = new DirectoryEntry();
            de.Tag = (Tag)buff.ToInt(0, 2, isBigEndian);
            de.DataType = (DataType)buff.ToInt(2, 2, isBigEndian);
            de.DataCount = buff.ToInt(4, 4, isBigEndian);
            var byteCount = GetByteCountByDataType(de.DataType);
            var dataByteCount = byteCount * de.DataCount;
            if (dataByteCount < 5)
            {
                de.Data = buff.Cut(8, 4);
            }
            else
            {
                var index = buff.ToInt(8, 4, isBigEndian);
                var oldPosition = stream.Position;
                stream.Position = index;
                de.Data = new byte[dataByteCount];
                stream.Read(de.Data, 0, de.Data.Length);
                stream.Position = oldPosition;
            }
            return de;
        }

        public bool Save(Stream stream, bool isBigEndian, bool isLastOne = true)
        {
            Contract.Requires(Pixels != null);
            if (Properties.Compression != Compression.No && Properties.Compression != Compression.LZW)
            {
                throw new NotImplementedException();
            }
            var stride = Properties.Width * Properties.BytesPerPixel;
            int maxStripLength = Properties.Compression == Compression.No ? 102400 : 409600;//有压缩，则40k；没压缩，则10k
            if (stride > maxStripLength)
            {
                Properties.RowsPerStrip = 1;
            }
            else
            {
                Properties.RowsPerStrip = maxStripLength / stride;
            }
            var stripSize = stride * Properties.RowsPerStrip;
            List<byte[]> strips = new List<byte[]>(Pixels.Length / stripSize + 1);
            var handleCount = 0;
            //if (Properties.ColorType != ColorType.Cmyk) throw new NotImplementedException();

            int stripTotalContentLength = 0;
            while (handleCount < Pixels.Length)
            {
                var currSize = stripSize;
                if (currSize + handleCount > Pixels.Length) currSize = Pixels.Length - handleCount;
                var row = currSize / stride;
                var strip = new byte[currSize];
                Buffer.BlockCopy(Pixels, handleCount, strip, 0, strip.Length);
#if DEBUG
                Properties.Compression = Compression.No;
#endif
                if (Properties.Compression == Compression.No)
                {

                }
                else if (Properties.Compression == Compression.LZW)
                {
                    throw new NotImplementedException();
                    /*
                    Stream memoryStream = new MemoryStream(strip);
                    var lzw = new LZWEncoder(Properties.Width, row, strip, 200);
                    lzw.Encode(memoryStream);
                    var encodeData = new byte[memoryStream.Length];
                    Buffer.BlockCopy(strip, 0, encodeData, 0, encodeData.Length);
                    strip = encodeData;
                    */
                }
                else
                {
                    throw new NotImplementedException();
                }
                stripTotalContentLength += strip.Length;
                strips.Add(strip);
                handleCount += currSize;
            }

            EntryList.Clear();
            EntryList.Add(new DirectoryEntry(Tag.NewSubfileType, DataType.Long, 1, 0, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.ImageWidth, DataType.Short, 1, Properties.Width, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.ImageLength, DataType.Short, 1, Properties.Height, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.Compression, DataType.Short, 1, (int)Properties.Compression, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.SamplesPerPixel, DataType.Short, 1, Properties.BytesPerPixel, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.PhotometricInterpretation, DataType.Short, 1, (int)Properties.ColorType, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.PlanarConfiguration, DataType.Short, 1, Properties.PlanarConfiguration, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.RowsPerStrip, DataType.Short, 1, Properties.RowsPerStrip, isBigEndian));
            EntryList.Add(new DirectoryEntry(Tag.ResolutionUnit, DataType.Short, 1, (int)Properties.Unit, isBigEndian));
            //EntryList.Add(new DirectoryEntry(Tag.DotRange, DataType.Byte, 2, new byte[] { 0, 255,0,0 }));

            var xResolutionEntry = new DirectoryEntry() { DataType = DataType.Rational,Tag= Tag.XResolution,DataCount =1 };
            var yResolutionEntry = new DirectoryEntry() { DataType = DataType.Rational, Tag = Tag.YResolution, DataCount = 1 };
            EntryList.Add(xResolutionEntry);
            EntryList.Add(yResolutionEntry);

            var bitsPerSampleEntry = new DirectoryEntry() { DataType = DataType.Short, Tag = Tag.BitsPerSample };
            EntryList.Add(bitsPerSampleEntry);

            var stripOffsetEntry = new DirectoryEntry() { DataType = DataType.Long, Tag = Tag.StripOffsets,DataCount = strips.Count };
            var stripSizeEntry = new DirectoryEntry() { DataType = DataType.Long, Tag = Tag.StripByteCounts, DataCount = strips.Count };
            byte[] bitsPerSampleBuff;
            if (Properties.ColorType == ColorType.Cmyk)
            {
                if (isBigEndian) bitsPerSampleBuff = new byte[] { 0, 8, 0, 8, 0, 8, 0, 8 };
                else bitsPerSampleBuff = new byte[] { 8, 0, 8, 0, 8, 0, 8, 0 };
            }
            else if (Properties.ColorType == ColorType.Rgb)
            {
                if (isBigEndian) bitsPerSampleBuff = new byte[] { 0, 8, 0, 8, 0, 8 };
                else bitsPerSampleBuff = new byte[] { 8, 0, 8, 0, 8, 0 };
            }
            else
            {
                throw new NotImplementedException();
            }
            bitsPerSampleEntry.DataCount = bitsPerSampleBuff.Length / 2;

            EntryList.Add(stripOffsetEntry);
            EntryList.Add(stripSizeEntry);

            EntryCount = EntryList.Count;
            int headerEndPosion = (int)stream.Position + 2 + EntryCount * 12 + 4;//2个byte记录EntryCount,Entry数据，4个byte记录下一个IFD的位置
            var xResolutionPosition = headerEndPosion;
            var yResolutionPosition = xResolutionPosition + 8;
            xResolutionEntry.Data = xResolutionPosition.ToBytes(isBigEndian);
            yResolutionEntry.Data = yResolutionPosition.ToBytes(isBigEndian);

            var bitsPerSamplePosition = yResolutionPosition + 8;
            bitsPerSampleEntry.Data = bitsPerSamplePosition.ToBytes(isBigEndian);

            var stripOffsetPosition = bitsPerSamplePosition + bitsPerSampleBuff.Length;
            int nextIfdPostion;
            var stripOffsets = new int[strips.Count];
            if (strips.Count == 1)
            {
                stripOffsetEntry.Data = stripOffsetPosition.ToBytes(isBigEndian);
                stripSizeEntry.Data = strips[0].Length.ToBytes(isBigEndian);
                nextIfdPostion = stripOffsetPosition + stripTotalContentLength;
            }
            else
            {
                var stripSizePosition = stripOffsetPosition + strips.Count * GetByteCountByDataType(stripOffsetEntry.DataType);

                stripOffsetEntry.Data = stripOffsetPosition.ToBytes(isBigEndian);
                stripSizeEntry.Data = stripSizePosition.ToBytes(isBigEndian);

                stripOffsets[0] = stripOffsetPosition;
                for (var i = 1; i < strips.Count; i++)
                {
                    stripOffsets[i] = stripOffsets[i - 1] + strips[i - 1].Length;
                }
                nextIfdPostion = stripSizePosition + strips.Count * GetByteCountByDataType(stripSizeEntry.DataType) + stripTotalContentLength;
            }

            stream.Write(EntryList.Count.ToBytes(2, isBigEndian), 0, 2);
            foreach (var entry in EntryList)
            {
                var bytes = entry.ToBytes(isBigEndian);
                Contract.Assert(bytes.Length == 12);
                stream.Write(bytes, 0, bytes.Length);
            }
            if (isLastOne)
            {
                nextIfdPostion = 0;
            }
            stream.Write(nextIfdPostion.ToBytes(isBigEndian), 0, 4);

            //写XResolutionEntry、YResolutionEntry
            stream.Write(ToRational(Properties.XResolution,isBigEndian),0,8);
            stream.Write(ToRational(Properties.YResolution, isBigEndian), 0, 8);

            //写BitsPerSample
            stream.Write(bitsPerSampleBuff, 0, bitsPerSampleBuff.Length);

            if (strips.Count > 1)
            {
                //写EntryOffset
                foreach (var offset in stripOffsets)
                {
                    stream.Write(offset.ToBytes(4, isBigEndian), 0, 4);
                }
                //写StripSize
                foreach (var strip in strips)
                {
                    stream.Write(strip.Length.ToBytes(4, isBigEndian), 0, 4);
                }
            }
            //写Strip内容
            foreach (var strip in strips)
            {
                stream.Write(strip, 0, strip.Length);
            }
            return true;
        }

        public byte[] GetPixels()
        {
            if (Pixels != null) return Pixels;

            var stride = Properties.BytesPerPixel * Properties.Width;//一行的数据量
            var pixels = new byte[stride * Properties.Height];
            var rowIndex = 0;
            var byteOffset = 0;
            foreach (var strip in StripList)
            {
                var data = strip;
                var dataLength = data.Length;
                var row = dataLength / Properties.BytesPerPixel / Properties.Width;
                if (row + rowIndex >= Properties.Height) row = Properties.Height - rowIndex - 1;

                dataLength = row * stride;
                Buffer.BlockCopy(data, 0, pixels, byteOffset, dataLength);

                byteOffset = dataLength;
                rowIndex += row;
            }
            Pixels = pixels;
            return pixels;
        }

        private int GetByteCountByDataType(DataType dataType)
        {
            switch (dataType)
            {
                case DataType.Byte:
                case DataType.SByte:
                case DataType.AscII:
                    return 1;
                case DataType.Short:
                case DataType.SShort:
                    return 2;
                case DataType.Long:
                case DataType.Float:
                    return 4;
                case DataType.Rational:
                case DataType.SRational:
                case DataType.Double:
                    return 8;
                default:
                    throw new NotImplementedException();
            }
        }

        private byte[] ReadStrip(Stream stream,int offset,int rows,int count,Compression compression)
        {
            if (stream.Position != offset)
            {
                stream.Position = offset;
            }
            byte[] strip;
            if (compression == Compression.LZW)
            {
                throw new NotImplementedException();

                /*LZWDecoder lzwDecoder = new LZWDecoder();
                
                var colorSize = stream.ReadByte();
                var colorSize2 = colorSize;
                stream.Position -= 1;
                var depth = 0;
                while (colorSize > 1)
                {
                    colorSize >>= 1;
                    depth++;
                }
                var oldPostion = stream.Position;
                strip = lzwDecoder.DecodePixels( stream,Properties.Width, rows, depth);
                if (oldPostion + count != stream.Position)
                {
                    var readSize = stream.Position - oldPostion;
                }
                else
                {
                }
                */
            }
            else
            {
                strip = new byte[count];
                stream.Read(strip, 0, strip.Length);
            }
            return strip;
        }

        private int TryGetIntValue(Tag tag, bool isBigEndian, int defaultValue = 0)
        {
            var entry = EntryList.FirstOrDefault(p => p.Tag == tag);
            if (entry != null) return GetIntValue(entry, isBigEndian);
            else return defaultValue;
        }

        private int GetIntValue(DirectoryEntry entry,bool isBigEndian)
        {
            var value = 0;
            if (entry.DataType == DataType.Short)
            {
                if (isBigEndian)
                {
                    value = entry.Data.ToInt(2, 2, isBigEndian);
                }
                else
                {
                    value = entry.Data.ToInt(0, 2, isBigEndian);
                }
            }
            else if (entry.DataType == DataType.Long)
            {
                value = entry.Data.ToInt(0, 4, isBigEndian);
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
        private int TryGetRationalValue(Tag tag,bool isBigEndian, int defaultValue = 0)
        {
            var entry = EntryList.FirstOrDefault(p => p.Tag == tag);
            if (entry == null) return defaultValue;
            /*
             * 前面是分子、后边是
             * the first represents the numerator of a
            fraction; the second, the denominator.
            */
            var fraction = entry.Data.ToInt(0, 4, isBigEndian);
            var denominator = entry.Data.ToInt(4,4,isBigEndian);
            if (denominator <= 0) return fraction;
            var value = (int)Math.Round(((double)fraction) / denominator);
            return value;
        }

        private byte[] ToRational(int value, bool isBigEndian)
        {
            var denominator = 1000;//分母
            var fraction =  (value * denominator);//分子
            var fractionBytes = fraction.ToBytes(isBigEndian);
            var denominatorBytes = denominator.ToBytes(isBigEndian);
            var bytes = new byte[8];
            Buffer.BlockCopy(fractionBytes, 0,bytes,0,4);
            Buffer.BlockCopy(denominatorBytes, 0, bytes, 4, 4);
            return bytes;
        }
    }

}
