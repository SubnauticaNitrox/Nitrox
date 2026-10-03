using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;
using Nitrox.Server.Subnautica.Models.PlayerProperties;
using Nitrox.Server.Subnautica.Services;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerStatsProcessor(PlayerService playerService, ILogger<PlayerStatsProcessor> logger) : IAuthPacketProcessor<PlayerStats>
{
    private readonly PlayerService playerService = playerService;
    private readonly ILogger<PlayerStatsProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, PlayerStats packet)
    {
        if (packet.SessionId != context.Sender)
        {
            logger.ZLogWarningOnce($"Player ID mismatch (received: {packet.SessionId}, real: {context.Sender})");
            packet.SessionId = context.Sender;
        }
        playerService.GetProperty<StatsProperty>(context.Sender).Value = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        await context.SendToOthersAsync(packet);
    }
}
