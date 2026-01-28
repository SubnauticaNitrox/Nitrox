using System;
using Nitrox.Model.Core;

namespace Nitrox.Model.DataStructures;

/// <summary>
///     A simulated entity that is tracked by the Nitrox server so that it knows which connected game client owns (and simulates) the entity.
///     See <see cref="SimulationLockType"/> for more information.
/// </summary>
[Serializable]
public class SimulatedEntity
{
    /// <summary>
    ///     True if entity isn't static (e.g. welded to world).
    /// </summary>
    public bool ChangesPosition { get; }
    public NitroxId Id { get; }
    public SessionId SessionId { get; }
    public SimulationLockType LockType { get; }

    public SimulatedEntity(NitroxId id, SessionId sessionId, bool changesPosition, SimulationLockType lockType)
    {
        Id = id;
        SessionId = sessionId;
        ChangesPosition = changesPosition;
        LockType = lockType;
    }

    public override string ToString()
    {
        return $"[SimulatedEntity Id: {Id}, {nameof(SessionId)}: {SessionId}, ChangesPosition: {ChangesPosition}, LockType: {LockType}]";
    }
}
