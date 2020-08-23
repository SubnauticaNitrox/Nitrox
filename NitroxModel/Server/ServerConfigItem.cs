﻿using System;
using System.ComponentModel;
using System.Configuration;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxModel.Server
{
    public class ServerConfigItem<T>
    {
        private readonly string name;
        private T value;

        public T Value
        {
            get => value;

            set
            {
                try
                {
                    Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    config.AppSettings.Settings[name].Value = value?.ToString();
                    config.Save(ConfigurationSaveMode.Minimal);

                    this.value = value;

                    ConfigurationManager.RefreshSection("appSettings");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Can't refresh server app settings");
                }
            }

        }

        public ServerConfigItem(string itemName, T defaultValue)
        {
            Validate.NotNull(itemName);

            name = itemName;
            value = defaultValue; //not Value, we don't want to rewrite the config to default value again and again

            try
            {
                TypeConverter converter = TypeDescriptor.GetConverter(typeof(T));
                string text = ConfigurationManager.AppSettings[itemName];

                // Empty string are ignored
                if (typeof(T) == typeof(string) && string.IsNullOrWhiteSpace(text))
                {
                    return;
                }

                // Enum members are assumed to be uppercased
                if (typeof(T).IsEnum)
                {
                    text = text.ToUpper();
                }

                value = (T)converter.ConvertFromString(text);
            }
            catch (Exception)
            {
                Log.Error($"Error while creating ServerConfigItem {itemName}, Restoring to default value");
                Value = defaultValue;
            }
        }
    }
}
