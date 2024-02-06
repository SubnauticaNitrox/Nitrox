using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    class EntitySpawnedByClientProcessor : AuthenticatedPacketProcessor<EntitySpawnedByClient>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;
        private readonly WorldEntityManager worldEntityManager;
        private readonly EntitySimulation entitySimulation;

        public EntitySpawnedByClientProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
        {
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

            if (entity is WorldEntity worldEntity)
            {
                worldEntityManager.TrackEntityInTheWorld(worldEntity);

                if (packet.RequireSimulation)
                {
                    SimulatedEntity simulatedEntity = entitySimulation.AssignNewEntityToPlayer(entity, playerWhoSpawned);

                    SimulationOwnershipChange ownershipChangePacket = new SimulationOwnershipChange(simulatedEntity);
                    playerManager.SendPacketToAllPlayers(ownershipChangePacket);
                }
            }

            SpawnEntities spawnEntities = new(entity, packet.RequireRespawn);
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != playerWhoSpawned;
                if (isOtherPlayer && player.CanSee(entity))
                {
                    player.SendPacket(spawnEntities);
                }
            }
        }
    }
}
