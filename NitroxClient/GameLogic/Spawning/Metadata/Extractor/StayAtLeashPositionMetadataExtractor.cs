using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public sealed class StayAtLeashPositionMetadataExtractor : EntityMetadataExtractor<Creature, StayAtLeashPositionMetadata>
{
    public override StayAtLeashPositionMetadata Extract(Creature entity)
    {
        return new(entity.leashPosition.ToDto());
    }
}
