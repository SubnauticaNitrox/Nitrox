using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace NitroxLauncher
{
    public static partial class Localization
    {
        public static readonly string DefaultLocalization = "en";

        public enum Language
        {
            EN,
            ES,
            FR
        }

        public static string GetCurrentCultureName()
        {
            string cultureName = CultureInfo.CurrentCulture.Name;

            if (string.IsNullOrEmpty(cultureName))
            {
                CultureInfo.CurrentCulture = new CultureInfo("en-US", false);
                cultureName = "en";
            }

            return cultureName.Substring(0, 2);
        }

        public static void SetupLanguage(ResourceDictionary element, string lang)
        {
            if (!string.IsNullOrEmpty(lang) && lang != DefaultLocalization)
            {
                string path = GetLocalizationPath(lang);
                SwapRessourceDictionnary(element, path);
            }
        }

        public static void SetupLanguage(ResourceDictionary element, Language lang)
        {
            string path = GetLocalizationPath(lang.ToString().ToLower());
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
