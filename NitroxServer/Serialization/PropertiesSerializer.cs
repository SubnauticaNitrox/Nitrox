using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NitroxModel.Logger;
using NitroxModel.Server;

namespace NitroxServer.Serialization
{
    public static class PropertiesSerializer
    {
        private readonly static FieldInfo[] fields = typeof(Properties).GetFields();
        private readonly static PropertyInfo[] properties = typeof(Properties).GetProperties().Where(p => p.CanWrite).ToArray();

        private const string CONFIG_NAME = "config.properties";

        public static Properties Deserialize()
        {
            if (!File.Exists(CONFIG_NAME))
            {
                return new Properties();
            }

            using (StreamReader reader = new StreamReader(new FileStream(CONFIG_NAME, FileMode.Open), Encoding.UTF8))
            {
                Properties props = new Properties();

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

                        bool fieldSet = SetField(property, props);
                        bool propertySet = SetProperty(property, props);

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

        public static void Serialize(Properties props)
        {
            using (StreamWriter stream = new StreamWriter(new FileStream(CONFIG_NAME, FileMode.OpenOrCreate), Encoding.UTF8))
            {
                stream.WriteLine("# Server settings can be changed here");

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

        private static bool SetField(string[] property, Properties props)
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

        private static bool SetProperty(string[] property, Properties props)
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
