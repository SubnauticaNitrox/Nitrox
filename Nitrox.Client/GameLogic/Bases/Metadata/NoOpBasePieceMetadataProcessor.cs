using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Metadata;

namespace Nitrox.Client.GameLogic.Bases.Metadata
{
    public class NoOpBasePieceMetadataProcessor : BasePieceMetadataProcessor
    {
        public override void UpdateMetadata(NitroxId id, BasePieceMetadata metadata)
        {
            // No-op
        }
    }
}
