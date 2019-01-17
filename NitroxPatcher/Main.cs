using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;
using NitroxClient;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxPatcher.Patches;
using UnityEngine;

namespace NitroxPatcher
{
    public static class Main
    {
        private static NitroxPatch[] patches;
        private static readonly HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");
        private static bool isApplied;
        
        public static void Execute()
        {
            Log.EnableInGameMessages();
            
            if (patches != null)
            {
                Log.Warn("Patches have already been detected! Call Apply or Restore instead.");
                return;
            }

            Log.Info("Registering Dependencies");

            // Our application's entry point. First, register client dependencies with AutoFac.
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar());

            Log.Info("Patching Subnautica...");

            // Enabling this creates a log file on your desktop (why there?), showing the emitted IL instructions.
            HarmonyInstance.DEBUG = false;

            IEnumerable<NitroxPatch> discoveredPatches = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(p => typeof(NitroxPatch).IsAssignableFrom(p) &&
                            p.IsClass && !p.IsAbstract
                      )
                .Select(Activator.CreateInstance)
                .Cast<NitroxPatch>();

            IEnumerable<IGrouping<string, NitroxPatch>> splittedPatches = discoveredPatches.GroupBy(p => p.GetType().Namespace);

            splittedPatches.First(g => g.Key == "NitroxPatcher.Patches.Persistent").ForEach(p =>
            {
                Log.Info("Applying persistent patch " + p.GetType());
                p.Patch(harmony);
            });

            patches = splittedPatches.First(g => g.Key == "NitroxPatcher.Patches").ToArray();
            Multiplayer.OnBeforeMultiplayerStart += Apply;
            Multiplayer.OnAfterMultiplayerEnd += Restore;
            Log.Info("Completed patching using " + Assembly.GetExecutingAssembly().FullName);

            Log.Info("Enabling developer console.");
            DevConsole.disableConsole = false;
            Application.runInBackground = true;
            Log.Info($"Unity run in background set to {Application.runInBackground.ToString().ToUpperInvariant()}.");

            ApplyNitroxBehaviours();
        }

        public static void Apply()
        {
            Validate.NotNull(patches, "No patches have been discovered yet! Run Execute() first.");

            if (isApplied)
            {
                return;
            }

            patches.ForEach(patch =>
            {
                Log.Info("Applying " + patch.GetType());
                patch.Patch(harmony);
            });

            isApplied = true;
        }

        /// <summary>
        /// If the player starts the main menu for the first time, or returns from a (multiplayer) session, get rid of all the patches if applicable.
        /// </summary>
        public static void Restore()
        {
            Validate.NotNull(patches, "No patches have been discovered yet! Run Execute() first.");

            if (!isApplied)
            {
                return;
            }

            patches.ForEach(patch =>
            {
                Log.Info("Restoring " + patch.GetType());
                patch.Restore(harmony);
            });

            isApplied = false;
        }

        private static void ApplyNitroxBehaviours()
        {
            Log.Info("Applying Nitrox behaviours..");

            GameObject nitroxRoot = new GameObject();
            nitroxRoot.name = "Nitrox";
            nitroxRoot.AddComponent<NitroxBootstrapper>();

            Log.Info("Behaviours applied.");
        }
    }
}
