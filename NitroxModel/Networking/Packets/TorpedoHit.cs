using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record TorpedoHit : Packet
{
    public NitroxId BulletId { get; }
    public NitroxVector3 Position { get; }
    public NitroxQuaternion Rotation { get; }

    public TorpedoHit(NitroxId bulletId, NitroxVector3 position, NitroxQuaternion rotation)
    {
        BulletId = bulletId;
        Position = position;
        Rotation = rotation;
    }
}
