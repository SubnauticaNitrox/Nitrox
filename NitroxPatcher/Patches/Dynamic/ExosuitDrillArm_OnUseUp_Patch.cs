using System.Reflection;
using NitroxClient.GameLogic;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitDrillArm_OnUseUp_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitDrillArm t) => ((IExosuitArm)t).OnUseUp(out Reflect.Ref<float>.Field));

    public static void Prefix(ExosuitDrillArm __instance)
    {
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitDrillArmModule, __instance.exosuit, __instance, ExosuitArmAction.END_USE_TOOL);
    }
}
