using System;
using System.Collections.Generic;

namespace Nitrox.Model.DataStructures.GameLogic
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
    }
}
