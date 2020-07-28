using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxModel_Subnautica.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class CyclopsFireCreatedProcessor : ClientPacketProcessor<CyclopsFireCreated>
    {
        private readonly Fires fires;

        public CyclopsFireCreatedProcessor(Fires fires)
        {
            this.fires = fires;
        }

        public override void Process(CyclopsFireCreated packet)
        {
            fires.Create(packet.FireCreatedData);
        }
    }
}
