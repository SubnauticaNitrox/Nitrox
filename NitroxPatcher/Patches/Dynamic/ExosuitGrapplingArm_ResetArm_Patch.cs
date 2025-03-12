using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitGrapplingArm_ResetArm_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitGrapplingArm t) => ((IExosuitArm)t).ResetArm());

    public static void Prefix(ExosuitGrapplingArm __instance)
    {
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance.exosuit, __instance, ExosuitArmAction.END_USE_TOOL);
    }
}
