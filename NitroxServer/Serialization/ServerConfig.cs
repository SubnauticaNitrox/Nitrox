using System;
using System.Configuration;
using System.Text;
using System.ComponentModel;

namespace NitroxServer.ConfigParser
{
    public class ServerConfig
    {
        private readonly ServerConfigItem<int>    portSetting = new ServerConfigItem<int>("Port", 11000);
        private readonly ServerConfigItem<int>    saveIntervalSetting   = new ServerConfigItem<int>("SaveInterval", 60000);
        private readonly ServerConfigItem<int>    maxConnectionsSetting = new ServerConfigItem<int>("MaxConnections", 100);
        private readonly ServerConfigItem<bool>   disableConsoleSetting = new ServerConfigItem<bool>("DisableConsole", true);
        private readonly ServerConfigItem<string> saveNameSetting       = new ServerConfigItem<string>("SaveName", "save");
        private readonly ServerConfigItem<string> serverPasswordSetting = new ServerConfigItem<string>("ServerPassword", "");
        private readonly ServerConfigItem<string> adminPasswordSetting = new ServerConfigItem<string>("AdminPassword", GenerateRandomString(12, false));
        private readonly ServerConfigItem<string> gameModeSetting    = new ServerConfigItem<string>("GameMode", "Survival");
        private readonly ServerConfigItem<string> networkingType = new ServerConfigItem<string>("NetworkingType", "lidgren");
        
        public int ServerPort { get { return portSetting.Value; } }
        public int SaveInterval { get { return saveIntervalSetting.Value; } }
        public int MaxConnections { get { return maxConnectionsSetting.Value; } }
        public bool DisableConsole { get { return disableConsoleSetting.Value; } }
        public string SaveName { get { return saveNameSetting.Value; } }
        public string ServerPassword { get { return serverPasswordSetting.Value; } }
        public string AdminPassword { get { return adminPasswordSetting.Value; } }
        public string GameMode { get { return gameModeSetting.Value; } }
        public string NetworkingType { get { return networkingType.Value; } }
        
        // Generate a random string with a given size and case.   
        // If second parameter is true, the return string is lowercase  
        public static string GenerateRandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
            {
                return builder.ToString().ToLower();
            }
            return builder.ToString();  
        }

        public void ChangeAdminPassword(string pw)
        {
            adminPasswordSetting.Value = pw;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[adminPasswordSetting.Name].Value = pw;
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }

        public void ChangeServerPassword(string pw)
        {
            serverPasswordSetting.Value = pw;

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings[serverPasswordSetting.Name].Value = pw;
            config.Save(ConfigurationSaveMode.Modified);

            ConfigurationManager.RefreshSection("appSettings");
        }
    }
    
    internal class ServerConfigItem<T>
    {
        public T Value;
        public readonly string Name;
        public ServerConfigItem(string itemName, T defaultValue)
        {
            Name = itemName;
            Value = defaultValue;
            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                {
                    return;
                }

                string text = ConfigurationManager.AppSettings[itemName];

                // Empty string is ignored
                if (typeof(T) == typeof(string) && (string.IsNullOrEmpty(text) || string.IsNullOrEmpty(text)))
                {
                    return;
                }
                // Enum members are assumed to be Titlecased
                if (typeof(T).IsEnum)
                {
                    text = text.ToLower();
                    text = char.ToUpper(text[0]) + text.Substring(1);
                }

                Value = (T)converter.ConvertFromString(text);

            }
            catch (Exception) { }
        }
    }
}
