using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases.Metadata;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
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
