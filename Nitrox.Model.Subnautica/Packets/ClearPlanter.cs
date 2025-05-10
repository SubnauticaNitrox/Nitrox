using System;
using NitroxModel.DataStructures;
using NitroxModel.Networking.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public record ClearPlanter : Packet
{
    public NitroxId PlanterId { get; }

    public ClearPlanter(NitroxId planterId)
    {
        PlanterId = planterId;
    }
}
