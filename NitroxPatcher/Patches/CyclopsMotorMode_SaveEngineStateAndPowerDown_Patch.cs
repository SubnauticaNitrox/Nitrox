using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches.Client
{
    public class CyclopsMotorMode_SaveEngineStateAndPowerDown_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsMotorMode);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SaveEngineStateAndPowerDown", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsMotorMode __instance)
        {
            __instance.ReflectionSet("engineOnOldState", __instance.engineOn);
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
