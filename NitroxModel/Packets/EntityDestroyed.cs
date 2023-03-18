using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class EntityDestroyed : Packet
{
    public NitroxId Id { get; }

    public EntityDestroyed(NitroxId id)
    {
        Id = id;
    }
}
