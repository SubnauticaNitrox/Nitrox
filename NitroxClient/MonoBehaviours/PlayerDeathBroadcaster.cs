using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.InGame;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class PlayerDeathBroadcaster : MonoBehaviour
{
    private LocalPlayer localPlayer;

    public void Awake()
    {
        localPlayer = this.Resolve<LocalPlayer>();

        Player.main.playerDeathEvent.AddHandler(this, PlayerDeath);
    }

    private void PlayerDeath(Player player)
    {
        if (localPlayer.MarkDeathPointsWithBeacon)
        {
            DeathBeacon.SpawnDeathBeacon(player.transform.position.ToDto(), localPlayer.PlayerName);
        }
        localPlayer.BroadcastDeath(player.transform.position);
    }

    public void OnDestroy()
    {
        Player.main.playerDeathEvent.RemoveHandler(this, PlayerDeath);
    }
}
