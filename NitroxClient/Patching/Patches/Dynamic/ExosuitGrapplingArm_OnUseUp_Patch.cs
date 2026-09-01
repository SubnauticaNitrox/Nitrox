using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class ExosuitGrapplingArm_OnUseUp_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitGrapplingArm t) => ((IExosuitArm)t).OnUseUp(out Reflect.Ref<float>.Field));

    public static void Prefix(ExosuitGrapplingArm __instance)
    {
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance.exosuit, __instance, ExosuitArmAction.END_USE_TOOL);
    }
}
