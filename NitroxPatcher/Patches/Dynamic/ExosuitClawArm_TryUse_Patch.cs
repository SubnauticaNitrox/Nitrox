using System.Reflection;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.GameLogic;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitClawArm_TryUse_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitClawArm t) => t.TryUse(out Reflect.Ref<float>.Field));

    public static void Postfix(bool __result, ExosuitClawArm __instance, float ___cooldownTime)
    {
        if (__result)
        {
            ExosuitArmAction action;

            // Check cooldown to determine if the arm is picking up something or punching something
            if (___cooldownTime == __instance.cooldownPickup)
            {
                action = ExosuitArmAction.START_USE_TOOL;
            }
            else if (___cooldownTime == __instance.cooldownPunch)
            {
                action = ExosuitArmAction.ALT_HIT;
            }
            else
            {
                Log.Error("Cooldown time does not match pickup or punch time");
                return;
            }

            Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitClawArmModule, __instance.exosuit, __instance, action);
        }
    }
}
