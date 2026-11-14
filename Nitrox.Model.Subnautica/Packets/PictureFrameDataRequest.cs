using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

/// <summary>
/// Sent by a client the first time it needs to render a specific picture frame's content and has the cache doesn't hit
/// </summary>
[Serializable]
public class PictureFrameDataRequest : Packet
{
    public NitroxId FrameId { get; }
    public string ContentHash { get; }

    public PictureFrameDataRequest(NitroxId frameId, string contentHash)
    {
        FrameId = frameId;
        ContentHash = contentHash;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_UNORDERED;
        UdpChannel = UdpChannelId.PICTURE_FRAMES;
    }
}
