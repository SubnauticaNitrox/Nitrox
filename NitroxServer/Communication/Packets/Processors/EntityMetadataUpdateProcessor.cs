using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;
using NitroxServer.GameLogic.Entities;

namespace NitroxServer.Communication.Packets.Processors;

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
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            Log.Error($"Entity metadata {packet.NewValue.GetType()} updated on an entity unknown to the server {packet.Id}");
            return;
        }

        if (TryProcessMetadata(sendingPlayer, entity, packet.NewValue))
        {
            entity.Metadata = packet.NewValue;
            SendUpdateToVisiblePlayers(packet, sendingPlayer, entity);
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

    private bool TryProcessMetadata(Player sendingPlayer, Entity entity, EntityMetadata metadata)
    {
        return metadata switch
        {
            PlayerMetadata playerMetadata => ProcessPlayerMetadata(sendingPlayer, entity, playerMetadata),

            // Allow metadata updates from any player by default
            _ => true
        };
    }

    private bool ProcessPlayerMetadata(Player sendingPlayer, Entity entity, PlayerMetadata metadata)
    {
        if (sendingPlayer.GameObjectId == entity.Id)
        {
            sendingPlayer.EquippedItems.Clear();
            foreach (PlayerMetadata.EquippedItem item in metadata.EquippedItems)
            {
                sendingPlayer.EquippedItems.Add(item.Slot, item.Id);
            }

            return true;
        }

        Log.WarnOnce($"Player {sendingPlayer.Name} tried updating metadata of another player's entity {entity.Id}");
        return false;
    }
}
