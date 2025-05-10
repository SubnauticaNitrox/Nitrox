using System;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Networking.Packets;

[Serializable]
public abstract record Movement : Packet
{
    public abstract ushort PlayerId { get; }
    public abstract NitroxVector3 Position { get; }
    public abstract NitroxVector3 Velocity { get; }
    public abstract NitroxQuaternion BodyRotation { get; }
    public abstract NitroxQuaternion AimingRotation { get; }
}
