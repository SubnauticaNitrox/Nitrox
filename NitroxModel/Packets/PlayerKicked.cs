using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerKicked : Packet
    {
        public string Reason;

        public PlayerKicked(string reason)
        {
            Reason = reason; // Going to want to implement this later
        }
    }
}
