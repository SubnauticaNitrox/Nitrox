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

        /// <remarks>
        ///     This is the only field in this class that is very personal to the player this will be sent to.
        ///     The other ones are shared by everyone and are related to overall story progress.
        /// </remarks>
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
