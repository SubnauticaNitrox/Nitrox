using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EntityMetadataUpdateProcessor(PlayerService playerService, EntityRegistry entityRegistry, ILogger<EntityMetadataUpdateProcessor> logger) : IAuthPacketProcessor<EntityMetadataUpdate>
{
    private readonly PlayerService playerService = playerService;
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ILogger<EntityMetadataUpdateProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, EntityMetadataUpdate packet)
    {
        if (!entityRegistry.TryGetEntityById(packet.Id, out Entity entity))
        {
            logger.ZLogError($"Entity metadata {packet.NewValue.GetType()} updated on an entity unknown to the server {packet.Id}");
            return;
        }

        if (TryProcessMetadata(context.Sender, entity, packet.NewValue))
        {
            entity.Metadata = packet.NewValue;
            await SendUpdateToVisiblePlayersAsync(context, packet, entity);
        }
    }

    private async Task SendUpdateToVisiblePlayersAsync(AuthProcessorContext context, EntityMetadataUpdate packet, Entity entity)
    {
        foreach (SessionId player in playerService.GetSessionIds())
        {
            bool updateVisibleToPlayer = playerService.GetProperty<VisibleCellsProperty>(player).CanSee(entity);
            if (player != context.Sender && updateVisibleToPlayer)
            {
                await context.SendAsync(packet, player);
            }
        }
    }

    private bool TryProcessMetadata(SessionId sendingPlayer, Entity entity, EntityMetadata metadata)
    {
        return metadata switch
        {
            PlayerMetadata playerMetadata => ProcessPlayerMetadata(sendingPlayer, entity, playerMetadata),

            // temperature is a ratchet (see ThermalPlant.QueryTemperature's Mathf.Max), so drop stale updates instead of
            // relaying them: every client near a thermal plant reports the same rise, and only the first one is news
            ThermalPlantMetadata thermalPlantMetadata => entity.Metadata is not ThermalPlantMetadata currentMetadata || thermalPlantMetadata.Temperature > currentMetadata.Temperature,

            // Allow metadata updates from any player by default
            _ => true
        };
    }

    private bool ProcessPlayerMetadata(SessionId sendingPlayer, Entity entity, PlayerMetadata metadata)
    {
        if (playerService.GetProperty<GameObjectIdProperty>(sendingPlayer).Value == entity.Id)
        {
            playerService.GetProperty<EquippedItemsProperty>(sendingPlayer).Value
                         .ClearAndSet(metadata.EquippedItems, item => item.Slot, item => item.Id);
            return true;
        }

        logger.ZLogWarningOnce($"Player {playerService.GetProperty<NameProperty>(sendingPlayer).Value} tried updating metadata of another player's entity {entity.Id}");
        return false;
    }
}
