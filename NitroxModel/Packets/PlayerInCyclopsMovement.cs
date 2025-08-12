using System;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Networking;

namespace NitroxModel.Packets;

[Serializable]
public class PlayerInCyclopsMovement : Packet
{
    public ushort PlayerId { get; }
    public NitroxVector3 LocalPosition { get; }
    public NitroxQuaternion LocalRotation { get; }

    public PlayerInCyclopsMovement(ushort playerId, NitroxVector3 localPosition, NitroxQuaternion localRotation)
    {
        PlayerId = playerId;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }
}
