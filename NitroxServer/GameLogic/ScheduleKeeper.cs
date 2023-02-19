using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic
{
    public class ScheduleKeeper
    {
        private readonly ThreadSafeDictionary<string, NitroxScheduledGoal> scheduledGoals = new();
        private readonly PDAStateData pdaStateData;
        private readonly StoryGoalData storyGoalData;
        private readonly TimeKeeper timeKeeper;
        private readonly PlayerManager playerManager;

        private float ElapsedSecondsFloat => (float)timeKeeper.ElapsedSeconds;

        public ScheduleKeeper(PDAStateData pdaStateData, StoryGoalData storyGoalData, TimeKeeper timeKeeper, PlayerManager playerManager)
        {
            this.pdaStateData = pdaStateData;
            this.storyGoalData = storyGoalData;
            this.timeKeeper = timeKeeper;
            this.playerManager = playerManager;

            // We still want to get a "replicated" list in memory
            for (int i = storyGoalData.ScheduledGoals.Count - 1; i >= 0; i--)
            {
                NitroxScheduledGoal scheduledGoal = storyGoalData.ScheduledGoals[i];
                // In the unlikely case that there's a duplicated entry
                if (scheduledGoals.TryGetValue(scheduledGoal.GoalKey, out NitroxScheduledGoal alreadyInGoal))
                {
                    // We remove the goal that's already in if it's planned for later than the first one
                    if (scheduledGoal.TimeExecute <= alreadyInGoal.TimeExecute)
                    {
                        UnScheduleGoal(alreadyInGoal.GoalKey);
                    }
                    continue;
                }

                scheduledGoals.Add(scheduledGoal.GoalKey, scheduledGoal);
            }
        }

        public List<NitroxScheduledGoal> GetScheduledGoals()
        {
            return scheduledGoals.Values.ToList();
        }

        public bool ContainsScheduledGoal(string goalKey)
        {
            return scheduledGoals.ContainsKey(goalKey);
        }

        public void ScheduleGoal(NitroxScheduledGoal scheduledGoal)
        {
            // Only add if it's not in already
            if (!scheduledGoals.ContainsKey(scheduledGoal.GoalKey))
            {
                // If it's not already in any PDA stuff (completed goals or PDALog)
                if (!IsAlreadyRegistered(scheduledGoal.GoalKey))
                {
                    if (scheduledGoal.TimeExecute > ElapsedSecondsFloat)
                    {
                        scheduledGoals.Add(scheduledGoal.GoalKey, scheduledGoal);
                    }
                }
            }
        }

        /// <param name="becauseOfTime">
        ///     When the server starts, it happens that there are still some goals that were supposed to happen
        ///     but didn't, so to make sure that they happen on at least one client, we postpone its execution
        /// </param>
        public void UnScheduleGoal(string goalKey, bool becauseOfTime = false)
        {
            if (!scheduledGoals.TryGetValue(goalKey, out NitroxScheduledGoal scheduledGoal))
            {
                return;
            }
            // The best solution, to ensure any bad simulation of client side, is to postpone the execution
            // If the goal is already done, no need to check anything
            if (becauseOfTime && !IsAlreadyRegistered(goalKey))
            {
                scheduledGoal.TimeExecute = ElapsedSecondsFloat + 15;
                playerManager.SendPacketToAllPlayers(new Schedule(scheduledGoal.TimeExecute, goalKey, scheduledGoal.GoalType));
                return;
            }
            scheduledGoals.Remove(goalKey);
        }

        public bool IsAlreadyRegistered(string goalKey)
        {
            return pdaStateData.PdaLog.Any(entry => entry.Key == goalKey)
                || storyGoalData.CompletedGoals.Contains(goalKey);
        }
    }
}
