using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Communication.Packets.Processors.Abstract;
using NitroxServer.GameLogic;

namespace NitroxServer.Communication.Packets.Processors;

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
#if SUBNAUTICA
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water, packet.InfectionAmount);
#elif BELOWZERO
        player.Stats = new PlayerStatsData(packet.Oxygen, packet.MaxOxygen, packet.Health, packet.Food, packet.Water);
#endif
        playerManager.SendPacketToOtherPlayers(packet, player);
    }
}
