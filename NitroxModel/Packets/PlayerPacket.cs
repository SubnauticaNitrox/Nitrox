using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class PlayerPacket : Packet
    {
        public String PlayerId { get; protected set; }

        public PlayerPacket(String playerId)
        {
            this.PlayerId = playerId;
        }
    }
}
