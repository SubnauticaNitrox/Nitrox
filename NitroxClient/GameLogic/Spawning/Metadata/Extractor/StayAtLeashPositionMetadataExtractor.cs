using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public sealed class StayAtLeashPositionMetadataExtractor : EntityMetadataExtractor<Creature, StayAtLeashPositionMetadata>
{
    public override StayAtLeashPositionMetadata Extract(Creature entity)
    {
        return new(entity.leashPosition.ToDto());
    }
}
