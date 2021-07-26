using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Persistent
{
    internal class SentrySdk_Start_Patch : NitroxPatch, IPersistentPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SentrySdk);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Start", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(SentrySdk __instance)
        {
            UnityEngine.Object.Destroy(__instance);
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
