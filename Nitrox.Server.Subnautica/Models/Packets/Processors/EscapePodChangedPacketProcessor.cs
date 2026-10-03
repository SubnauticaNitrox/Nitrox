using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class EscapePodChangedPacketProcessor(EntityRegistry entityRegistry, PlayerService playerService) : IAuthPacketProcessor<EscapePodChanged>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly PlayerService playerService = playerService;

    public async Task Process(AuthProcessorContext context, EscapePodChanged packet)
    {
        entityRegistry.ReparentEntity(playerService.GetProperty<GameObjectIdProperty>(context.Sender).Value, packet.EscapePodId.OrNull());
        playerService.GetProperty<SubRootIdProperty>(context.Sender).Value = packet.EscapePodId.HasValue ? packet.EscapePodId.Value : null;
        await context.SendToOthersAsync(packet);
    }
}
