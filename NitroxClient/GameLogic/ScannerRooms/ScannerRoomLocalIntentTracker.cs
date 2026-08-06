using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace NitroxClient.GameLogic.ScannerRooms;

/// <summary>
/// De-duplicates nested vanilla scanner callbacks while keeping cancellation (a null selection) distinct from
/// having no pending intent.
/// </summary>
internal sealed class ScannerRoomLocalIntentTracker
{
    private bool hasPendingIntent;
    private string? pendingSelection;

    public bool TryBegin(NitroxTechType? desiredTechType)
    {
        string? desiredSelection = desiredTechType?.Name;
        if (hasPendingIntent && pendingSelection == desiredSelection)
        {
            return false;
        }

        hasPendingIntent = true;
        pendingSelection = desiredSelection;
        return true;
    }

    public void Clear()
    {
        hasPendingIntent = false;
        pendingSelection = null;
    }
}
