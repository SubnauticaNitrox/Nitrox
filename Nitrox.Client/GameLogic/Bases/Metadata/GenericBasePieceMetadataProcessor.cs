using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Metadata;

namespace Nitrox.Client.GameLogic.Bases.Metadata
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
