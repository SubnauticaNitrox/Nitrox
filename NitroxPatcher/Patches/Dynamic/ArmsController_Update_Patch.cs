using System.Reflection;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ArmsController_Update_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((ArmsController t) => t.Update());

    public static bool Prefix(ArmsController __instance)
    {
        if (__instance.smoothSpeedAboveWater == 0)
        {
            if (__instance.reconfigureWorldTarget)
            {
                __instance.Reconfigure(null);
                __instance.reconfigureWorldTarget = false;
            }

            __instance.leftAim.Update(__instance.ikToggleTime);
            __instance.rightAim.Update(__instance.ikToggleTime);
            __instance.UpdateHandIKWeights();
            return false;
        }

        return true;
    }
}
