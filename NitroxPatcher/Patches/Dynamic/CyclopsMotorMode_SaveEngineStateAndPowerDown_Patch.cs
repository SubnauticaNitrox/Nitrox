using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsMotorMode_SaveEngineStateAndPowerDown_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsMotorMode t) => t.SaveEngineStateAndPowerDown());

    public static bool Prefix(CyclopsMotorMode __instance)
    {
        // SN disable the engine if the player leave the cyclops. So this must be avoided.
        __instance.engineOnOldState = __instance.engineOn;
        return false;
    }
}
