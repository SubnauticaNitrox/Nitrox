using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.MonoBehaviours;
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
            Log.Debug($"Processing ConstructionCompleted [PieceId: {completedPacket.PieceId}, BaseId: {completedPacket.BaseId}, Bypass: {completedPacket.BypassExistingNitroxId}]");
            if (completedPacket.BypassExistingNitroxId.HasValue)
            {
                ThrottledBuilder.main.DestroyedGhostsIds.Add(completedPacket.BypassExistingNitroxId.Value);
            }
            buildEventQueue.EnqueueConstructionCompleted(completedPacket.PieceId, completedPacket.BaseId);
        }
    }
}
