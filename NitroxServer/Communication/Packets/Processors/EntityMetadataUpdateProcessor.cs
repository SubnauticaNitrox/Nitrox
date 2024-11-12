using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;
using System.Linq;

namespace NitroxServer.Communication.Packets.Processors
{
    public class EntityMetadataUpdateProcessor : AuthenticatedPacketProcessor<EntityMetadataUpdate>
    {
        private readonly PlayerManager playerManager;
        private readonly EntityRegistry entityRegistry;

        public EntityMetadataUpdateProcessor(PlayerManager playerManager, EntityRegistry entityRegistry)
        {
            this.playerManager = playerManager;
            this.entityRegistry = entityRegistry;
        }

        public override void Process(EntityMetadataUpdate packet, Player sendingPlayer)
        {
            Optional<Entity> entity = entityRegistry.GetEntityById(packet.Id);

            if (entity.HasValue)
            {
                entity.Value.Metadata = packet.NewValue;
                SendUpdateToVisiblePlayers(packet, sendingPlayer, entity.Value);

                ProcessMetadata(entity.Value, packet.NewValue);
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
                bool updateVisibleToPlayer = player.CanSee(entity);

                if (player != sendingPlayer && updateVisibleToPlayer)
                {
                    player.SendPacket(packet);
                }
            }
        }

        // TODO: replace temporary code with a serious implementation
        private void ProcessMetadata(Entity entity, EntityMetadata metadata)
        {
            if (metadata is PlayerMetadata playerMetadata)
            {
                Player player = playerManager.GetConnectedPlayers().Where(p => p.GameObjectId == entity.Id).FirstOrDefault();
                if (player != null)
                {
                    player.EquippedItems.Clear();
                    foreach (PlayerMetadata.EquippedItem item in playerMetadata.EquippedItems)
                    {
                        player.EquippedItems.Add(item.Slot, item.Id);
                    }
                }
            }
        }
    }
}
