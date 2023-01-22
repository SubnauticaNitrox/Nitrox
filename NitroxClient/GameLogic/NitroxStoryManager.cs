using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using Story;

namespace NitroxClient.GameLogic;

// TODO: Make it static if necessary
public class NitroxStoryManager
{
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
