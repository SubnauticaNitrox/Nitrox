using System;
using Newtonsoft.Json;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [Serializable]
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class StoryTimingData
    {
        [JsonProperty, ProtoMember(1)]
        public double ElapsedTime { get; set; }

        [JsonProperty, ProtoMember(2)]
        public double? AuroraExplosionTime { get; set; }

        public static StoryTimingData From(EventTriggerer eventTriggerer)
        {
            return new StoryTimingData
            {
                ElapsedTime = eventTriggerer.GetRealElapsedTime(),
                AuroraExplosionTime = eventTriggerer.AuroraExplosionTime
            };
        }
    }
}
