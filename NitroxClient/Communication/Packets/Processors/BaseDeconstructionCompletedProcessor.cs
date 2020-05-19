using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BaseDeconstructionCompletedProcessor : ClientPacketProcessor<BaseDeconstructionCompleted>
    {
        private BuildThrottlingQueue buildEventQueue;

        public BaseDeconstructionCompletedProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(BaseDeconstructionCompleted packet)
        {
            buildEventQueue.EnqueueDeconstructionCompleted(packet.Id);
        }
    }
}
