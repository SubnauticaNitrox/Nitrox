using System;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

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
