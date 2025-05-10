using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EntityMetadataUpdateProcessor(EntityRegistry entityRegistry, ILogger<EntityMetadataUpdateProcessor> logger) : IAuthPacketProcessor<EntityMetadataUpdate>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<EntityMetadataUpdateProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, EntityMetadataUpdate packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            logger.ZLogError($"Entity metadata {packet.NewValue.GetType():@TypeName} updated on an entity unknown to the server {packet.Id}");
            return;
        }

        // TODO: FIX
        // if (TryProcessMetadata(context.Sender, entity, packet.NewValue))
        // {
        //     entity.Metadata = packet.NewValue;
        //     SendUpdateToVisiblePlayers(packet, context.Sender, entity);
        // }
    }

    private void SendUpdateToVisiblePlayers(EntityMetadataUpdate packet, PeerId sendingPlayer, Entity entity)
    {
        // TODO: FIX WITH DATABASE
        // foreach (NitroxServer.Player player in playerService.GetConnectedPlayersAsync())
        // {
        //     bool updateVisibleToPlayer = player.CanSee(entity);
        //
        //     if (player != sendingPlayer && updateVisibleToPlayer)
        //     {
        //         player.SendPacket(packet);
        //     }
        // }
    }

    private bool TryProcessMetadata(PeerId sendingPlayer, Entity entity, EntityMetadata metadata)
    {
        return metadata switch
        {
            PlayerMetadata playerMetadata => ProcessPlayerMetadata(sendingPlayer, entity, playerMetadata),

            // Allow metadata updates from any player by default
            _ => true
        };
    }

    private bool ProcessPlayerMetadata(PeerId sendingPlayer, Entity entity, PlayerMetadata metadata)
    {
        // TODO: FIX WITH DATABASE
        // if (sendingPlayer.GameObjectId == entity.Id)
        // {
        //     sendingPlayer.EquippedItems.Clear();
        //     foreach (PlayerMetadata.EquippedItem item in metadata.EquippedItems)
        //     {
        //         sendingPlayer.EquippedItems.Add(item.Slot, item.Id);
        //     }
        //
        //     return true;
        // }
        //
        // Log.WarnOnce($"Player {sendingPlayer.Name} tried updating metadata of another player's entity {entity.Id}");
        // return false;

        return false;
    }
}
