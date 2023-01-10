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

        [DataMember(Order = 4)]
        public float? SunbeamCountdownStartingTime { get; set; }


        public static StoryTimingData From(EventTriggerer eventTriggerer)
        {
            return new StoryTimingData
            {
                ElapsedSeconds = eventTriggerer.ElapsedSeconds,
                AuroraExplosionTime = eventTriggerer.AuroraExplosionTimeMs,
                AuroraWarningTime = eventTriggerer.AuroraWarningTimeMs,
                SunbeamCountdownStartingTime = eventTriggerer.SunbeamCountdownStartingTime
            };
        }
    }
}
