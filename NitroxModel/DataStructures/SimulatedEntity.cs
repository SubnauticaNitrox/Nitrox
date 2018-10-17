using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SimulatedEntity
    {
        public bool ChangesPosition { get; }
        public string Guid { get; }
        public ulong LPlayerId { get; }
        public SimulationLockType LockType { get; }

        public SimulatedEntity(string guid, ulong playerId, bool changesPosition, SimulationLockType lockType)
        {
            Guid = guid;
            LPlayerId = playerId;
            ChangesPosition = changesPosition;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulatedEntity Guid: " + Guid + " PlayerId: " + LPlayerId + " IsEntity: " + ChangesPosition + " LockType: " + LockType + "]";
        }
    }
}
