using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerStatsProcessor : ClientPacketProcessor<PlayerStats>
{
    private readonly PlayerManager playerManager;

    public PlayerStatsProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public override void Process(PlayerStats playerStats)
    {
        if (playerManager.TryFind(playerStats.PlayerId, out RemotePlayer remotePlayer))
        {
            RemotePlayerVitals vitals = remotePlayer.vitals;
            vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
            vitals.SetHealth(playerStats.Health);
            vitals.SetFood(playerStats.Food);
            vitals.SetWater(playerStats.Water);
#if SUBNAUTICA
            remotePlayer.UpdateHealthAndInfection(playerStats.Health, playerStats.InfectionAmount);
#elif BELOWZERO
            remotePlayer.UpdateHealth(playerStats.Health);
#endif
        }
    }
}
