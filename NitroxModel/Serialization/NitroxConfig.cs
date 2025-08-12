using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace NitroxModel.Serialization;

public abstract class NitroxConfig<T> where T : NitroxConfig<T>, new()
{
    private static readonly Dictionary<string, object> unserializedMembersWarnOnceCache = [];
    private static readonly Dictionary<string, MemberInfo> typeCache = [];

    private readonly char[] newlineChars = Environment.NewLine.ToCharArray();
    private readonly object locker = new();

    public abstract string FileName { get; }

    public static T Load(string saveDir)
    {
        T config = new();
        config.Deserialize(saveDir);
        return config;
    }

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

                    if (!NitroxConfig<T>.SetMemberValue(this, member, keyValuePair[1]))
                    {
                        (Type type, object value) logData = member switch
                        {
                            FieldInfo field => (field.FieldType, field.GetValue(this)),
                            PropertyInfo prop => (prop.PropertyType, prop.GetValue(this)),
                            _ => (typeof(string), "")
                        };
                        Log.Warn($@"Property ""({logData.type.Name}) {member.Name}"" has an invalid value {NitroxConfig<T>.StringifyValue(keyValuePair[1])} on line {lineNum}. Using default value: {NitroxConfig<T>.StringifyValue(logData.value)}");
                    }
                }
                else
                {
                    Log.Error($"Incorrect format detected on line {lineNum} in {Path.GetFullPath(Path.Combine(saveDir, FileName))}:{Environment.NewLine}{readLine}");
                }
            }

            if (unserializedMembers.Count != 0)
            {
                string[] unserializedProps = unserializedMembers
                                             .Select(m =>
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

                                                 if (unserializedMembersWarnOnceCache.TryGetValue(m.Name, out object cachedValue))
                                                 {
                                                     if (Equals(value, cachedValue))
                                                     {
                                                         return null;
                                                     }
                                                 }
                                                 unserializedMembersWarnOnceCache[m.Name] = value;
                                                 return $" - {m.Name}: {value}";
                                             })
                                             .Where(i => i != null)
                                             .ToArray();
                if (unserializedProps.Length > 0)
                {
                    Log.Warn($"{FileName} is using default values for the missing properties:{Environment.NewLine}{string.Join(Environment.NewLine, unserializedProps)}");
                }
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
                Directory.CreateDirectory(saveDir);
                using StreamWriter stream = new(new FileStream(Path.Combine(saveDir, FileName), FileMode.Create, FileAccess.Write), Encoding.UTF8);
                WritePropertyDescription(type, stream);

                foreach (string name in typeCachedDict.Keys)
                {
                    MemberInfo member = typeCachedDict[name];

                    FieldInfo field = member as FieldInfo;
                    if (field != null)
                    {
                        WritePropertyDescription(member, stream);
                        NitroxConfig<T>.WriteProperty(field, field.GetValue(this), stream);
                    }

                    PropertyInfo property = member as PropertyInfo;
                    if (property != null)
                    {
                        WritePropertyDescription(member, stream);
                        NitroxConfig<T>.WriteProperty(property, property.GetValue(this), stream);
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
    public UpdateDiposable Update(string saveDir)
    {
        return new UpdateDiposable(this, saveDir);
    }

    private static Dictionary<string, MemberInfo> GetTypeCacheDictionary()
    {
        Type type = typeof(T);
        if (typeCache.Count == 0)
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

    private static string StringifyValue(object value) => value switch
    {
        string _ => $@"""{value}""",
        null => @"""""",
        _ => value.ToString()
    };

    private static bool SetMemberValue(NitroxConfig<T> instance, MemberInfo member, string valueFromFile)
    {
        object ConvertFromStringOrDefault(Type typeOfValue, out bool isDefault, object defaultValue = default)
        {
            try
            {
                object newValue = TypeDescriptor.GetConverter(typeOfValue).ConvertFrom(null!, CultureInfo.InvariantCulture, valueFromFile);
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

    private static void WriteProperty<TMember>(TMember member, object value, StreamWriter stream) where TMember : MemberInfo
    {
        stream.Write(member.Name);
        stream.Write('=');
        stream.WriteLine(Convert.ToString(value, CultureInfo.InvariantCulture));
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

    public readonly struct UpdateDiposable : IDisposable
    {
        private string SaveDir { get; }
        private NitroxConfig<T> Config { get; }

        public UpdateDiposable(NitroxConfig<T> config, string saveDir)
        {
            config.Deserialize(saveDir);
            SaveDir = saveDir;
            Config = config;
        }

        public void Dispose() => Config.Serialize(SaveDir);
    }
}
