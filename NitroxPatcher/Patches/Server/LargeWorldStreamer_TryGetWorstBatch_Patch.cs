using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class LargeWorldStreamer_TryGetWorstBatch_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LargeWorldStreamer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryGetWorstBatch", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static bool Prefix(out bool __result, out Int3 worst)
        {
            worst = Int3.zero;
            __result = false;
            return false;
        }        

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
