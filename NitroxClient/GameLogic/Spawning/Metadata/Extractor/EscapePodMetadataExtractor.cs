using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class EscapePodMetadataExtractor : EntityMetadataExtractor<EscapePod, EscapePodMetadata>
{
    public override EscapePodMetadata Extract(EscapePod entity)
    {
        Radio radio = entity.radioSpawner.spawnedObj.RequireComponent<Radio>();
        return new EscapePodMetadata(entity.liveMixin.IsFullHealth(), radio.liveMixin.IsFullHealth());
    }
}
