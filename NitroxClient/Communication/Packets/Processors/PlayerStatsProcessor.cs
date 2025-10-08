using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

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
            RemotePlayerVitals vitals = remotePlayer.Vitals;
            if (vitals)
            {
                vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
                vitals.SetHealth(playerStats.Health);
                vitals.SetFood(playerStats.Food);
                vitals.SetWater(playerStats.Water);
            }
            remotePlayer.UpdateHealthAndInfection(playerStats.Health, playerStats.InfectionAmount);
        }
    }
}
