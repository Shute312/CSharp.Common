using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Images.Tiff
{
    internal enum DataType
    {
        /// <summary>
        /// 8位无标记整数
        /// </summary>
        Byte = 1,
        /// <summary>
        /// 7位ASCII码加1位二进制0
        /// </summary>
        AscII = 2,
        /// <summary>
        /// 16位无标记整数
        /// </summary>
        Short = 3,
        /// <summary>
        /// 32位无标记整数,
        /// </summary>
        Long = 4,
        /// <summary>
        /// 有理数，2个LONG，第一个是分子，第二个是分母
        /// </summary>
        Rational = 5,
        SByte = 6,
        Undefined = 7,
        SShort = 8,
        SRational = 10,
        Float = 11,
        Double = 12,
    }

    internal enum Tag
    {
        /// <summary>
        ///  每字节的数据，其位的排列方式,1 从左到右,2 从右到左
        /// </summary>
        FillOrder = 266,

        ImageWidth = 256,

        /// <summary>
        /// Height
        /// </summary>
        ImageLength = 257,

        /// <summary>
        /// 压缩方式
        /// 1 数据没有压缩,
        /// 2 采用CCITT Group 31 压缩,
        /// 3 采用CITT Group 3 Fax T4 压缩,
        /// 4 采用CITT Group 3 Fax T6 压缩,
        /// 5 LZW压缩,6 JPEG压缩,
        /// 32773 PackBits压缩)
        /// </summary>
        Compression = 259,

        //1(no Compression),2(HuffmanCompression),32773(PackBits Compression))
        /// <summary>
        /// 图像所采用的色彩系统
        /// 0 对黑白及灰度图像而言，0为白色.
        /// 1 对黑白及灰度图像而言，0为黑色
        /// 2 图像数据以R,G,B的方式存储
        /// 3 图像数据采用调色板索引
        /// 4 单色的透明混迭图像
        /// 5 图像数据以C,M,Y,K的方式存储(含 CMY)
        /// 6 图像数据以Y,Cb,Cr的方式存储
        /// 8 图像以L*,a*,b*的方式存储
        /// </summary>
        PhotometricInterpretation = 262,

        /// <summary>
        /// 这个字段用于表示每个样本值都是一个索引,在ColorMap字段中指定的一组颜色值。无损耗的彩色传真模式支持带颜色的图像ITULAB编码。SamplesPerPixel值必须为1。
        /// 0: not a palette-color image
        /// 1: palette-color image
        /// </summary>
        Indexed = 364,

        /// <summary>
        /// 数据带偏移量
        /// </summary>
        StripOffsets = 273,

        /// <summary>
        /// 数据带行数
        /// </summary>
        RowsPerStrip = 278,

        /// <summary>
        /// 数据带字节数
        /// </summary>
        StripByteCounts = 279,

        /// <summary>
        /// 单位长度上的图像水平分辨率
        /// </summary>
        XResolution = 282,

        /// <summary>
        /// 单位长度上的图像垂直分辨率
        /// </summary>
        YResolution = 283,

        /// <summary>
        /// 用于非四边形显示的图像),2(英寸),3(厘米)根据ImageWidth及ImageLength用于计算图像在显示时的尺寸
        /// 1 没有指定单位
        /// 2 英寸为单位
        /// 3 厘米为单位
        /// </summary>
        ResolutionUnit = 296,

        /// <summary>
        /// 样点位数
        /// 1: binary mask   
        /// 8: 8 bits per color sample
        /// 9-16: optional 12 bits/sample
        /// </summary>
        BitsPerSample = 258,
        /// <summary>
        /// Used when Foreground or Background layer is a palette-color image
        ///  3 * (2**BitsPerSample)
        /// </summary>
        ColorMap = 320,
        /// <summary>
        /// 像素样点数
        /// </summary>
        SamplesPerPixel = 277,

        /// <summary>
        ///  每个像素的额外组成(0 未指定数据,1 与Alpha通道有关,2 与Alpha通道无关)
        /// </summary>
        ExtraSamples = 338,

        /// <summary>
        /// 创建日期(AscII)
        /// image creation in 24-hour format "YYYY:MM:DD HH:MM:SS".
        /// </summary>
        CreateTime = 306,

        /// <summary>
        /// 每个未使用块在文件中的字节数
        /// </summary>
        FreeByteCounts = 289,

        /// <summary>
        /// 每个未使用块在文件中的偏移量
        /// </summary>
        FreeOffsets = 288,

        /// <summary>
        /// 灰度响应曲线(2**BitsPerSample)
        /// </summary>
        GrayResponseCurve = 291,

        /// <summary>
        /// 灰度响应曲线的单位(1表示1/10,2表示1/100,3表示1/1000,4表示1/10000,5表示1/100000,默认值2)
        /// </summary>
        GrayResponseUnit = 290,

        /// <summary>
        /// (AscII类型)
        /// </summary>
        Make = 271,

        /// <summary>
        /// 最大取样值(Default:(2**BitsPerSample)-1)
        /// </summary>
        MaxSampleValue = 281,

        /// <summary>
        /// 最小取样值(Default:0)
        /// </summary>
        MinSampleValue = 280,

        /// <summary>
        /// 扫描仪的型号(AscII类型)
        /// </summary>
        Model = 272,

        /// <summary>
        /// 图像形态(1 表示一幅全分辨率的图像,2 表示一幅降低分辨率后的图像,3 表示一幅多页图像中的一页,4 表示一幅透明混迭(Transparency mask)图像)
        /// </summary>
        NewSubfileType = 254,

        /// <summary>
        /// SubfileType 已经由NewSubfileType标签取代
        /// (Long类型)
        /// </summary>
        SubfileType = 255,

        /// <summary>
        /// 定义由非黑白图像变换至黑白图像的技术
        /// 1 未采用任何技术
        /// 2 采用抖色(dither)或半色调(halftone)的技术
        /// 3 采用误差扩散(error diffusion)技术
        /// </summary>
        Threashholding = 263,

        /// <summary>
        /// 像行列的编排方向
        /// 1表示1行在上方，1列在左方；
        /// 2表示1行在上方，1列在右方；
        /// 3表示1行在下方，1列在右方；
        /// 4表示1行在下方，1列在左方；
        /// 5表示1行在左方，1列在上方；
        /// 6表示1行在右方，1列在上方；
        /// 7表示1行在右方，1列在下方；
        /// 8表示1行在左方，1列在下方
        /// Default:1,
        /// </summary>
        Orientation = 274,
        /// <summary>
        /// 图像数据的平面排列方式
        ///  1 单平面格式
        ///  2 多平面格式
        /// </summary>
        PlanarConfiguration = 284,

        /// <summary>
        /// 第一个数字表示页码（第一个页面的0）;
        /// 第二个数字是文档的总页数。如果第二个值是0，那么总页数是不可用的。
        /// </summary>
        PageNumber = 297,

        /// <summary>
        /// An IFD containing global parameters. It is recommended that a TIFF writer place this field in the first IFD, where a TIFF reader would find it quickly.
        /// </summary>
        GlobalParametersIFD = 400,

        /// <summary>
        /// he profile that applies to this file; a profile is subset of the full set of permitted fields and field values of TIFF for facsimile. The currently defined values are:
        /// 0: does not conform to a profile defined for TIFF for facsimile
        /// 1: minimal black & white lossless, Profile S
        /// 2: extended black & white lossless, Profile F
        /// 3: lossless JBIG black & white, Profile J
        /// 4: lossy color and grayscale, Profile C
        /// 5: lossless color and grayscale, Profile L
        /// 6: Mixed Raster Content, Profile M
        /// (Byte类型)
        /// </summary>
        FaxProfile = 402,

        /// <summary>
        /// This field indicates which coding methods are used in the file. A bit value of 1 indicates which of the following coding methods is used:
        /// Bit 0: unspecified compression,
        /// Bit 1: 1-dimensional coding, ITU-T Rec. T.4 (MH - Modified Huffman),
        /// Bit 2: 2-dimensional coding, ITU-T Rec. T.4 (MR - Modified Read),
        /// Bit 3: 2-dimensional coding, ITU-T Rec. T.6 (MMR - Modified MR),
        /// Bit 4: ITU-T Rec. T.82 coding, using ITU-T Rec.T.85 (JBIG),
        /// Bit 5: ITU-T Rec. T.81 (Baseline JPEG),
        /// Bit 6: ITU-T Rec. T.82 coding, using ITU-T Rec.T.43 (JBIG color),
        /// Bits 7-31: reserved for future use
        /// （LONG类型）
        /// </summary>
        CodingMethods = 403,

        /// <summary>
        /// The year of the standard specified by the FaxProfile field, given as 4 characters, e.g. '1997'; used in lossy and lossless color modes.
        /// (Byte类型)
        /// </summary>
        VersionYear = 404,

        /// <summary>
        /// The mode of the standard specified by the FaxProfile field. Avalue of 0 indicates Mode 1.0; used in Mixed Raster Content mode.
        /// (Byte类型)
        /// </summary>
        ModeNumber = 405,

        /// <summary>
        /// XML packet containing XMP metadata
        /// </summary>
        XMP = 700,

        /// <summary>
        /// 网点范围
        /// (0％和100％网点对应的颜色值
        /// (Byte\Short类型)
        /// </summary>
        DotRange = 336,

        /// <summary>
        /// A mathematical operator that is applied to the image data before an encoding scheme is applied.
        /// </summary>
        Predictor = 317,

        #region Named by Shute

        /// <summary>
        /// ICC 特性文件
        /// 分色、RGB或其他图像的颜色值还可以通过ICC特性文件进一步描述。如果用到ICC特性文件，就应该使用ICC特性文件标签
        /// </summary>
        InterColorProfile = 34675,

        /// <summary>
        /// 颜色特征描述
        /// 分色、RGB或其他图像的颜色值可以通过1SO 12641、ISO 12642或命名色空间(如IEC 61966 2l 定义的sRGB)中说明的数据表格进一步描述。
        /// 通过使用颜色特征描述字段，这些ASCII数据表格嵌入到它们所描述的图像文件中
        /// Specifies ASCII table or other reference per ISO 12641 and ISO 12642.
        /// </summary>
        ColorCharacterization = 34029,

        /// <summary>
        /// 色表
        /// 在彩色线条图像中，颜色值在色表中进行编码。色表是TIFF／IT中ColorTable字段的值
        /// Color value in a color pallette.
        /// </summary>
        ColorTable = 34022,

        /// <summary>
        /// 图像颜色指示
        /// Indicates if image (foreground) color or transparency is specified.
        /// </summary>
        ImageColorIndicator = 34023,
        /// <summary>
        /// 背景颜色指示
        /// </summary>
        BackgroundColorIndicator = 34024,

        /// <summary>
        /// 图像颜色值
        /// Specifies image (foreground) color.
        /// </summary>
        ImageColorValue = 34025,
        /// <summary>
        /// 背景颜色值
        /// </summary>
        BackgroundColorValue = 34026,

        /// <summary>
        /// Specifies data values for 0 percent and 100 percent pixel intensity.
        /// </summary>
        PixelIntensityRange = 34027,

        /// <summary>
        /// 透明指示
        /// Specifies if transparency is used in HC file.
        /// </summary>
        TransparencyIndicator = 34028,

        /// <summary>
        /// Specifies CMYK equivalent for specific separations.
        /// </summary>
        CMYKEquivalent = 34032,

        /// <summary>
        /// A pointer to the Exif IFD.
        /// </summary>
        ExifIFD = 34665,

        /// <summary>
        /// 图像相对于原点在x方向的偏移量
        /// </summary>
        XOffset = 286,

        /// <summary>
        /// 图像相对于原点在y方向的偏移量
        /// </summary>
        YOffset = 287,

        /// <summary>
        /// 色序
        /// </summary>
        ColorSequence = 34017,

        /// <summary>
        /// 油墨组
        /// 1表示采用CMYK色序
        /// 字段值2表示除值1外的其他油墨组或色序(也就是说非CMYK色序的油墨组)。
        /// 油墨数量字段值说明油墨或分色的数量。通常等于像素样点数字段值，但也可能与色序字段中说明的颜色数相同。
        /// </summary>
        InkGroup = 332,

        /// <summary>
        /// 油墨数量
        /// </summary>
        InkNumber = 334,
        #endregion


        #region NoImportant
        /// <summary>
        /// 作者名称(AscII类型)
        /// </summary>
        Author = 315,

        /// <summary>(AscII类型)
        /// 版权信息
        /// </summary>
        CopyRight = 33432,

        /// <summary>
        /// 制作此图像的计算机及其操作系统(AscII类型)
        /// </summary>
        HostComputer = 312,

        /// <summary>
        /// 地理位置，在哪里创建图像的
        /// </summary>
        Address = 34016,


        /// <summary>
        /// 图形说明(AscII类型)
        /// </summary>
        ImageDescription = 270,

        /// <summary>
        /// 生成图像的软件名称及版本号(AscII类型)
        /// </summary>
        Software = 305,

        /// <summary>
        /// The name of the scanned document.
        /// </summary>
        DocumentName = 269,
        #endregion
    }

    public enum Unit
    {
        Unknown = 1,
        /// <summary>
        /// 英寸
        /// </summary>
        Inch = 2,
        /// <summary>
        /// 厘米
        /// </summary>
        Centimetre = 3,
    }

    public enum Compression
    {
        No = 1,
        CittGroup31 = 2,
        CittGroup3FaxT4 = 3,
        CittGroup3FaxT6 = 4,
        LZW = 5,
        JPEG = 6
    }

    public enum ColorType
    {
        /// <summary>
        /// 对黑白及灰度图像而言，0为白色.
        /// </summary>
        WhiteBlack = 0,
        /// <summary>
        /// 对黑白及灰度图像而言，0为黑色
        /// </summary>
        BlackWhite = 1,
        /// <summary>
        /// 图像数据以R,G,B的方式存储
        /// </summary>
        Rgb=2,
        /// <summary>
        ///  图像数据采用调色板索引
        /// </summary>
        Index = 3,
        /// <summary>
        /// 单色的透明混迭图像
        /// </summary>
        Cross = 4,
        /// <summary>
        /// 图像数据以C,M,Y,K的方式存储(含 CMY)
        /// </summary>
        Cmyk = 5,
        /// <summary>
        /// 图像数据以Y,Cb,Cr的方式存储
        /// </summary>
        YCbCr =6,
        /// <summary>
        /// 图像以L*,a*,b*的方式存储
        /// </summary>
        Lab = 8,
        ITULAB = 10,
    }

}
