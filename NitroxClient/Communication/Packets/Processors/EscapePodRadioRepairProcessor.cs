using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxClient.GameLogic;

namespace NitroxClient.Communication.Packets.Processors
{
    public class EscapePodRadioRepairProcessor : ClientPacketProcessor<EscapePodRadioRepair>
    {
        private readonly EscapePodManager escapePodManager;

        public EscapePodRadioRepairProcessor(EscapePodManager escapePodManager)
        {
            this.escapePodManager = escapePodManager;
        }

        public override void Process(EscapePodRadioRepair packet)
        {
            escapePodManager.OnRadioRepair(packet.Id);
        }
    }
}
