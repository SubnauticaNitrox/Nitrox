using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public abstract class Movement : Packet
{
    public abstract ushort PlayerId { get; }
    public abstract NitroxVector3 Position { get; }
    public abstract NitroxVector3 Velocity { get; }
    public abstract NitroxQuaternion BodyRotation { get; }
    public abstract NitroxQuaternion AimingRotation { get; }
}
