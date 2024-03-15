using System;
using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
///     Patch to suppress SentrySdk NRE as it's destroyed by us
/// </summary>
public sealed partial class SystemsSpawner_SetupSingleton_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(Reflect.Method((SystemsSpawner s) => s.SetupSingleton(default)));

    public static void Finalizer(ref Exception __exception)
    {
        if (__exception is NullReferenceException)
        {
            __exception = null;
        }
    }
}
