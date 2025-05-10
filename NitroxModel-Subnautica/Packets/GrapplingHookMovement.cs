using System;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets;

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
