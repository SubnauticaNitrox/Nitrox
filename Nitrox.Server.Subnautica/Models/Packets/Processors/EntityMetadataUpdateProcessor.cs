using System.Collections.Generic;
using Nitrox.Model.Core;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntityMetadataUpdateProcessor(PlayerManager playerManager, EntityRegistry entityRegistry, ILogger<EntityMetadataUpdateProcessor> logger) : IAuthPacketProcessor<EntityMetadataUpdate>
{
    private readonly PlayerManager playerManager = playerManager;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<EntityMetadataUpdateProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, EntityMetadataUpdate packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            logger.ZLogError($"Entity metadata {packet.NewValue.GetType()} updated on an entity unknown to the server {packet.Id}");
            return;
        }

        if (!TryProcessMetadata(context.Sender, entity, packet.NewValue, out EntityMetadata metadataToApply))
        {
            return;
        }

        entity.Metadata = metadataToApply;
        EntityMetadataUpdate updateToForward = ReferenceEquals(metadataToApply, packet.NewValue) ? packet : new EntityMetadataUpdate(packet.Id, metadataToApply);
        await SendUpdateToVisiblePlayersAsync(context, updateToForward, entity);
    }

    private async Task SendUpdateToVisiblePlayersAsync(AuthProcessorContext context, EntityMetadataUpdate packet, Entity entity)
    {
        foreach (Player player in playerManager.GetConnectedPlayers())
        {
            bool updateVisibleToPlayer = player.CanSee(entity);
            if (player != context.Sender && updateVisibleToPlayer)
            {
                await context.SendAsync(packet, player.SessionId);
            }
        }
    }

    private bool TryProcessMetadata(Player sendingPlayer, Entity entity, EntityMetadata incoming, out EntityMetadata metadataToApply)
    {
        switch (incoming)
        {
            case PlayerMetadata playerMetadata:
                metadataToApply = incoming;
                return ProcessPlayerMetadata(sendingPlayer, entity, playerMetadata);

            // Merge so each client reports only their own hatch usage.
            case EscapePodMetadata escapePodMetadata:
                metadataToApply = MergeEscapePodMetadata(entity, escapePodMetadata);
                return true;

            default:
                // Allow metadata updates from any player by default
                metadataToApply = incoming;
                return true;
        }
    }

    private static EscapePodMetadata MergeEscapePodMetadata(Entity entity, EscapePodMetadata incoming)
    {
        if (entity.Metadata is not EscapePodMetadata existing)
        {
            return incoming;
        }

        HashSet<SessionId> bottomHatchUsedBy = [.. existing.PlayersWithBottomHatchUsed, .. incoming.PlayersWithBottomHatchUsed];
        HashSet<SessionId> topHatchUsedBy = [.. existing.PlayersWithTopHatchUsed, .. incoming.PlayersWithTopHatchUsed];
        return new EscapePodMetadata(incoming.PodRepaired, incoming.RadioRepaired, [.. bottomHatchUsedBy], [.. topHatchUsedBy]);
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

        logger.ZLogWarningOnce($"Player {sendingPlayer.Name} tried updating metadata of another player's entity {entity.Id}");
        return false;
    }
}
