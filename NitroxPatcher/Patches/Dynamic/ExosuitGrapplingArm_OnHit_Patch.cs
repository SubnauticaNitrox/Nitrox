using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitGrapplingArm_OnHit_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitGrapplingArm t) => t.OnHit());

    public static bool Prefix(ExosuitGrapplingArm __instance)
    {
        if (!__instance.exosuit.GetPilotingMode())
        {
            // We suppress this method if it is called from another player pilot, so we can use our own implementation
            return false;
        }

        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance.exosuit, __instance, ExosuitArmAction.START_USE_TOOL);
        return true;
    }
}
