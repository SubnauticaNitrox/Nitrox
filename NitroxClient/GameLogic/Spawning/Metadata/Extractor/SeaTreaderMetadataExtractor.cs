using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class SeaTreaderMetadataExtractor : EntityMetadataExtractor<SeaTreader, SeaTreaderMetadata>
{
    public override SeaTreaderMetadata Extract(SeaTreader seaTreader)
    {
        if (!DayNightCycle.main)
        {
            return null;
        }
        float grazingEndTime = DayNightCycle.main.timePassedAsFloat + seaTreader.grazingTimeLeft;
        return new(seaTreader.reverseDirection, grazingEndTime, seaTreader.leashPosition.ToDto());
    }
}
