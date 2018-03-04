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
    }
}
