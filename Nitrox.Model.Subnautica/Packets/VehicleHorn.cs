using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public sealed class VehicleHorn : Packet
{
    public const float COOLDOWN_SECONDS = 1.5f;
    public const float MAX_AUDIBLE_DISTANCE = 500f;

    public NitroxId VehicleId { get; }

    public VehicleHorn(NitroxId vehicleId)
    {
        VehicleId = vehicleId;
    }

    public override string ToString()
    {
        return $"[{nameof(VehicleHorn)} - {nameof(VehicleId)}: {VehicleId}]";
    }
}
