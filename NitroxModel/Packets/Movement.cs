using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.Packets;

[Serializable]
public abstract class Movement : Packet
{
    public abstract NitroxId Id { get; }
    public abstract NitroxVector3 Position { get; }
    public abstract NitroxQuaternion Rotation { get; }
}
