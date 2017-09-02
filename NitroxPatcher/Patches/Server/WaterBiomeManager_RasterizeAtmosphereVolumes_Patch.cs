using Harmony;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Server
{
    public class WaterBiomeManager_RasterizeAtmosphereVolumes_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(WaterBiomeManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("RasterizeAtmosphereVolumes", BindingFlags.NonPublic | BindingFlags.Instance);

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
