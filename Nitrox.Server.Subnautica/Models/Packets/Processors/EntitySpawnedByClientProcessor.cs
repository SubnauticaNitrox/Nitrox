using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntitySpawnedByClientProcessor : AuthenticatedPacketProcessor<EntitySpawnedByClient>
{
    private readonly IPacketSender packetSender;
    private readonly PlayerManager playerManager;
    private readonly EntityRegistry entityRegistry;
    private readonly WorldEntityManager worldEntityManager;
    private readonly EntitySimulation entitySimulation;

    public EntitySpawnedByClientProcessor(IPacketSender packetSender, PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
    {
        this.packetSender = packetSender;
        this.playerManager = playerManager;
        this.entityRegistry = entityRegistry;
        this.worldEntityManager = worldEntityManager;
        this.entitySimulation = entitySimulation;
    }

    public override void Process(EntitySpawnedByClient packet, Player playerWhoSpawned)
    {
        Entity entity = packet.Entity;

        // If the entity already exists in the registry, it is fine to update.  This is a normal case as the player
        // may have an item in their inventory (that the registry knows about) then wants to spawn it into the world.
        entityRegistry.AddOrUpdate(entity);

        SimulatedEntity simulatedEntity = null;
        if (entity is WorldEntity worldEntity)
        {
            worldEntityManager.TrackEntityInTheWorld(worldEntity);
        }

        if (packet.RequireSimulation && entitySimulation.TryAssignEntityToPlayer(entity, playerWhoSpawned, true, out simulatedEntity))
        {
            SimulationOwnershipChange ownershipChangePacket = new(simulatedEntity);
            packetSender.SendPacketToAllAsync(ownershipChangePacket);
        }

        SpawnEntities spawnEntities = new(entity, simulatedEntity, packet.RequireRespawn);
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            bool isOtherPlayer = player != playerWhoSpawned;
            if (isOtherPlayer && player.CanSee(entity))
            {
                packetSender.SendPacketAsync(spawnEntities, player.SessionId);
            }
        }
    }
}
