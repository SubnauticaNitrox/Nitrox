using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PictureFrameDataResponse : Packet
{
    public NitroxId FrameId { get; }
    public string ContentHash { get; }
    public byte[] JpegBytes { get; }
    public bool Found { get; }

    public PictureFrameDataResponse(NitroxId frameId, string contentHash, byte[] jpegBytes, bool found)
    {
        FrameId = frameId;
        ContentHash = contentHash;
        JpegBytes = jpegBytes;
        Found = found;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_UNORDERED;
        UdpChannel = UdpChannelId.PICTURE_FRAMES;
    }
}
