using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ExtensionMethods
{
    public static class ValueEx
    {
        public static int ToInt(this byte[] buff, bool isBigEndian = false)
        {
            return ToInt(buff, 0, buff.Length, isBigEndian);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buff"></param>
        /// <param name="startIndex"></param>
        /// <param name="count"></param>
        /// <param name="isBigEndian">true:高位在字节流的末端；</param>
        /// <returns></returns>
        public static int ToInt(this byte[] buff, int startIndex, int count, bool isBigEndian)
        {
            var value = 0;
            if (isBigEndian)
            {
                if (count == 4)
                {
                    value = (buff[startIndex] << 24) | (buff[startIndex + 1] << 16) | (buff[startIndex + 2] << 8) | buff[startIndex + 3];
                }
                else if (count == 2)
                {
                    value = (buff[startIndex] << 8) | (buff[startIndex + 1]);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        value <<= 8;
                        value |= buff[startIndex + i];
                    }
                }
            }
            else
            {
                if (count == 4)
                {
                    value = (buff[startIndex+3] << 24) | (buff[startIndex + 2] << 16) | (buff[startIndex + 1] << 8) | buff[startIndex];
                }
                else if (count == 2)
                {
                    value = (buff[startIndex + 1] << 8) | (buff[startIndex]);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        value |= buff[startIndex + i] << (i * 8);
                    }
                }
            }
            return value;
        }

        public static byte[] Cut(this byte[] buff, int startIndex, int count)
        {
            byte[] data = new byte[count];
            Buffer.BlockCopy(buff, startIndex, data,0, count);
            return data;
        }

        public static byte[] ToBytes(this int value, bool isBigEndian)
        {
            return ToBytes(value, 4, isBigEndian);
        }

        public static byte[] ToBytes(this int value, int count, bool isBigEndian)
        {
            var bytes = new byte[count];
            Contract.Assert(count < 5 && count > 0);
            if (isBigEndian)
            {
                if (count == 4)
                {
                    bytes[3] = (byte)value;
                    bytes[2] = (byte)(value>>8);
                    bytes[1] = (byte)(value >> 16);
                    bytes[0] = (byte)(value >> 24);
                }
                else if (count == 2)
                {
                    bytes[1] = (byte)(value >> 8);
                    bytes[0] = (byte)(value);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        bytes[i] = (byte)(value >> ((bytes.Length - i - 1) * 8));
                    }
                }
            }
            else
            {
                if (count == 4)
                {
                    bytes[0] = (byte)value;
                    bytes[1] = (byte)(value >> 8);
                    bytes[2] = (byte)(value >> 16);
                    bytes[3] = (byte)(value >> 24);
                }
                else if (count == 2)
                {
                    bytes[0] = (byte)value;
                    bytes[1] = (byte)(value >> 8);
                }
                else
                {
                    for (int i = 0; i < count; i++)
                    {
                        bytes[i] = (byte)(value >> (i * 8));
                    }
                }
            }
            return bytes;
        }
    }
}
