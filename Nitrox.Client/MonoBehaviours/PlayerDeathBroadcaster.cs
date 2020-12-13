using Nitrox.Client.GameLogic;
using Nitrox.Model.Core;
using UnityEngine;

namespace Nitrox.Client.MonoBehaviours
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
