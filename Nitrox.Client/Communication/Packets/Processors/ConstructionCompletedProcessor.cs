using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.Bases;
using Nitrox.Model.Logger;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class ConstructionCompletedProcessor : ClientPacketProcessor<ConstructionCompleted>
    {
        private BuildThrottlingQueue buildEventQueue;

        public ConstructionCompletedProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(ConstructionCompleted completedPacket)
        {
            Log.Debug("Processing ConstructionCompleted " + completedPacket.PieceId + " " + completedPacket.BaseId);
            buildEventQueue.EnqueueConstructionCompleted(completedPacket.PieceId, completedPacket.BaseId);
        }
    }
}
