using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class BaseDeconstructionBeginProcessor : ClientPacketProcessor<BaseDeconstructionBegin>
    {
        private BuildThrottlingQueue buildEventQueue;

        public BaseDeconstructionBeginProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(BaseDeconstructionBegin packet)
        {
            buildEventQueue.EnqueueDeconstructionBegin(packet.Id);
        }
    }
}
