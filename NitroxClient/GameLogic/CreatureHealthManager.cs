using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxClient.GameLogic;

/// <summary>
///     Makes the simulating player the only one to change a creature's health so that the damage of every player adds up:<br/>
///     - a player who doesn't simulate a creature doesn't change its health, the hits they cause are sent to the simulating player
///     (<see cref="CreatureDamageAction" />) who applies them like its own<br/>
///     - the simulating player shares the resulting health (<see cref="CreatureHealthChanged" />) so that the next simulating player continues from it
/// </summary>
/// <remarks>
///     Deaths are still replicated by RemoveCreatureCorpse (or EntityDestroyed). Vehicles and the Cyclops are handled by <see cref="LiveMixinManager" />.
/// </remarks>
public class CreatureHealthManager
{
    private const float HEALTH_BROADCAST_INTERVAL = 0.2f;
    private const float HIT_BATCH_INTERVAL = 0.1f;
    private const float OUTGOING_HIT_IDLE_TIME = 5f;
    private const float FORWARDED_HIT_WAIT_TIME = 1f;

    private readonly IPacketSender packetSender;
    private readonly ThrottledPacketSender throttledPacketSender;
    private readonly SimulationOwnership simulationOwnership;

    private readonly Dictionary<(NitroxId CreatureId, DamageType Type), OutgoingHit> outgoingHits = [];
    private readonly List<(NitroxId CreatureId, DamageType Type)> idleOutgoingHitKeys = [];
    private readonly List<(CreatureDamageAction Packet, float ReceivedTime)> pendingHits = [];
    private readonly DamageInfo cosmeticDamageInfo = new();

    /// <summary>
    ///     Creature currently taking a hit from <see cref="TryApplyForwardedHit" />.
    /// </summary>
    private LiveMixin remoteDamageTarget;

    /// <summary>
    ///     Above 0 while a local player's attack which gives no dealer to LiveMixin.TakeDamage runs (Prawn suit claw and drill, punching a bleeder).
    /// </summary>
    public int LocalAttackCount;

    public CreatureHealthManager(IPacketSender packetSender, ThrottledPacketSender throttledPacketSender, SimulationOwnership simulationOwnership)
    {
        this.packetSender = packetSender;
        this.throttledPacketSender = throttledPacketSender;
        this.simulationOwnership = simulationOwnership;
    }

    /// <summary>
    ///     Creatures whose health only changes for their simulating player. Their death must be replicable because only that player kills them
    ///     (CreatureDeath_OnKillAsync_Patch, or LiveMixin_Kill_Patch for destroyOnDeath).
    /// </summary>
    public static bool IsHealthSynced(LiveMixin liveMixin, [NotNullWhen(true)] out NitroxId? creatureId)
    {
        creatureId = null;
        return liveMixin.GetComponent<Creature>() &&
               (liveMixin.destroyOnDeath || liveMixin.GetComponent<CreatureDeath>()) &&
               liveMixin.TryGetNitroxId(out creatureId);
    }

