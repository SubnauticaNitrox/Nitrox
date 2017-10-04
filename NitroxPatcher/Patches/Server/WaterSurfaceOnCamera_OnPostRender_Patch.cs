using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class WaterSurfaceOnCamera_OnPostRender_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(WaterSurfaceOnCamera);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("OnPostRender", BindingFlags.NonPublic | BindingFlags.Instance);
        
        private static bool isHeadless = (Array.IndexOf(Environment.GetCommandLineArgs(), "-nographics") >= 0);

        public static bool Prefix()
        {
            return !isHeadless;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
