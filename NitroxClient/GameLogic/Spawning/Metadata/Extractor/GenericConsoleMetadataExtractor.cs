using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class GenericConsoleMetadataExtractor : EntityMetadataExtractor<GenericConsole, GenericConsoleMetadata>
{
    public override GenericConsoleMetadata Extract(GenericConsole entity)
    {
        return new(entity.gotUsed);
    }
}
