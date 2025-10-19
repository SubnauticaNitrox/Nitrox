using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.Packets;
using Nitrox.Server.Subnautica.Models.Packets.Processors.Core;
using Nitrox.Server.Subnautica.Models.GameLogic;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

public class PlayerStatsProcessor : AuthenticatedPacketProcessor<PlayerStats>
{
    private readonly PlayerManager playerManager;

    public PlayerStatsProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayerStats packet, Player player)
    {
        if (packet.PlayerId != player.Id)
        {
            Log.WarnOnce($"[{nameof(PlayerStatsProcessor)}] Player ID mismatch (received: {packet.PlayerId}, real: {player.Id})");
            packet.PlayerId = player.Id;
        }
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
