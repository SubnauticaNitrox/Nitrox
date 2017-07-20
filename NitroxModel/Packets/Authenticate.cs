using System;
using System.Collections.Generic;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Authenticate : PlayerPacket
    {
        public String User { get; set; }
        public String Password { get; set; }

        public Authenticate(String playerId) : base(playerId) { }
    }
}
