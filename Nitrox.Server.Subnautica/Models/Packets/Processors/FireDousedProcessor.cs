using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class FireDousedProcessor(EntitySimulation entitySimulation, WorldEntityManager worldEntityManager) : IAuthPacketProcessor<FireDoused>
{
    private readonly EntitySimulation entitySimulation = entitySimulation;
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, FireDoused packet)
    {
        if (packet.Health <= 0f)
        {
            entitySimulation.EntityDestroyed(packet.Id);
            worldEntityManager.TryDestroyEntity(packet.Id, out _);
        }

        await context.SendToOthersAsync(packet);
    }
}
