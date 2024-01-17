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
        public double? AuroraCountdownTime { get; set; }

        [DataMember(Order = 3)]
        public double? AuroraWarningTime { get; set; }

        [DataMember(Order = 4)]
        public double RealTimeElapsed { get; set; }

        [DataMember(Order = 5)]
        public double? AuroraRealExplosionTime { get; set; }

        public static StoryTimingData From(StoryManager storyManager, TimeKeeper timeKeeper)
        {
            return new StoryTimingData
            {
                ElapsedSeconds = timeKeeper.ElapsedSeconds,
                AuroraCountdownTime = storyManager.AuroraCountdownTimeMs,
                AuroraWarningTime = storyManager.AuroraWarningTimeMs,
                RealTimeElapsed = timeKeeper.RealTimeElapsed,
                AuroraRealExplosionTime = storyManager.AuroraRealExplosionTime
            };
        }
    }
}
