using Harmony;
using NitroxClient.Communication.Packets.Processors;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class EscapePod_Awake_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(EscapePod);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("Awake", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static bool Prefix(EscapePod __instance)
        {
            return !BroadcastEscapePodsProcessor.SURPRESS_ESCAPE_POD_AWAKE_METHOD;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
