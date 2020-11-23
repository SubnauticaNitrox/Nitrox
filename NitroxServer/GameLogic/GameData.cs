using NitroxServer.GameLogic.Unlockables;
using System;
using Newtonsoft.Json;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Bases
{
    [Serializable]
    [ProtoContract, JsonObject(MemberSerialization.OptIn)]
    public class GameData
    {
        [JsonProperty, ProtoMember(1)]
        public PDAStateData PDAState { get; set; }

        [JsonProperty, ProtoMember(2)]
        public StoryGoalData StoryGoals { get; set; }

        [JsonProperty, ProtoMember(3)]
        public StoryTimingData StoryTiming { get; set; }
    }
}
