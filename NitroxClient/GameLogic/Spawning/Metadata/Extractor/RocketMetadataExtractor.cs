using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;

namespace NitroxClient.GameLogic.Spawning.Metadata.Extractor;

public class RocketMetadataExtractor : EntityMetadataExtractor<Rocket, RocketMetadata>
{
    public override RocketMetadata Extract(Rocket rocket)
    {
        RocketPreflightCheckManager rocketPreflightCheckManager = rocket.RequireComponent<RocketPreflightCheckManager>();
        List<int> prechecks = rocketPreflightCheckManager.preflightChecks.Select(i => (int)i).ToList();

        return new(rocket.currentRocketStage, DayNightCycle.main.timePassedAsFloat, (int)rocket.elevatorState, rocket.elevatorPosition, prechecks);
    }
}
