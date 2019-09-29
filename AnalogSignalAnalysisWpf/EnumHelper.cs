using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalogSignalAnalysisWpf
{

    public class EnumHelper
    {
        public static string GetDescription(object enumObj)
        {
            return (Attribute.GetCustomAttribute(enumObj.GetType().GetField(enumObj.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description;
        }

        public static T GetEnum<T>(string description)
        {
            if (!string.IsNullOrEmpty(description))
            {
                foreach (var item in Enum.GetValues(typeof(T)))
                {
                    if ((Attribute.GetCustomAttribute(item.GetType().GetField(item.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description.Equals(description))
                    {
                        return (T)item;
                    }
                }
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

            foreach (var item in Enum.GetValues(typeof(T)))
            {
                string description = (Attribute.GetCustomAttribute(item.GetType().GetField(item.ToString()), typeof(DescriptionAttribute)) as DescriptionAttribute).Description;
                descriptions.Add(description);
            }

            return descriptions;
        }
    }

}
