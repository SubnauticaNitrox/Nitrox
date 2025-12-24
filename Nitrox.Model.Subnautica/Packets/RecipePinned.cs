using System;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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
