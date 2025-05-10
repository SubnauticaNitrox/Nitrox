using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using NitroxModel.Networking.Packets;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal class PlayerStatsProcessor(ILogger<PlayerStatsProcessor> logger) : IAuthPacketProcessor<PlayerStats>
{
    private readonly ILogger<PlayerStatsProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerStats packet)
    {
        if (packet.SessionId != context.Sender.SessionId)
        {
            // TODO: LOG ONCE
            logger.ZLogWarning($"Player ID mismatch (received: {packet.SessionId}, real: {context.Sender.SessionId})");
            packet.SessionId = context.Sender.SessionId;
        }
        // TODO: USE DATABASE
        // player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        await context.ReplyToOthers(packet);
    }
}
