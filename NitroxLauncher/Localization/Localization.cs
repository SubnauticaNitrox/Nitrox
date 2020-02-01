using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
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
        public static readonly short Version = 1;

        public static Language GetCurrentCultureLanguage()
        {
            string cultureName = CultureInfo.CurrentUICulture.Name?.Substring(0, 2);
            return getLanguage(cultureName);
        }

        public static Language getLanguage(string val) => Enum.TryParse<Language>(val, true, out Language lang) ? lang : Language.DEFAULT;

        public static Language[] getAvailableLanguages() => (Language[]) Enum.GetValues(typeof(Language));

        public static void SetupLanguage(ResourceDictionary element, Language lang)
        {
            string path = GetLocalizationPath(lang.GetAttribute<DescriptionAttribute>().Description);
            SwapRessourceDictionnary(element, path);
        }

        private static string GetLocalizationPath(string lang)
        {
            string directory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return Path.Combine(directory, "Localization", $"Localization-{lang}.xaml");
        }

        private static void SwapRessourceDictionnary(ResourceDictionary element, string path)
        {
            if (File.Exists(path))
            {
                ResourceDictionary languageDictionary = new ResourceDictionary()
                {
                    Source = new Uri(path)
                };

                if ((languageDictionary["Version"] as short? ?? -1) < Version)
                {
                    MessageBox.Show($"The language at '{path}' is Outdated, it must be in version {Version} instead");
                    return;
                }

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
            else
            {
#if DEBUG
                MessageBox.Show($"The language at '{path}' not found");
#endif
                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            }

        }
    }
}
