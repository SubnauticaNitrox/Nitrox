using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;
using Nitrox.Server.Communication.Packets.Processors.Abstract;
using Nitrox.Server.GameLogic;
using Nitrox.Server.GameLogic.Entities;

namespace Nitrox.Server.Communication.Packets.Processors
{
    public class EntityMetadataUpdateProcessor : AuthenticatedPacketProcessor<EntityMetadataUpdate>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityManager entityManager;

        public EntityMetadataUpdateProcessor(PlayerManager playerManager, EntityManager entityManager)
        {
            this.playerManager = playerManager;
            this.entityManager = entityManager;
        }

        public override void Process(EntityMetadataUpdate packet, Player sendingPlayer)
        {
            Optional<Entity> entity = entityManager.GetEntityById(packet.Id);

            if (entity.HasValue)
            {
                entity.Value.Metadata = packet.NewValue;
                SendUpdateToVisiblePlayers(packet, sendingPlayer, entity.Value);
            }
            else
            {
                Log.Error($"Entity metadata updated on an entity unknown to the server {packet.Id} {packet.NewValue.GetType()} ");
            }
        }
        
        private void SendUpdateToVisiblePlayers(EntityMetadataUpdate packet, Player sendingPlayer, Entity entity)
        {
            foreach (Player player in playerManager.GetConnectedPlayers())
            {
                bool updateVisibleToPlayer = player.CanSee(entity) || entity.ExistsInGlobalRoot;

                if (player != sendingPlayer && updateVisibleToPlayer)
                {
                    player.SendPacket(packet);
                }
            }
        }
    }
}
