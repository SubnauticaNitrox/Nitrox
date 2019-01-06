using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;

namespace NitroxClient.GameLogic.Bases.Metadata
{
    public abstract class GenericBasePieceMetadataProcessor<T> : BasePieceMetadataProcessor where T : BasePieceMetadata
    {
        public abstract void UpdateMetadata(string guid, T metadata);

        public override void UpdateMetadata(string guid, BasePieceMetadata metadata)
        {
            UpdateMetadata(guid, (T)metadata);
        }
    }
}
