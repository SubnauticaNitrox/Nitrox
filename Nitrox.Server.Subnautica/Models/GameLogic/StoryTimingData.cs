using System.Runtime.Serialization;

namespace Nitrox.Server.Subnautica.Models.GameLogic
{
    [Serializable]
    [DataContract]
    internal sealed class StoryTimingData
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

        public static StoryTimingData From(StoryManager storyManager, TimeService timeService)
        {
            return new StoryTimingData
            {
                ElapsedSeconds = timeService.GameTime.TotalSeconds,
                AuroraCountdownTime = storyManager.AuroraCountdownTimeMs,
                AuroraWarningTime = storyManager.AuroraWarningTimeMs,
                RealTimeElapsed = timeService.RealTime.TotalMilliseconds,
                AuroraRealExplosionTime = storyManager.AuroraRealExplosionTime.TotalSeconds
            };
        }
    }
}
