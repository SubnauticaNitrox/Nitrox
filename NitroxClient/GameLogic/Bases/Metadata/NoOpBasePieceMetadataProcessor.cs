using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public class NoOpBasePieceMetadataProcessor : BasePieceMetadataProcessor
    {
        public override void UpdateMetadata(NitroxId id, BasePieceMetadata metadata, bool initialSync)
        {
            // No-op
        }
    }
}
