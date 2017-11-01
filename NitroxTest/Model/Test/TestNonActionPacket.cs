using NitroxModel.Packets;
using System;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestNonActionPacket : Packet
    {
        public String PlayerId { get; }

        public TestNonActionPacket(string playerId)
        {
            PlayerId = playerId;
        }
    }
}
