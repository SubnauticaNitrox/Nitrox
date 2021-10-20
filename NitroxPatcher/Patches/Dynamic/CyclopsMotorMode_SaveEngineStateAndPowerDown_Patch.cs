using System.Reflection;
using HarmonyLib;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    public class CyclopsMotorMode_SaveEngineStateAndPowerDown_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsMotorMode t) => t.SaveEngineStateAndPowerDown());

        public static bool Prefix(CyclopsMotorMode __instance)
        {
            // SN disable the engine if the player leave the cyclops. So this must be avoided.
            __instance.engineOnOldState = __instance.engineOn;
            return false;
        }

        public override void Patch(Harmony harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}
