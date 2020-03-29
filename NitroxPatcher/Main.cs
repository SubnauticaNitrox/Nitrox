using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Harmony;
using NitroxClient;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxPatcher.Modules;
using NitroxPatcher.Patches;
using UnityEngine;

namespace NitroxPatcher
{
    public static class Main
    {
        /// <summary>
        ///     Dependency Injection container used by NitroxPatcher only.
        /// </summary>
        private static IContainer container;

        private static readonly HarmonyInstance harmony = HarmonyInstance.Create("com.nitroxmod.harmony");
        private static bool isApplied;

        public static void Execute()
        {
            Log.EnableInGameMessages();

            if (container != null)
            {
                Log.Warn("Patches have already been detected! Call Apply or Restore instead.");
                return;
            }

            Log.Info("Registering Dependencies");
            container = CreatePatchingContainer();
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar());

            InitPatches();
            ApplyNitroxBehaviours();
        }

        public static void Apply()
        {
            Validate.NotNull(container, "No patches have been discovered yet! Run Execute() first.");
            if (isApplied)
            {
                return;
            }

            foreach (IDynamicPatch patch in container.Resolve<IDynamicPatch[]>())
            {
                Log.Info("Applying dynamic patch " + patch.GetType().Name);
                patch.Patch(harmony);
            }

            isApplied = true;
        }

        /// <summary>
        ///     If the player starts the main menu for the first time, or returns from a (multiplayer) session, get rid of all the
        ///     patches if applicable.
        /// </summary>
        public static void Restore()
        {
            Validate.NotNull(container, "No patches have been discovered yet! Run Execute() first.");
            if (!isApplied)
            {
                return;
            }

            foreach (IDynamicPatch patch in container.Resolve<IDynamicPatch[]>())
            {
                Log.Info("Restoring dynamic patch " + patch.GetType().Name);
                patch.Restore(harmony);
            }

            isApplied = false;
        }

        private static void InitPatches()
        {
            Log.Info("Patching Subnautica...");

            // Enabling this creates a log file on your desktop (why there?), showing the emitted IL instructions.
            HarmonyInstance.DEBUG = false;

            foreach (IPersistentPatch patch in container.Resolve<IEnumerable<IPersistentPatch>>())
            {
                Log.Info("Applying persistent patch " + patch.GetType().Name);
                patch.Patch(harmony);
            }

            Multiplayer.OnBeforeMultiplayerStart += Apply;
            Multiplayer.OnAfterMultiplayerEnd += Restore;
            Log.Info("Completed patching using " + Assembly.GetExecutingAssembly().FullName);
        }

        private static IContainer CreatePatchingContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterModule(new NitroxPatchesModule());
            return builder.Build();
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
