using System;
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
        public static T Deserialize<T>() where T : IProperties, new()
        {
            T props = new T();
            if (!File.Exists(props.FileName))
            {
                return props;
            }
            FieldInfo[] fields = typeof(T).GetFields().Where(f => f.Attributes != FieldAttributes.NotSerialized).ToArray();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToArray();

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
                        string[] property = readLine.Split('=');

                        bool fieldSet = SetField(property, props, fields);
                        bool propertySet = SetProperty(property, props, properties);

                        if (!fieldSet && !propertySet)
                        {
                            Log.Error($"{property[0]} does not exist!");
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

        public static void Serialize<T>(T props) where T : IProperties, new()
        {
            FieldInfo[] fields = typeof(T).GetFields().Where(f => f.Attributes != FieldAttributes.NotSerialized).ToArray();
            PropertyInfo[] properties = typeof(T).GetProperties().Where(p => p.CanWrite).ToArray();

            using (StreamWriter stream = new StreamWriter(new FileStream(props.FileName, FileMode.OpenOrCreate), Encoding.UTF8))
            {
                stream.WriteLine("# Settings can be changed here");

                foreach (FieldInfo field in fields)
                {
                    stream.WriteLine($"{field.Name}={field.GetValue(props)}");
                }

                foreach (PropertyInfo property in properties)
                {
                    stream.WriteLine($"{property.Name}={property.GetValue(props)}");
                }
            }
        }

        private static bool SetField<T>(string[] property, T props, FieldInfo[] fields) where T : IProperties, new()
        {
            FieldInfo field = fields.FirstOrDefault(f => f.Name == property[0]);
            if (field == null)
            {
                return false;
            }

            if (bool.TryParse(property[1], out bool boolean))
            {
                field.SetValue(props, boolean);
            }
            else if (int.TryParse(property[1], out int integer))
            {
                field.SetValue(props, integer);
            }
            else if (float.TryParse(property[1], out float floatingPoint))
            {
                field.SetValue(props, floatingPoint);
            }
            else if (Enum.TryParse(property[1], out ServerGameMode gameMode))
            {
                field.SetValue(props, gameMode);
            }
            else if (Enum.TryParse(property[1], out ServerSerializerMode serializerMode))
            {
                field.SetValue(props, serializerMode);
            }
            else // treat as string
            {
                field.SetValue(props, property[1]);
            }
            return true;
        }

        private static bool SetProperty<T>(string[] property, T props, PropertyInfo[] properties) where T : IProperties, new()
        {
            PropertyInfo propertyInfo = properties.FirstOrDefault(p => p.Name == property[0]);

            if (propertyInfo == null)
            {
                return false;
            }

            if (bool.TryParse(property[1], out bool boolean))
            {
                propertyInfo.SetValue(props, boolean);
            }
            else if (int.TryParse(property[1], out int integer))
            {
                propertyInfo.SetValue(props, integer);
            }
            else if (float.TryParse(property[1], out float floatingPoint))
            {
                propertyInfo.SetValue(props, floatingPoint);
            }
            else if (Enum.TryParse(property[1], out ServerGameMode gameMode))
            {
                propertyInfo.SetValue(props, gameMode);
            }
            else if (Enum.TryParse(property[1], out ServerSerializerMode serializerMode))
            {
                propertyInfo.SetValue(props, serializerMode);
            }
            else // treat as string
            {
                propertyInfo.SetValue(props, property[1]);
            }
            return true;
        }
    }
}
