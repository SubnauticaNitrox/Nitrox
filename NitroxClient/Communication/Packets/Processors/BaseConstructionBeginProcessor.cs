using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxClient.GameLogic.Bases;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BaseConstructionBeginProcessor : ClientPacketProcessor<BaseConstructionBegin>
    {
        private BuildThrottlingQueue buildEventQueue;

        public BaseConstructionBeginProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(BaseConstructionBegin packet)
        {
            buildEventQueue.EnqueueConstructionBegin(packet.BasePiece);
        }
    }
}
