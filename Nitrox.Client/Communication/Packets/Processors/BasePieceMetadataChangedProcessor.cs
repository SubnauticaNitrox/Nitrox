using Nitrox.Client.Communication.Packets.Processors.Abstract;
using Nitrox.Client.GameLogic.Bases.Metadata;
using Nitrox.Model.Packets;

namespace Nitrox.Client.Communication.Packets.Processors
{
    public class BasePieceMetadataChangedProcessor : ClientPacketProcessor<BasePieceMetadataChanged>
    {
        public override void Process(BasePieceMetadataChanged packet)
        {
            BasePieceMetadataProcessor metadataProcessor = BasePieceMetadataProcessor.FromMetaData(packet.Metadata);
            metadataProcessor.UpdateMetadata(packet.PieceId, packet.Metadata);
        }
    }
}
