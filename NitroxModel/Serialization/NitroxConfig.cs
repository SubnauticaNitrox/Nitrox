using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxModel.Serialization
{
    public abstract class NitroxConfig<T> where T : NitroxConfig<T>
    {
        // ReSharper disable once StaticMemberInGenericType
        private static readonly Dictionary<string, MemberInfo> typeCache = new();
        private readonly object locker = new();
        private readonly char[] newlineChars = Environment.NewLine.ToCharArray();

        public abstract string FileName { get; }

        public void Deserialize(string saveDir)
        {
            if (!File.Exists(Path.Combine(saveDir, FileName)))
            {
                return;
            }

            lock (locker)
            {
                Type type = GetType();
                Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary();
                using StreamReader reader = new(new FileStream(Path.Combine(saveDir, FileName), FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);

                HashSet<MemberInfo> unserializedMembers = new(typeCachedDict.Values);
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
                        // Ignore case for property names in file.
                        if (!typeCachedDict.TryGetValue(keyValuePair[0].ToLowerInvariant(), out MemberInfo member))
                        {
                            Log.Warn($"Property or field {keyValuePair[0]} does not exist on type {type.FullName}!");
                            continue;
                        }

                        unserializedMembers.Remove(member); // This member was serialized in the file 

                        if (!SetMemberValue(this, member, keyValuePair[1]))
                        {
                            (Type type, object value) data = member switch
                            {
                                FieldInfo field => (field.FieldType, field.GetValue(this)),
                                PropertyInfo prop => (prop.PropertyType, prop.GetValue(this)),
                                _ => (typeof(string), "")
                            };
                            Log.Warn($@"Property ""({data.type.Name}) {member.Name}"" has an invalid value {StringifyValue(keyValuePair[1])} on line {lineNum}. Using default value: {StringifyValue(data.value)}");
                        }
                    }
                    else
                    {
                        Log.Error($"Incorrect format detected on line {lineNum} in {Path.GetFullPath(Path.Combine(saveDir, FileName))}:{Environment.NewLine}{readLine}");
                    }
                }

                if (unserializedMembers.Any())
                {
                    IEnumerable<string> unserializedProps = unserializedMembers.Select(m =>
                    {
                        object value = null;
                        if (m is FieldInfo field)
                        {
                            value = field.GetValue(this);
                        }
                        else if (m is PropertyInfo prop)
                        {
                            value = prop.GetValue(this);
                        }

                        return $" - {m.Name}: {value}";
                    });

                    Log.Warn($@"{FileName} is using default values for the missing properties:{Environment.NewLine}{string.Join(Environment.NewLine, unserializedProps)}");
                }
            }
        }

        public void Serialize(string saveDir)
        {
            lock (locker)
            {
                Type type = GetType();
                Dictionary<string, MemberInfo> typeCachedDict = GetTypeCacheDictionary();
                try
                {
                    using StreamWriter stream = new(new FileStream(Path.Combine(saveDir, FileName), FileMode.Create, FileAccess.Write), Encoding.UTF8);
                    WritePropertyDescription(type, stream);

                    foreach (string name in typeCachedDict.Keys)
                    {
                        MemberInfo member = typeCachedDict[name];

                        FieldInfo field = member as FieldInfo;
                        if (field != null)
                        {
                            WritePropertyDescription(member, stream);
                            WriteProperty(field, field.GetValue(this), stream);
                        }

                        PropertyInfo property = member as PropertyInfo;
                        if (property != null)
                        {
                            WritePropertyDescription(member, stream);
                            WriteProperty(property, property.GetValue(this), stream);
                        }
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    Log.Error($"Config file {FileName} exists but is a hidden file and cannot be modified, config file will not be updated. Please make file accessible");
                }
            }
        }

        /// <summary>
        ///     Ensures updates are properly persisted to the backing config file without overwriting user edits.
        /// </summary>
        /// <param name="config"></param>
        public void Update(string saveDir, Action<T> config = null)
        {
            try
            {
                Deserialize(saveDir);
                config?.Invoke(this as T);
            }
            finally
            {
                Serialize(saveDir);
            }
        }

        private static Dictionary<string, MemberInfo> GetTypeCacheDictionary()
        {
            Type type = typeof(T);
            if (!typeCache.Any())
            {
                IEnumerable<MemberInfo> members = type.GetFields()
                                                      .Where(f => f.Attributes != FieldAttributes.NotSerialized)
                                                      .Concat(type.GetProperties()
                                                                  .Where(p => p.CanWrite)
                                                                  .Cast<MemberInfo>());

                try
                {
                    foreach (MemberInfo member in members)
                    {
                        typeCache.Add(member.Name.ToLowerInvariant(), member);
                    }
                }
                catch (ArgumentException e)
                {
                    Log.Error(e, $"Type {type.FullName} has properties that require case-sensitivity to be unique which is unsuitable for .properties format.");
                    throw;
                }
            }

            return typeCache;
        }

        private string StringifyValue(object value)
        {
            return value switch
            {
                string _ => $@"""{value}""",
                null => @"""""",
                _ => value.ToString()
            };
        }

        private bool SetMemberValue(NitroxConfig<T> instance, MemberInfo member, string valueFromFile)
        {
            object ConvertFromStringOrDefault(Type typeOfValue, out bool isDefault, object defaultValue = default)
            {
                try
                {
                    object newValue = TypeDescriptor.GetConverter(typeOfValue).ConvertFrom(valueFromFile);
                    isDefault = false;
                    return newValue;
                }
                catch (Exception)
                {
                    isDefault = true;
                    return defaultValue;
                }
            }

            bool usedDefault;
            switch (member)
            {
                case FieldInfo field:
                    field.SetValue(instance, ConvertFromStringOrDefault(field.FieldType, out usedDefault, field.GetValue(instance)));
                    return !usedDefault;
                case PropertyInfo prop:
                    prop.SetValue(instance, ConvertFromStringOrDefault(prop.PropertyType, out usedDefault, prop.GetValue(instance)));
                    return !usedDefault;
                default:
                    throw new Exception($"Serialized member must be field or property: {member}.");
            }
        }

        private void WriteProperty<TMember>(TMember member, object value, StreamWriter stream) where TMember : MemberInfo
        {
            stream.Write(member.Name);
            stream.Write('=');
            stream.WriteLine(value);
        }

        private void WritePropertyDescription(MemberInfo member, StreamWriter stream)
        {
            PropertyDescriptionAttribute attribute = member.GetCustomAttribute<PropertyDescriptionAttribute>();
            if (attribute != null)
            {
                foreach (string line in attribute.Description.Split(newlineChars))
                {
                    stream.Write("# ");
                    stream.WriteLine(line);
                }
            }
        }
    }
}
