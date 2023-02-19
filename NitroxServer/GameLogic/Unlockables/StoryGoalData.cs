using System.Collections.Generic;
using System.Runtime.Serialization;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxServer.GameLogic.Unlockables
{
    [DataContract]
    public class StoryGoalData
    {
        [DataMember(Order = 1)]
        public ThreadSafeSet<string> CompletedGoals { get; } = new();

        [DataMember(Order = 2)]
        public ThreadSafeList<string> RadioQueue { get; } = new();

        [DataMember(Order = 3)]
        public ThreadSafeSet<string> GoalUnlocks { get; } = new();

        [DataMember(Order = 4)]
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

        public InitialStoryGoalData GetInitialStoryGoalData(ScheduleKeeper scheduleKeeper, Player player)
        {
            return new InitialStoryGoalData(new List<string>(CompletedGoals), new List<string>(RadioQueue), new List<string>(GoalUnlocks), scheduleKeeper.GetScheduledGoals(), new(player.PersonalCompletedGoalsWithTimestamp));
        }
    }
}
