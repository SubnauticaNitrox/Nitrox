using System;
using Nitrox.Model.Core;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class FootstepPacket : Packet
{
    public SessionId SessionId { get; }
    public StepSounds AssetIndex { get; }

    public FootstepPacket(SessionId sessionId, StepSounds assetIndex)
    {
        SessionId = sessionId;
        AssetIndex = assetIndex;

        DeliveryMethod = Networking.NitroxDeliveryMethod.DeliveryMethod.UNRELIABLE_SEQUENCED;
        UdpChannel = UdpChannelId.MOVEMENTS;
    }

    public enum StepSounds : byte
    {
        PRECURSOR,
        METAL,
        LAND
    }
}
