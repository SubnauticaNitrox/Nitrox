using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class TorpedoShot : Packet
{
    public NitroxId BulletId { get; }
    public NitroxTechType TechType { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }
    public float Speed { get; }
    public float LifeTime { get; }

    public TorpedoShot(NitroxId bulletId, NitroxTechType techType, NitroxVector3 position, NitroxQuaternion rotation, float speed, float lifeTime)
    {
        BulletId = bulletId;
        TechType = techType;
        Position = position;
        Rotation = rotation;
        Speed = speed;
        LifeTime = lifeTime;
    }
}
