using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : Packet
    {
        public ulong PlayerId { get; }

        public Disconnect(ulong playerId)
        {
            PlayerId = playerId;
        }
    }
}
