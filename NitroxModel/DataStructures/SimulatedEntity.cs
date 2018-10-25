using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SimulatedEntity
    {
        public bool ChangesPosition { get; }
        public string Guid { get; }
        public ulong PlayerId { get; }
        public SimulationLockType LockType { get; }

        public SimulatedEntity(string guid, ulong playerId, bool changesPosition, SimulationLockType lockType)
        {
            Guid = guid;
            PlayerId = playerId;
            ChangesPosition = changesPosition;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulatedEntity Guid: " + Guid + " PlayerId: " + PlayerId + " IsEntity: " + ChangesPosition + " LockType: " + LockType + "]";
        }
    }
}
