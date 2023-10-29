using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

public abstract class VehicleMetadataExtractor<I, O> : NamedColoredMetadataExtractor<I, O> where O : VehicleMetadata
{
    public float GetHealth(LiveMixin liveMixin)
    {
        return liveMixin.health;
    }
}
