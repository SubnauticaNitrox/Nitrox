using System;
using Newtonsoft.Json;

namespace NitroxServer.GameLogic;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class StoryTimingData
{
    [JsonProperty]
    public double ElapsedTime { get; set; }

    [JsonProperty]
    public double? AuroraExplosionTime { get; set; }

    [JsonProperty]
    public double? AuroraWarningTime { get; set; }

    public static StoryTimingData From(EventTriggerer eventTriggerer)
    {
        return new StoryTimingData
        {
            ElapsedTime = eventTriggerer.ElapsedTimeMs,
            AuroraExplosionTime = eventTriggerer.AuroraExplosionTimeMs,
            AuroraWarningTime = eventTriggerer.AuroraWarningTimeMs,
        };
    }
}
