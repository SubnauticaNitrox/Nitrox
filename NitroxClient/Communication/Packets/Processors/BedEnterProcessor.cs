using NitroxClient.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    class BedEnterProcessor : ClientPacketProcessor<BedEnter>
    {
        public override void Process(BedEnter packet)
        {
        }
    }
}
