using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EntityDestroyedPacketProcessor : AuthenticatedPacketProcessor<EntityDestroyed>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;
        private readonly WorldEntityManager worldEntityManager;
        private readonly EntitySimulation entitySimulation;

        public EntityDestroyedPacketProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, WorldEntityManager worldEntityManager, EntitySimulation entitySimulation)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
            this.worldEntityManager = worldEntityManager;
            this.entitySimulation = entitySimulation;
        }

        public override void Process(EntityDestroyed packet, Player destroyingPlayer)
        {
            entitySimulation.EntityDestroyed(packet.Id);

            Optional<Entity> entity = entityRegistry.RemoveEntity(packet.Id);

            if (!entity.HasValue)
            {
                return;
            }

            if (entity.Value is WorldEntity worldEntity)
            {
                worldEntityManager.StopTrackingEntity(worldEntity);
            }

            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool isOtherPlayer = player != destroyingPlayer;
                if (isOtherPlayer && player.CanSee(entity.Value))
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}
