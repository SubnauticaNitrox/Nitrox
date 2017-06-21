using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxTest.Model
{
    public class TestActionPacket : PlayerActionPacket
    {
        public TestActionPacket(string playerId, Vector3 eventPosition) : base(playerId, eventPosition)
        {
        }
    }
}
