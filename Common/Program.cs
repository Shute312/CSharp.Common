using Common.Encode;
using Common.Excel;
using Common.Excel.Export;
using Common.Excel.Export.Models;
using Common.ExtensionMethods;
using Common.Images.Tiff;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            //Stream imageStream = new FileStream(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\Pantone.jpg", FileMode.Open, FileAccess.Read, FileShare.Read);
            //BitmapSource myBitmapSource = BitmapFrame.Create(imageStream);
            //FormatConvertedBitmap newFormatedBitmapSource = new FormatConvertedBitmap();
            //newFormatedBitmapSource.BeginInit();
            //newFormatedBitmapSource.Source = myBitmapSource;
            //newFormatedBitmapSource.DestinationFormat = PixelFormats.Cmyk32;
            //newFormatedBitmapSource.EndInit();
            //BitmapEncoder encoder = new TiffBitmapEncoder();
            //encoder.Frames.Add(BitmapFrame.Create(newFormatedBitmapSource));
            //Stream cmykStream = new FileStream(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\PantoneTest.tif",
            //FileMode.Create, FileAccess.Write, FileShare.Write);
            //encoder.Save(cmykStream);
            //cmykStream.Close();

            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\temp.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Cmyk\2018-7-6.tif";
            var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Cmyk\Colorspace-01.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\PantoneTest.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\Pantone.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\PantoneColorspace.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\Little.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\label_code.tif";
            //var tiffPath = @"E:\code\HiddenCode\Document\实验模式图样\Colorful\label.tif";

            BitmapTiff tiff = new BitmapTiff();
            tiff.Load(tiffPath);
            var colors = tiff.GetPixel(100,130);
            //tiff.Save(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\temp.tif");
            var bitmap = tiff.GetArgbBitmap();
            bitmap.Save(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\temp.bmp");

            //Demo.TestLayout();
            //Demo.TestEpPlus();
            //Demo.TestFormat();
        }
    }
}
