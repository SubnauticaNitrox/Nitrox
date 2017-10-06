using Harmony;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxPatcher.Patches;
using NitroxReloader;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace NitroxPatcher
{
    public static class Main
    {
        private static NitroxPatch[] patches;
        private static readonly HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");
        private static bool isApplied;

        public static void Execute()
        {
            Log.SetLevel(Log.LogLevel.ConsoleInfo | Log.LogLevel.ConsoleDebug | Log.LogLevel.InGameMessages);

            if (patches != null)
            {
                Log.Info("[NITROX] Warning: patches have already been detected! Call Apply or Restore instead.");
                return;
            }

            Log.Info("Patching Subnautica...");

            // Enabling this creates a log file on your desktop (why there?), showing the emitted IL instructions.
            HarmonyInstance.DEBUG = false;


            string serverNameSpace = "NitroxPatcher.Patches.Server";
            bool serverPatching = (Array.IndexOf(Environment.GetCommandLineArgs(), "-server") >= 0);

            Log.Info("Applying " + ((serverPatching) ? "server" : "client") + " patches");

            var discoveredPatches = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => typeof(NitroxPatch).IsAssignableFrom(p) &&
                            p.IsClass &&
                            !p.IsAbstract &&
                            p.Namespace != serverNameSpace ^ serverPatching
                      )
                .Select(Activator.CreateInstance)
                .Cast<NitroxPatch>();

            if (!serverPatching)
            {
                var splittedPatches = discoveredPatches.GroupBy(p => p.GetType().Namespace);

                splittedPatches.First(g => g.Key == "NitroxPatcher.Patches.ClientBase").ForEach(p =>
                {
                    Log.Info("[NITROX] Applying base patch " + p.GetType());
                    p.Patch(harmony);
                });

                patches = splittedPatches.First(g => g.Key == "NitroxPatcher.Patches.Client").ToArray();

                // TODO: Implement this a bit neater...
                // While keeping in mind that NitroxClient cannot have a reference to NitroxPatcher, due to circular dependencies.
                NitroxClient.MonoBehaviours.Gui.MainMenu.JoinServer.OnMultiplayerStarted += Apply;
            }
            else
            {
                patches = discoveredPatches.ToArray();
                Apply();
            }

            Log.Info("Completed patching using " + Assembly.GetExecutingAssembly().FullName);

            InitializeReloader(serverPatching);

            DevConsole.disableConsole = false;
        }

        public static void Apply()
        {
            Validate.NotNull(patches, "No patches have been discovered yet! Run Execute() first.");

            if (isApplied)
                return;

            patches.ForEach(patch =>
            {
                Log.Info("[NITROX] Applying " + patch.GetType());
                patch.Patch(harmony);
            });

            isApplied = true;
        }

        public static void Restore()
        {
            Validate.NotNull(patches, "No patches have been discovered yet! Run Execute() first.");

            if (!isApplied)
                return;

            patches.ForEach(patch =>
            {
                Log.Info("[NITROX] Restoring " + patch.GetType());
                patch.Restore();
            });

            isApplied = false;
        }

        [Conditional("DEBUG")]
        private static void InitializeReloader(bool serverPatching)
        {
            // Whitelist needs to be split, as both game instances load all four libraries
            // (because this patcher references both server and client, so no matter what instance we are on,
            //  AppDomain.CurrentDomain.GetAssemblies() returns both).
            if (serverPatching)
            {
                Reloader.Initialize("NitroxModel.dll", "NitroxPatcher.dll", "NitroxServer.dll");
            }
            else
            {
                Reloader.Initialize("NitroxModel.dll", "NitroxPatcher.dll", "NitroxClient.dll");
            }
        }
    }
}
