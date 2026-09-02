using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FireDousedProcessor(EntityRegistry entityRegistry) : IAuthPacketProcessor<FireDoused>
{
    private readonly EntityRegistry entityRegistry = entityRegistry;

    public async Task Process(AuthProcessorContext context, FireDoused packet)
    {
        if (packet.IsExtinguished)
        {
            entityRegistry.RemoveEntity(packet.Id);
        }

        await context.SendToOthersAsync(packet);
    }
}
