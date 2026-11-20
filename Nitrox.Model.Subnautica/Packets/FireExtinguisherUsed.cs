using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class FireExtinguisherUsed : Packet
{
    public NitroxId Id { get; }
    public bool IsActive { get; }

    public FireExtinguisherUsed(NitroxId id, bool isActive)
    {
        Id = id;
        IsActive = isActive;
    }
}
