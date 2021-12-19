using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using HarmonyLib;
using LitJson;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent
{
    internal class Language_LoadLanguageFile_Patch : NitroxPatch, IPersistentPatch
    {
        private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Language t) => t.LoadLanguageFile(default(string)));

        public static void Postfix(string language, Dictionary<string, string> ___strings)
        {
            string[] files = {
                Path.Combine(NitroxUser.LauncherPath, "LanguageFiles", "English.json"), // Using English as fallback.
                Path.Combine(NitroxUser.LauncherPath, "LanguageFiles", language + ".json")
            };

            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    Log.Warn($"No language file was found for at {file}. Using English as fallback");
                    continue;
                }

                JsonData json;
                using (StreamReader streamReader = new(file))
                {
                    try
                    {
                        json = JsonMapper.ToObject(streamReader);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, $"Error while reading language file {language}.json");
                        return;
                    }
                }

                foreach (string key in json.Keys)
                {
                    if (json[key].IsString)
                    {
                        ___strings[key] = (string)json[key];
                    }
                }
            }

        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
