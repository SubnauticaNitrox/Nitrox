using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
