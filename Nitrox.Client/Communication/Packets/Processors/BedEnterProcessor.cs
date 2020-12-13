using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    class BedEnterProcessor : ClientPacketProcessor<BedEnter>
    {
        public override void Process(BedEnter packet)
        {
        }
    }
}
