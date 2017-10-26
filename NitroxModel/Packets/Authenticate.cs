using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Authenticate : Packet
    {
        public String PlayerId { get; }
        public String User { get; }
        public String Password { get; }

        public Authenticate(String playerId)
        {
            PlayerId = playerId;
        }
    }
}
