using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record EntityDestroyed : Packet
{
    public NitroxId Id { get; }

    public EntityDestroyed(NitroxId id)
    {
        Id = id;
    }
}
