using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
///     This is the absolute damage state. The current simulation owner is the only one who sends this packet to the server
/// </summary>
internal sealed class CyclopsDamageProcessor(IPacketSender packetSender, ILogger<CyclopsDamageProcessor> logger) : AuthenticatedPacketProcessor<CyclopsDamage>
{
    private readonly ILogger<CyclopsDamageProcessor> logger = logger;
    private readonly IPacketSender packetSender = packetSender;

    public override void Process(CyclopsDamage packet, Player simulatingPlayer)
    {
        logger.ZLogDebug($"New cyclops damage from player #{simulatingPlayer.Id}: {packet}");

        packetSender.SendPacketToOthersAsync(packet, simulatingPlayer.SessionId);
    }
}
