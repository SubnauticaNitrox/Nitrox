using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Packets;

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
