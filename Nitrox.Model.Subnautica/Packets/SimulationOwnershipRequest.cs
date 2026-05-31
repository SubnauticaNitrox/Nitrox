using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class SimulationOwnershipRequest : Packet
{
    public SessionId SessionId { get; }
    public NitroxId Id { get; }
    public SimulationLockType LockType { get; }

    public SimulationOwnershipRequest(SessionId sessionId, NitroxId id, SimulationLockType lockType)
    {
        SessionId = sessionId;
        Id = id;
        LockType = lockType;
    }
}
