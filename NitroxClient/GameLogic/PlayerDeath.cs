using NitroxClient.Communication;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class PlayerDeath
    {
        private PacketSender packetSender;

        public PlayerDeath(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastPlayerDeath(Vector3 deathPosition)
        {
            PlayerDeathEvent playerDeath = new PlayerDeathEvent(packetSender.PlayerId, deathPosition);
            packetSender.Send(playerDeath);
        }
    }
}
