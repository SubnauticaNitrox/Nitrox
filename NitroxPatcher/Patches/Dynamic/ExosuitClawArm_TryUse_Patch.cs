using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitClawArm_TryUse_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitClawArm t) => t.TryUse(out Reflect.Ref<float>.Field));

    public static void Postfix(bool __result, ExosuitClawArm __instance, float ___cooldownTime)
    {
        if (__result)
        {
            NitroxServiceLocator.LocateService<ExosuitModuleEvent>().BroadcastClawUse(__instance, ___cooldownTime);
        }
    }
}
