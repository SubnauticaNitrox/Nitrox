using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerStatsProcessor(IPacketSender packetSender, ILogger<PlayerStatsProcessor> logger) : IAuthPacketProcessor<PlayerStats>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly ILogger<PlayerStatsProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerStats packet)
    {
        if (packet.PlayerId != context.Sender.Id)
        {
            logger.ZLogWarningOnce($"Player ID mismatch (received: {packet.PlayerId}, real: {context.Sender.Id})");
            packet.PlayerId = context.Sender.SessionId;
        }
        context.Sender.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        await context.SendToOthersAsync(packet);
    }
}
