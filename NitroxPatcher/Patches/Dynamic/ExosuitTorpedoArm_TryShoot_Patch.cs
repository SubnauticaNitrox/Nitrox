using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitTorpedoArm_TryShoot_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitTorpedoArm t) => t.TryShoot(out Reflect.Ref<float>.Field, default));

    public static void Postfix(ExosuitTorpedoArm __instance)
    {
        ExosuitArmAction action = __instance.animator.GetBool(ExosuitModuleEvent.UseToolAnimation) ? ExosuitArmAction.START_USE_TOOL : ExosuitArmAction.END_USE_TOOL;
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitTorpedoArmModule, __instance.exosuit, __instance, action);
    }
}
