using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.Bases;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
