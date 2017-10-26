using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class AuthenticatedPacket : Packet
    {
        public string PlayerId { get; }

        public AuthenticatedPacket(string playerId)
        {
            PlayerId = playerId;
        }
    }
}
