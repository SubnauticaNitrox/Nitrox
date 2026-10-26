using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// Processes a client's request for a picture frame's bytes
/// </summary>
internal sealed class PictureFrameDataRequestProcessor(IOptions<SubnauticaServerOptions> options, EntityRegistry entityRegistry, PictureFrameStorageService storage, ILogger<PictureFrameDataRequestProcessor> logger) : IAuthPacketProcessor<PictureFrameDataRequest>
{
    public async Task Process(AuthProcessorContext context, PictureFrameDataRequest packet)
    {
        if (options.Value.PictureFrameSync == PictureFrameSyncMode.OFF)
        {
            return;
        }

        if (!storage.TryConsumeRequestToken(context.Sender.SessionId))
        {
            logger.ZLogWarning($"Player {context.Sender.Name} exceeded the picture frame request rate limit, dropping request for {packet.FrameId}");
            return;
        }

        if (!entityRegistry.TryGetEntityById(packet.FrameId, out Entity entity) || !context.Sender.CanSee(entity))
        {
            await context.ReplyAsync(new PictureFrameDataResponse(packet.FrameId, packet.ContentHash, [], false));
            return;
        }

        bool found = storage.TryGet(packet.ContentHash, out byte[]? jpegBytes);
        await context.ReplyAsync(new PictureFrameDataResponse(packet.FrameId, packet.ContentHash, found ? jpegBytes! : [], found));
    }
}
