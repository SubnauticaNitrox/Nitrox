using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
/// </summary>
internal sealed class CyclopsDamageProcessor(ILogger<CyclopsDamageProcessor> logger) : IAuthPacketProcessor<CyclopsDamage>
{
    private readonly ILogger<CyclopsDamageProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, CyclopsDamage packet)
    {
        logger.ZLogDebug($"New cyclops damage from player #{context.Sender.SessionId}: {packet}");

        await context.SendToOthersAsync(packet);
    }
}
