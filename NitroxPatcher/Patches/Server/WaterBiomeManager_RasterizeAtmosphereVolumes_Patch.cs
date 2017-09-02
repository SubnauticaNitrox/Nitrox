using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class WaterBiomeManager_RasterizeAtmosphereVolumes_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(WaterBiomeManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RasterizeAtmosphereVolumes", BindingFlags.NonPublic | BindingFlags.Instance);

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
