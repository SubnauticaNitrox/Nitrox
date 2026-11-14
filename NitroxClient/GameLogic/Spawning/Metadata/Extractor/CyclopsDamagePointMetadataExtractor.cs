using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class CyclopsDamagePointMetadataExtractor : EntityMetadataExtractor<CyclopsDamagePoint, CyclopsDamagePointMetadata>
{
    public override CyclopsDamagePointMetadata Extract(CyclopsDamagePoint damagePoint)
    {
        return new CyclopsDamagePointMetadata(damagePoint.liveMixin.health);
    }
}
