using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private readonly BuildThrottlingQueue buildEventQueue;

        public InitialPlayerSyncProcessor(BuildThrottlingQueue buildEventQueue)
        {
            Log.Info("InitialPlayerSyncProcessor");
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(InitialPlayerSync packet)
        {
            Log.Info("Received initial sync packet with " + packet.BasePieces.Count + " base pieces");

            foreach(BasePiece basePiece in packet.BasePieces)
            {
                buildEventQueue.EnqueueBasePiecePlaced(basePiece);
                
                if (basePiece.ConstructionCompleted)
                {
                    buildEventQueue.EnqueueConstructionCompleted(basePiece.Guid, basePiece.NewBaseGuid);
                }
                else
                {
                    buildEventQueue.EnqueueAmountChanged(basePiece.Guid, basePiece.ConstructionAmount);
                }
            }
        }
    }
}
