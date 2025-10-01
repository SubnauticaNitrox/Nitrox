using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class FabricatorStateChanged : Packet
{
    public NitroxId Id { get; }

    public bool IsCrafting { get; }

    public FabricatorStateChanged(NitroxId id, bool isCrafting)
    {
        Id = id;
        IsCrafting = isCrafting;
    }
}
