using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
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
