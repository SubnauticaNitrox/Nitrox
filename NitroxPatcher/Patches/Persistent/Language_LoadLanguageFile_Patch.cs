using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using LitJson;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

public class Language_LoadLanguageFile_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Language t) => t.LoadLanguageFile(default));

    private static readonly Dictionary<string, Tuple<string, string>> languageToIsoCode = new(); // First Tuple item is region specific (en-US), second isn't (en)

    public static void Postfix(string language, Dictionary<string, string> ___strings)
    {
        if (!TryLoadLanguageFile("en", ___strings)) //Loading english as fallback for missing files or keys
        {
            Log.Error($"The English language file could not be loaded");
            return;
        }

        if (!TryLoadLanguageFile(languageToIsoCode[language].Item1, ___strings))
        {
            if (!TryLoadLanguageFile(languageToIsoCode[language].Item2, ___strings))
            {
                Log.Warn($"No language file was found for {language}. Using English as fallback");
            }
        }

        Language.main.ParseMetaData();
    }

    private static bool TryLoadLanguageFile(string fileName, IDictionary<string, string> strings)
    {
        string filePath = Path.Combine(NitroxUser.LanguageFilesPath, $"{fileName}.json");

        if (!File.Exists(filePath))
        {
            return false;
        }

        using StreamReader streamReader = new(filePath);
        try
        {
            JsonData json = JsonMapper.ToObject(streamReader);

            foreach (string key in json.Keys)
            {
                JsonData entry = json[key];
                if (entry.IsString)
                {
                    strings[key] = (string)entry;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Error while reading language file {fileName}.json");
        }

        return false;
    }

    public override void Patch(Harmony harmony)
    {
        List<string> existingLanguageNames = Directory.EnumerateFiles(SNUtils.InsideUnmanaged("LanguageFiles"), "*.json").Select(Path.GetFileNameWithoutExtension).ToList();

        foreach (CultureInfo culture in CultureInfo.GetCultures(CultureTypes.AllCultures))
        {
            if (!languageToIsoCode.ContainsKey(culture.EnglishName) && !string.IsNullOrEmpty(culture.Name) && existingLanguageNames.Contains(culture.EnglishName))
            {
                languageToIsoCode.Add(culture.EnglishName, new Tuple<string, string>(culture.Name, culture.TwoLetterISOLanguageName));
            }
        }

        // This language isn't registered in CultureInfo
        if (existingLanguageNames.Contains("Spanish (Latin America)"))
        {
            languageToIsoCode.Add("Spanish (Latin America)", new Tuple<string, string>("es-419", "es"));
        }

        PatchPostfix(harmony, TARGET_METHOD, ((Action<string, Dictionary<string, string>>)Postfix).Method);
    }
}
