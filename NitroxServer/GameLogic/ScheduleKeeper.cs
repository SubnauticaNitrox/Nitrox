using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic
{
    public class ScheduleKeeper
    {
        public Dictionary<string, NitroxScheduledGoal> Schedule = new Dictionary<string, NitroxScheduledGoal>();
        private ThreadSafeList<NitroxScheduledGoal> scheduledGoals;
        private PDAStateData pdaStateData;
        private StoryGoalData storyGoalData;
        private EventTriggerer eventTriggerer;
        private PlayerManager playerManager;
        public float CurrentTime => (float)eventTriggerer.GetRealElapsedTime();

        public ScheduleKeeper(ThreadSafeList<NitroxScheduledGoal> scheduledGoals, PDAStateData pdaStateData, StoryGoalData storyGoalData, EventTriggerer eventTriggerer, PlayerManager playerManager)
        {
            this.pdaStateData = pdaStateData;
            this.storyGoalData = storyGoalData;
            this.eventTriggerer = eventTriggerer;
            this.playerManager = playerManager;

            // This one var is the reference to the original one
            this.scheduledGoals = scheduledGoals;
            // We still want to get a "replicated" list in memory
            for (int i = scheduledGoals.Count - 1; i >= 0; i--)
            {
                NitroxScheduledGoal scheduledGoal = scheduledGoals[i];
                // In the unlikely case that there's a duplicated entry
                if (Schedule.TryGetValue(scheduledGoal.GoalKey, out NitroxScheduledGoal alreadyInGoal))
                {
                    // We remove the goal that's already in if it's planned for later than the first one
                    if (NitroxScheduledGoal.GetLatestOfTwoGoals(scheduledGoal, alreadyInGoal).TimeExecute == alreadyInGoal.TimeExecute)
                    {
                        UnScheduleGoal(alreadyInGoal.GoalKey);
                    }
                    continue;
                }
                
                Schedule.Add(scheduledGoal.GoalKey, scheduledGoal);
            }

            CleanScheduledGoals();
        }

        public List<NitroxScheduledGoal> GetScheduledGoals()
        {
            CleanScheduledGoals();
            return new(Schedule.Values);
        }

        public bool ContainsScheduledGoal(string goalKey)
        {
            return Schedule.ContainsKey(goalKey);
        }

        public void ScheduleGoal(NitroxScheduledGoal scheduledGoal)
        {
            // Only add if it's not in already
            if (!Schedule.ContainsKey(scheduledGoal.GoalKey))
            {
                // If it's not already in any PDA stuff (completed goals or PDALog)
                if (!IsAlreadyRegistered(scheduledGoal.GoalKey))
                {
                    if (scheduledGoal.TimeExecute > CurrentTime)
                    {
                        Schedule.Add(scheduledGoal.GoalKey, scheduledGoal);
                        scheduledGoals.Add(scheduledGoal);
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
            if (!Schedule.TryGetValue(goalKey, out NitroxScheduledGoal scheduledGoal))
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
            Schedule.Remove(goalKey);
            scheduledGoals.Remove(scheduledGoal);
        }
        
        // Cleans from duplicated entries and outdated entries
        public void CleanScheduledGoals()
        {
            for (int i = scheduledGoals.Count - 1; i >= 0; i--)
            {
                NitroxScheduledGoal scheduledGoal = scheduledGoals[i];

                // will let the check occur or not depending on if the goal was already unscheduled
                bool removed = false;
                for (int j = scheduledGoals.Count - 1; j >= 0; j--)
                {
                    NitroxScheduledGoal otherGoal = scheduledGoals[j];
                    if (otherGoal.GoalKey == scheduledGoal.GoalKey && otherGoal.TimeExecute != scheduledGoal.TimeExecute)
                    {
                        removed = true;
                        // Remove the latest one
                        UnScheduleGoal(NitroxScheduledGoal.GetLatestOfTwoGoals(scheduledGoal, otherGoal).GoalKey);
                    }
                }
                // Check if it's still in, then if it's supposed to have occurred
                if (!removed)
                {
                    if (scheduledGoal.TimeExecute < CurrentTime)
                    {
                        UnScheduleGoal(scheduledGoal.GoalKey, true);
                    }
                }
            }
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

        public bool IsAlreadyRegistered(string goalKey)
        {
            return pdaStateData.PdaLog.Any(entry => entry.Key == goalKey)
                || storyGoalData.CompletedGoals.Contains(goalKey);
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
