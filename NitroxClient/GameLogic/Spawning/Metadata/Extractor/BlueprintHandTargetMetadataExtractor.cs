using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

/// <summary>
/// Extracts the current state of a DataBox (BlueprintHandTarget) for syncing to new players.
/// </summary>
public class BlueprintHandTargetMetadataExtractor : EntityMetadataExtractor<BlueprintHandTarget, BlueprintHandTargetMetadata>
{
    public override BlueprintHandTargetMetadata Extract(BlueprintHandTarget entity)
    {
        return new BlueprintHandTargetMetadata(entity.used);
    }
}
