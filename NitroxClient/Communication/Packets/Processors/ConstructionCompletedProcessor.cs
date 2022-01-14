using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
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
            Log.Debug($"Processing ConstructionCompleted [PieceId: {completedPacket.PieceId}, BaseId: {completedPacket.BaseId}]");
            buildEventQueue.EnqueueConstructionCompleted(completedPacket.PieceId, completedPacket.BaseId);
        }
    }
}
