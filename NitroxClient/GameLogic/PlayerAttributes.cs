using NitroxClient.Communication;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class PlayerAttributes
    {
        private readonly PacketSender packetSender;

        public PlayerAttributes(PacketSender packetSender)
        {
            this.packetSender = packetSender;
        }

        public void BroadcastPlayerStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(packetSender.PlayerId, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }
    }
}
