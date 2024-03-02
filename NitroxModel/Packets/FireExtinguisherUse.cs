using NitroxModel.DataStructures;
using System;

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
