using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitGrapplingArm_OnHit_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitGrapplingArm t) => t.OnHit());

    public static bool Prefix(ExosuitGrapplingArm __instance)
    {
        Exosuit componentInParent = __instance.GetComponentInParent<Exosuit>();

        if (componentInParent != null)
        {
            if (!componentInParent.GetPilotingMode())
            {
                // We suppress this method if it is called from another player pilot, so we can use our own implementation
                // See: ExosuitModuleEvents.UseGrapplingarm -> onHit Section
                return false;
            }
        }

        return true;
    }

    public static void Postfix(ExosuitGrapplingArm __instance)
    {
        Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitGrapplingArmModule, __instance.exosuit, __instance, ExosuitArmAction.START_USE_TOOL);
    }
}
