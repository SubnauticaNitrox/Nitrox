using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.InitialSync.Abstract;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using Story;

namespace NitroxClient.GameLogic.InitialSync;

public class StoryGoalInitialSyncProcessor : InitialSyncProcessor
{
    private readonly TimeManager timeManager;

    public StoryGoalInitialSyncProcessor(TimeManager timeManager)
    {
        this.timeManager = timeManager;

        AddStep(SetTimeData);
        AddStep(SetupStoryGoalManager);
        AddStep(SetupTrackers);
        AddStep(SetupAuroraAndSunbeam);
        AddStep(SetScheduledGoals);
    }

    private static void SetupStoryGoalManager(InitialPlayerSync packet)
    {
        List<string> completedGoals = packet.StoryGoalData.CompletedGoals;
        List<string> radioQueue = packet.StoryGoalData.RadioQueue;
        Dictionary<string, float> personalGoals = packet.StoryGoalData.PersonalCompletedGoalsWithTimestamp;
        StoryGoalManager storyGoalManager = StoryGoalManager.main;

        storyGoalManager.completedGoals.AddRange(completedGoals);

        storyGoalManager.pendingRadioMessages.AddRange(radioQueue);
        storyGoalManager.PulsePendingMessages();

        // Restore states of GoalManager and the (tutorial) arrow system
        foreach (KeyValuePair<string, float> entry in personalGoals)
        {
            Goal entryGoal = GoalManager.main.goals.Find(goal => goal.customGoalName.Equals(entry.Key));
            if (entryGoal != null)
            {
                entryGoal.SetTimeCompleted(entry.Value);
            }
        }
        GoalManager.main.completedGoalNames.AddRange(personalGoals.Keys);
        PlayerWorldArrows.main.completedCustomGoals.AddRange(personalGoals.Keys);

        // Deactivate the current arrow if it was completed
        if (personalGoals.Any(goal => goal.Key.Equals(WorldArrowManager.main.currentGoalText)))
        {
            WorldArrowManager.main.DeactivateArrow();
        }

        Log.Info($"""
        Received initial sync packet with:
        - Completed story goals : {completedGoals.Count}
        - Personal goals        : {personalGoals.Count}
        - Radio queue           : {radioQueue.Count}
        """);
    }

    private static void SetupTrackers(InitialPlayerSync packet)
    {
        List<string> completedGoals = packet.StoryGoalData.CompletedGoals;
        StoryGoalManager storyGoalManager = StoryGoalManager.main;
        OnGoalUnlockTracker onGoalUnlockTracker = storyGoalManager.onGoalUnlockTracker;

        // Initialize CompoundGoalTracker and OnGoalUnlockTracker and clear their already completed goals
        // StoryGoalManager.OnSceneObjectsLoaded() is already applied so we can directly modify the trackers' content

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

        // OnGoalUnlock might trigger the creation of a signal which is later on set to invisible when getting close to it
        // the invisibility is managed by PingInstance_Set_Patches and is restored during PlayerPreferencesInitialSyncProcessor
        // So we still need to recreate the signals at every game launch

        // To avoid having the SignalPing play its sound we just make its notification null while triggering it
        // (the sound is something like "coordinates added to the gps" or something)
        PDANotification pdaNotification = onGoalUnlockTracker.signalPrefab.GetComponent<SignalPing>().vo;
        onGoalUnlockTracker.signalPrefab.GetComponent<SignalPing>().vo = null;

        foreach (OnGoalUnlock onGoalUnlock in onGoalUnlockTracker.unlockData.onGoalUnlocks)
        {
            if (completedGoals.Contains(onGoalUnlock.goal))
            {
                // Code adapted from OnGoalUnlock.Trigger
                foreach (UnlockSignalData unlockSignalData in onGoalUnlock.signals)
                {
                    unlockSignalData.Trigger(onGoalUnlockTracker);
                }
            }
        }
        
        // recover the notification sound
        onGoalUnlockTracker.signalPrefab.GetComponent<SignalPing>().vo = pdaNotification;
    }

    // Must happen after CompletedGoals
    private static void SetupAuroraAndSunbeam(InitialPlayerSync packet)
    {
        TimeData timeData = packet.TimeData;

        AuroraWarnings auroraWarnings = UnityEngine.Object.FindObjectOfType<AuroraWarnings>();
        auroraWarnings.timeSerialized = DayNightCycle.main.timePassedAsFloat;
        auroraWarnings.OnProtoDeserialize(null);

        CrashedShipExploder.main.version = 2;
        StoryManager.UpdateAuroraData(timeData.AuroraEventData);
        CrashedShipExploder.main.timeSerialized = DayNightCycle.main.timePassedAsFloat;
        CrashedShipExploder.main.OnProtoDeserialize(null);

        // Sunbeam countdown is deducted from the scheduled goal PrecursorGunAimCheck
        NitroxScheduledGoal sunbeamCountdownGoal = packet.StoryGoalData.ScheduledGoals.Find(goal => string.Equals(goal.GoalKey, "PrecursorGunAimCheck", StringComparison.OrdinalIgnoreCase));
        if (sunbeamCountdownGoal != null)
        {
            StoryGoalCustomEventHandler.main.countdownActive = true;
            StoryGoalCustomEventHandler.main.countdownStartingTime = sunbeamCountdownGoal.TimeExecute - 2370;
            // See StoryGoalCustomEventHandler.endTime for calculation (endTime - 30 seconds)
        }
    }

    // Must happen after CompletedGoals
    private static void SetScheduledGoals(InitialPlayerSync packet)
    {
        List<NitroxScheduledGoal> scheduledGoals = packet.StoryGoalData.ScheduledGoals;

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

        RefreshStoryWithLatestData();
    }

    // Must happen after CompletedGoals
    private static void RefreshStoryWithLatestData()
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
    }

    private void SetTimeData(InitialPlayerSync packet)
    {
        timeManager.ProcessUpdate(packet.TimeData.TimePacket);
        timeManager.InitRealTimeElapsed(packet.TimeData.TimePacket.RealTimeElapsed, packet.TimeData.TimePacket.UpdateTime, packet.IsFirstPlayer);
        timeManager.AuroraRealExplosionTime = packet.TimeData.AuroraEventData.AuroraRealExplosionTime;
    }
}
