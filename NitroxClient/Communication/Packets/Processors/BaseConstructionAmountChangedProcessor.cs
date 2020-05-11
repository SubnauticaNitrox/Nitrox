using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BaseConstructionAmountChangedProcessor : ClientPacketProcessor<BaseConstructionAmountChanged>
    {
        private BuildThrottlingQueue buildEventQueue;

        public BaseConstructionAmountChangedProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }        

        public override void Process(BaseConstructionAmountChanged amountChanged)
        {
            buildEventQueue.EnqueueAmountChanged(amountChanged.Id, amountChanged.ConstructionAmount);
        }
    }
}
