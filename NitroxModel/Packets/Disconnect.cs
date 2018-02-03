using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : Packet
    {
        public string PlayerId { get; }

        public Disconnect(string playerId)
        {
            PlayerId = playerId;
        }
    }
}
