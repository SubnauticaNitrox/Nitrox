using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.InGame;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerDeathBroadcaster : MonoBehaviour
{
    private LocalPlayer localPlayer;
    private SeamothPassengers seamothPassengers;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();
        seamothPassengers = this.Resolve<SeamothPassengers>();

        Player.main.playerDeathEvent.AddHandler(this, OnPlayerDeath);
    }

    private void OnPlayerDeath(Player player)
    {
        seamothPassengers.ExitLocal(true, false);

        if (localPlayer.MarkDeathPointsWithBeacon)
        {
            DeathBeacon.SpawnDeathBeacon(player.transform.position.ToDto(), localPlayer.PlayerName);
        }
        localPlayer.BroadcastDeath(player.transform.position);
    }

    public void OnDestroy()
    {
        Player.main.playerDeathEvent.RemoveHandler(this, OnPlayerDeath);
    }
}
