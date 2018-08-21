using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : Packet
    {
        public ushort PlayerId { get; }

        public Disconnect(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}
