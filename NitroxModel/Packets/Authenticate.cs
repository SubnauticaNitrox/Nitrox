using System;
using System.Collections.Generic;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class Authenticate : Packet
    {
        public String PlayerId { get; set; }
        public String User { get; set; }
        public String Password { get; set; }

        public Authenticate(String playerId)
        {
            this.PlayerId = playerId;
        }
    }
}
