using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

public sealed partial class ExosuitDrillArm_OnUseDown_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitDrillArm t) => ((IExosuitArm)t).OnUseDown(out Reflect.Ref<float>.Field));

    public static void Prefix(ExosuitDrillArm __instance)
    {
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitDrillArmModule, __instance.exosuit, __instance, ExosuitArmAction.START_USE_TOOL);
    }
}
