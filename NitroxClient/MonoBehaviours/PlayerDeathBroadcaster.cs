using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class PlayerDeathBroadcaster : MonoBehaviour
    {
        private LocalPlayer localPlayer;

        public void Awake()
        {
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();

            Player.main.playerDeathEvent.AddHandler(this, PlayerDeath);
        }

        private void PlayerDeath(Player player)
        {
            localPlayer.BroadcastDeath(player.transform.position);
        }
    }
}
