using System;
using System.Configuration;
using System.Text;
using System.ComponentModel;
using NitroxModel.Logger;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.ConfigParser
{
    public class ServerConfig
    {
        private readonly ServerConfigItem<int> portSetting = new ServerConfigItem<int>("Port", 11000);
        private readonly ServerConfigItem<int> saveIntervalSetting = new ServerConfigItem<int>("SaveInterval", 60000);
        private readonly ServerConfigItem<int> maxConnectionsSetting = new ServerConfigItem<int>("MaxConnections", 100);
        private readonly ServerConfigItem<bool> disableConsoleSetting = new ServerConfigItem<bool>("DisableConsole", true);
        private readonly ServerConfigItem<string> saveNameSetting = new ServerConfigItem<string>("SaveName", "save");
        private readonly ServerConfigItem<string> serverPasswordSetting = new ServerConfigItem<string>("ServerPassword", "");
        private readonly ServerConfigItem<string> adminPasswordSetting = new ServerConfigItem<string>("AdminPassword", GenerateRandomString(12, false));
        private readonly ServerConfigItem<string> gameModeSetting = new ServerConfigItem<string>("GameMode", "Survival");
        private readonly ServerConfigItem<float> oxygenSetting = new ServerConfigItem<float>("StartOxygen", 45);
        private readonly ServerConfigItem<float> maxOxygenSetting = new ServerConfigItem<float>("StartMaxOxygen", 45);
        private readonly ServerConfigItem<float> healthSetting = new ServerConfigItem<float>("StartHealth", 80);
        private readonly ServerConfigItem<float> foodSetting = new ServerConfigItem<float>("StartFood", 50.5f);
        private readonly ServerConfigItem<float> waterSetting = new ServerConfigItem<float>("StartWater", 90.5f);
        private readonly ServerConfigItem<float> infectionSetting = new ServerConfigItem<float>("StartInfection", 0);

        public int ServerPort { get { return portSetting.Value; } }
        public int SaveInterval { get { return saveIntervalSetting.Value; } }
        public int MaxConnections { get { return maxConnectionsSetting.Value; } }
        public bool DisableConsole { get { return disableConsoleSetting.Value; } }
        public string SaveName { get { return saveNameSetting.Value; } }
        public string ServerPassword { get { return serverPasswordSetting.Value; } }
        public string AdminPassword { get { return adminPasswordSetting.Value; } }
        public string GameMode { get { return gameModeSetting.Value; } }
        public PlayerStatsData DefaultPlayerStats { get { return new PlayerStatsData(oxygenSetting.Value, maxOxygenSetting.Value, healthSetting.Value, foodSetting.Value, waterSetting.Value, infectionSetting.Value); } }

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

        private void RefreshAppSettingsValue(string key, string value)
        {
            try
            {
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.AppSettings.Settings[key].Value = value;
                config.Save(ConfigurationSaveMode.Modified);

                ConfigurationManager.RefreshSection("appSettings");
            }
            catch (Exception ex)
            {
                Log.Error("Can't refresh server app settings", ex);
            }
        }

        public void ChangeAdminPassword(string password)
        {
            adminPasswordSetting.Value = password;
            RefreshAppSettingsValue(adminPasswordSetting.Name, password);
        }

        public void ChangeServerPassword(string password)
        {
            serverPasswordSetting.Value = password;
            RefreshAppSettingsValue(serverPasswordSetting.Name, password);
        }

        public void ChangeServerGameMode(string gamemode)
        {
            gameModeSetting.Value = gamemode;
            RefreshAppSettingsValue(gameModeSetting.Name, gamemode);
        }

        public void ChangeServerSaveInterval(int intervalInMillis)
        {
            saveIntervalSetting.Value = intervalInMillis;
            RefreshAppSettingsValue(saveIntervalSetting.Name, intervalInMillis.ToString());
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
                if (typeof(T) == typeof(string) && (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)))
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
