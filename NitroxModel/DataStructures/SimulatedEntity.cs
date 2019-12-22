using System;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class SimulatedEntity
    {
        public bool ChangesPosition { get; }
        public NitroxId Id { get; }
        public NitroxId PlayerId { get; }
        public SimulationLockType LockType { get; }

        public SimulatedEntity(NitroxId id, NitroxId playerId, bool changesPosition, SimulationLockType lockType)
        {
            Id = id;
            PlayerId = playerId;
            ChangesPosition = changesPosition;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulatedEntity Id: " + Id + " PlayerId: " + PlayerId + " IsEntity: " + ChangesPosition + " LockType: " + LockType + "]";
        }
    }
}
