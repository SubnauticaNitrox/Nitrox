using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Connect : Packet
    {
        public Connect(String playerId) : base(playerId) { }
    }
}
