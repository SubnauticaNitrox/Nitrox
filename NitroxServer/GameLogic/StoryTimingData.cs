using System;
using System.Runtime.Serialization;

namespace NitroxServer.GameLogic
{
    [Serializable]
    [DataContract]
    public class StoryTimingData
    {
        [DataMember(Order = 1)]
        public double ElapsedTime { get; set; }

        [DataMember(Order = 2)]
        public double? AuroraExplosionTime { get; set; }

        [DataMember(Order = 3)]
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
}
