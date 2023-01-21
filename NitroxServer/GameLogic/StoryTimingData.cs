using System;
using System.Runtime.Serialization;

namespace NitroxServer.GameLogic
{
    [Serializable]
    [DataContract]
    public class StoryTimingData
    {
        [DataMember(Order = 1)]
        public double ElapsedSeconds { get; set; }

        [DataMember(Order = 2)]
        public double? AuroraExplosionTime { get; set; }

        [DataMember(Order = 3)]
        public double? AuroraWarningTime { get; set; }

        public static StoryTimingData From(StoryManager storyManager)
        {
            return new StoryTimingData
            {
                ElapsedSeconds = storyManager.ElapsedSeconds,
                AuroraExplosionTime = storyManager.AuroraExplosionTimeMs,
                AuroraWarningTime = storyManager.AuroraWarningTimeMs
            };
        }
    }
}
