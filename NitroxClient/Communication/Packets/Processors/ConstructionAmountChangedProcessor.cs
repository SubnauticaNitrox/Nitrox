using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class ConstructionAmountChangedProcessor : ClientPacketProcessor<ConstructionAmountChanged>
    {
        private BuildThrottlingQueue buildEventQueue;

        public ConstructionAmountChangedProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(ConstructionAmountChanged amountChanged)
        {
            buildEventQueue.EnqueueAmountChanged(amountChanged.Id, amountChanged.ConstructionAmount);
        }
    }
}
