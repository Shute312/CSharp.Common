using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.ExtensionMethods
{
    public static class StreamEx
    {
        public static byte[] ReadAll(this Stream stream, int offset = 0, int length = 0)
        {
            return IO.StreamEx.ReadAll(stream,offset,length);
        }
        public static int WriteTo(this Stream input, Stream output) {
            return IO.StreamEx.Write(input, output);
        }
        public static Stream ToStream(this byte[] buff)
        {
            if (buff == null) return null;
            if (buff.Length == 0) return new MemoryStream();
            return new MemoryStream(buff);
        }

        public static void Skip(this Stream stream, int byteCount)
        {
            Contract.Assert(byteCount>-1);
            if (byteCount == 0) return;
            if (stream.CanSeek)
            {
                stream.Position += byteCount;
            }
            else
            {
                int remain = byteCount;
                const int BLOCK_SIZE = 1024;
                while (remain > 0)
                {
                    byte[] buff;
                    if (remain > BLOCK_SIZE)
                    {
                        buff = new byte[BLOCK_SIZE];
                        remain -= BLOCK_SIZE;
                    }
                    else {
                        buff = new byte[remain];
                    }
                    stream.Read(buff, 0, buff.Length);
                }
            }
        }
    }
}
