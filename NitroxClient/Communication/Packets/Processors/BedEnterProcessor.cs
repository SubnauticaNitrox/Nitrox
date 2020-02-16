using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class BedEnterProcessor : ClientPacketProcessor<BedEnter>
    {
        public override void Process(BedEnter packet)
        {
        }
    }
}
