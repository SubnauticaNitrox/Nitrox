using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic.PictureFrames;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class PictureFrameDataResponseProcessor(PictureFrameCache pictureFrameCache) : IClientPacketProcessor<PictureFrameDataResponse>
{
    public Task Process(ClientProcessorContext context, PictureFrameDataResponse packet)
    {
        pictureFrameCache.OnResponseReceived(packet.ContentHash, packet.JpegBytes, packet.Found);
        return Task.CompletedTask;
    }
}
