using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class FireExtinguisherUse : Packet
{
    public NitroxId ItemId { get; }
    public bool Activated { get; }

    public FireExtinguisherUse(NitroxId itemId, bool activated)
    {
        ItemId = itemId;
        Activated = activated;
    }
}
