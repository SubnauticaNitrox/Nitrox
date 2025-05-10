using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class LeakRepairedProcessor(WorldEntityManager worldEntityManager) : IAuthPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;

    public async Task Process(AuthProcessorContext context, LeakRepaired packet)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            await context.ReplyToOthers(packet);
        }
    }
}
