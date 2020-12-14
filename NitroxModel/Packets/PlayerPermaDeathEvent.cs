using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerPermaDeathEvent : Packet
    {
        public PlayerPermaDeathEvent()
        {
        }

        public override string ToString()
        {
            return "[PlayerPermaDeathEvent]";
        }
    }
}
