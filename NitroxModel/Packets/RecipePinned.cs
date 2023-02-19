using System;

namespace NitroxModel.Packets;

[Serializable]
public class RecipePinned : Packet
{
    public int TechType { get; }
    public bool Pinned { get; }

    public RecipePinned(int techType, bool pinned)
    {
        TechType = techType;
        Pinned = pinned;
    }
}
