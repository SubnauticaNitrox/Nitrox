using NitroxModel.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxClient.Communication.Packets.Processors.Base
{
    public abstract class PacketProcessor
    {
        public abstract void ProcessPacket(Packet packet);
    }
}
