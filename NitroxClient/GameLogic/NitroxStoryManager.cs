using System.Collections.Generic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using Story;
using UnityEngine;

namespace NitroxClient.GameLogic;

// TODO: Make it static if necessary
public class NitroxStoryManager
{
    public static void ScanCompleted(NitroxId entityId, bool destroy)
    {
        PDAScanner.cachedProgress.Remove($"{entityId}");
        if (NitroxEntity.TryGetObjectFrom(entityId, out GameObject scanObject))
        {
            // Copy the SendMessage from PDAScanner.Scan() but we don't care about the EntryData
            scanObject.SendMessage("OnScanned", null, SendMessageOptions.DontRequireReceiver);
            if (!destroy)
            {
                PDAScanner.fragments.Add($"{entityId}", 1f);
            }
            else
            {
                PDAScanner.fragments.Remove($"{entityId}");
                GameObject.Destroy(scanObject);
            }
        }
    }

    // TODO: Maybe move those in a common place with server-side StoryManager
    private static readonly List<string> auroraEvents = new() { "Story_AuroraWarning1", "Story_AuroraWarning2", "Story_AuroraWarning3", "Story_AuroraWarning4", "Story_AuroraExplosion" };

    public static void UpdateAuroraData(CrashedShipExploderData crashedShipExploderData)
    {
        CrashedShipExploder.main.timeToStartCountdown = crashedShipExploderData.TimeToStartCountdown;
        CrashedShipExploder.main.timeToStartWarning = crashedShipExploderData.TimeToStartWarning;
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
        foreach (string eventKey in auroraEvents)
        {
            StoryGoalManager.main.completedGoals.Remove(eventKey);
            PDALog.Remove(eventKey);
        }
    }
}
