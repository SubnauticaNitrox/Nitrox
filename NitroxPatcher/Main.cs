using System.Collections.Generic;
using System.Reflection;
using Autofac;
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
        private static IDynamicPatch[] patches;
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

            IContainer container = InitializeDependencyContainer();
            foreach (IPersistentPatch patch in container.Resolve<IEnumerable<IPersistentPatch>>())
            {
                Log.Info("Applying persistent patch " + patch.GetType().Name);
                patch.Patch(harmony);
            }
            patches = container.Resolve<IDynamicPatch[]>();

            Multiplayer.OnBeforeMultiplayerStart += Apply;
            Multiplayer.OnAfterMultiplayerEnd += Restore;
            Log.Info("Completed patching using " + Assembly.GetExecutingAssembly().FullName);

            ApplyNitroxBehaviours();
        }

        public static void Apply()
        {
            Validate.NotNull(patches, "No patches have been discovered yet! Run Execute() first.");

            if (isApplied)
            {
                return;
            }

            foreach (IDynamicPatch patch in patches)
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
            Validate.NotNull(patches, "No patches have been discovered yet! Run Execute() first.");
            if (!isApplied)
            {
                return;
            }

            foreach (IDynamicPatch patch in patches)
            {
                Log.Info("Restoring dynamic patch " + patch.GetType().Name);
                patch.Restore(harmony);
            }

            isApplied = false;
        }

        private static IContainer InitializeDependencyContainer()
        {
            ContainerBuilder builder = new ContainerBuilder();

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IPersistentPatch>()
                .AsImplementedInterfaces();

            builder
                .RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .AssignableTo<IDynamicPatch>()
                .AsImplementedInterfaces();

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
