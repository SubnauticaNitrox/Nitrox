using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class ExosuitGrapplingArm_FixedUpdate_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((ExosuitGrapplingArm t) => t.FixedUpdate());

    public static void Postfix(ExosuitGrapplingArm __instance)
    {
        if (__instance.exosuit.TryGetIdOrWarn(out NitroxId id) &&
            Resolve<SimulationOwnership>().HasAnyLockType(id) &&
            !__instance.hook.resting)
        {
            Exosuit.Arm armSide = ExosuitModuleEvent.GetArmSide(__instance);
            Rigidbody rb = __instance.hook.RequireComponent<Rigidbody>();
            Resolve<IPacketSender>().Send(new GrapplingHookMovement(id, armSide, rb.position.ToDto(), rb.velocity.ToDto(), rb.rotation.ToDto()));
        }
    }
}
