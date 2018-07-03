using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.ExtensionMethods;

namespace Common.Images.Tiff
{
    //todo 在图像里，加入压缩的，Author:Shute,HostComputer:192.168.1.179,ImageDescription:ColorfulCode,Model:Indigo;Software:ColorfulEncoder 1.0; Address:金库
    //ref: https://tools.ietf.org/html/rfc2301#section-2.1.1
    //ref: http://www.cnprint.org/bbs/thread/75/7375/
    public class TiffDecode
    {
        const int MIN_LENGTH = 20;
        public TiffDecode(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new IOException("不存在文件");
            }
            using (var stream = File.Open(filePath, FileMode.Open))
            {
                var ifh = new ImageFileHeader(stream);
                bool isBigEndian = ifh.IsBigEndian;
                //第一个IFD位置偏移
                var nextPosition = ifh.FirstImageFileDirectoryIndex;
                ImageFileDirectionList = new List<ImageFileDirection>();
                while (true)
                {
                    if (nextPosition !=stream.Position)
                    {
                        Contract.Requires(nextPosition>stream.Position);
                        stream .Position = nextPosition;
                    }
                    var ifd = new ImageFileDirection(stream,isBigEndian);
                    ImageFileDirectionList.Add(ifd);
                    if (ifd.NextIFDIndex == 0) break;
                    nextPosition = ifd.NextIFDIndex;
                }
            }
        }

        public Bitmap GetRgbFrame(int index= 0)
        {
            return ImageFileDirectionList[index].GetRgbImage();
        }

        private List<ImageFileDirection> ImageFileDirectionList;
    }
}
