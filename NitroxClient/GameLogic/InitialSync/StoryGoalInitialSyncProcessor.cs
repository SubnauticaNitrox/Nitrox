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
            SetupAurora(packet),
            SetScheduledGoals(packet)
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

        // TODO: Move PlayerInitialSyncProcessor.SetPlayerCompletedGoals to here

        StringBuilder builder = new("Received initial sync packet with");
        builder.AppendLine($"""
             - Completed story goals: {completedGoals.Count}
             - Radio queue : {radioQueue.Count}
            """);
        Log.Info($"{builder}");
        yield break;
    }

    // Must happen after CompletedGoals
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

        StoryGoalCustomEventHandler.main.Awake();

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
