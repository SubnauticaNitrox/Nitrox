using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Disconnect : Packet
    {

        public Disconnect(String playerId) : base(playerId) {}
    }
}
