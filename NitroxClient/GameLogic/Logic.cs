using NitroxClient.Communication;
using System;

namespace NitroxClient.GameLogic
{
    public class Logic
    {
        public Building Building { get; private set; }

        public Logic(PacketSender packetSender)
        {
            this.Building = new Building(packetSender);
        }
    }
}