    /// <returns>True if the damage must change the local health</returns>
    public bool ShouldApplyDamage(LiveMixin liveMixin, NitroxId creatureId, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
    {
        // A hit forwarded to us, or our own hit sent back because nobody simulates the creature
        if (remoteDamageTarget == liveMixin)
        {
            return true;
        }

        HitSource source = GetHitSource(dealer, type);

        if (simulationOwnership.HasAnyLockType(creatureId))
        {
            // The player simulating the dealer computes this hit on its side and forwards it to us
            return source != HitSource.RemoteCopy;
        }

        if (source == HitSource.World && !liveMixin.GetComponent<RemotelyControlled>())
        {
            // Nobody streams this creature to us so it's most likely not simulated at all (e.g. creatures the server doesn't simulate):
            // it keeps being damaged locally like before instead of becoming immune to the environment and other creatures
            return true;
        }

        if (!liveMixin.IsAlive() || liveMixin.invincible)
        {
            return false;
        }

        if (source == HitSource.Local && originalDamage > 0f)
        {
            ForwardHit(liveMixin, creatureId, originalDamage, position, type, dealer);
        }

        // The health stays untouched but the creature still reacts locally like before (effects, flinch, a reaper releasing a grabbed vehicle)
        PlayCosmeticDamage(liveMixin, originalDamage, position, type, dealer);
        return false;
    }

    /// <returns>True if the heal must change the local health</returns>
    public bool ShouldApplyHeal(LiveMixin liveMixin, NitroxId creatureId)
    {
        return simulationOwnership.HasAnyLockType(creatureId) || !liveMixin.GetComponent<RemotelyControlled>();
    }

    /// <summary>
    ///     Applies a hit that the server routed to us, whether or not we hold the lock: the server's routing decides,
    ///     and our own hit comes back when nobody simulates the creature.
    /// </summary>
    public void ReceiveForwardedHit(CreatureDamageAction packet)
    {
        if (!TryApplyForwardedHit(packet))
        {
            pendingHits.Add((packet, Time.time));
        }
    }

    /// <summary>
    ///     Called after a creature's local health changed (LiveMixin.TakeDamage and LiveMixin.AddHealth postfixes).
    /// </summary>
    public void OnHealthChanged(LiveMixin liveMixin)
    {
        if (!IsHealthSynced(liveMixin, out NitroxId creatureId))
        {
            return;
        }

        // Not the bare id: Entities.BroadcastMetadataUpdate throttles EntityMetadataUpdate with the id as key in the same sender
        object dedupeKey = (typeof(CreatureHealthChanged), creatureId);
        // Checked before the lock: CreatureDeath_OnKillAsync_Patch already released it during Kill
        if (!liveMixin.IsAlive())
        {
            throttledPacketSender.RemovePendingPackets(dedupeKey);
            return;
        }

        if (simulationOwnership.HasAnyLockType(creatureId))
        {
            throttledPacketSender.SendThrottled(new CreatureHealthChanged(creatureId, liveMixin.health), _ => dedupeKey, HEALTH_BROADCAST_INTERVAL);
        }
    }

    public void ApplyRemoteHealth(CreatureHealthChanged packet)
    {
        // The simulating player's own value is the real one, which also makes it ignore late reports of the previous simulating player
        if (!(packet.Health > 0f) || simulationOwnership.HasAnyLockType(packet.CreatureId))
        {
            return;
        }

        // The id can belong to what the creature turned into (e.g. a cooked fish), and a dead creature only changes through RemoveCreatureCorpse
        if (!NitroxEntity.TryGetComponentFrom(packet.CreatureId, out LiveMixin liveMixin) || !liveMixin.IsAlive() || !liveMixin.GetComponent<Creature>())
        {
            return;
        }

        // Written directly so that no damage receiver, effect or death runs. Never 0, otherwise RemoveCreatureCorpseProcessor would skip the death replay
        liveMixin.health = Mathf.Min(packet.Health, liveMixin.maxHealth);
        // Cold and poison damage heal back over time on the simulating player's side, whose health is what we just received
        if (liveMixin.tempDamage > 0f)
        {
            liveMixin.tempDamage = 0f;
        }
    }

    /// <summary>
    ///     Called every frame by <see cref="Multiplayer" />.
    /// </summary>
    public void Update()
    {
        float now = Time.time;
        FlushOutgoingHits(now);
        RetryPendingHits(now);
    }

    private HitSource GetHitSource(GameObject dealer, DamageType type)
    {
        // Prawn suit claw and drill, punching a bleeder: no dealer (see the *_OnHit_Patch scopes)
        if (LocalAttackCount > 0)
        {
            return HitSource.Local;
        }
        if (!dealer)
        {
            return HitSource.World;
        }
        // Knife and thermoblade
        if (dealer == Player.mainObject)
        {
            return HitSource.Local;
        }
        // Seamoth perimeter defense: spawned under the seamoth for its pilot (SeaMoth.OnUpgradeModuleUse) and as a copy for the others (SeamothModuleActionProcessor)
        if (dealer.GetComponent<ElectricalDefense>())
        {
            Vehicle vehicle = dealer.GetComponentInParent<Vehicle>();
            return vehicle && IsSimulatedLocally(vehicle.gameObject) ? HitSource.Local : HitSource.RemoteCopy;
        }
        if (dealer.GetComponent<Creature>())
        {
            // Sea dragon attacks only run for the sea dragon's simulating player (SeaDragonMeleeAttack_*_Patch), other creatures' attacks run for everyone
            if (dealer.GetComponent<SeaDragon>())
            {
                return IsSimulatedLocally(dealer) ? HitSource.Local : HitSource.RemoteCopy;
            }
            // A creature thrown with a propulsion cannon deals impact damage (always Collide) like any thrown object, its other attacks don't change
            if (type != DamageType.Collide || !dealer.GetComponent<PropulseCannonAmmoHandler>())
            {
                return HitSource.World;
            }
        }
        // Only the local player's repulsion cannon throws an object which we don't simulate (propulsion cannon grabs take the lock),
        // and this throw isn't replicated to the other players
        if (type == DamageType.Collide && dealer.GetComponent<PropulseCannonAmmoHandler>() && !IsSimulatedLocally(dealer))
        {
            return HitSource.Local;
        }
        // Impacts of vehicles, the Cyclops and thrown objects: DealDamageOnImpact passes its own game object
        if (dealer.GetComponent<DealDamageOnImpact>())
        {
            if (IsSimulatedLocally(dealer))
            {
                return HitSource.Local;
            }
            return IsSimulatedRemotely(dealer) ? HitSource.RemoteCopy : HitSource.World;
        }
        return HitSource.World;
    }

    private bool IsSimulatedLocally(GameObject gameObject)
    {
        return gameObject.TryGetNitroxId(out NitroxId id) && simulationOwnership.HasAnyLockType(id);
    }

    private static bool IsSimulatedRemotely(GameObject gameObject)
    {
        // Vehicles and the Cyclops simulated by another player are moved by a MovementReplicator, other entities by RemotelyControlled
        return gameObject.GetComponent<MovementReplicator>() || gameObject.GetComponent<RemotelyControlled>();
    }

    private void ForwardHit(LiveMixin liveMixin, NitroxId creatureId, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
    {
        (NitroxId CreatureId, DamageType Type) key = (creatureId, type);
        if (!outgoingHits.TryGetValue(key, out OutgoingHit hit))
        {
            hit = new OutgoingHit();
            outgoingHits.Add(key, hit);
        }

        NitroxId? dealerId = null;
        if (dealer)
        {
            dealer.TryGetNitroxId(out dealerId);
        }

        hit.PendingDamage += originalDamage;
        // Like in LiveMixin.TakeDamage, no position means the creature's position
        hit.PendingLocalPosition = position == default(Vector3) ? Vector3.zero : liveMixin.transform.InverseTransformPoint(position);
        hit.PendingDealerId = dealerId;

        // The first hit goes immediately, the next ones within HIT_BATCH_INTERVAL are summed (e.g. the drill) and sent by FlushOutgoingHits
        float now = Time.time;
        if (now >= hit.LastSendTime + HIT_BATCH_INTERVAL)
        {
            SendHit(creatureId, type, hit, now);
        }
    }

    private void SendHit(NitroxId creatureId, DamageType type, OutgoingHit hit, float now)
    {
        Log.Debug($"[{nameof(CreatureHealthManager)}] Sending a hit of {hit.PendingDamage} ({type}) on creature {creatureId} to its simulating player");
        packetSender.Send(new CreatureDamageAction(creatureId, hit.PendingDamage, type, hit.PendingLocalPosition.ToDto(), Optional.OfNullable(hit.PendingDealerId), 0));
        hit.LastSendTime = now;
        hit.PendingDamage = 0f;
        hit.PendingDealerId = null;
    }

    private void FlushOutgoingHits(float now)
    {
        foreach (KeyValuePair<(NitroxId CreatureId, DamageType Type), OutgoingHit> pair in outgoingHits)
        {
            OutgoingHit hit = pair.Value;
            if (hit.PendingDamage > 0f)
            {
                if (now >= hit.LastSendTime + HIT_BATCH_INTERVAL)
                {
                    SendHit(pair.Key.CreatureId, pair.Key.Type, hit, now);
                }
            }
            else if (now >= hit.LastSendTime + OUTGOING_HIT_IDLE_TIME)
            {
                idleOutgoingHitKeys.Add(pair.Key);
            }
        }

        foreach ((NitroxId CreatureId, DamageType Type) key in idleOutgoingHitKeys)
        {
            outgoingHits.Remove(key);
        }
        idleOutgoingHitKeys.Clear();
    }

    private void RetryPendingHits(float now)
    {
        for (int i = pendingHits.Count - 1; i >= 0; i--)
        {
            (CreatureDamageAction packet, float receivedTime) = pendingHits[i];
            if (TryApplyForwardedHit(packet))
            {
                pendingHits.RemoveAt(i);
                continue;
            }
            if (now < receivedTime + FORWARDED_HIT_WAIT_TIME)
            {
                continue;
            }
            pendingHits.RemoveAt(i);
            Reroute(packet);
        }
    }

    private void Reroute(CreatureDamageAction packet)
    {
        if (packet.Hops >= CreatureDamageAction.MAX_HOPS)
        {
            Log.Debug($"[{nameof(CreatureHealthManager)}] Lost a hit of {packet.OriginalDamage} on creature {packet.CreatureId}: it isn't loaded here");
            return;
        }

        // We don't have the creature (anymore): the server sends the hit to whoever simulates it now
        packetSender.Send(new CreatureDamageAction(packet.CreatureId, packet.OriginalDamage, packet.Type, packet.LocalPosition, packet.DealerId, (byte)(packet.Hops + 1)));
    }

    /// <returns>False if the creature isn't loaded (yet), true if the hit was applied or dropped</returns>
    private bool TryApplyForwardedHit(CreatureDamageAction packet)
    {
        // Not spawned yet (the lock is deferred until SpawnEntities is done) or already unloaded
        if (!NitroxEntity.TryGetComponentFrom(packet.CreatureId, out LiveMixin liveMixin))
        {
            return false;
        }

        // Late hit on a dead creature, or the id now belongs to what the creature turned into (e.g. a cooked fish)
        if (!liveMixin.IsAlive() || !liveMixin.GetComponent<Creature>())
        {
            return true;
        }

        GameObject dealer = null;
        if (packet.DealerId.HasValue && NitroxEntity.TryGetObjectFrom(packet.DealerId.Value, out GameObject dealerObject))
        {
            dealer = dealerObject;
        }

        LiveMixin previousTarget = remoteDamageTarget;
        remoteDamageTarget = liveMixin;
        try
        {
            // A regular TakeDamage, NOT inside LiveMixinManager.IsRemoteHealthChanging: the creature reacts (FleeOnDamage, AggressiveOnDamage),
            // CreatureDeath.lastDamageWasHeat is set, and a death spawns the respawner and is broadcast (CreatureDeath_OnKillAsync_Patch)
            liveMixin.TakeDamage(packet.OriginalDamage, liveMixin.transform.TransformPoint(packet.LocalPosition.ToUnity()), packet.Type, dealer);
        }
        finally
        {
            remoteDamageTarget = previousTarget;
        }
        return true;
    }

    /// <summary>
    ///     What LiveMixin.TakeDamage does, without changing the health and without the low-health looping effect and Kill.
    /// </summary>
    private void PlayCosmeticDamage(LiveMixin liveMixin, float originalDamage, Vector3 position, DamageType type, GameObject dealer)
    {
        float damage = liveMixin.shielded ? 0f : DamageSystem.CalculateDamage(originalDamage, type, liveMixin.gameObject, dealer);
        Vector3 hitPosition = position == default(Vector3) ? liveMixin.transform.position : position;

        // The simulating player plays and broadcasts the real sounds
        using (FMODSystem.SuppressSendingSounds())
        {
            // DamageInfo.Clear doesn't reset the dealer
            cosmeticDamageInfo.Clear();
            cosmeticDamageInfo.originalDamage = originalDamage;
            cosmeticDamageInfo.damage = damage;
            cosmeticDamageInfo.position = hitPosition;
            cosmeticDamageInfo.type = type;
            cosmeticDamageInfo.dealer = dealer;
            liveMixin.NotifyAllAttachedDamageReceivers(cosmeticDamageInfo);

            if (liveMixin.shielded)
            {
                return;
            }

            if (liveMixin.damageClip && damage > 0f && damage >= liveMixin.minDamageForSound && type != DamageType.Radiation)
            {
                global::Utils.PlayEnvSound(liveMixin.damageClip, hitPosition, 20f);
            }
        }

        if (type == DamageType.Electrical)
        {
            if (liveMixin.electricalDamageEffect && Time.time > liveMixin.timeLastElecDamageEffect + 2.5f)
            {
                FixedBounds fixedBounds = liveMixin.GetComponent<FixedBounds>();
                Bounds bounds = fixedBounds ? fixedBounds.bounds : global::UWE.Utils.GetEncapsulatedAABB(liveMixin.gameObject, -1);
                GameObject effect = global::UWE.Utils.InstantiateWrap(liveMixin.electricalDamageEffect, bounds.center, Quaternion.identity);
                effect.transform.parent = liveMixin.transform;
                effect.transform.localScale = bounds.size * 0.65f;
                liveMixin.timeLastElecDamageEffect = Time.time;
            }
            return;
        }

        if (damage > 0f && liveMixin.damageEffect && Time.time > liveMixin.timeLastDamageEffect + 1f &&
            type is DamageType.Normal or DamageType.Collide or DamageType.Explosive or DamageType.Puncture or DamageType.LaserCutter or DamageType.Drill)
        {
            global::Utils.SpawnPrefabAt(liveMixin.damageEffect, liveMixin.transform, hitPosition);
            liveMixin.timeLastDamageEffect = Time.time;
        }
    }

    private enum HitSource
    {
        /// <summary>
        ///     Damage which the creature's simulating player also gets on its side (environment, creature attacks, replicated torpedoes...).
        /// </summary>
        World,

        /// <summary>
        ///     Damage computed on our side only (the local player's attacks, what we simulate): sent to the creature's simulating player.
        /// </summary>
        Local,

        /// <summary>
        ///     Our copy of something another player simulates, who sends its own version of the hit.
        /// </summary>
        RemoteCopy
    }

    private sealed class OutgoingHit
    {
        public float LastSendTime = float.MinValue;
        public float PendingDamage;
        public Vector3 PendingLocalPosition;
        public NitroxId? PendingDealerId;
    }
}
