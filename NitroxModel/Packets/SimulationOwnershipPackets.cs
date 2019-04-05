using System;
using System.Collections.Generic;
using System.Text;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : Packet
    {
        public ushort PlayerId { get; }
        public string Guid { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipRequest(ushort playerId, string guid, SimulationLockType lockType)
        {
            PlayerId = playerId;
            Guid = guid;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - PlayerId: " + PlayerId + " Guid: " + Guid + " PlayerId: " + PlayerId + " LockType: " + LockType + "]";
        }
    }

    [Serializable]
    public class SimulationOwnershipResponse : Packet
    {
        public string Guid { get; }
        public bool LockAquired { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipResponse(string guid, bool lockAquired, SimulationLockType lockType)
        {
            Guid = guid;
            LockAquired = lockAquired;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipResponse - Guid: " + Guid + " LockAquired: " + LockAquired + " LockType: " + LockType + "]";
        }
    }

    [Serializable]
    public class SimulationOwnershipChange : Packet
    {
        public List<SimulatedEntity> Entities { get; }

        public SimulationOwnershipChange(string guid, ushort owningPlayerId, SimulationLockType lockType)
        {
            Entities = new List<SimulatedEntity>
            {
                new SimulatedEntity(guid, owningPlayerId, false, lockType)
            };
        }

        public SimulationOwnershipChange(List<SimulatedEntity> entities)
        {
            Entities = entities;
        }

        public SimulationOwnershipChange(SimulatedEntity entity)
        {
            Entities = new List<SimulatedEntity>
            {
                entity
            };
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder("[SimulationOwnershipChange - ");

            foreach (SimulatedEntity entity in Entities)
            {
                stringBuilder.Append(entity.ToString());
            }

            stringBuilder.Append("]");

            return stringBuilder.ToString();
        }
    }

    [Serializable]
    public class SimulationOwnershipRelease : Packet
    {
        public ushort PlayerId { get; }
        public string Guid { get; }

        public SimulationOwnershipRelease(ushort playerId, string guid)
        {
            PlayerId = playerId;
            Guid = guid;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRelease - PlayerId: " + PlayerId + " Guid: " + Guid + " PlayerId: " + PlayerId + "]";
        }
    }
}
