using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using UnityEngine;

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
        typeof(AttackLastTarget), typeof(AttackCyclops)
    ];

    /// <summary>
    /// In the future, ensure all creatures are synced. We want each of them to be individually
    /// checked (that all their actions are synced) before marking them as synced.
    /// </summary>
    private readonly HashSet<Type> syncedCreatureWhitelist =
    [
        typeof(ReaperLeviathan), typeof(SeaDragon)
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

        ErrorMessage.AddMessage($"[SEND] reaper action: {newAction.GetType().FullName}");
        packetSender.Send(new CreatureActionChanged(creatureId, newAction.GetType().FullName));
    }

    public void CreatureActionChanged(NitroxId id, string creatureActionTypeName)
    {
        if (!NitroxEntity.TryGetComponentFrom(id, out Creature creature))
        {
            return;
        }
        if (cachedCreatureActionTypeByFullName.TryGetValue(creatureActionTypeName, out Type creatureActionType))
        {
            ErrorMessage.AddMessage($"[GET] {creatureActionType}");
            if (creature.TryGetComponent(creatureActionType, out Component component) && component is CreatureAction creatureAction)
            {
                actions[creature] = creatureAction;
            }
        }
    }

    public static void AggressiveWhenSeeTargetChanged(NitroxId creatureId, NitroxId targetId, bool locked, float aggressionAmount)
    {
        if (!NitroxEntity.TryGetComponentFrom(creatureId, out AggressiveWhenSeeTarget aggressiveWhenSeeTarget) ||
            !NitroxEntity.TryGetObjectFrom(targetId, out GameObject targetObject))
        {
            return;
        }

        ErrorMessage.AddMessage($"[GET] {aggressiveWhenSeeTarget.gameObject.name} chases {targetObject.name}");

        Creature creature = aggressiveWhenSeeTarget.creature;

        // Code from AggressiveWhenSeeTarget.ScanForAggressionTarget
        creature.Aggression.Value = aggressionAmount;
        LastTarget lastTarget = aggressiveWhenSeeTarget.lastTarget;
        lastTarget.SetTargetInternal(targetObject);
        lastTarget.targetLocked = locked;

        if (aggressiveWhenSeeTarget.sightedSound != null && !aggressiveWhenSeeTarget.sightedSound.GetIsPlaying())
        {
            // TODO: Adapt this code when #1780 is merged
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

        ErrorMessage.AddMessage($"[GET] {attackCyclops.gameObject.name} attacks {targetObject.name}");

        // Kinda stuff from AttackCyclops.UpdateAggression
        attackCyclops.aggressiveToNoise.Value = aggressiveToNoiseAmount;
        // Force currentTarget to null to ensure SetCurrentTarget detects a change
        attackCyclops.currentTarget = null;
        attackCyclops.SetCurrentTarget(targetObject, targetObject.GetComponent<CyclopsDecoy>());
    }

    public bool TryGetActionForCreature(Creature creature, out CreatureAction action)
    {
        // TODO: Fix ondeath cinematic being played for all players when getting bitten by a reaper
        // TODO: When #2043 is merged, blacklist the cinematic
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
