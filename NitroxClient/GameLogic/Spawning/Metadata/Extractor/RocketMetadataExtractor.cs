using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using NitroxClient.GameLogic.Spawning.Metadata.Extractor.Abstract;

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
