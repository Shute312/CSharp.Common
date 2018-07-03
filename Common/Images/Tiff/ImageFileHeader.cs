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

    public class ImageFileHeader
    {
        public ImageFileHeader(Stream stream)
        {
            Contract.Assert(stream.CanRead);
            Contract.Assert(stream.Position == 0);
            var buff = new byte[8];
            stream.Read(buff, 0, buff.Length);
            Init(buff);
        }

        public ImageFileHeader(byte[] buff)
        {
            Contract.Assert(buff != null && buff.Length == 8);
            Init(buff);
        }

        private void Init(byte[] buff)
        {
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
                throw new NotImplementedException("未知的高低位表示方案");
            }
            Version = buff.ToInt(2, 2, IsBigEndian);
            Contract.Requires(Version == 42);

            FirstImageFileDirectoryIndex = buff.ToInt(4, 4, IsBigEndian);//第一个IFD的偏移量。可以在任意位置， 但必须是在一个字的边界，也就是说必须是2的整数倍。
            Contract.Requires(FirstImageFileDirectoryIndex % 2 == 0);
        }
        public bool IsBigEndian { get; private set; }
        public int FirstImageFileDirectoryIndex { get; private set; }
        public int Version { get; private set; }

    }
}
