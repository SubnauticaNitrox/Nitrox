using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntitySpawnedByClientProcessor(EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
    : IAuthPacketProcessor<EntitySpawnedByClient>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly EntitySimulation entitySimulation = entitySimulation;

    public async Task Process(AuthProcessorContext context, EntitySpawnedByClient packet)
    {
        Entity entity = packet.Entity;

        // If the entity already exists in the registry, it is fine to update.  This is a normal case as the player
        // may have an item in their inventory (that the registry knows about) then wants to spawn it into the world.
        entityRegistry.AddOrUpdate(entity);

        SimulatedEntity simulatedEntity = null;
        if (entity is WorldEntity worldEntity)
        {
            worldEntityManager.TrackEntityInTheWorld(worldEntity);

            if (packet.RequireSimulation)
            {
                simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, context.Sender.PlayerId);

                await context.ReplyToAll(new SimulationOwnershipChange(simulatedEntity));
            }
        }

        SpawnEntities spawnEntities = new(entity, simulatedEntity, packet.RequireRespawn);
        // TODO: FIX WITH DATABASE
        // foreach (NitroxServer.Player player in playerManager.GetConnectedPlayersAsync())
        // {
        //     bool isOtherPlayer = player != context.Sender;
        //     if (isOtherPlayer && player.CanSee(entity))
        //     {
        //         player.SendPacket(spawnEntities);
        //     }
        // }
    }
}
