﻿using System;
using System.Collections.Generic;
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
    }
}
