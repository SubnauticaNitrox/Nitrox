using System;
using Newtonsoft.Json;
using NitroxServer.GameLogic.Unlockables;
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

        public static GameData From(PDAStateData pdaState, StoryGoalData storyGoals, ScheduleKeeper scheduleKeeper, EventTriggerer eventTriggerer)
        {
            return new GameData
            {
                PDAState = pdaState,
                StoryGoals = StoryGoalData.From(storyGoals, scheduleKeeper),
                StoryTiming = StoryTimingData.From(eventTriggerer)
            };
        }
    }
}
