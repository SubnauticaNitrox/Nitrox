using System;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class StasisSphereShot : Packet
{
    public ushort PlayerId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }
    public float Speed { get; }
    public float LifeTime { get; }
    public float ChargeNormalized { get; }

    public StasisSphereShot(ushort playerId, NitroxVector3 position, NitroxQuaternion rotation, float speed, float lifeTime, float chargeNormalized)
    {
        PlayerId = playerId;
        Position = position;
        Rotation = rotation;
        Speed = speed;
        LifeTime = lifeTime;
        ChargeNormalized = chargeNormalized;
    }
}
