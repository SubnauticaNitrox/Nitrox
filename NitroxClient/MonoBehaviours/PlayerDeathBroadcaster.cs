using NitroxClient.GameLogic;
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
        localPlayer.BroadcastDeath(player.transform.position);
    }

    public void OnDestroy()
    {
        Player.main.playerDeathEvent.RemoveHandler(this, PlayerDeath);
    }
}
