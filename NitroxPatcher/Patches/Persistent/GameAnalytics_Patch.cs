using System;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
///     Patch to disable any analytics uploads to UWE servers. Since game is modded it's not useful for UWE and it's less code to run for us.
/// </summary>
internal class GameAnalytics_Patch : NitroxPatch, IPersistentPatch
{
    /// <summary>
    ///     Disables analytics uploads.
    /// </summary>
    private static readonly MethodInfo TARGET_METHOD_SDK = Reflect.Method((SentrySdk t) => t.Start());

    /// <summary>
    ///     Skip manager create as well to remove NRE in Player.log on exit (OnDestroy will try access null SentrySdk instance).
    /// </summary>
    private static readonly MethodInfo TARGET_METHOD_MANAGER = Reflect.Method((SentrySdkManager t) => t.Awake());

    public static bool Prefix(SentrySdk __instance)
    {
        UnityEngine.Object.Destroy(__instance);
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_SDK, ((Func<SentrySdk, bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_MANAGER, ((Func<SentrySdk, bool>)Prefix).Method);
    }
}
