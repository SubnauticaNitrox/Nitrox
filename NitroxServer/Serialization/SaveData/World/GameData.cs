using System;
using Newtonsoft.Json;
using NitroxServer.GameLogic.WorldTiming;

namespace NitroxServer.Serialization.SaveData;

[Serializable]
[JsonObject(MemberSerialization.OptIn)]
public class GameData
{
    [JsonProperty]
    public PDAStateData PDAState { get; set; }

    [JsonProperty]
    public StoryGoalData StoryGoals { get; set; }

    [JsonProperty]
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
