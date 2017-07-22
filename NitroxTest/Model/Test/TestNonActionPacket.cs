using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxTest.Model
{
    [Serializable]
    public class TestNonActionPacket : AuthenticatedPacket
    {
        public TestNonActionPacket(string playerId) : base(playerId)
        {
        }
    }
}
