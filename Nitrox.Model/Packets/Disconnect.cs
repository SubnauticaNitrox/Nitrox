using System;

namespace Nitrox.Model.Packets
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
