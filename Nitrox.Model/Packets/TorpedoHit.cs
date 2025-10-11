using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;

namespace Nitrox.Model.Packets;

[Serializable]
public class TorpedoHit : Packet
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
