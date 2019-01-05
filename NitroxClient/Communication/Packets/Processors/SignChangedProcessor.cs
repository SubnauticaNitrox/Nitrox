using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Bases.Metadata;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class SignChangedProcessor : ClientPacketProcessor<SignChanged>
    {
        public override void Process(SignChanged packet)
        {
            SignMetadata signMetadata = packet.SignMetadata;

            BasePieceMetadataProcessor metadataProcessor = BasePieceMetadataProcessor.FromMetaData(signMetadata);
            metadataProcessor.UpdateMetadata(signMetadata.Guid, signMetadata);
        }
    }
}
