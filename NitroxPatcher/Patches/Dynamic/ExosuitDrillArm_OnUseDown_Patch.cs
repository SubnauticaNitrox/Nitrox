using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitDrillArm_OnUseDown_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method((ExosuitTorpedoArm t) => ((IExosuitArm)t).OnUseDown(out Reflect.Ref<float>.Field));

    public static void Prefix(ExosuitDrillArm __instance)
    {
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitDrillArmModule, __instance, ExosuitArmAction.START_USE_TOOL);
    }
}
