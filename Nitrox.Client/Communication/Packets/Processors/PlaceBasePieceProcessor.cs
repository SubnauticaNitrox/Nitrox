using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.Bases;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
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
