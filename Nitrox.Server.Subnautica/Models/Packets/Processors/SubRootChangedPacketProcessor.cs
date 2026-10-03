using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SubRootChangedPacketProcessor(EntityRegistry entityRegistry, PlayerService playerService) : IAuthPacketProcessor<SubRootChanged>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, SubRootChanged packet)
    {
        entityRegistry.ReparentEntity(playerService.GetProperty<GameObjectIdProperty>(context.Sender).Value, packet.SubRootId.OrNull());
        playerService.GetProperty<SubRootIdProperty>(context.Sender).Value = packet.SubRootId.HasValue ? packet.SubRootId.Value : null;
        await context.SendToOthersAsync(packet);
    }
}
