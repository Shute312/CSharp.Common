using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.IO
{
    public static class FileUtils
    {
        /// <summary>
        /// 是否网络路径
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="defaultWeb">无法识别的相对路径，默认为本地或者网络路径</param>
        /// <returns></returns>
        public static bool IsWebPath(string filePath, bool defaultWeb = false)
        {
            Contract.Assert(!string.IsNullOrEmpty(filePath));
            //是否带有协议头
            if (filePath.IndexOf("://") > -1) return true;
            //是否是.net服务器路径
            if (filePath.StartsWith("~/")) return true;
            //匹配域名
            Regex reg = new Regex(@"^[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+$");
            if (reg.IsMatch(filePath)) return true;
            //本地路径
            if (filePath.IndexOf(":\\") < 2) return false;
            //其他情况
            return defaultWeb;
        }
        /// <summary>
        /// 保存文件，写入失败时，不会影响原有文件
        /// </summary>
        /// <returns></returns>
        public static bool Write(string filePath, Stream stream)
        {
            Contract.Assert(!string.IsNullOrEmpty(filePath));
            if (File.Exists(filePath))
            {
                var tempFilePath = GetUniqueName(filePath);
                try
                {
                    var output = File.Create(tempFilePath);
                    int length = (int)(stream.Length - stream.Position);
                    if (StreamEx.Write(stream, output) == length)
                    {
                        if (ExchangeFileName(tempFilePath, filePath))
                        {
                            File.Delete(tempFilePath);
                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                }
                return false;
            }
            else
            {
                try
                {
                    var output = File.Create(filePath);
                    int length = (int)(stream.Length - stream.Position);
                    return StreamEx.Write(stream, output) == length;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// 获得唯一名称(全路径)，用于同名文件不覆盖
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static string GetUniqueName(string filePath)
        {
            Contract.Assert(!string.IsNullOrEmpty(filePath));
            var fileName = Path.GetFileName(filePath);
            var dir = Path.GetDirectoryName(filePath);
            return GetUniqueName(dir, fileName);
        }

        /// <summary>
        /// 获得唯一名称(全路径)，用于同名文件不覆盖
        /// </summary>
        /// <param name="dir"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetUniqueName(string dir,string fileName)
        {
            Contract.Assert(!string.IsNullOrEmpty(dir));
            Contract.Assert(!string.IsNullOrEmpty(fileName));
            var name = Path.GetFileNameWithoutExtension(fileName);
            var exName = Path.GetExtension(fileName);
            int number = 1;
            string newFilePath = null;
            while (true)
            {
                string newFileName;
                if(string.IsNullOrEmpty(exName))
                    newFileName = string.Format("{0}({1})",name,number);
                else
                    newFileName = string.Format("{0}({1}){2}", name, number,exName);

                newFilePath = Path.Combine(dir,newFileName);

                if (!File.Exists(newFileName)) break;
                number++;
            }
            return newFilePath;
        }

        /// <summary>
        /// 交换文件名
        /// </summary>
        /// <param name="filePath1"></param>
        /// <param name="filePath2"></param>
        /// <returns></returns>
        public static bool ExchangeFileName(string filePath1, string filePath2)
        {
            try
            {
                string filePath3 = GetUniqueName(filePath1);
                File.Move(filePath1, filePath3);
                File.Move(filePath2, filePath1);
                File.Move(filePath3, filePath2);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
