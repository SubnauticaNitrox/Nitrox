using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class PrecursorDisableGunTerminalMetadataExtractor : EntityMetadataExtractor<PrecursorDisableGunTerminal, PrecursorDisableGunTerminalMetadata>
{
    public override PrecursorDisableGunTerminalMetadata Extract(PrecursorDisableGunTerminal entity)
    {
        return new(entity.firstUse);
    }
}
