using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic;

/// <summary>
/// Attached to a RemotePlayer. Useful to determine that this script's GameObject is in the EntityRoot of a RemotePlayer.
/// </summary>
/// <remarks>
/// The EntityRoot of an object is defined as the top most GameObject as when an object hierarchy first was spawned in. Either from a prefab, or in Nitrox' case, a cloned root game object.
/// </remarks>
public class RemotePlayerIdentifier : MonoBehaviour, IObstacle
{
    public RemotePlayer RemotePlayer;

    public bool CanDeconstruct(out string reason)
    {
        reason = Language.main.Get("Nitrox_RemotePlayerObstacle");
        return false;
    }
}
