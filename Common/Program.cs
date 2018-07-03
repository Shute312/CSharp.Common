using Common.Excel;
using Common.Excel.Export;
using Common.Excel.Export.Models;
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
            //TiffDecode decode = new TiffDecode(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\Pantone.tif");
            TiffDecode decode = new TiffDecode(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\label_code.tif");
            //TiffDecode decode = new TiffDecode(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\未标题-2.tif");
            //TiffDecode decode = new TiffDecode(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\Little.tif");
            var bitmap = decode.GetRgbFrame();
            bitmap.Save(@"E:\code\HiddenCode\Document\实验模式图样\Colorful\Temp.jpg");

            //Demo.TestLayout();
            //Demo.TestEpPlus();
            //Demo.TestFormat();
        }

    }
}
