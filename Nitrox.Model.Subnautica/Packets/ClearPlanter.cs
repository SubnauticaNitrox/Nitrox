using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class ClearPlanter : Packet
{
    public NitroxId PlanterId { get; }

    public ClearPlanter(NitroxId planterId)
    {
        PlanterId = planterId;
    }
}
