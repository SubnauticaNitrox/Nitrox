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
            string[] files = {
                Path.Combine(NitroxAppData.Instance.LauncherPath, "LanguageFiles", "English.json"), //Using English as fallback.
                Path.Combine(NitroxAppData.Instance.LauncherPath, "LanguageFiles", language + ".json")
            };

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    Log.Warn($"No language file was found for at {file}. Using English as fallback");
                }

                JsonData json;
                using (StreamReader streamReader = new StreamReader(file))
                {
                    try
                    {
                        json = JsonMapper.ToObject((TextReader)streamReader);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error while reading language file {language}.json");
                        return;
                    }
                }

                foreach (string key in json.Keys)
                {
                    ___strings[key] = (string)json[key];
                }
            }

        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
