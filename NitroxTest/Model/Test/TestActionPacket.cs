using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.DataStructures;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestActionPacket : PlayerActionPacket
    {
        public TestActionPacket(string playerId, Vector3 eventPosition) : base(playerId, eventPosition)
        {
        }
    }
}
