using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.IO
{
    public static class FileUtils
    {
        /// <summary>
        /// 保存文件，写入失败时，不会影响原有文件
        /// </summary>
        /// <returns></returns>
        public static bool Write(string filePath,Stream stream)
        {
            Contract.Assert(!string.IsNullOrEmpty(filePath));
            if (File.Exists(filePath))
            {
                var tempFilePath = GetUniqueName(filePath);
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

        public static string GetUniqueName(string filePath)
        {
            var fileName = Path.GetFileName(filePath);
            var dir = Path.GetDirectoryName(filePath);
            return GetUniqueName(dir, fileName);
        }

        public static string GetUniqueName(string dir,string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) throw new ArgumentException();
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
