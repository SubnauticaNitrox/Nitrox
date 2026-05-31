using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerSeeOutOfCellEntityProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<PlayerSeeOutOfCellEntity>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public Task Process(AuthProcessorContext context, PlayerSeeOutOfCellEntity packet)
    {
        if (entityRegistry.GetEntityById(packet.EntityId).HasValue)
        {
            context.Sender.OutOfCellVisibleEntities.Add(packet.EntityId);
        }
        return Task.CompletedTask;
    }
}
