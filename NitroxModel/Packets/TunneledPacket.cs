using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxModel.Packets
{
    public class TunneledPacket : Packet
    {
        public Packet Packet { get; private set; }
        public int HostRelativeId { get; private set; }

        public TunneledPacket(Packet packet, int hostRelativeId)
        {
            Packet = packet;
            HostRelativeId = hostRelativeId;
        }
    }
}
