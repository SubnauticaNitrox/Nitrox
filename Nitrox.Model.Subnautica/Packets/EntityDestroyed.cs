using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class EntityDestroyed : Packet
{
    public NitroxId Id { get; }

    public EntityDestroyed(NitroxId id)
    {
        Id = id;
    }
}
