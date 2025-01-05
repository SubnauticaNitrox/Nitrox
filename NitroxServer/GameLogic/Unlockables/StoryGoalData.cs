using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Unlockables;

[DataContract]
public class StoryGoalData
{
    [DataMember(Order = 1)]
    public ThreadSafeSet<string> CompletedGoals { get; } = [];

    [DataMember(Order = 2)]
    public ThreadSafeQueue<string> RadioQueue { get; } = [];

    [DataMember(Order = 3)]
    public ThreadSafeList<NitroxScheduledGoal> ScheduledGoals { get; set; } = [];

    public bool RemovedLatestRadioMessage()
    {
        if (RadioQueue.Count <= 0)
        {
            return false;
        }

        string message = RadioQueue.Dequeue();

        // Just like StoryGoalManager.ExecutePendingRadioMessage
        CompletedGoals.Add($"OnPlay{message}");

        return true;
    }

    public static StoryGoalData From(StoryGoalData storyGoals, ScheduleKeeper scheduleKeeper)
    {
        storyGoals.ScheduledGoals = new ThreadSafeList<NitroxScheduledGoal>(scheduleKeeper.GetScheduledGoals());
        return storyGoals;
    }

    public InitialStoryGoalData GetInitialStoryGoalData(ScheduleKeeper scheduleKeeper, Player player)
    {
        return new InitialStoryGoalData(new List<string>(CompletedGoals), new List<string>(RadioQueue), scheduleKeeper.GetScheduledGoals(), new(player.PersonalCompletedGoalsWithTimestamp));
    }
}
