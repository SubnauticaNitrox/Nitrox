using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic;

/// <summary>
/// Attached to a RemotePlayer. Useful to determine that this script's GameObject is in the root of a RemotePlayer.
/// </summary>
public class RemotePlayerIdentifier : MonoBehaviour, IObstacle
{
    public RemotePlayer RemotePlayer;

    public bool CanDeconstruct(out string reason)
    {
        reason = Language.main.Get("RemotePlayerObstacle");
        return false;
    }
}
