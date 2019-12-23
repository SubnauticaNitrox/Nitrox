﻿using NitroxModel.DataStructures;
using System;

namespace NitroxModel.Packets
{
    [Serializable]
    public class SimulationOwnershipRequest : Packet
    {
        public ushort PlayerId { get; }
        public NitroxId Id { get; }
        public SimulationLockType LockType { get; }

        public SimulationOwnershipRequest(ushort playerId, NitroxId id, SimulationLockType lockType)
        {
            PlayerId = playerId;
            Id = id;
            LockType = lockType;
        }

        public override string ToString()
        {
            return "[SimulationOwnershipRequest - PlayerId: " + PlayerId + " Id: " + Id + " PlayerId: " + PlayerId + " LockType: " + LockType + "]";
        }
    }
}
