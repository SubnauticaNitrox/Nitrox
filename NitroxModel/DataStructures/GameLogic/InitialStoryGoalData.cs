using System;
using System.Collections.Generic;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class InitialStoryGoalData
    {
        public List<string> CompletedGoals { get; set; }
        public List<string> RadioQueue { get; set; }
        public List<string> GoalUnlocks { get; set; }

        protected InitialStoryGoalData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialStoryGoalData(List<string> completedGoals, List<string> radioQueue, List<string> goalUnlocks)
        {
            CompletedGoals = completedGoals;
            RadioQueue = radioQueue;
            GoalUnlocks = goalUnlocks;
        }

        public override string ToString()
        {
            return $"[InitialStoryGoalData - CompletedGoals: {string.Join(", ", CompletedGoals)} RadioQueue: {string.Join(", ", RadioQueue)} GoalUnlocks: {string.Join(", ", GoalUnlocks)}]";
        }
    }
}
