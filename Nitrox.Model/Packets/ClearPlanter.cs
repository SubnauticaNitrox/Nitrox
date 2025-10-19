using System;
using Nitrox.Model.DataStructures;

namespace Nitrox.Model.Packets;

[Serializable]
public class ClearPlanter : Packet
{
    public NitroxId PlanterId { get; }

    public ClearPlanter(NitroxId planterId)
    {
        PlanterId = planterId;
    }
}
