using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    public class DirectoryEntry
    {
        public Tag Tag;
        public DataType DataType;
        public int DataByteCount;
        public byte[] Data;
    }

}
