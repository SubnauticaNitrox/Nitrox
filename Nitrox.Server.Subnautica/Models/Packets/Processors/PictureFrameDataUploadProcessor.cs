using Nitrox.Model.Server;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.GameLogic.PictureFrames;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

/// <summary>
/// a client will send their picture to the server after it's gone through the encoding process
/// server verifies and broadcasts the uploaded picture frame's bytes
/// </summary>
internal sealed class PictureFrameDataUploadProcessor(IOptions<SubnauticaServerOptions> options, EntityRegistry entityRegistry, PictureFrameStorageService storage, ILogger<PictureFrameDataUploadProcessor> logger) : IAuthPacketProcessor<PictureFrameDataUpload>
{
    public Task Process(AuthProcessorContext context, PictureFrameDataUpload packet)
    {
        if (options.Value.PictureFrameSync == PictureFrameSyncMode.OFF)
        {
            return Task.CompletedTask;
        }

        if (!storage.TryConsumeUploadToken(context.Sender.SessionId))
        {
            logger.ZLogWarning($"Player {context.Sender.Name} exceeded the picture frame upload rate limit, dropping upload for {packet.FrameId}");
            return Task.CompletedTask;
        }

        if (packet.JpegBytes.Length == 0 || packet.JpegBytes.Length > options.Value.PictureFrameMaxBytes)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} sent an oversized picture frame upload ({packet.JpegBytes.Length} bytes) for {packet.FrameId}, dropping");
            return Task.CompletedTask;
        }

        if (PictureFrameStorageService.ComputeHash(packet.JpegBytes) != packet.ContentHash)
        {
            logger.ZLogWarning($"Player {context.Sender.Name} sent a picture frame upload for {packet.FrameId} whose bytes don't match the claimed content hash, dropping");
            return Task.CompletedTask;
        }

        if (!entityRegistry.TryGetEntityById(packet.FrameId, out Entity entity) || !context.Sender.CanSee(entity))
        {
            logger.ZLogWarning($"Player {context.Sender.Name} tried uploading a picture for an entity they can't see: {packet.FrameId}");
            return Task.CompletedTask;
        }

        storage.Store(packet.ContentHash, packet.JpegBytes);
        return Task.CompletedTask;
    }
}
