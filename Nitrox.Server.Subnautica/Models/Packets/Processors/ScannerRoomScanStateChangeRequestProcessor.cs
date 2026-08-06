using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ScannerRoomScanStateChangeRequestProcessor(
    ScannerRoomScanStateService scanStateService,
    ILogger<ScannerRoomScanStateChangeRequestProcessor> logger) : IAuthPacketProcessor<ScannerRoomScanStateChangeRequest>
{
    private readonly ScannerRoomScanStateService scanStateService = scanStateService;
    private readonly ILogger<ScannerRoomScanStateChangeRequestProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ScannerRoomScanStateChangeRequest packet)
    {
        ScannerRoomScanStateChangeResult result = scanStateService.Change(packet.MapRoomId, packet.DesiredTechType);
        ScannerRoomScanStateChanged changedPacket = new(packet.MapRoomId, result.State);

        if (result.Status == ScannerRoomScanStateChangeStatus.Changed)
        {
            await context.SendToAllAsync(changedPacket);
            return;
        }

        // The sender may already have applied the requested transition optimistically. Always return the unchanged
        // canonical value for duplicate and rejected requests so it can reconcile without affecting other clients.
        await context.ReplyAsync(changedPacket);
        if (result.Status is ScannerRoomScanStateChangeStatus.Rejected or ScannerRoomScanStateChangeStatus.InvalidRoom)
        {
            logger.ZLogWarning($"Rejected Scanner Room scan-state request for room {packet.MapRoomId} from {context.Sender.Name}: {result.Status}.");
        }
    }
}
