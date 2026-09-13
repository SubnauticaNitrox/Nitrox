using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerSeeOutOfCellEntityProcessor(EntityRegistry entityRegistry, PlayerService playerService) : IAuthPacketProcessor<PlayerSeeOutOfCellEntity>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;

    public Task Process(AuthProcessorContext context, PlayerSeeOutOfCellEntity packet)
    {
        if (entityRegistry.GetEntityById(packet.EntityId).HasValue)
        {
            playerService.GetProperty<OutOfCellVisibleEntitiesProperty>(context.Sender).Value.Add(packet.EntityId);
        }
        return Task.CompletedTask;
    }
}
