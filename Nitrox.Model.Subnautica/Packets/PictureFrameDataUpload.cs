using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Networking;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PictureFrameDataUpload : Packet
{
    public NitroxId FrameId { get; }
    public string ContentHash { get; }
    public byte[] JpegBytes { get; }

    public PictureFrameDataUpload(NitroxId frameId, string contentHash, byte[] jpegBytes)
    {
        FrameId = frameId;
        ContentHash = contentHash;
        JpegBytes = jpegBytes;
        DeliveryMethod = NitroxDeliveryMethod.DeliveryMethod.RELIABLE_UNORDERED;
        UdpChannel = UdpChannelId.PICTURE_FRAMES;
    }
}
