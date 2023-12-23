using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class RadiationMetadataExtractor : EntityMetadataExtractor<RadiationLeak, RadiationMetadata>
{
    public override RadiationMetadata Extract(RadiationLeak leak)
    {
        // Note: this extractor should only be used when this radiation leak is being repaired
        TimeManager timeManager = NitroxServiceLocator.LocateService<TimeManager>();
        float realTimeFix = leak.liveMixin.IsFullHealth() ? (float)timeManager.RealTimeElapsed : -1;
        return new(leak.liveMixin.health, realTimeFix);
    }
}
