using Oculus.Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace NitroxPatcher
{
    public class QModManager
    {
        private static readonly string qModBaseDir = Environment.CurrentDirectory + @"\QMods";
        private static List<QMod> loadedMods = new List<QMod>();
        private static bool patched = false;


        public static void Patch()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var allDlls = new DirectoryInfo(qModBaseDir).GetFiles("*.dll", SearchOption.AllDirectories);
                foreach (var dll in allDlls)
                {
                    Console.WriteLine(Path.GetFileNameWithoutExtension(dll.Name) + " " + args.Name);
                    if (args.Name.Contains(Path.GetFileNameWithoutExtension(dll.Name)))
                    {
                        return Assembly.LoadFrom(dll.FullName);
                    }
                }

                return null;
            };

            if (patched)
                return;

            patched = true;

            if (!Directory.Exists(qModBaseDir))
            {
                Console.WriteLine("[QModManager] ERR: QMod directory was not found");
                Directory.CreateDirectory(qModBaseDir);
                Console.WriteLine("[QModManager] INFO: Created QMod directory at {0}", qModBaseDir);
                return;
            }

            var subDirs = Directory.GetDirectories(qModBaseDir);
            var lastMods = new List<QMod>();
            var firstMods = new List<QMod>();
            var otherMods = new List<QMod>();

            foreach (var subDir in subDirs)
            {
                var jsonFile = Path.Combine(subDir, "mod.json");

                if (!File.Exists(jsonFile))
                {
                    Console.WriteLine("[QModManager] ERR: Mod is missing a mod.json file");
                    File.WriteAllText(jsonFile, JsonConvert.SerializeObject(new QMod()));
                    Console.WriteLine("[QModManager] INFO: A template for mod.json was generated at {0}", jsonFile);
                    continue;
                }

                QMod mod = QMod.FromJsonFile(Path.Combine(subDir, "mod.json"));

                if (mod.Equals(null)) // QMod.FromJsonFile will throw parser errors
                    continue;

                if (mod.Enable.Equals(false))
                {
                    Console.WriteLine("[QModManager] WARN: {0} is disabled via config, skipping", mod.DisplayName);
                    continue;
                }

                var modAssemblyPath = Path.Combine(subDir, mod.AssemblyName);

                if (!File.Exists(modAssemblyPath))
                {
                    Console.WriteLine("[QModManager] ERR: No matching dll found at {0} for {1}", modAssemblyPath, mod.Id);
                    continue;
                }

                mod.loadedAssembly = Assembly.LoadFrom(modAssemblyPath);
                mod.modAssemblyPath = modAssemblyPath;

                if (mod.Priority.Equals("Last"))
                {
                    lastMods.Add(mod);
                    continue;
                }
                else if (mod.Priority.Equals("First"))
                {
                    firstMods.Add(mod);
                    continue;
                }
                else
                {
                    otherMods.Add(mod);
                    continue;
                }
            }

            foreach (var mod in firstMods)
            {
                if (mod != null)
                    loadedMods.Add(LoadMod(mod));
            }

            foreach (var mod in otherMods)
            {
                if (mod != null)
                    loadedMods.Add(LoadMod(mod));
            }

            foreach (var mod in lastMods)
            {
                if (mod != null)
                    loadedMods.Add(LoadMod(mod));
            }
        }

        private static QMod LoadMod(QMod mod)
        {
            if (mod == null)
                return null;

            Console.WriteLine("[QModManager] INFO: Loading Mod: {0}, Version: {1} with ID: {2} by {3}", mod.DisplayName, mod.Version, mod.Id, mod.Author);

            //Call Patch-Method
            if (string.IsNullOrEmpty(mod.EntryMethod) || mod.EntryMethod.Equals("Namespace.Class.Method of Harmony.PatchAll or your equivalent"))
            {
                Console.WriteLine("[QModManager] ERR: No EntryMethod specified for {0}", mod.Id);
            }
            else
            {
                try
                {
                    var entryMethodSig = mod.EntryMethod.Split('.');
                    var entryType = string.Join(".", entryMethodSig.Take(entryMethodSig.Length - 1).ToArray());
                    var entryMethod = entryMethodSig[entryMethodSig.Length - 1];

                    MethodInfo qPatchMethod = mod.loadedAssembly.GetType(entryType).GetMethod(entryMethod);
                    qPatchMethod.Invoke(mod.loadedAssembly, new object[] { });
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("[QModManager] ERR: Could not parse EntryMethod {0} for {1}", mod.AssemblyName, mod.Id);
                    Console.WriteLine(e.InnerException.Message);
                    return null;
                }
                catch (TargetInvocationException e)
                {
                    Console.WriteLine("[QModManager] ERR: Invoking the specified EntryMethod {0} failed for {1}", mod.EntryMethod, mod.Id);
                    Console.WriteLine(e.InnerException.Message);
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[QModManager] ERR: Something unexpected happened");
                    Console.WriteLine(e.Message);
                    return null;
                }
            }

            //Call Nitrox-Multiplayer specific Method
            if (string.IsNullOrEmpty(mod.MultiplayerMethod) || mod.MultiplayerMethod.Equals("Namespace.Class.Method of a Method which will be called when Nitrox is installed"))
            {
                Console.WriteLine("[QModManager] INFO: No Nitrox-Multiplayer-Method specified for {0}, it is probably a Client-Sided Mod", mod.Id);
            }
            else
            {
                try
                {
                    var nitroxMethodSig = mod.MultiplayerMethod.Split('.');
                    var nitroxType = string.Join(".", nitroxMethodSig.Take(nitroxMethodSig.Length - 1).ToArray());
                    var nitroxMethod = nitroxMethodSig[nitroxMethodSig.Length - 1];

                    MethodInfo qNitroxMethod = mod.loadedAssembly.GetType(nitroxType).GetMethod(nitroxMethod);
                    qNitroxMethod.Invoke(mod.loadedAssembly, new object[] { });
                }
                catch (ArgumentNullException e)
                {
                    Console.WriteLine("[QModManager] ERR: Could not parse Nitrox-Multiplayer-Method {0} for {1}", mod.AssemblyName, mod.Id);
                    Console.WriteLine(e.InnerException.Message);
                    return null;
                }
                catch (TargetInvocationException e)
                {
                    Console.WriteLine("[QModManager] ERR: Invoking the specified Nitrox-Multiplayer-Method {0} failed for {1}", mod.EntryMethod, mod.Id);
                    Console.WriteLine(e.InnerException.Message);
                    return null;
                }
                catch (Exception e)
                {
                    Console.WriteLine("[QModManager] ERR: Something unexpected happened");
                    Console.WriteLine(e.Message);
                    return null;
                }
            }

            Console.WriteLine("[QModManager] INFO: Successfully loaded Mod: {0}", mod.DisplayName);
            return mod;
        }
    }
}
