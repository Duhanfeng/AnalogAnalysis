using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace AnalogSignalAnalysisWpf
{

    public class EnumHelper
    {
        public static string GetDescription(object enumObj)
        {
            string description = "";

            try
            {
                if (Enum.IsDefined(enumObj.GetType(), enumObj))
                {
                    description = (Attribute.GetCustomAttribute(enumObj.GetType().GetField(enumObj.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
                }
            }
            catch (Exception)
            {

            }

            return description;
        }

        public static T GetEnum<T>(string description)
        {
            try
            {
                if (!string.IsNullOrEmpty(description))
                {
                    foreach (var item in Enum.GetValues(typeof(T)))
                    {
                        if ((Attribute.GetCustomAttribute(item.GetType().GetField(item.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description?.Equals(description) == true)
                        {
                            return (T)item;
                        }
                    }
                }
            }
            catch (Exception)
            {

            }

            return default(T);
        }

        /// <summary>
        /// 获取所有的描述
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<string> GetAllDescriptions<T>()
        {
            List<string> descriptions = new List<string>();

            try
            {
                foreach (var item in Enum.GetValues(typeof(T)))
                {
                    string description = (Attribute.GetCustomAttribute(item.GetType().GetField(item.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute)?.Description;
                    if (!string.IsNullOrEmpty(description))
                    {
                        descriptions.Add(description);
                    }
                }

            }
            catch (Exception)
            {

            }

            return descriptions;
        }
    }

}
