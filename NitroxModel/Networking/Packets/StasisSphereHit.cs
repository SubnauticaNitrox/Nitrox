using System;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record StasisSphereHit : Packet
{
    public ushort PlayerId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }
    public float ChargeNormalized { get; }
    public float Consumption { get; }

    public StasisSphereHit(ushort playerId, NitroxVector3 position, NitroxQuaternion rotation, float chargeNormalized, float consumption)
    {
        PlayerId = playerId;
        Position = position;
        Rotation = rotation;
        ChargeNormalized = chargeNormalized;
        Consumption = consumption;
    }
}
