using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic
{
    public class ScheduleKeeper
    {
        public Dictionary<string, FakeScheduledGoal> Schedule = new Dictionary<string, FakeScheduledGoal>();
        private ThreadSafeCollection<FakeScheduledGoal> scheduledGoals;
        private PDAStateData pdaStateData;
        private StoryGoalData storyGoalData;
        private EventTriggerer eventTriggerer;
        private PlayerManager playerManager;
     
        public ScheduleKeeper(ThreadSafeCollection<FakeScheduledGoal> scheduledGoals, PDAStateData pdaStateData, StoryGoalData storyGoalData)
        {
            this.pdaStateData = pdaStateData;
            this.storyGoalData = storyGoalData;

            if (scheduledGoals == null)
            {
                return;
            }
            // This one var is the reference to the original one
            this.scheduledGoals = scheduledGoals;
            // We still want to get a "replicated" list in memory
            for (int i = scheduledGoals.Count - 1; i >= 0; i--)
            {
                FakeScheduledGoal scheduledGoal = scheduledGoals[i];
                // In the unlikely case that there's a duplicated entry
                if (Schedule.TryGetValue(scheduledGoal.GoalKey, out FakeScheduledGoal alreadyInGoal))
                {
                    // We remove the goal that's already in if it's planned for later than the first one
                    if (GetLatestOfTwoGoals(scheduledGoal, alreadyInGoal).TimeExecute == alreadyInGoal.TimeExecute)
                    {
                        UnScheduleGoal(alreadyInGoal.GoalKey);
                    }
                    // Else we simply don't add it
                    continue;
                }
                
                Schedule.Add(scheduledGoal.GoalKey, scheduledGoal);
            }
        }

        public List<FakeScheduledGoal> GetScheduledGoals()
        {
            CleanScheduledGoals();
            return new(Schedule.Values);
        }

        public bool ContainsScheduledGoal(string goalKey)
        {
            return Schedule.TryGetValue(goalKey, out FakeScheduledGoal value);
        }

        public void ScheduleGoal(FakeScheduledGoal scheduledGoal)
        {
            // Some weird goal that activates when the player swims, tho it should not get here
            if (scheduledGoal.GoalKey == "PlayerDiving")
            {
                Log.WarnOnce("Some weird thing happened: server received a goal named \"PlayerDiving\", even though the client is not supposed to send them. Maybe one of them is playing with an outdated version ?");
                return;
            }

            // Only add if it's not in already
            if (!Schedule.TryGetValue(scheduledGoal.GoalKey, out FakeScheduledGoal value))
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

        public void UnScheduleGoal(string goalKey)
        {
            UnScheduleGoal(goalKey, false);
        }

        public void UnScheduleGoal(string goalKey, bool becauseOfTime)
        {
            if (!Schedule.TryGetValue(goalKey, out FakeScheduledGoal scheduledGoal))
            {
                return;
            }
            // The best solution, to ensure any bad simulation of client side, is to postpone the execution
            // If the goal is already done, no need to check anything
            if (becauseOfTime && !IsAlreadyRegistered(goalKey))
            {
                scheduledGoal.TimeExecute = CurrentTime + 15;
                if (playerManager.GetConnectedPlayers().Count > 0)
                {
                    playerManager.SendPacketToAllPlayers(new Schedule(CurrentTime + 15, goalKey, scheduledGoal.GoalType));
                    // Log.Debug($"Players connected, sending them the scheduledGoal [{scheduledGoal.GoalKey}]");
                }
                /* else
                {
                    Log.Debug($"No player connected, scheduledGoal [{scheduledGoal.GoalKey}] will be postponed");
                }*/
                return;
            }
            Schedule.Remove(goalKey);
            scheduledGoals.Remove(scheduledGoal);
        }

        public void Init(EventTriggerer eventTriggerer, PlayerManager playerManager)
        {
            this.eventTriggerer = eventTriggerer;
            this.playerManager = playerManager;
            // Only clean scheduled goals when these classes are avalaible
            CleanScheduledGoals();
        }
        
        // Cleans from duplicated entries and outdated entries
        public void CleanScheduledGoals()
        {
            // Log.Debug($"CleanScheduledGoals() [time={CurrentTime}]");
            for (int i = scheduledGoals.Count - 1; i >= 0; i--)
            {
                FakeScheduledGoal scheduledGoal = scheduledGoals[i];

                // will let the check occur or not depending on if the goal was already unscheduled
                bool removed = false;
                for (int j = scheduledGoals.Count - 1; j >= 0; j--)
                {
                    FakeScheduledGoal otherGoal = scheduledGoals[j];
                    if (otherGoal.GoalKey == scheduledGoal.GoalKey && otherGoal.TimeExecute != scheduledGoal.TimeExecute)
                    {
                        removed = true;
                        // Log.Debug($"Removing because duplicated [GoalKey={scheduledGoal.GoalKey}, time={scheduledGoal.TimeExecute}, otherTime={otherGoal.TimeExecute}] [Diff={Math.Abs(otherGoal.TimeExecute - scheduledGoal.TimeExecute)}]");
                        // Remove the latest one
                        UnScheduleGoal(GetLatestOfTwoGoals(scheduledGoal, otherGoal).GoalKey);
                    }
                }
                // Check if it's still in, then if it's supposed to have occurred
                if (!removed)
                {
                    // Log.Debug($"Checking: [{scheduledGoal.GoalKey}: {scheduledGoal.TimeExecute}<{CurrentTime}]");
                    if (scheduledGoal.TimeExecute < CurrentTime)
                    {
                        // Log.Debug($"Already passed [{scheduledGoal.GoalKey}]");
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

        public FakeScheduledGoal GetLatestOfTwoGoals(FakeScheduledGoal firstGoal, FakeScheduledGoal secondGoal)
        {
            if (firstGoal.TimeExecute >= secondGoal.TimeExecute)
            {
                return firstGoal;
            }
            return secondGoal;
        }

        public bool IsAlreadyRegistered(string goalKey)
        {
            bool alreadyRegistered = pdaStateData.PdaLog.Any(entry => entry.Key == goalKey);
            if (!alreadyRegistered)
            {
                alreadyRegistered = storyGoalData.CompletedGoals.Contains(goalKey);
            }
            return alreadyRegistered;
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }

        public float CurrentTime => (float)eventTriggerer.GetRealElapsedTime();
    }
}
