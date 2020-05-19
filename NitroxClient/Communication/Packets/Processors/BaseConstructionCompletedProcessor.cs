using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BaseConstructionCompletedProcessor : ClientPacketProcessor<BaseConstructionCompleted>
    {
        private BuildThrottlingQueue buildEventQueue;

        public BaseConstructionCompletedProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(BaseConstructionCompleted completedPacket)
        {
            buildEventQueue.EnqueueConstructionCompleted(completedPacket.PieceId);
        }
    }
}
