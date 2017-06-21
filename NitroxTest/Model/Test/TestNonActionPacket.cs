using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxTest.Model
{
    public class TestNonActionPacket : Packet
    {
        public TestNonActionPacket(string playerId) : base(playerId)
        {
        }
    }
}
