using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class StasisSphereShot : Packet
{
    public SessionId SessionId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }
    public float Speed { get; }
    public float LifeTime { get; }
    public float ChargeNormalized { get; }

    public StasisSphereShot(SessionId sessionId, NitroxVector3 position, NitroxQuaternion rotation, float speed, float lifeTime, float chargeNormalized)
    {
        SessionId = sessionId;
        Position = position;
        Rotation = rotation;
        Speed = speed;
        LifeTime = lifeTime;
        ChargeNormalized = chargeNormalized;
    }
}
