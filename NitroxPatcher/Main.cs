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

            string serverNameSpace = "NitroxPatcher.Patches.Server";
            bool serverPatching = (Array.IndexOf(Environment.GetCommandLineArgs(), "-server") >= 0);

            Console.WriteLine("[NITROX] Applying " + ((serverPatching) ? "server" : "client") + " patches");

            Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => typeof(NitroxPatch).IsAssignableFrom(p) &&
                            p.IsClass &&
                            !p.IsAbstract &&
                            p.Namespace != serverNameSpace ^ serverPatching
                      )
                .Select(Activator.CreateInstance)
                .Cast<NitroxPatch>()
                .ToList()
                .ForEach(patch =>
                {
                    Console.WriteLine("[NITROX] Applying " + patch.GetType());
                    patch.Patch(harmony);
                });

            Console.WriteLine("[NITROX] Completed patching using " + Assembly.GetExecutingAssembly().FullName);

            Console.WriteLine("[NITROX] Initializing reloader...");

            // Whitelist needs to be split, as both game instances load all four libraries
            // (because this patcher references both server and client, so no matter what instance we are on,
            //  AppDomain.CurrentDomain.GetAssemblies() returns both).
            if (serverPatching)
            {
                new Reloader("NitroxModel.dll", "NitroxPatcher.dll", "NitroxServer.dll");
            }
            else
            {
                new Reloader("NitroxModel.dll", "NitroxPatcher.dll", "NitroxClient.dll");
            }

            Console.WriteLine("[NITROX] Reloader initialized");

            DevConsole.disableConsole = false;
        }
    }
}
