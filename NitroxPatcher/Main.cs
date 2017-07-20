using Harmony;
using NitroxPatcher.Patches;
using System;
using System.Linq;
using System.Reflection;
using NitroxReloader;

namespace NitroxPatcher
{
    class Main
    {
        public static void Execute()
        {
            Console.WriteLine("[NITROX] Patching Subnautica...");

            // Enabling this creates a log file on your desktop (why there?), showing the emitted IL instructions.
            HarmonyInstance.DEBUG = false;

            HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");

            //AppDomain.CurrentDomain.GetAssemblies()
            //.SelectMany(a => a.GetTypes().Where(p => typeof(NitroxPatch).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract))
            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => typeof(NitroxPatch).IsAssignableFrom(p) && p.IsClass && !p.IsAbstract)
                .Select(Activator.CreateInstance)
                .Cast<NitroxPatch>()
                .ForEach(patch =>
                {
                    Console.WriteLine("[NITROX] Applying " + patch.GetType());
                    patch.Patch(harmony);
                });

            Console.WriteLine("[NITROX] Completed patching using " + Assembly.GetExecutingAssembly().FullName);

            Console.WriteLine("[NITROX] Initializing reloader...");

            new Reloader();

            Console.WriteLine("[NITROX] Reloader initialized");

            DevConsole.disableConsole = false;
        }
    }
}
