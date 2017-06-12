using System;
using System.Collections.Generic;
using System.Text;

namespace NitroxModel.Packets
{
    [Serializable]
    public abstract class Packet
    {
        public String PlayerId { get; protected set; }

        public Packet(String playerId)
        {
            this.PlayerId = playerId;
        }
    }
}
