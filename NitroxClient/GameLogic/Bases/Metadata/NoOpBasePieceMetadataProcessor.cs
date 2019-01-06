using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public class NoOpBasePieceMetadataProcessor : BasePieceMetadataProcessor
    {
        public override void UpdateMetadata(string guid, BasePieceMetadata metadata)
        {
            // No-op
        }
    }
}
