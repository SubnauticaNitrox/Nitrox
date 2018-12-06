using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

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
            Log.Debug("Processing ConstructionCompleted " + completedPacket.Guid + " " + completedPacket.NewBaseCreatedGuid);
            buildEventQueue.EnqueueConstructionCompleted(completedPacket.Guid, completedPacket.NewBaseCreatedGuid);
        }
    }
}
