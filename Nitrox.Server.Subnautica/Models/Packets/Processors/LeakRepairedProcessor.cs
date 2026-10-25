using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

sealed class LeakRepairedProcessor(WorldEntityManager worldEntityManager, ILogger<LeakRepairedProcessor> logger) : IAuthPacketProcessor<LeakRepaired>
{
    private readonly WorldEntityManager worldEntityManager = worldEntityManager;
    private readonly ILogger<LeakRepairedProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, LeakRepaired packet)
    {
        if (worldEntityManager.TryDestroyEntity(packet.LeakId, out _))
        {
            await context.SendToOthersAsync(packet);
        }
        else
        {
            logger.ZLogWarning($"Leak entity {packet.LeakId} not found in registry (base: {packet.BaseId}, cell: {packet.RelativeCell}). The repair will not be forwarded to other players.");
        }
    }
}
