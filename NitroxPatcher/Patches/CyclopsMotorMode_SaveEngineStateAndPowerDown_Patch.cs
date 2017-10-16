using Harmony;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CyclopsMotorMode_SaveEngineStateAndPowerDown_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CyclopsMotorMode);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("SaveEngineStateAndPowerDown", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CyclopsMotorMode __instance)
        {
            //SN disable the engine if the player leave the cyclops. So this must be avoided.
            __instance.ReflectionSet("engineOnOldState", __instance.engineOn);
            return false;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
