using System;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record RecipePinned : Packet
{
    public int TechType { get; }
    public bool Pinned { get; }

    public RecipePinned(int techType, bool pinned)
    {
        TechType = techType;
        Pinned = pinned;
    }
}
