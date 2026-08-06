using System.Reflection;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class CyclopsHornButton_OnPress_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsHornButton t) => t.OnPress());

    public static bool Prefix(CyclopsHornButton __instance)
    {
        Resolve<VehicleHorns>().HandleLocalHonk(__instance.subRoot.gameObject, __instance.hornSFX);
        return false;
    }
}
