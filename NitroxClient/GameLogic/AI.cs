using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;
using static NitroxModel.Packets.RangedAttackLastTargetUpdate;

namespace NitroxClient.GameLogic;

public class AI
{
    private readonly IPacketSender packetSender;
    private readonly Dictionary<Creature, CreatureAction> actions = [];
    private readonly Dictionary<string, Type> cachedCreatureActionTypeByFullName;

    /// <summary>
    /// Contains the types derived from CreatureAction which should be synced.
    /// Actions concerning the creature movement should be ignored as it's already done through SplineFollowing
    /// </summary>
    private readonly HashSet<Type> creatureActionWhitelist =
    [
        typeof(AttackLastTarget), typeof(RangedAttackLastTarget), typeof(AttackCyclops), typeof(Poop)
    ];

    /// <summary>
    /// In the future, ensure all creatures are synced. We want each of them to be individually
    /// checked (that all their actions are synced) before marking them as synced.
    /// </summary>
    private readonly HashSet<Type> syncedCreatureWhitelist =
    [
        typeof(ReaperLeviathan), typeof(SeaDragon), typeof(SeaTreader), typeof(GhostLeviathan)
    ];

    public AI(IPacketSender packetSender)
    {
        this.packetSender = packetSender;
        Assembly assembly = Assembly.GetAssembly(typeof(CreatureAction));
        cachedCreatureActionTypeByFullName = assembly.GetTypes()
                                                     .Where(type => typeof(CreatureAction).IsAssignableFrom(type))
                                                     .ToDictionary(t => t.FullName, t => t);
    }

    public void BroadcastNewAction(NitroxId creatureId, Creature creature, CreatureAction newAction)
    {
        if (!syncedCreatureWhitelist.Contains(creature.GetType()))
        {
            return;
        }

        packetSender.Send(new CreatureActionChanged(creatureId, newAction.GetType().FullName));
    }

    public void CreatureActionChanged(NitroxId id, string creatureActionTypeName)
    {
        if (!NitroxEntity.TryGetComponentFrom(id, out Creature creature))
        {
            return;
        }
        if (cachedCreatureActionTypeByFullName.TryGetValue(creatureActionTypeName, out Type creatureActionType) &&
            creature.TryGetComponent(creatureActionType, out Component component) && component is CreatureAction creatureAction)
        {
            actions[creature] = creatureAction;
        }
    }
#if SUBNAUTICA
    public static void AggressiveWhenSeeTargetChanged(NitroxId creatureId, NitroxId targetId, bool locked, float aggressionAmount)
#elif BELOWZERO
    public static void AggressiveWhenSeeTargetChanged(NitroxId creatureId, NitroxId targetId, bool locked, float aggressionAmount, float targetPriority)
#endif
    {
        if (!NitroxEntity.TryGetComponentFrom(creatureId, out AggressiveWhenSeeTarget aggressiveWhenSeeTarget) ||
            !NitroxEntity.TryGetObjectFrom(targetId, out GameObject targetObject))
        {
            return;
        }

        Creature creature = aggressiveWhenSeeTarget.creature;

        // Code from AggressiveWhenSeeTarget.ScanForAggressionTarget
        creature.Aggression.Value = aggressionAmount;
        LastTarget lastTarget = aggressiveWhenSeeTarget.lastTarget;
#if SUBNAUTICA
        lastTarget.SetTargetInternal(targetObject);
#elif BELOWZERO
        lastTarget.SetTargetInternal(targetObject, targetPriority);
#endif
        lastTarget.targetLocked = locked;

        if (aggressiveWhenSeeTarget.sightedSound && !aggressiveWhenSeeTarget.sightedSound.GetIsPlaying())
        {
            // This call doesn't broadcast a sound packet
            aggressiveWhenSeeTarget.sightedSound.StartEvent();
        }

        if (creature.TryGetComponent(out AttackLastTarget attackLastTarget))
        {
            attackLastTarget.currentTarget = targetObject;
        }
    }

    public static void AttackCyclopsTargetChanged(NitroxId creatureId, NitroxId targetId, float aggressiveToNoiseAmount)
    {
        if (!NitroxEntity.TryGetComponentFrom(creatureId, out AttackCyclops attackCyclops) ||
            !NitroxEntity.TryGetObjectFrom(targetId, out GameObject targetObject))
        {
            return;
        }

        // Kinda stuff from AttackCyclops.UpdateAggression
        attackCyclops.aggressiveToNoise.Value = aggressiveToNoiseAmount;
        // Force currentTarget to null to ensure SetCurrentTarget detects a change
        attackCyclops.currentTarget = null;
        attackCyclops.SetCurrentTarget(targetObject, targetObject.GetComponent<CyclopsDecoy>());
    }

    public static void RangedAttackLastTargetUpdate(NitroxId creatureId, NitroxId targetId, int attackTypeIndex, ActionState state)
    {
        if (!NitroxEntity.TryGetComponentFrom(creatureId, out RangedAttackLastTarget rangedAttackLastTarget) ||
            !NitroxEntity.TryGetObjectFrom(targetId, out GameObject targetObject))
        {
            return;
        }

        RangedAttackLastTarget.RangedAttackType attackType = rangedAttackLastTarget.attackTypes[attackTypeIndex];
        rangedAttackLastTarget.currentAttack = attackType;
        rangedAttackLastTarget.currentTarget = targetObject;

        switch (state)
        {
            case ActionState.CHARGING:
                rangedAttackLastTarget.StartCharging(attackType);
                break;
            case ActionState.CASTING:
                rangedAttackLastTarget.StartCasting(attackType);
                break;
        }
    }

    public static void CreaturePoopPerformed(NitroxId creatureId)
    {
        if (NitroxEntity.TryGetComponentFrom(creatureId, out Poop poop))
        {
            // Code from Poop.Perform
            SafeAnimator.SetBool(poop.creature.GetAnimator(), poop.animationParameterName, false);
            poop.recourceSpawned = true;
        }
    }

    public bool TryGetActionForCreature(Creature creature, out CreatureAction action)
    {
        return actions.TryGetValue(creature, out action);
    }

    public bool IsCreatureActionWhitelisted(CreatureAction creatureAction)
    {
        return creatureActionWhitelist.Contains(creatureAction.GetType());
    }

    public bool IsCreatureWhitelisted(Creature creature)
    {
        return syncedCreatureWhitelist.Contains(creature.GetType());
    }
}
