using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class AuthenticatedPacket : Packet
    {
        public String PlayerId { get; protected set; }

        public AuthenticatedPacket(String playerId)
        {
            this.PlayerId = playerId;
        }
    }
}
