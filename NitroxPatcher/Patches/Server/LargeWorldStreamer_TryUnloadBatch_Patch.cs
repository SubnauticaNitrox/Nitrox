using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class LargeWorldStreamer_TryUnloadBatch_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(LargeWorldStreamer);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("TryUnloadBatch", BindingFlags.NonPublic | BindingFlags.Instance);
        
        public static bool Prefix(bool __result)
        {
            __result = false;
            return NitroxServer.Server.ALLOW_MAP_CLIPPING;
        }        

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
