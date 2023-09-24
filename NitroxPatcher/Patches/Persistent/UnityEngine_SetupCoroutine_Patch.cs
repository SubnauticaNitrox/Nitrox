using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Makes sure that Coroutine aren't fully blocked once they encounter an Exception
/// </summary>
public sealed partial class UnityEngine_SetupCoroutine_Patch : NitroxPatch, IPersistentPatch
{
    public static readonly MethodInfo TARGET_METHOD = Assembly.GetAssembly(typeof(GameObject))
                                                              .GetType("UnityEngine.SetupCoroutine")
                                                              .GetMethod("InvokeMoveNext", BindingFlags.Public | BindingFlags.Static);

    public static Exception Finalizer(Exception __exception)
    {
        if (__exception == null)
        {
            return null;
        }
        // TODO: Remove before merging
        Log.Info($"Finalizer skipped an exception: {__exception}");
        return null;
    }
}
