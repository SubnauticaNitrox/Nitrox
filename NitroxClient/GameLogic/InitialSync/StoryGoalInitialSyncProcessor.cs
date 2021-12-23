using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.GameLogic.InitialSync
{
    public class StoryGoalInitialSyncProcessor : InitialSyncProcessor
    {

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            SetCompletedStoryGoals(packet.StoryGoalData.CompletedGoals);
            waitScreenItem.SetProgress(0.2f);
            yield return null;

            SetRadioQueue(packet.StoryGoalData.RadioQueue);
            waitScreenItem.SetProgress(0.4f);
            yield return null;

            SetGoalUnlocks(packet.StoryGoalData.GoalUnlocks);
            waitScreenItem.SetProgress(0.6f);
            yield return null;

            SetBiomeGoalTrackerGoals();
            waitScreenItem.SetProgress(0.8f);
            yield return null;

            SetScheduledGoals(packet.StoryGoalData.ScheduledGoals);
            waitScreenItem.SetProgress(1f);
            yield return null;
        }

        private void SetRadioQueue(List<string> radioQueue)
        {
            StoryGoalManager.main.pendingRadioMessages.AddRange(radioQueue);
            StoryGoalManager.main.PulsePendingMessages();
            Log.Info($"Radio queue: [{string.Join(", ", radioQueue.ToArray())}]");
        }

        private void SetCompletedStoryGoals(List<string> storyGoalData)
        {
            StoryGoalManager.main.completedGoals.Clear();
            StoryGoalManager.main.completedGoals.AddRange(storyGoalData);

            Log.Info($"Received initial sync packet with {storyGoalData.Count} completed story goals");
        }

        private void SetGoalUnlocks(List<string> goalUnlocks)
        {
            foreach (string goalUnlock in goalUnlocks)
            {
                StoryGoalManager.main.onGoalUnlockTracker.NotifyGoalComplete(goalUnlock);
            }
        }

        private void SetBiomeGoalTrackerGoals()
        {
            Dictionary<string, PDALog.Entry> entries = PDALog.entries;
            List<BiomeGoal> goals = BiomeGoalTracker.main.goals;
            int alreadyIn = 0;
            for (int i = goals.Count - 1; i >= 0; i--)
            {
                if (entries.ContainsKey(goals[i].key))
                {
                    goals.Remove(goals[i]);
                    alreadyIn++;
                }
            }
            Log.Debug($"{alreadyIn} pda log entries were removed from the goals");
        }

        private void SetScheduledGoals(List<NitroxScheduledGoal> scheduledGoals)
        {
            Dictionary<string, PDALog.Entry> entries = PDALog.entries;
            // Need to clear some duplicated goals that might have appeared during loading and before sync
            for (int i = StoryGoalScheduler.main.schedule.Count - 1; i >= 0; i--)
            {
                ScheduledGoal scheduledGoal = StoryGoalScheduler.main.schedule[i];
                if (entries.ContainsKey(scheduledGoal.goalKey))
                {
                    StoryGoalScheduler.main.schedule.Remove(scheduledGoal);
                }
            }

            foreach (NitroxScheduledGoal scheduledGoal in scheduledGoals)
            {
                ScheduledGoal goal = new ScheduledGoal();
                goal.goalKey = scheduledGoal.GoalKey;
                goal.goalType = (Story.GoalType)System.Enum.Parse(typeof(Story.GoalType), scheduledGoal.GoalType);
                goal.timeExecute = scheduledGoal.TimeExecute;
                if (goal.timeExecute >= DayNightCycle.main.timePassedAsDouble
                    && !StoryGoalScheduler.main.schedule.Any(alreadyInGoal => alreadyInGoal.goalKey == goal.goalKey)
                    && !entries.TryGetValue(goal.goalKey, out PDALog.Entry value)
                    && !StoryGoalManager.main.completedGoals.Contains(goal.goalKey))
                {
                    StoryGoalScheduler.main.schedule.Add(goal);
                }
            }
        }
    }
}
