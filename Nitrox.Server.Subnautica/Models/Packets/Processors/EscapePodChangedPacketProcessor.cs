using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class EscapePodChangedPacketProcessor(ILogger<EscapePodChangedPacketProcessor> logger) : IAuthPacketProcessor<EscapePodChanged>
{
    private readonly ILogger<EscapePodChangedPacketProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, EscapePodChanged packet)
    {
        logger.ZLogDebug($"Processing packet {packet}");
        // TODO: USE DATABASE HERE
        // player.SubRootId = packet.EscapePodId;
        await context.ReplyToOthers(packet);
    }
}
