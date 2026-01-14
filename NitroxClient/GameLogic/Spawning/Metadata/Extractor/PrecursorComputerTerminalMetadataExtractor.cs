using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PrecursorComputerTerminalMetadataExtractor : EntityMetadataExtractor<PrecursorComputerTerminal, PrecursorComputerTerminalMetadata>
{
    public override PrecursorComputerTerminalMetadata Extract(PrecursorComputerTerminal entity)
    {
        return new(entity.used);
    }
}
