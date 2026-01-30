using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class StasisSphereHit : Packet
{
    public SessionId SessionId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }
    public float ChargeNormalized { get; }
    public float Consumption { get; }

    public StasisSphereHit(SessionId sessionId, NitroxVector3 position, NitroxQuaternion rotation, float chargeNormalized, float consumption)
    {
        SessionId = sessionId;
        Position = position;
        Rotation = rotation;
        ChargeNormalized = chargeNormalized;
        Consumption = consumption;
    }
}
