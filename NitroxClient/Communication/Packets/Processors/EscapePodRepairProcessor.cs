using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class EscapePodRepairProcessor : ClientPacketProcessor<EscapePodRepair>
    {
        private readonly EscapePodManager escapePodManager;

        public EscapePodRepairProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override void Process(EscapePodRepair packet)
        {
            escapePodManager.OnRepair(packet.Id);
        }
    }
}
