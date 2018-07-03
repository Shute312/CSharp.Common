using Common.Decode;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    public class Strip
    {
        public Strip(Stream stream,int width,int height,int count, Compression compression)
        {
            if (compression != Compression.No)
            {
                LZWDecoder lzwDecoder = new LZWDecoder(stream);
                byte[] indices = lzwDecoder.DecodePixels(width, height, 32);
                Data = indices;
            }
            else
            {
                var buff = new byte[count];
                stream.Read(buff,0,buff.Length);
                Data = buff;
            }
        }
        internal byte[] Data;
    }
}
