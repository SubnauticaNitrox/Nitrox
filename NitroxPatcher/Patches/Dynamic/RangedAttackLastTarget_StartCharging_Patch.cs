using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Prevents non simulating players from running locally <see cref="RangedAttackLastTarget.StartCharging"/>.
/// Broadcasts this event on the simulating player.
/// </summary>
public sealed partial class RangedAttackLastTarget_StartCharging_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((RangedAttackLastTarget t) => t.StartCharging(default));

    public static void Prefix(RangedAttackLastTarget __instance)
    {
        BroadcastRangedAttack(__instance, RangedAttackLastTargetUpdate.ActionState.CHARGING);
    }

    /// <summary>
    /// Broadcasts a range attack CHARGING or CASTING state if the attacking creature is whitelisted, valid and if the target is valid.
    /// </summary>
    /// <returns>
    /// true if the broadcast was done (all conditions met)
    /// </returns>
    public static void BroadcastRangedAttack(RangedAttackLastTarget attackBehaviour, RangedAttackLastTargetUpdate.ActionState actionState)
    {
        // should action be broadcasted
        if (!Resolve<AI>().IsCreatureWhitelisted(attackBehaviour.creature))
        {
            return;
        }

        // Attacker object validity
        if (!attackBehaviour.TryGetNitroxId(out NitroxId creatureId) || !Resolve<SimulationOwnership>().HasAnyLockType(creatureId))
        {
            return;
        }

        // Target validity
        if (!attackBehaviour.currentTarget || !attackBehaviour.currentTarget.TryGetNitroxId(out NitroxId targetId))
        {
            return;
        }

        int attackTypeIndex = attackBehaviour.attackTypes.GetIndex(attackBehaviour.currentAttack);

        Resolve<IPacketSender>().Send(new RangedAttackLastTargetUpdate(creatureId, targetId, attackTypeIndex, actionState));
    }
}
