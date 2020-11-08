using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NitroxModel.Logger;
using NitroxModel.Server;

namespace NitroxModel.Serialization
{
    public static class PropertiesSerializer
    {
        private static readonly Dictionary<Type, Dictionary<string, MemberInfo>> typeCache = new Dictionary<Type, Dictionary<string, MemberInfo>>();

        public static T Deserialize<T>() where T : IProperties, new()
        {
            T props = new T();
            if (!File.Exists(props.FileName))
            {
                return props;
            }

            Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary<T>();

            using (StreamReader reader = new StreamReader(new FileStream(props.FileName, FileMode.Open), Encoding.UTF8))
            {
                while (!reader.EndOfStream)
                {
                    string readLine = reader.ReadLine();
                    if (readLine[0] == '#')
                    {
                        continue;
                    }

                    if (readLine.Contains('='))
                    {
                        string[] property = readLine.Split(new char[] { '=' }, 2);
                        if (!typeCachedDict.TryGetValue(property[0], out MemberInfo member))
                        {
                            Log.Error($"{property[0]} does not exist!");
                        }

                        FieldInfo field = member as FieldInfo;
                        if (field != null)
                        {
                            field.SetValue(props, TypeDescriptor.GetConverter(field.FieldType).ConvertFrom(property[1]));
                        }

                        PropertyInfo prop = member as PropertyInfo;
                        if (prop != null)
                        {
                            prop.SetValue(props, TypeDescriptor.GetConverter(prop.PropertyType).ConvertFrom(property[1]));
                        }
                    }
                    else
                    {
                        Log.Error("Incorrect format detected in config.properties!");
                    }
                }

                return props;
            }
        }

        private static Dictionary<string, MemberInfo> GetTypeCacheDictionary<T>()
        {
            if (!typeCache.TryGetValue(typeof(T), out Dictionary<string, MemberInfo> typeCachedDict))
            {
                typeCachedDict = typeof(T).GetFields()
                    .Where(f => f.Attributes != FieldAttributes.NotSerialized)
                    .Cast<MemberInfo>()
                    .Concat(typeof(T).GetProperties()
                    .Where(p => p.CanWrite).Cast<MemberInfo>())
                    .ToDictionary(n => n.Name);

                typeCache.Add(typeof(T), typeCachedDict);
            }
            return typeCachedDict;
        }

        public static void Serialize<T>(T props) where T : IProperties, new()
        {
            Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary<T>();

            using (StreamWriter stream = new StreamWriter(new FileStream(props.FileName, FileMode.OpenOrCreate), Encoding.UTF8))
            {
                stream.WriteLine("# Settings can be changed here");

                foreach (string name in typeCachedDict.Keys)
                {
                    MemberInfo member = typeCachedDict[name];

                    FieldInfo field = member as FieldInfo;
                    if (field != null)
                    {
                        PropertyDescriptionAttribute attribute = (PropertyDescriptionAttribute)field.GetCustomAttribute(typeof(PropertyDescriptionAttribute));
                        if (attribute != null)
                        {
                            stream.WriteLine($"# {attribute.Description}");
                        }
                        stream.WriteLine($"{name}={field.GetValue(props)}");
                    }

                    PropertyInfo property = member as PropertyInfo;
                    if (property != null)
                    {
                        PropertyDescriptionAttribute attribute = (PropertyDescriptionAttribute)property.GetCustomAttribute(typeof(PropertyDescriptionAttribute));
                        if (attribute != null)
                        {
                            stream.WriteLine($"# {attribute.Description}");
                        }
                        stream.WriteLine($"{name}={property.GetValue(props)}");
                    }
                }
            }
        }
    }
}
