using NitroxClient.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class BedEnterProcessor : ClientPacketProcessor<BedEnter>
    {
        public override void Process(BedEnter packet)
        {
        }
    }
}
