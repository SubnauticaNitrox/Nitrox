using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class SubRootChangedPacketProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<SubRootChanged>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, SubRootChanged packet)
    {
        entityRegistry.ReparentEntity(context.Sender.GameObjectId, packet.SubRootId.OrNull());
        context.Sender.SubRootId = packet.SubRootId;
        await context.SendToOthersAsync(packet);
    }
}
