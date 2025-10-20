using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

class PlayerStatsProcessor : AuthenticatedPacketProcessor<PlayerStats>
{
    private readonly PlayerManager playerManager;
    private readonly ILogger<PlayerStatsProcessor> logger;

    public PlayerStatsProcessor(PlayerManager playerManager, ILogger<PlayerStatsProcessor> logger)
    {
        this.playerManager = playerManager;
        this.logger = logger;
    }

    public override void Process(PlayerStats packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            logger.ZLogWarningOnce($"Player ID mismatch (received: {packet.PlayerId}, real: {player.Id})");
            packet.PlayerId = player.Id;
        }
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
