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
#if SUBNAUTICA
        [DataMember(Order = 2)]
        public double? AuroraCountdownTime { get; set; }

        [DataMember(Order = 3)]
        public double? AuroraWarningTime { get; set; }
#endif

        public static StoryTimingData From(StoryManager storyManager, TimeKeeper timeKeeper)
        {
            return new StoryTimingData
            {
                ElapsedSeconds = timeKeeper.ElapsedSeconds,
#if SUBNAUTICA
                AuroraCountdownTime = storyManager.AuroraCountdownTimeMs,
                AuroraWarningTime = storyManager.AuroraWarningTimeMs
#endif
            };
        }
    }
}
