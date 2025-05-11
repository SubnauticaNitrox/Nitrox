using System.Reflection;
using NitroxClient.GameLogic;
using NitroxModel.Helper;
using NitroxModel_Subnautica.Packets;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitPropulsionArm_Update_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitPropulsionArm t) => ((IExosuitArm)t).Update(ref Reflect.Ref<Quaternion>.Field));

    public static void Prefix(ExosuitPropulsionArm __instance, out bool __state)
    {
        __state = __instance.propulsionCannon.animator.GetBool(ExosuitModuleEvent.useToolAnimation);
    }

    public static void Postfix(ExosuitPropulsionArm __instance, ref bool __state)
    {
        bool isUsing = __instance.propulsionCannon.animator.GetBool(ExosuitModuleEvent.useToolAnimation);

        if (__state != isUsing)
        {
            ExosuitArmAction action = isUsing ? ExosuitArmAction.START_USE_TOOL : ExosuitArmAction.END_USE_TOOL;
            Resolve<ExosuitModuleEvent>().BroadcastArmAction(TechType.ExosuitPropulsionArmModule, __instance.exosuit, __instance, action);
        }
    }
}
