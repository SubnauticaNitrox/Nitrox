using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class EscapePodMetadataExtractor : EntityMetadataExtractor<EscapePod, EscapePodMetadata>
{
    public override EscapePodMetadata Extract(EscapePod entity)
    {
        Radio radio = entity.radioSpawner.spawnedObj.RequireComponent<Radio>();
        return new EscapePodMetadata(entity.liveMixin.IsFullHealth(), radio.liveMixin.IsFullHealth());
    }
}
