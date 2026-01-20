using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class PlayerStatsProcessor(IPacketSender packetSender, ILogger<PlayerStatsProcessor> logger) : AuthenticatedPacketProcessor<PlayerStats>
{
    private readonly IPacketSender packetSender = packetSender;
    private readonly ILogger<PlayerStatsProcessor> logger = logger;

    public override void Process(PlayerStats packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            logger.ZLogWarningOnce($"Player ID mismatch (received: {packet.PlayerId}, real: {player.Id})");
            packet.PlayerId = player.SessionId;
        }
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        packetSender.SendPacketToOthersAsync(packet, player.SessionId);
    }
}
