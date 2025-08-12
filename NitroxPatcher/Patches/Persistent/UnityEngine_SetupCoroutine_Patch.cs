using System;
using System.Reflection;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent;

/// <summary>
/// Makes sure that Coroutine aren't fully blocked once they encounter an Exception
/// </summary>
public sealed partial class UnityEngine_SetupCoroutine_Patch : NitroxPatch, IPersistentPatch
{
    // UnityEngine DLLs aren't publicized so we can't access this class as done in other patches
    public static readonly MethodInfo TARGET_METHOD = Assembly.GetAssembly(typeof(GameObject))
                                                              .GetType("UnityEngine.SetupCoroutine")
                                                              .GetMethod("InvokeMoveNext", BindingFlags.Public | BindingFlags.Static);

    public static Exception Finalizer(Exception __exception)
    {
        if (__exception != null)
        {
            Log.Error(__exception);
        }
        return null;
    }
}
