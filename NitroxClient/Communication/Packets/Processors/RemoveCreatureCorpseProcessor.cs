using System.Collections;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using Nitrox.Model.Subnautica.DataStructures;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class RemoveCreatureCorpseProcessor : IClientPacketProcessor<RemoveCreatureCorpse>
{
    private readonly Entities entities;
    private readonly LiveMixinManager liveMixinManager;
    private readonly SimulationOwnership simulationOwnership;

    public RemoveCreatureCorpseProcessor(Entities entities, LiveMixinManager liveMixinManager, SimulationOwnership simulationOwnership)
    {
        this.entities = entities;
        this.liveMixinManager = liveMixinManager;
        this.simulationOwnership = simulationOwnership;
    }

    public Task Process(IPacketProcessContext context, RemoveCreatureCorpse packet)
    {
        entities.RemoveEntity(packet.CreatureId);
        if (!NitroxEntity.TryGetComponentFrom(packet.CreatureId, out CreatureDeath creatureDeath))
        {
            entities.MarkForDeletion(packet.CreatureId);
            Log.Warn($"[{nameof(RemoveCreatureCorpseProcessor)}] Could not find entity with id: {packet.CreatureId} to remove corpse from.");
            return Task.CompletedTask;
        }

        creatureDeath.transform.localPosition = packet.DeathPosition.ToUnity();
        creatureDeath.transform.localRotation = packet.DeathRotation.ToUnity();
        CoroutineHost.StartCoroutine(SimplerOnKillAsync(creatureDeath, packet.CreatureId));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Calls only some parts from <see cref="CreatureDeath.OnKillAsync"/> to avoid sending packets from it
    /// or already synced behaviour (like spawning another respawner from the remote clients)
    /// </summary>
    public IEnumerator SimplerOnKillAsync(CreatureDeath creatureDeath, NitroxId creatureId)
    {
        // Ensure we don't broadcast anything from this kill event
        simulationOwnership.StopSimulatingEntity(creatureId);

        // Remove the position broadcasting stuff from it
        EntityPositionBroadcaster.RemoveEntityMovementControl(creatureDeath.gameObject, creatureId);

        // Receiving this packet means the creature is dead
        liveMixinManager.SyncRemoteHealth(creatureDeath.liveMixin, 0);

        // To avoid SpawnRespawner to be called
        creatureDeath.respawn = false;
        creatureDeath.hasSpawnedRespawner = true;

        // To avoid the cooked data section
        bool lastDamageWasHeat = creatureDeath.lastDamageWasHeat;
        creatureDeath.lastDamageWasHeat = false;

        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            yield return creatureDeath.OnKillAsync();
        }

        // Restore the field in case it was useful
        creatureDeath.lastDamageWasHeat = lastDamageWasHeat;
    }
}
