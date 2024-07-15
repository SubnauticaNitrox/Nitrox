using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;
using NitroxModel;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents non simulating players from running locally <see cref="RangedAttackLastTarget.StartCasting"/>.
/// Broadcasts this event on the simulating player.
/// </summary>
public sealed partial class RangedAttackLastTarget_StartCharging_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((RangedAttackLastTarget t) => t.StartCharging(default));

    public static void Prefix(RangedAttackLastTarget __instance)
    {
        if (!Resolve<AI>().IsCreatureWhitelisted(__instance.creature) ||
            !__instance.TryGetNitroxId(out NitroxId creatureId) ||
            !Resolve<SimulationOwnership>().HasAnyLockType(creatureId) ||
            !__instance.currentTarget || !__instance.currentTarget.TryGetNitroxId(out NitroxId targetId))
        {
            return;
        }

        int attackTypeIndex = __instance.attackTypes.GetIndex(__instance.currentAttack);

        Resolve<IPacketSender>().Send(new RangedAttackLastTargetUpdate(creatureId, targetId, attackTypeIndex, RangedAttackLastTargetUpdate.ActionState.CHARGING));
        ErrorMessage.AddMessage($"[SEND] {__instance.name} charges against {__instance.currentTarget.name}");
    }
}

