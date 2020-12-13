using System;
using Nitrox.Model.Packets;

namespace Nitrox.Test.Model.Test
{
    [Serializable]
    public class TestNonActionPacket : Packet
    {
        public ushort PlayerId { get; }

        public TestNonActionPacket(ushort playerId)
        {
            PlayerId = playerId;
        }
    }
}
