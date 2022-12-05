using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets;

[Serializable]
public class VehicleDestroyed : Packet
{
    public NitroxId Id { get; }

    public VehicleDestroyed(NitroxId id)
    {
        Id = id;
    }
}
