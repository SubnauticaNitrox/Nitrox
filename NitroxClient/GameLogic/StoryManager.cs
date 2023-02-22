using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using Story;
using UnityEngine;

namespace NitroxClient.GameLogic;

public static class StoryManager
{
    public static void ScanCompleted(NitroxId entityId, bool destroy)
    {
        PDAScanner.cachedProgress.Remove(entityId.ToString());
        if (NitroxEntity.TryGetObjectFrom(entityId, out GameObject scanObject))
        {
            // Copy the SendMessage from PDAScanner.Scan() but we don't care about the EntryData
            scanObject.SendMessage("OnScanned", null, SendMessageOptions.DontRequireReceiver);
            if (!destroy)
            {
                PDAScanner.fragments.Add(entityId.ToString(), 1f);
            }
            else
            {
                PDAScanner.fragments.Remove(entityId.ToString());
                GameObject.Destroy(scanObject);
            }
        }
    }

    public static void UpdateAuroraData(AuroraEventData auroraEventData)
    {
        CrashedShipExploder.main.timeToStartCountdown = auroraEventData.TimeToStartCountdown;
        CrashedShipExploder.main.timeToStartWarning = auroraEventData.TimeToStartWarning;
    }

    public static void RestoreAurora()
    {
        CleanAuroraEvents();
        // Same logic as in OnConsoleCommand_restoreship but without the time recalculation
        CrashedShipExploder.main.SwapModels(false);
        CrashedShipExploder.main.fxControl.StopAndDestroy(0, 0f);
        CrashedShipExploder.main.fxControl.StopAndDestroy(1, 0f);
    }

    /// <summary>
    /// Removes the aurora-related entries from StoryGoalManager's completedEvents and PDALog
    /// </summary>
    public static void CleanAuroraEvents()
    {
        foreach (string eventKey in AuroraEventData.GoalNames)
        {
            StoryGoalManager.main.completedGoals.Remove(eventKey);
            PDALog.Remove(eventKey);
        }
    }
}
