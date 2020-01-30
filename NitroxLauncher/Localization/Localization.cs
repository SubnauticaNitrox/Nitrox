using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using NitroxModel;

namespace NitroxLauncher
{
    public enum Language
    {
        [Description("en")]
        EN,
        [Description("es")]
        ES,
        [Description("fr")]
        FR,
        [Description("en")]
        DEFAULT,
    }

    public static class Localization
    {
        public static Language GetCurrentCultureName()
        {
            string cultureName = CultureInfo.CurrentCulture.Name?.Substring(0, 2);
            return Enum.TryParse<Language>(cultureName, true, out Language lang) ? lang : Language.DEFAULT;
        }

        public static void SetupLanguage(ResourceDictionary element, Language lang)
        {
            string path = GetLocalizationPath(lang.GetAttribute<DescriptionAttribute>().Description);
            SwapRessourceDictionnary(element, path);
        }

        private static string GetLocalizationPath(string lang)
        {
            string directory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, $"Localization/Localization-{lang}.xaml");
        }

        private static void SwapRessourceDictionnary(ResourceDictionary element, string path)
        {
            if (File.Exists(path))
            {
                ResourceDictionary languageDictionary = new ResourceDictionary()
                {
                    Source = new Uri(path)
                };

                int id = -1;

                for (int i = 0; i < element.MergedDictionaries.Count; i++)
                {
                    if (element.MergedDictionaries[i].Contains("LocalizationString"))
                    {
                        id = i;
                        break;
                    }
                }

                if (id == -1)
                {
                    element.MergedDictionaries.Add(languageDictionary);
                }
                else
                {
                    element.MergedDictionaries[id] = languageDictionary;
                }
            }
#if DEBUG
            else
            {
                MessageBox.Show($"The language at '{path}' not found");
            }
#endif
        }
    }
}
