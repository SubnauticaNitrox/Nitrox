using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Autofac;
using HarmonyLib;
using HarmonyLib.Tools;
using NitroxClient;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxPatcher.Modules;
using NitroxPatcher.Patches;
using UnityEngine;

namespace NitroxPatcher;

internal static class Patcher
{
    /// <summary>
    ///     Dependency Injection container used by NitroxPatcher only.
    /// </summary>
    private static IContainer container;

    private static readonly Harmony harmony = new("com.nitroxmod.harmony");
    private static bool isApplied;

    /// <summary>
    ///     Applies all the dynamic patches defined in Patches namespace.
    ///     Persistent patches are applied when the game initializes (i.e. before main menu).
    ///     See <see cref="Patcher.Initialize"/>.
    /// </summary>
    public static void Apply()
    {
        Validate.NotNull(container, "No patches have been discovered yet! Run Execute() first.");
        if (isApplied)
        {
            return;
        }

        foreach (IDynamicPatch patch in container.Resolve<IDynamicPatch[]>())
        {
            Log.Debug($"Applying dynamic patch {patch.GetType().Name}");
            try
            {
                patch.Patch(harmony);
            }
            catch (Exception e)
            {
                Log.Error($"Error patching {patch.GetType().Name}. Error: {e.Message}");
            }
        }

        isApplied = true;
    }

    /// <summary>
    ///     Removes all the dynamic patches defined by <see cref="NitroxPatcher"/>.
    ///     <p/>
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
            Log.Debug($"Restoring dynamic patch {patch.GetType().Name}");
            patch.Restore(harmony);
        }

        isApplied = false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Initialize()
    {
        Optional.ApplyHasValueCondition<UnityEngine.Object>(o => (bool)o);

        if (container != null)
        {
            throw new Exception($"Patches have already been detected! Call {nameof(Apply)} or {nameof(Restore)} instead.");
        }
        Log.Info("Registering dependencies");
        container = CreatePatchingContainer();
        try
        {
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar());
        }
        catch (ReflectionTypeLoadException ex)
        {
            Log.Error($"Failed to load one or more dependency types for Nitrox. Assembly: {ex.Types.FirstOrDefault()?.Assembly.FullName ?? "unknown"}");
            foreach (Exception loaderEx in ex.LoaderExceptions)
            {
                Log.Error(loaderEx);
            }
            throw;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error while initializing and loading dependencies.");
            throw;
        }

        InitPatches();
        ApplyNitroxBehaviours();
    }

    private static void InitPatches()
    {
        Log.Info("Patching Subnautica...");

        // Enabling this creates a log file on your desktop (why there?), showing the emitted IL instructions.
        HarmonyFileLog.Enabled = false;

        foreach (IPersistentPatch patch in container.Resolve<IEnumerable<IPersistentPatch>>())
        {
            Log.Debug($"Applying persistent patch {patch.GetType().Name}");
            patch.Patch(harmony);
        }

        Multiplayer.OnBeforeMultiplayerStart += Apply;
        Multiplayer.OnAfterMultiplayerEnd += Restore;
        Log.Info("Completed patching");
    }

    private static IContainer CreatePatchingContainer()
    {
        ContainerBuilder builder = new();
        builder.RegisterModule(new NitroxPatchesModule());
        return builder.Build();
    }

    private static void ApplyNitroxBehaviours()
    {
        Log.Info("Applying Nitrox behaviours..");

        GameObject nitroxRoot = new();
        nitroxRoot.name = "Nitrox";
        nitroxRoot.AddComponent<NitroxBootstrapper>();

        Log.Info("Behaviours applied.");
    }
}
