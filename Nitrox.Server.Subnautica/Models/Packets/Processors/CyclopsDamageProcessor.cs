using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Model.Subnautica.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
/// </summary>
internal sealed class CyclopsDamageProcessor(ILogger<CyclopsDamageProcessor> logger) : IAuthPacketProcessor<CyclopsDamage>
{
    public async Task Process(AuthProcessorContext context, CyclopsDamage packet)
    {
        logger.ZLogDebug($"New cyclops damage from {context.Sender} {packet}");
        await context.ReplyToOthers(packet);
    }
}
