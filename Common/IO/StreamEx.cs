using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common.IO
{
    public static class StreamEx
    {
        public const long BUFFER_LENGHT = 4096;//4k
        const int TIME_BREAK = 10;

        public static int Write(Stream input, Stream output)
        {
            Contract.Requires(input.CanRead);
            Contract.Requires(output.CanWrite);
            int length = (int)(input.Length - input.Position);
            byte[] buff = new byte[BUFFER_LENGHT < length ? BUFFER_LENGHT : length];
            int total = 0, waitCount = 0;
            while (total < length && waitCount < 5)
            {
                int remain = length - total;
                if (remain > buff.Length) remain = buff.Length;
                int count = input.Read(buff, 0, remain);
                if (count == 0)
                {
                    waitCount++;
                    Thread.Sleep(10);
                }
                else
                {
                    output.Write(buff,0,count);
                    total += count;
                }
            }
            if(total>0) output.Flush();
            return total;
        }
        public static byte[] ReadAll(Stream stream, int offset = 0, int length = 0,int timeout = 100)
        {
            byte[] buff = null;
            if (stream.CanRead)
            {
                if (length <= 0)
                    length = (int)(stream.Length - stream.Position) - offset;
                buff = new byte[length];
                int total = 0;
                int time = 0;
                while (offset < length && time < timeout)
                {
                    int remain = length - total;
                    if (remain > buff.Length) remain = buff.Length;
                    int count = stream.Read(buff, total, buff.Length - offset);
                    if (count == 0)
                    {
                        Thread.Sleep(TIME_BREAK);
                        time += TIME_BREAK;
                    }
                    else
                    {
                        total += count;
                    }
                }
            }
            return buff;
        }
    }
}
