using System;
using NitroxModel.Packets;

namespace Nitrox.Test.Client.Communication
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
