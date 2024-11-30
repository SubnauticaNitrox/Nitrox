using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public class TorpedoTargetAcquired : Packet
{
    public NitroxId BulletId { get; }
    public NitroxId TargetId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }

    public TorpedoTargetAcquired(NitroxId bulletId, NitroxId targetId, NitroxVector3 position, NitroxQuaternion rotation)
    {
        BulletId = bulletId;
        TargetId = targetId;
        Position = position;
        Rotation = rotation;
    }
}
