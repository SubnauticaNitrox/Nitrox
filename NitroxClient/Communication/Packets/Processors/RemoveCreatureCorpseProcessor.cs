using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using UWE;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class RemoveCreatureCorpseProcessor(Entities entities, LiveMixinManager liveMixinManager, SimulationOwnership simulationOwnership) : IClientPacketProcessor<RemoveCreatureCorpse>
{
    private readonly Entities entities = entities;
    private readonly LiveMixinManager liveMixinManager = liveMixinManager;
    private readonly SimulationOwnership simulationOwnership = simulationOwnership;

    /// <summary>
    ///     Calls only some parts from <see cref="CreatureDeath.OnKillAsync" /> to avoid sending packets from it
    ///     or already synced behaviour (like spawning another respawner from the remote clients)
    /// </summary>
    public static void SafeOnKillAsync(CreatureDeath creatureDeath, NitroxId creatureId, SimulationOwnership simulationOwnership, LiveMixinManager liveMixinManager)
    {
        StopReplicatingCreature(creatureDeath, creatureId, simulationOwnership);

        // To avoid SpawnRespawner to be called
        creatureDeath.respawn = false;
        creatureDeath.hasSpawnedRespawner = true;

        // To avoid the cooked data section
        creatureDeath.lastDamageWasHeat = false;

        // Receiving this packet means the creature is dead
        LiveMixin liveMixin = creatureDeath.liveMixin;
        liveMixin.health = 0f;
        liveMixin.tempDamage = 0f;
        // We don't care what's inside the damage info
        liveMixin.damageInfo.Clear();
        liveMixin.NotifyAllAttachedDamageReceivers(liveMixin.damageInfo);

        using (PacketSuppressor<EntitySpawnedByClient>.Suppress())
        using (PacketSuppressor<RemoveCreatureCorpse>.Suppress())
        {
            CoroutineUtils.PumpCoroutine(creatureDeath.OnKillAsync());
        }
    }

    private static void StopReplicatingCreature(CreatureDeath creatureDeath, NitroxId creatureId, SimulationOwnership simulationOwnership)
    {
        // Ensure we don't broadcast anything from this kill event
        simulationOwnership.StopSimulatingEntity(creatureId);

        // Remove the position broadcasting stuff from it
        EntityPositionBroadcaster.RemoveEntityMovementControl(creatureDeath.gameObject, creatureId);
    }

    public Task Process(ClientProcessorContext context, RemoveCreatureCorpse packet)
    {
        entities.RemoveEntity(packet.CreatureId);

        if (entities.SpawningEntities)
        {
            entities.MarkForDeletion(packet.CreatureId);
        }

        // This packet is sent to every player, including the ones which don't have the creature loaded
        if (!NitroxEntity.TryGetComponentFrom(packet.CreatureId, out CreatureDeath creatureDeath))
        {
            Log.Debug($"[{nameof(RemoveCreatureCorpseProcessor)}] Could not find entity with id: {packet.CreatureId} to remove corpse from.");
            return Task.CompletedTask;
        }

        // The creature can already be dead if we killed it at the same time, in which case its death was already processed
        if (!creatureDeath.liveMixin.IsAlive())
        {
            StopReplicatingCreature(creatureDeath, packet.CreatureId, simulationOwnership);
            return Task.CompletedTask;
        }

        creatureDeath.transform.SetPositionAndRotation(packet.DeathPosition.ToUnity(), packet.DeathRotation.ToUnity());

        // Only the simulating player spawns respawners so that a creature dying on several clients at once only gets one.
        // When another player killed it, we still hold the lock at this point because the server revokes it without notifying us
        if (simulationOwnership.HasAnyLockType(packet.CreatureId) && creatureDeath.respawn && !creatureDeath.hasSpawnedRespawner)
        {
            creatureDeath.SpawnRespawner();
        }

        SafeOnKillAsync(creatureDeath, packet.CreatureId, simulationOwnership, liveMixinManager);

        // SafeOnKillAsync only replicates CreatureDeath's part of the death, the creature's other components must also be notified.
        // Like in LiveMixin.Kill, this happens once the health is 0 (and we don't simulate the creature anymore)
        liveMixinManager.ReplayRemoteKill(creatureDeath.liveMixin);
        return Task.CompletedTask;
    }
}
