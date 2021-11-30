using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic
{
    public class ScheduleKeeper
    {
        private readonly Dictionary<string, NitroxScheduledGoal> scheduledGoals = new();
        private readonly PDAStateData pdaStateData;
        private readonly StoryGoalData storyGoalData;
        private readonly EventTriggerer eventTriggerer;
        private readonly PlayerManager playerManager;

        public float CurrentTime => (float)eventTriggerer.GetRealElapsedTime();

        public ScheduleKeeper(ThreadSafeList<NitroxScheduledGoal> nitroxScheduledGoals, PDAStateData pdaStateData, StoryGoalData storyGoalData, EventTriggerer eventTriggerer, PlayerManager playerManager)
        {
            this.pdaStateData = pdaStateData;
            this.storyGoalData = storyGoalData;
            this.eventTriggerer = eventTriggerer;
            this.playerManager = playerManager;

            // We still want to get a "replicated" list in memory
            for (int i = nitroxScheduledGoals.Count - 1; i >= 0; i--)
            {
                NitroxScheduledGoal scheduledGoal = nitroxScheduledGoals[i];
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
                    if (scheduledGoal.TimeExecute > CurrentTime)
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
                scheduledGoal.TimeExecute = CurrentTime + 15;
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

        public void SendCurrentTimePacket(bool initialSync)
        {
            playerManager.SendPacketToAllPlayers(new TimeChange(CurrentTime, initialSync));
        }

        // We shall prefer this method to the TimeKeeper's one that are based on a weirdly-made time
        // While this one is based on the working EventTriggerer's time which works with stopwatches
        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    eventTriggerer.ElapsedTime += 1200.0 - CurrentTime % 1200.0 + 600.0;
                    break;
                case TimeModification.NIGHT:
                    eventTriggerer.ElapsedTime += 1200.0 - CurrentTime % 1200.0;
                    break;
                case TimeModification.SKIP:
                    eventTriggerer.ElapsedTime += 600.0 - CurrentTime % 600.0;
                    break;
            }

            SendCurrentTimePacket(false);
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
