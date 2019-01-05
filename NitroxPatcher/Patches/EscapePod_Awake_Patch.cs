using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches
{
    public class EscapePod_Awake_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EscapePod);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix(EscapePod __instance)
        {
            return !EscapePodManager.SURPRESS_ESCAPE_POD_AWAKE_METHOD;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
