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
    internal class TiffDecode
    {
        public TiffDecode()
        {
            ImageFileDirectionList = new List<ImageFileDirection>();
        }
        public bool IsBigEndian { get; private set; }

        public int FrameCount { get { return ImageFileDirectionList.Count; } }

        private List<ImageFileDirection> ImageFileDirectionList;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="quickly">为true时，快速加载，只解析每个帧的头信息，而不解析所有内容(第一帧为完整加载)</param>
        /// <returns></returns>
        public bool Load(Stream stream, bool quickly = false)
        {
            var ifh = new ImageFileHeader();
            if (!ifh.Load(stream)) return false;

            IsBigEndian = ifh.IsBigEndian;
            //第一个IFD位置偏移
            var nextPosition = ifh.FirstFramePosition;
            while (true)
            {
                if (nextPosition != stream.Position)
                {
                    Contract.Requires(nextPosition > stream.Position);
                    stream.Position = nextPosition;
                }
                var ifd = new ImageFileDirection();
                if (quickly)
                {
                    ifd.LoadHeader(stream, IsBigEndian);
                }
                else
                {
                    ifd.Load(stream, IsBigEndian);
                }
                ImageFileDirectionList.Add(ifd);
                if (ifd.NextIfdIndex == 0) break;
                nextPosition = ifd.NextIfdIndex;
            }
            Contract.Requires(ImageFileDirectionList.Count > 0);
            return true;
        }

        /// <summary>
        /// 获得某帧的内容
        /// </summary>
        /// <param name="stream">如果启用快速模式，则必须传入stream</param>
        /// <param name="index"></param>
        /// <returns></returns>
        public ImageFileDirection GetFrame(int index, Stream stream = null)
        {
            var frame = ImageFileDirectionList[index];
            frame.LoadContent(stream, IsBigEndian);
            return frame;
        }

        public bool Load(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new IOException("不存在文件");
            }
            using (var stream = File.Open(filePath, FileMode.Open))
            {
                return Load(stream);
            }
        }
    }
}
