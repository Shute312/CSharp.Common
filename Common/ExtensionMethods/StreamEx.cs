using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.ExtensionMethods
{
    public static class StreamEx
    {
        public static byte[] ReadAll(this Stream stream, int offset = 0, long length = 0)
        {
            byte[] buff = null;
            if (stream.CanRead)
            {
                if (length <= 0)
                    length = stream.Length - stream.Position - offset;
                buff = new byte[length];
                int count = 0;
                int waitCount = 0;
                while (offset < length - 1 && waitCount < 5)
                {
                    count = stream.Read(buff, offset, buff.Length - offset);
                    if (count == 0)
                    {
                        waitCount++;
                        Thread.Sleep(20);
                    }
                    else
                    {
                        offset += count;
                    }
                }
            }
            return buff;
        }

        public static Stream ToStream(this byte[] buff)
        {
            if (buff == null) return null;
            if (buff.Length == 0) return new MemoryStream();
            return new MemoryStream(buff);
        }
    }
}
