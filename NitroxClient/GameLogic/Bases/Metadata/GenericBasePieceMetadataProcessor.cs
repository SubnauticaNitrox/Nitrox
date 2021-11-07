using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public abstract class GenericBasePieceMetadataProcessor<T> : BasePieceMetadataProcessor where T : BasePieceMetadata
    {
        public abstract void UpdateMetadata(NitroxId id, T metadata, bool initialSync = false);

        public override void UpdateMetadata(NitroxId id, BasePieceMetadata metadata, bool initialSync = false)
        {
            UpdateMetadata(id, (T)metadata, initialSync);
        }
    }
}
