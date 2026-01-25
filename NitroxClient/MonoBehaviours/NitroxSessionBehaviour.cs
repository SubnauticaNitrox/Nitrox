using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Base class for Monos that exist only during a multiplayer session
/// This auto-handles cleanup
/// </summary>
public abstract class NitroxSessionBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        Multiplayer.OnAfterMultiplayerEnd += OnSessionEnd;
    }

    protected virtual void OnDestroy()
    {
        Multiplayer.OnAfterMultiplayerEnd -= OnSessionEnd;
    }

    protected virtual void OnSessionEnd(){ }
}
