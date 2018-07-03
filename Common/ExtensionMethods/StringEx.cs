using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.ExtensionMethods
{
    public static class StringEx
    {
        public static string ToString(this byte[] buff, string encoding="utf-8", int offset = 0, int length = 0)
        {
            return ToString(buff, Encoding.GetEncoding(encoding), offset, length);
        }
        public static string ToString(this byte[] buff, Encoding encoding, int offset = 0, int length = 0)
        {
            if (buff == null) return null;
            if (buff.Length == 0) return string.Empty;
            if (offset > 0)
            {
                if (length > 0)
                    return encoding.GetString(buff, offset, length);
                else
                    return encoding.GetString(buff, offset, buff.Length - offset);
            }
            else
            {
                if (length > 0)
                    return encoding.GetString(buff, 0, length);
                else
                    return encoding.GetString(buff);
            }
        }

        public static string ToString(this Stream stream, Encoding encoding, int offset = 0, int length = -1)
        {
            byte[] buff = stream.ReadAll(offset, length);
            return buff.ToString(encoding);
        }

        public static byte[] ToBytes(this string str, string encoding = "utf-8")
        {
            return ToBytes(str, Encoding.GetEncoding(encoding));
        }
        public static byte[] ToBytes(this string str, Encoding encoding)
        {
            if (str == null) return null;
            if (str.Length == 0) return new byte[0];
            return encoding.GetBytes(str);
        }

        public static int ParseInt(this string str, int defaultValue)
        {
            int value;
            if (int.TryParse(str, out value)) return value;
            return defaultValue;
        }

        public static int ParseHexInt(this string str, int defaultValue)
        {
            if (string.IsNullOrEmpty(str)) return defaultValue;
            int value = 0;
            foreach (var ch in str)
            {
                value <<= 4;
                if (ch >= '0' && ch <= '9')
                {
                    value &= (int)(ch - '0');
                }
                else if (ch >= 'a' && ch <= 'f')
                {
                    value &= ((int)(ch - 'a') + 10);
                }
                else if (ch >= 'A' && ch <= 'F')
                {
                    value &= ((int)(ch - 'A') + 10);
                }
                else return defaultValue;
            }
            return value;
        }

    }
}
