using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NitroxModel.Logger;

namespace NitroxModel.Serialization
{
    public static class PropertiesWriter
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
            using StreamReader reader = new StreamReader(new FileStream(props.FileName, FileMode.Open), Encoding.UTF8);

            char[] lineSeparator = { '=' };
            int lineNum = 0;
            string readLine;
            while ((readLine = reader.ReadLine()) != null)
            {
                lineNum++;
                if (readLine.Length < 1 || readLine[0] == '#')
                {
                    continue;
                }

                if (readLine.Contains('='))
                {
                    string[] keyValuePair = readLine.Split(lineSeparator, 2);
                    keyValuePair[0] = keyValuePair[0].ToLowerInvariant(); // Ignore case for property names in file.
                    if (!typeCachedDict.TryGetValue(keyValuePair[0], out MemberInfo member))
                    {
                        Log.Warn($"Property {keyValuePair[0]} does not exist on type {typeof(T).FullName}!");
                        continue;
                    }

                    FieldInfo field = member as FieldInfo;
                    if (field != null)
                    {
                        field.SetValue(props, TypeDescriptor.GetConverter(field.FieldType).ConvertFrom(keyValuePair[1]));
                    }
                    PropertyInfo prop = member as PropertyInfo;
                    if (prop != null)
                    {
                        prop.SetValue(props, TypeDescriptor.GetConverter(prop.PropertyType).ConvertFrom(keyValuePair[1]));
                    }
                }
                else
                {
                    Log.Error($"Incorrect format detected on line {lineNum} in {Path.GetFullPath(props.FileName)}:{Environment.NewLine}{readLine}");
                }
            }

            return props;
        }

        public static void Serialize<T>(T props) where T : IProperties, new()
        {
            Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary<T>();

            using StreamWriter stream = new StreamWriter(new FileStream(props.FileName, FileMode.OpenOrCreate), Encoding.UTF8);
            WritePropertyDescription(typeof(T), stream);

            foreach (string name in typeCachedDict.Keys)
            {
                MemberInfo member = typeCachedDict[name];

                FieldInfo field = member as FieldInfo;
                if (field != null)
                {
                    WritePropertyDescription(member, stream);
                    WriteProperty(field, field.GetValue(props), stream);
                }

                PropertyInfo property = member as PropertyInfo;
                if (property != null)
                {
                    WritePropertyDescription(member, stream);
                    WriteProperty(property, property.GetValue(props), stream);
                }
            }
        }

        private static Dictionary<string, MemberInfo> GetTypeCacheDictionary<T>()
        {
            if (!typeCache.TryGetValue(typeof(T), out Dictionary<string, MemberInfo> typeCachedDict))
            {
                IEnumerable<MemberInfo> members = typeof(T).GetFields()
                                                           .Where(f => f.Attributes != FieldAttributes.NotSerialized)
                                                           .Concat(typeof(T).GetProperties()
                                                                            .Where(p => p.CanWrite)
                                                                            .Cast<MemberInfo>());

                try
                {
                    typeCachedDict = new Dictionary<string, MemberInfo>();
                    foreach (MemberInfo member in members)
                    {
                        typeCachedDict.Add(member.Name.ToLowerInvariant(), member);
                    }
                }
                catch (ArgumentException e)
                {
                    Log.Error(e, $"Type {typeof(T).FullName} has properties that require case-sensitivity to be unique which is unsuitable for .properties format.");
                    throw;
                }

                typeCache.Add(typeof(T), typeCachedDict);
            }
            return typeCachedDict;
        }

        private static void WriteProperty<T>(T member, object value, StreamWriter stream) where T : MemberInfo
        {
            stream.Write(member.Name);
            stream.Write("=");
            stream.WriteLine(value);
        }

        private static void WritePropertyDescription(MemberInfo member, StreamWriter stream)
        {
            PropertyDescriptionAttribute attribute = member.GetCustomAttribute<PropertyDescriptionAttribute>();
            if (attribute != null)
            {
                foreach (string line in attribute.Description.Split(Environment.NewLine.ToCharArray()))
                {
                    stream.Write("# ");
                    stream.WriteLine(line);
                }
            }
        }
    }
}
