using System;

namespace NitroxModel.DataStructures
{
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
        public ushort PlayerId { get; }
        public SimulationLockType LockType { get; }

        public SimulatedEntity(NitroxId id, ushort playerId, bool changesPosition, SimulationLockType lockType)
        {
            Id = id;
            PlayerId = playerId;
            ChangesPosition = changesPosition;
            LockType = lockType;
        }

        public override string ToString()
        {
            return $"[SimulatedEntity Id: '{Id}' PlayerId: {PlayerId} IsEntity: {ChangesPosition} LockType: {LockType}]";
        }
    }
}
