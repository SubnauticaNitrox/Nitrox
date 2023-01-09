using System.Collections;
using System.Collections.Generic;
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
            SetupAurora(packet),
            SetRadioQueue(packet),
            SetCompletedStoryGoals(packet),
            SetGoalUnlocks(packet),
            SetBiomeGoalTrackerGoals(packet),
            SetScheduledGoals(packet)
        };
    }

    private IEnumerator SetTimeData(InitialPlayerSync packet)
    {
        NitroxServiceLocator.LocateService<TimeManager>().ProcessUpdate(packet.InitialTimeData.TimePacket);
        yield break;
    }

    private IEnumerator SetupAurora(InitialPlayerSync packet)
    {
        // TODO: Separate this data from this packet
        InitialTimeData timeData = packet.InitialTimeData;

        AuroraWarnings auroraWarnings = GameObject.FindObjectOfType<AuroraWarnings>();
        auroraWarnings.timeSerialized = DayNightCycle.main.timePassedAsFloat;
        auroraWarnings.OnProtoDeserialize(null);

        CrashedShipExploder.main.version = 2;
        CrashedShipExploder.main.timeToStartCountdown = timeData.CrashedShipExploderData.TimeToStartCountdown;
        CrashedShipExploder.main.timeToStartWarning = timeData.CrashedShipExploderData.TimeToStartWarning;
        CrashedShipExploder.main.OnProtoDeserialize(null);

        // TODO: if (StoryGoalManager.main.IsGoalComplete(this.gunDeactivate.key))
        StoryGoalCustomEventHandler.main.gunDisabled = false;

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

    // Must happen after CompletedGoals
    private IEnumerator SetScheduledGoals(InitialPlayerSync packet)
    {
        List<NitroxScheduledGoal> scheduledGoals = packet.StoryGoalData.ScheduledGoals;
        List<string> goalKeys = scheduledGoals.ConvertAll((goal) => goal.GoalKey);

        foreach (NitroxScheduledGoal scheduledGoal in scheduledGoals)
        {
            // Clear duplicated goals that might have appeared during loading and before sync
            StoryGoalScheduler.main.schedule.RemoveAll(goal => goal.goalKey == scheduledGoal.GoalKey);

            ScheduledGoal goal = new()
            {
                goalKey = scheduledGoal.GoalKey,
                goalType = (Story.GoalType)scheduledGoal.GoalType,
                timeExecute = scheduledGoal.TimeExecute,
            };
            if (goal.timeExecute >= DayNightCycle.main.timePassedAsDouble && !StoryGoalManager.main.completedGoals.Contains(goal.goalKey))
            {
                StoryGoalScheduler.main.schedule.Add(goal);
            }
        }

        yield break;
    }
}
