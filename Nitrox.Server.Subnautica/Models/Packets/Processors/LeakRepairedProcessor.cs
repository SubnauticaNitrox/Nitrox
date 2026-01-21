using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class LeakRepairedProcessor(WorldEntityManager worldEntityManager) : IAuthPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, LeakRepaired packet)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            await context.SendToOthersAsync(packet);
        }
    }
}
