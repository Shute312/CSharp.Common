using Common.ExtensionMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    internal class DirectoryEntry
    {
        public DirectoryEntry() { }

        public DirectoryEntry(Tag tag, DataType dataType,int dataByteCount, int value, bool isBigEndian)
        {
            byte[] data;
            data = value.ToBytes(4, isBigEndian);
            Tag = tag;
            DataType = dataType;
            DataCount = dataByteCount;
            Data = data;
        }

        public DirectoryEntry(Tag tag, DataType dataType, int dataByteCount, byte[] data)
        {
            Tag = tag;
            DataType = dataType;
            DataCount = dataByteCount;
            Data = data;
        }
        public Tag Tag;
        public DataType DataType;
        public int DataCount;
        public byte[] Data;

        public byte[] ToBytes(bool isBigEndian)
        {
            List<byte> list = new List<byte>(12);
            list.AddRange(((int)Tag).ToBytes(2,isBigEndian));
            list.AddRange(((int)DataType).ToBytes(2, isBigEndian));
            list.AddRange((DataCount).ToBytes(4, isBigEndian));
            list.AddRange(Data);
            return list.ToArray();
        }
    }

}
