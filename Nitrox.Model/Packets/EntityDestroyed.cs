using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class EntityDestroyed : Packet
{
    public NitroxId Id { get; }

    public EntityDestroyed(NitroxId id)
    {
        Id = id;
    }
}
