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

        public InitialStoryGoalData()
        {
        }

        public InitialStoryGoalData(List<string> completedGoals, List<string> radioQueue, List<string> goalUnlocks)
        {
            CompletedGoals = completedGoals;
            RadioQueue = radioQueue;
            GoalUnlocks = goalUnlocks;
        }
    }
}
