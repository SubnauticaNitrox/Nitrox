using System.Collections.Generic;
using Newtonsoft.Json;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Unlockables;

[JsonObject(MemberSerialization.OptIn)]
public class StoryGoalData
{
    [JsonProperty]
    public ThreadSafeSet<string> CompletedGoals { get; } = new();

    [JsonProperty]
    public ThreadSafeList<string> RadioQueue { get; } = new();

    [JsonProperty]
    public ThreadSafeSet<string> GoalUnlocks { get; } = new();

    [JsonProperty]
    public ThreadSafeList<NitroxScheduledGoal> ScheduledGoals { get; set; } = new();

    public bool RemovedLatestRadioMessage()
    {
        if (RadioQueue.Count <= 0)
        {
            return false;
        }

        RadioQueue.RemoveAt(0);
        return true;
    }

    public static StoryGoalData From(StoryGoalData storyGoals, ScheduleKeeper scheduleKeeper)
    {
        storyGoals.ScheduledGoals = new ThreadSafeList<NitroxScheduledGoal>(scheduleKeeper.GetScheduledGoals());
        return storyGoals;
    }

    public InitialStoryGoalData GetInitialStoryGoalData(ScheduleKeeper scheduleKeeper)
    {
        return new InitialStoryGoalData(new List<string>(CompletedGoals), new List<string>(RadioQueue), new List<string>(GoalUnlocks), scheduleKeeper.GetScheduledGoals());
    }
}
