using System;
using System.Reflection;
using HarmonyLib;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CrashHome_Spawn_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CrashHome);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Spawn", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix() // Disables Crashfish automatic spawning on the client
        {
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
