using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.HUD;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerStatsProcessor : ClientPacketProcessor<PlayerStats>
{
    private readonly PlayerVitalsManager vitalsManager;

    public PlayerStatsProcessor(PlayerVitalsManager vitalsManager)
    {
        this.vitalsManager = vitalsManager;
    }

    public override void Process(PlayerStats playerStats)
    {
        if (vitalsManager.TryFindForPlayer(playerStats.PlayerId, out RemotePlayerVitals vitals))
        {
            vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
            vitals.SetHealth(playerStats.Health);
            vitals.SetFood(playerStats.Food);
            vitals.SetWater(playerStats.Water);
        }
    }
}
