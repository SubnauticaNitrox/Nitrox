using NitroxClient.Communication;

namespace NitroxClient.GameLogic
{
    public class Logic
    {
        public Building Building { get; private set; }
        public Chat Chat { get; private set; }

        public Logic(PacketSender packetSender)
        {
            this.Building = new Building(packetSender);
            this.Chat = new Chat(packetSender);
        }
    }
}
