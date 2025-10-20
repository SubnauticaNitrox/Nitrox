using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class EatableMetadataExtractor : EntityMetadataExtractor<Eatable, EatableMetadata>
{
    public override EatableMetadata Extract(Eatable eatable)
    {
        if (eatable.decomposes)
        {
            return new(eatable.timeDecayStart);
        }
        return null;
    }
}
