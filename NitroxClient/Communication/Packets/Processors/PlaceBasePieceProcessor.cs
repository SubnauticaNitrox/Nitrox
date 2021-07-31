using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceBasePieceProcessor : ClientPacketProcessor<PlaceBasePiece>
    {
        private BuildThrottlingQueue buildEventQueue;

        public PlaceBasePieceProcessor(BuildThrottlingQueue buildEventQueue)
        {
            this.buildEventQueue = buildEventQueue;
        }

        public override void Process(PlaceBasePiece packet)
        {
            buildEventQueue.EnqueueBasePiecePlaced(packet.BasePiece);
        }
    }
}
