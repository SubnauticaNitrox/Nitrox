using System;
using Nitrox.Model.Core;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PlayerInCyclopsMovement : Packet
{
    public SessionId SessionId { get; }
    public NitroxVector3 LocalPosition { get; }
    public NitroxQuaternion LocalRotation { get; }

    public PlayerInCyclopsMovement(SessionId sessionId, NitroxVector3 localPosition, NitroxQuaternion localRotation)
    {
        SessionId = sessionId;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }
}
