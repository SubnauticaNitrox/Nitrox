using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class AuthenticatedPacket : Packet
    {
        public String PlayerId { get; }

        public AuthenticatedPacket(String playerId)
        {
            PlayerId = playerId;
        }
    }
}
