using System;
using NitroxModel.Packets;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestNonActionPacket : Packet
    {
        public string PlayerId { get; }

        public TestNonActionPacket(string playerId)
        {
            PlayerId = playerId;
        }
    }
}
