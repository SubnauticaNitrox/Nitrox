using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using Story;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

public class StoryGoalInitialSyncProcessor : InitialSyncProcessor
{
    public override List<IEnumerator> GetSteps(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        return new List<IEnumerator> {
            SetTimeData(packet),
            SetRadioQueue(packet),
            SetCompletedStoryGoals(packet),
            SetGoalUnlocks(packet),
            SetBiomeGoalTrackerGoals(packet),
            SetScheduledGoals(packet)
        };
    }

    private IEnumerator SetTimeData(InitialPlayerSync packet)
    {
        InitialTimeData timeData = packet.InitialTimeData;

        NitroxServiceLocator.LocateService<TimeManager>().ProcessUpdate(timeData.TimePacket);

        AuroraWarnings auroraWarnings = GameObject.FindObjectOfType<AuroraWarnings>();
        auroraWarnings.timeSerialized = DayNightCycle.main.timePassedAsFloat;
        auroraWarnings.OnProtoDeserialize(null);

        CrashedShipExploder.main.version = 2;
        CrashedShipExploder.main.timeToStartCountdown = timeData.CrashedShipExploderData.TimeToStartCountdown;
        CrashedShipExploder.main.timeToStartWarning = timeData.CrashedShipExploderData.TimeToStartWarning;
        CrashedShipExploder.main.OnProtoDeserialize(null);
        yield break;
    }

    private IEnumerator SetRadioQueue(InitialPlayerSync packet)
    {
        List<string> radioQueue = packet.StoryGoalData.RadioQueue;

        StoryGoalManager.main.pendingRadioMessages.AddRange(radioQueue);
        StoryGoalManager.main.PulsePendingMessages();
        Log.Info($"Radio queue: [{string.Join(", ", radioQueue.ToArray())}]");
        yield break;
    }

    private IEnumerator SetCompletedStoryGoals(InitialPlayerSync packet)
    {
        List<string> storyGoalData = packet.StoryGoalData.CompletedGoals;
        StoryGoalManager.main.completedGoals.Clear();
        StoryGoalManager.main.completedGoals.AddRange(storyGoalData);

        Log.Info($"Received initial sync packet with {storyGoalData.Count} completed story goals");
        yield break;
    }

    private IEnumerator SetGoalUnlocks(InitialPlayerSync packet)
    {
        List<string> goalUnlocks = packet.StoryGoalData.GoalUnlocks;
        foreach (string goalUnlock in goalUnlocks)
        {
            StoryGoalManager.main.onGoalUnlockTracker.NotifyGoalComplete(goalUnlock);
        }
        yield break;
    }

    private IEnumerator SetBiomeGoalTrackerGoals(InitialPlayerSync packet)
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
        yield break;
    }

    private IEnumerator SetScheduledGoals(InitialPlayerSync packet)
    {
        List<NitroxScheduledGoal> scheduledGoals = packet.StoryGoalData.ScheduledGoals;

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

        yield break;
    }
}
