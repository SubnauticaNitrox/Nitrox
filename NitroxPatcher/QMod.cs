using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NitroxPatcher
{
    public class QMod
    {
        public string Id = "mod_id";
        public string DisplayName = "Display name";
        public string Author = "author";
        public string Version = "0.0.0";
        public string[] Requires = new string[] { };
        public bool Enable = false;
        public string AssemblyName = "dll filename";
        public string EntryMethod = "Namespace.Class.Method of Harmony.PatchAll or your equivalent";
        public string MultiplayerMethod = "Namespace.Class.Method of a Method which will be called when Nitrox is installed";
        public string Priority = "Last or First";
        public Dictionary<string, object> Config = new Dictionary<string, object>();

        [JsonIgnore]
        public Assembly loadedAssembly;

        [JsonIgnore]
        public string modAssemblyPath;


        public QMod() { }

        public static QMod FromJsonFile(string file)
        {
            try
            {
                var json = File.ReadAllText(file);
                return JsonConvert.DeserializeObject<QMod>(json);
            }
            catch (Exception e)
            {
                Console.WriteLine("QMOD ERR: mod.json deserialization failed!");
                Console.WriteLine(e.Message);
                return null;
            }
        }
    }
}

