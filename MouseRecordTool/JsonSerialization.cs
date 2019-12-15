using System;
using System.IO;
using Newtonsoft.Json;

namespace Framework.Infrastructure.Serialization
{
    /// <summary>
    /// JSON序列化器
    /// </summary>
    public static class JsonSerialization
    {
        #region 序列化

        /// <summary>
        /// 序列化对象
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <returns>执行结果</returns>
        public static string SerializeObject(object obj)
        {

            return JsonConvert.SerializeObject(obj, Formatting.Indented);
        }

        /// <summary>
        /// 序列化对象并保存到文件中
        /// </summary>
        /// <param name="obj">要序列化的对象</param>
        /// <param name="path">保存文件路径</param>
        /// <returns>执行结果</returns>
        public static bool SerializeObjectToFile(object obj, string path)
        {
            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    //如果目录不存在,则创建相应目录
                    string DirPath = Path.GetDirectoryName(path);
                    if ((!string.IsNullOrWhiteSpace(DirPath)) && (!Directory.Exists(DirPath)))
                    {
                        Directory.CreateDirectory(DirPath);
                    }

                    //序列化对象
                    string jsonString = JsonConvert.SerializeObject(obj, Formatting.Indented);
                    File.WriteAllText(path, jsonString);

                    return true;
                }
            }
            catch (Exception)
            {

            }

            return false;
        }

        #endregion

        #region 反序列化

        /// <summary>
        /// 反序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="value">反序列化字符串</param>
        /// <returns>反序列化得到的对象</returns>
        public static T DeserializeObject<T>(string value)
        {

            return JsonConvert.DeserializeObject<T>(value);
        }


        /// <summary>
        /// 从文件中反序列化对象
        /// </summary>
        /// <typeparam name="T">对象类型</typeparam>
        /// <param name="path">文件路径</param>
        /// <returns>反序列化得到的对象</returns>
        public static T DeserializeObjectFromFile<T>(string path)
        {
            try
            {
                //参数校验
                if (string.IsNullOrEmpty(path))
                {
                    return default(T);
                }

                //读取文件
                string jsonString = File.ReadAllText(path);

                return JsonConvert.DeserializeObject<T>(jsonString);
            }
            catch (Exception)
            {

            }

            return default(T);
        }

        #endregion
    }
}
