using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Networking;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerMovement : Movement
{
    public PlayerMovement(SessionId sessionId, NitroxVector3 position, NitroxVector3 velocity, NitroxQuaternion bodyRotation, NitroxQuaternion aimingRotation)
    {
        SessionId = sessionId;
        Position = position;
        Velocity = velocity;
        BodyRotation = bodyRotation;
        AimingRotation = aimingRotation;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }

    public override SessionId SessionId { get; }
    public override NitroxVector3 Position { get; }
    public override NitroxVector3 Velocity { get; }
    public override NitroxQuaternion BodyRotation { get; }
    public override NitroxQuaternion AimingRotation { get; }
}
