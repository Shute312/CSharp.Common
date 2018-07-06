using Common.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    internal class ImageFileHeader
    {
        public ImageFileHeader()
        {
        }

        public bool Load(Stream stream)
        {
            Contract.Assert(stream.CanRead && stream.CanSeek);
            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            var buff = new byte[8];
            stream.Read(buff, 0, buff.Length);
            if (buff[0] == 77 && buff[0] == 77)
            {
                IsBigEndian = true;
            }
            else if (buff[0] == 73 && buff[0] == 73)
            {
                IsBigEndian = false;
            }
            else
            {
                //未知的高低位表示方案
                return false;
            }
            Version = buff.ToInt(2, 2, IsBigEndian);
            FirstFramePosition = buff.ToInt(4, 4, IsBigEndian);//第一个IFD的偏移量。可以在任意位置， 但必须是在一个字的边界，也就是说必须是2的整数倍。

            return Version == 42 && FirstFramePosition % 2 == 0;
        }

        public bool Save(Stream stream,bool isBigEndian ,int firstFramePosition = 8)
        {
            Contract.Assert(stream.CanWrite && stream.CanSeek);
            Contract.Assert(firstFramePosition > 7);
            if (stream.Position != 0)
            {
                stream.Position = 0;
            }
            var buff = new byte[4];
            IsBigEndian = isBigEndian;
            if (isBigEndian)
            {
                buff[0] = 77;
                buff[1] = 77;
                buff[2] = 0;
                buff[3] = 42;
            }
            else
            {
                buff[0] = 73;
                buff[1] = 73;
                buff[2] = 42;
                buff[3] = 0;
            }
            stream.Write(buff, 0, buff.Length);
            var firstFramePositionBuff = firstFramePosition.ToBytes(isBigEndian);
            stream.Write(firstFramePositionBuff, 0, firstFramePositionBuff.Length);
            return true;
        }

        public bool IsBigEndian { get; private set; }
        public int FirstFramePosition { get; private set; }
        public int Version { get; private set; }
    }
}
