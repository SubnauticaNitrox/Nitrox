using System.Runtime.Serialization;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace Nitrox.Server.Subnautica.Models.GameLogic.Unlockables;

[DataContract]
internal sealed class StoryGoalData
{
    [DataMember(Order = 1)]
    public ThreadSafeSet<string> CompletedGoals { get; } = [];

    [DataMember(Order = 2)]
    public ThreadSafeQueue<string> RadioQueue { get; } = [];

    [DataMember(Order = 3)]
    public ThreadSafeList<NitroxScheduledGoal> ScheduledGoals { get; set; } = [];

    public static StoryGoalData From(StoryGoalData storyGoals, StoryScheduler storyScheduler)
    {
        storyGoals.ScheduledGoals = new ThreadSafeList<NitroxScheduledGoal>(storyScheduler.GetScheduledStories());
        return storyGoals;
    }
}
