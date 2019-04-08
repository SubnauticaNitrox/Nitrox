using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public abstract class GenericBasePieceMetadataProcessor<T> : BasePieceMetadataProcessor where T : BasePieceMetadata
    {
        public abstract void UpdateMetadata(NitroxId id, T metadata);

        public override void UpdateMetadata(NitroxId id, BasePieceMetadata metadata)
        {
            UpdateMetadata(id, (T)metadata);
        }
    }
}
