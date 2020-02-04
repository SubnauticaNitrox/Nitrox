using System;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
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
            string cultureName = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            return getLanguage(cultureName);
        }

        public static Language getLanguage(string val) => Enum.TryParse<Language>(val, true, out Language lang) ? lang : Language.DEFAULT;

        public static Language[] getAvailableLanguages() => (Language[])Enum.GetValues(typeof(Language));

        public static void SetupLanguage(ResourceDictionary element, Language lang)
        {
            string path = GetLocalizationPath(lang.GetAttribute<DescriptionAttribute>().Description);
            SwapResourceDictionary(element, path);
        }

        private static string GetLocalizationPath(string lang)
        {
            return Path.Combine("Localization", $"Localization-es.xaml");
        }

        private static void SwapResourceDictionary(ResourceDictionary element, string path)
        {
            if (Uri.TryCreate(path, UriKind.Relative, out Uri outUri))
            {
                ResourceDictionary languageDictionary = new ResourceDictionary()
                {
                    Source = outUri
                };

                if ((languageDictionary["Version"] as short? ?? -1) < Version)
                {
                    MessageBox.Show($"The language at '{outUri.OriginalString}' is Outdated, it must be in version {Version} instead");
                    return;
                }

                int id = element.MergedDictionaries.ToList().FindIndex(x => x.Contains("LocalizationString"));

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
                MessageBox.Show($"The language at '{outUri.OriginalString}' not found");
#endif
                CultureInfo.CurrentUICulture = new CultureInfo("en-US");
            }

        }
    }
}
