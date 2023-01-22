using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            SetupStoryGoalManager(packet),
            SetupTrackers(packet),
            SetupAuroraAndSunbeam(packet),
            SetScheduledGoals(packet),
            RefreshWithLatestData()
        };
    }

    private IEnumerator SetTimeData(InitialPlayerSync packet)
    {
        NitroxServiceLocator.LocateService<TimeManager>().ProcessUpdate(packet.InitialTimeData.TimePacket);
        yield break;
    }

    private IEnumerator SetupStoryGoalManager(InitialPlayerSync packet)
    {
        List<string> completedGoals = packet.StoryGoalData.CompletedGoals;
        List<string> radioQueue = packet.StoryGoalData.RadioQueue;
        StoryGoalManager storyGoalManager = StoryGoalManager.main;

        storyGoalManager.completedGoals.AddRange(completedGoals);

        storyGoalManager.pendingRadioMessages.AddRange(radioQueue);
        storyGoalManager.PulsePendingMessages();

        // Restore states of GoalManager and the (tutorial) arrow system
        foreach (KeyValuePair<string, float> entry in packet.CompletedGoals)
        {
            Goal entryGoal = GoalManager.main.goals.Find(goal => goal.customGoalName.Equals(entry.Key));
            if (entryGoal != null)
            {
                entryGoal.SetTimeCompleted(entry.Value);
            }
        }
        GoalManager.main.completedGoalNames.AddRange(packet.CompletedGoals.Keys);
        PlayerWorldArrows.main.completedCustomGoals.AddRange(packet.CompletedGoals.Keys);

        // Deactivate the current arrow if it was completed
        if (packet.CompletedGoals.Any(goal => goal.Equals(WorldArrowManager.main.currentGoalText)))
        {
            WorldArrowManager.main.DeactivateArrow();
        }
        
        StringBuilder builder = new();
        builder.AppendLine($"""
        Received initial sync packet with:
        - Completed story goals : {completedGoals.Count}
        - Personal goals        : {packet.CompletedGoals.Count}
        - Radio queue           : {radioQueue.Count}
        """);
        Log.Info($"{builder}");
        yield break;
    }

    private IEnumerator SetupTrackers(InitialPlayerSync packet)
    {
        List<string> completedGoals = packet.StoryGoalData.CompletedGoals;
        StoryGoalManager storyGoalManager = StoryGoalManager.main;

        // Initialize CompoundGoalTracker and OnGoalUnlockTracker and clear their already completed goals
        storyGoalManager.OnSceneObjectsLoaded();

        storyGoalManager.compoundGoalTracker.goals.RemoveAll(goal => completedGoals.Contains(goal.key));
        completedGoals.ForEach(goal => storyGoalManager.onGoalUnlockTracker.goalUnlocks.Remove(goal));

        // Clean LocationGoalTracker, BiomeGoalTracker and ItemGoalTracker already completed goals
        storyGoalManager.locationGoalTracker.goals.RemoveAll(goal => completedGoals.Contains(goal.key));
        storyGoalManager.biomeGoalTracker.goals.RemoveAll(goal => completedGoals.Contains(goal.key));

        List<TechType> techTypesToRemove = new();
        foreach (KeyValuePair<TechType, List<ItemGoal>> entry in storyGoalManager.itemGoalTracker.goals)
        {
            // Goals are all triggered at the same time but we don't know if some entries share certain goals
            if (entry.Value.All(goal => completedGoals.Contains(goal.key)))
            {
                techTypesToRemove.Add(entry.Key);
                continue;
            }
        }
        techTypesToRemove.ForEach(techType => storyGoalManager.itemGoalTracker.goals.Remove(techType));
        yield break;
    }

    // Must happen after CompletedGoals
    private IEnumerator SetupAuroraAndSunbeam(InitialPlayerSync packet)
    {
        // TODO: Separate this data from this packet
        InitialTimeData timeData = packet.InitialTimeData;

        AuroraWarnings auroraWarnings = GameObject.FindObjectOfType<AuroraWarnings>();
        auroraWarnings.timeSerialized = DayNightCycle.main.timePassedAsFloat;
        auroraWarnings.OnProtoDeserialize(null);

        CrashedShipExploder.main.version = 2;
        NitroxStoryManager.UpdateAuroraData(timeData.CrashedShipExploderData);        
        CrashedShipExploder.main.timeSerialized = DayNightCycle.main.timePassedAsFloat;
        CrashedShipExploder.main.OnProtoDeserialize(null);

        // Sunbeam countdown is deducted from the scheduled goal PrecursorGunAimCheck
        NitroxScheduledGoal sunbeamCountdownGoal = packet.StoryGoalData.ScheduledGoals.Find(goal => goal.GoalKey == "PrecursorGunAimCheck");
        if (sunbeamCountdownGoal != null)
        {
            StoryGoalCustomEventHandler.main.countdownActive = true;
            StoryGoalCustomEventHandler.main.countdownStartingTime = sunbeamCountdownGoal.TimeExecute - 2370;
            // See StoryGoalCustomEventHandler.endTime for calculation (endTime - 30 seconds)
        }

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

    // Must happen after CompletedGoals
    private IEnumerator RefreshWithLatestData()
    {
        // If those aren't set up yet, they'll initialize correctly in time
        // Else, we need to force them to acquire the right data
        if (StoryGoalCustomEventHandler.main)
        {
            StoryGoalCustomEventHandler.main.Awake();
        }
        if (PrecursorGunStoryEvents.main)
        {
            PrecursorGunStoryEvents.main.Start();
        }
        yield break;
    }
}
