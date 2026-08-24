using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

internal sealed class StoryGoalData
{
    public ThreadSafeSet<string> CompletedGoals { get; } = [];

    public ThreadSafeQueue<string> RadioQueue { get; } = [];

    public ThreadSafeList<NitroxScheduledGoal> ScheduledGoals { get; set; } = [];

    public static StoryGoalData From(StoryGoalData storyGoals, StoryScheduler storyScheduler)
    {
        storyGoals.ScheduledGoals = new ThreadSafeList<NitroxScheduledGoal>(storyScheduler.GetScheduledStories());
        return storyGoals;
    }
}
