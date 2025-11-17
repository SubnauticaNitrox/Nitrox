using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class GrapplingHookMovement : Packet
{
    public NitroxId ExosuitId { get; }
    public Exosuit.Arm ArmSide { get; }
    public NitroxVector3 Position { get; }
    public NitroxVector3 Velocity { get; }
    public NitroxQuaternion Rotation { get; }

    public GrapplingHookMovement(NitroxId exosuitId, Exosuit.Arm armSide, NitroxVector3 position, NitroxVector3 velocity, NitroxQuaternion rotation)
    {
        ExosuitId = exosuitId;
        ArmSide = armSide;
        Position = position;
        Velocity = velocity;
        Rotation = rotation;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }
}
