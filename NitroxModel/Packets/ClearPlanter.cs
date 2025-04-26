using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class ClearPlanter : Packet
{
    public NitroxId PlanterId { get; }

    public ClearPlanter(NitroxId planterId)
    {
        PlanterId = planterId;
    }
}
