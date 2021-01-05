using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using LitJson;
using NitroxModel.Helper;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Persistent
{
    internal class Language_LoadLanguageFile_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(Language);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("LoadLanguageFile", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void Postfix(string language, Dictionary<string, string> ___strings)
        {
            string filepath = Path.Combine(NitroxAppData.Instance.LauncherPath, "LanguageFiles", language + ".json");

            if (!File.Exists(filepath))
            {
                Log.Warn($"No language file was found for {language}. Using english.");
                filepath = Path.Combine(NitroxAppData.Instance.LauncherPath, "LanguageFiles", "English.json");
            }

            JsonData json = null;
            using (StreamReader streamReader = new StreamReader(filepath))
            {
                try
                {
                    json = JsonMapper.ToObject((TextReader)streamReader);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"Error while reading language file ({language})");
                    return;
                }
            }

            foreach (string key in (IEnumerable<string>)json.Keys)
            {
                ___strings[key] = (string)json[key];
            }
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
