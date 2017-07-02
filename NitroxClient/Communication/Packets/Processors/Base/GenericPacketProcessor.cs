using NitroxClient.Communication.Packets.Processors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors.Base
{
    public abstract class GenericPacketProcessor<T> : PacketProcessor where T : Packet
    {
        public override void ProcessPacket(Packet packet)
        {
            Process((T)packet);
        }

        public abstract void Process(T packet);
    }
}
