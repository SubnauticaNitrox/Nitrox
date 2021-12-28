using System.Collections.Generic;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    public class InitialStoryGoalData
    {
        [Index(0)]
        public virtual List<string> CompletedGoals { get; set; }
        [Index(1)]
        public virtual List<string> RadioQueue { get; set; }
        [Index(2)]
        public virtual List<string> GoalUnlocks { get; set; }
        [Index(3)]
        public virtual List<NitroxScheduledGoal> ScheduledGoals { get; set; }

        public InitialStoryGoalData()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public InitialStoryGoalData(List<string> completedGoals, List<string> radioQueue, List<string> goalUnlocks, List<NitroxScheduledGoal> scheduledGoals)
        {
            CompletedGoals = completedGoals;
            RadioQueue = radioQueue;
            GoalUnlocks = goalUnlocks;
            ScheduledGoals = scheduledGoals;
        }
    }
}
