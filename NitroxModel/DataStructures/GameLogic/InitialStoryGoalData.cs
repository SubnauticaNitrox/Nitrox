using System;
using System.Collections.Generic;
using BinaryPack.Attributes;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialStoryGoalData
    {
        public List<string> CompletedGoals { get; set; }
        public List<string> RadioQueue { get; set; }
        public List<string> GoalUnlocks { get; set; }
        public List<NitroxScheduledGoal> ScheduledGoals { get; set; }
        public Dictionary<string, float> PersonalCompletedGoalsWithTimestamp { get; set; }

        [IgnoreConstructor]
        protected InitialStoryGoalData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialStoryGoalData(List<string> completedGoals, List<string> radioQueue, List<string> goalUnlocks, List<NitroxScheduledGoal> scheduledGoals, Dictionary<string, float> personalCompletedGoalsWithTimestamp)
        {
            CompletedGoals = completedGoals;
            RadioQueue = radioQueue;
            GoalUnlocks = goalUnlocks;
            ScheduledGoals = scheduledGoals;
            PersonalCompletedGoalsWithTimestamp = personalCompletedGoalsWithTimestamp;
        }
    }
}
