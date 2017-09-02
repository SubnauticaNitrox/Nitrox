using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class WaterSurfaceOnCamera_OnPostRender_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(WaterSurfaceOnCamera);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPostRender", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Prefix()
        {
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
