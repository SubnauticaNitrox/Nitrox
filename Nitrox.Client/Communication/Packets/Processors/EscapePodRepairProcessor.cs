using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
