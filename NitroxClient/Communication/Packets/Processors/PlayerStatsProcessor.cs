using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.HUD;
using NitroxModel.Networking.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class PlayerStatsProcessor : IClientPacketProcessor<PlayerStats>
{
    private readonly PlayerManager playerManager;

    public PlayerStatsProcessor(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
    }

    public Task Process(IPacketProcessContext context, PlayerStats playerStats)
    {
        if (playerManager.TryFind(playerStats.SessionId, out RemotePlayer remotePlayer))
        {
            RemotePlayerVitals vitals = remotePlayer.vitals;
            vitals.SetOxygen(playerStats.Oxygen, playerStats.MaxOxygen);
            vitals.SetHealth(playerStats.Health);
            vitals.SetFood(playerStats.Food);
            vitals.SetWater(playerStats.Water);
            remotePlayer.UpdateHealthAndInfection(playerStats.Health, playerStats.InfectionAmount);
        }

        return Task.CompletedTask;
    }
}
