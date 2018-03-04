using System.IO;
using Common.ExtensionMethods;
using System.Web.Script.Serialization;
using System;

namespace Common.Serialization
{
    public class Json
    {
        //todo 修改为不抛出异常，失败时返回null

        public T Deserialize<T>(Stream stream)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            T obj = serializer.Deserialize<T>(stream.ReadAll().ToString());
            return obj;
        }
        public T Deserialize<T>(string str)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            T obj = serializer.Deserialize<T>(str);
            return obj;
        }
        public string Serialize<T>(T obj)
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            string result = serializer.Serialize(obj);
            return result;
        }
        public bool SerializeToFile<T>(T obj, string filePath)
        {
            var json = Serialize<T>(obj);
            var stream = json.ToBytes().ToStream();
            return IO.FileUtils.Write(filePath, stream);
        }
        public T DeserializeFromFile<T>(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}
