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

        public static StoryTimingData From(StoryManager storyManager)
        {
            return new StoryTimingData
            {
                ElapsedSeconds = storyManager.ElapsedSeconds,
                AuroraCountdownTime = storyManager.AuroraCountdownTimeMs,
                AuroraWarningTime = storyManager.AuroraWarningTimeMs
            };
        }
    }
}
