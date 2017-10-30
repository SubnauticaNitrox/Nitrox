using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Authenticate : Packet
    {
        public string PlayerId { get; }
        public string User { get; }
        public string Password { get; }

        public Authenticate(string playerId)
        {
            PlayerId = playerId;
        }
    }
}
