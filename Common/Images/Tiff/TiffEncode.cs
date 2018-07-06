using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    internal class TiffEncode
    {
        public TiffEncode()
        {
            frameList = new List<ImageFileDirection>();
        }

        private List<ImageFileDirection> frameList;

        public bool AddFrame(ImageFileDirection frame)
        {
            Contract.Assert(frame != null);
            ;
            if (frameList.Contains(frame))
            {
                return false;
            }
            frameList.Add(frame);
            return true;
        }

        public bool Save(Stream stream)
        {
            Contract.Assert(stream != null && stream.CanWrite && stream.CanSeek && stream.Position == 0);
            ImageFileHeader ifh = new ImageFileHeader();
            var nextIfdIndex = 8;
            bool isBigEndian = false;
            ifh.Save(stream, isBigEndian, nextIfdIndex);
            for(var i=0;i<frameList.Count;i++)
            {
                var frame = frameList[i];
                var isLastOne = i == frameList.Count - 1;
                frame.Save(stream, isBigEndian,isLastOne);
            }
            return true;
        }
    }
}
