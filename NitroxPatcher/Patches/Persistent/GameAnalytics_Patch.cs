using System;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
///     Patch to disable any analytics uploads to UWE servers. Since game is modded it's not useful for UWE and it's less
///     code to run for us.
/// </summary>
internal class GameAnalytics_Patch : NitroxPatch, IPersistentPatch
{
    /// <summary>
    ///     Disables analytics uploads.
    /// </summary>
    private static readonly MethodInfo TARGET_METHOD_SDK = Reflect.Method((SentrySdk t) => t.Start());

    /// <summary>
    ///     Skip manager create as well to remove NRE in Player.log on exit (OnDestroy will try access null SentrySdk
    ///     instance).
    /// </summary>
    private static readonly MethodInfo TARGET_METHOD_MANAGER = Reflect.Method((SentrySdkManager t) => t.Awake());
    private static readonly MethodInfo TARGET_METHOD_MANAGER_ONENABLE = Reflect.Method((SentrySdkManager t) => t.OnEnable());

    private static readonly MethodInfo TARGET_METHOD_TELEMETRY = Reflect.Method((Telemetry t) => t.Awake());
    private static readonly MethodInfo TARGET_METHOD_TELEMETRY_QUIT = Reflect.Method(() => Telemetry.SendGameQuit(default));
    private static readonly MethodInfo TARGET_METHOD_GAMEANALYTICS_SEND_EVENT = Reflect.Method(() => GameAnalytics.Send(default(GameAnalytics.Event), default(bool), default(string)));
    private static readonly MethodInfo TARGET_METHOD_GAMEANALYTICS_SEND_EVENTINFO = Reflect.Method(() => GameAnalytics.Send(default(GameAnalytics.EventInfo), default(bool), default(string)));
    private static readonly MethodInfo TARGET_METHOD_GAMEANALYTICS_SEND_LEGACYEVENT = Reflect.Method(() => GameAnalytics.LegacyEvent(default(GameAnalytics.Event), default(string)));
    private static readonly MethodInfo TARGET_METHOD_SPAWNER_ANALYTICS = Reflect.Method((SystemsSpawner t) => t.Awake());

    public static bool Prefix(SentrySdk __instance)
    {
        FieldInfo initializedField = __instance.GetType().GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance);
        if (initializedField == null)
        {
            Log.WarnOnce($"{nameof(SentrySdk)} could not be properly disabled because the field '_initialized' is missing. Expect NRE in logs where Telemetry events are processed.");
            return false;
        }
        initializedField.SetValue(__instance, false);
        return false;
    }

    public static bool Prefix(Telemetry __instance)
    {
        UnityEngine.Object.Destroy(__instance);
        return false;
    }

    public static bool Prefix()
    {
        return false;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_SDK, ((Func<SentrySdk, bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_MANAGER, ((Func<bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_MANAGER_ONENABLE, ((Func<bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_TELEMETRY, ((Func<Telemetry, bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_TELEMETRY_QUIT, ((Func<bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_GAMEANALYTICS_SEND_EVENT, ((Func<bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_GAMEANALYTICS_SEND_EVENTINFO, ((Func<bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_GAMEANALYTICS_SEND_LEGACYEVENT, ((Func<bool>)Prefix).Method);
        PatchPrefix(harmony, TARGET_METHOD_SPAWNER_ANALYTICS, ((Func<bool>)Prefix).Method);
    }
}
