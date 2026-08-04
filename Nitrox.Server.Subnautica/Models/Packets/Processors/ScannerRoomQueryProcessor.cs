using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Packets;
using Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.Packets.Core;

namespace Nitrox.Server.Subnautica.Models.Packets.Processors;

internal sealed class ScannerRoomQueryProcessor(
    ScannerRoomQueryService queryService,
    ScannerRoomQueryLimiter queryLimiter,
    ScannerRoomDiagnostics diagnostics,
    ILogger<ScannerRoomQueryProcessor> logger) : IAuthPacketProcessor<ScannerRoomQuery>
{
    private const int TARGETS_PER_PAGE = 256;
    private readonly ScannerRoomQueryService queryService = queryService;
    private readonly ScannerRoomQueryLimiter queryLimiter = queryLimiter;
    private readonly ScannerRoomDiagnostics diagnostics = diagnostics;
    private readonly ILogger<ScannerRoomQueryProcessor> logger = logger;

    public async Task Process(AuthProcessorContext context, ScannerRoomQuery packet)
    {
        if (!queryLimiter.TryEnter(context.Sender.SessionId, out IDisposable? lease))
        {
            diagnostics.QueryThrottled();
            await ReplyWithStatus(context, packet, ScannerRoomQueryStatus.Rejected, ScannerRoomQueryParameters.NormalizeRange(packet.ReportedRange));
            diagnostics.PagesSent(1);
            return;
        }

        using (lease)
        {
            ScannerRoomQueryResult result;
            try
            {
                result = await queryService.QueryAsync(
                    context.Sender,
                    packet.MapRoomId,
                    packet.ReportedRange,
                    packet.SelectedTechType,
                    packet.KnownRevision,
                    packet.ObservedOrigin);
            }
            catch (Exception ex)
            {
                logger.ZLogError(ex, $"Failed Scanner Room query {packet.RequestId} for room {packet.MapRoomId} from {context.Sender.Name}");
                await ReplyWithStatus(context, packet, ScannerRoomQueryStatus.Failed, ScannerRoomQueryParameters.NormalizeRange(packet.ReportedRange));
                diagnostics.PagesSent(1);
                return;
            }

            IReadOnlyList<ScannerRoomSnapshotPage> pages = CreatePages(packet, result);
            foreach (ScannerRoomSnapshotPage page in pages)
            {
                await context.ReplyAsync(page);
                diagnostics.PagesSent(1);
            }
        }
    }

    internal static IReadOnlyList<ScannerRoomSnapshotPage> CreatePages(ScannerRoomQuery packet, ScannerRoomQueryResult result)
    {
        if (result.Status != ScannerRoomQueryStatus.Complete)
        {
            return [CreatePage(packet, result, 0, 1, [], [])];
        }

        List<ScannerRoomSnapshotPage> pages = [];
        int pageCountValue = Math.Max(1, (result.Targets.Count + TARGETS_PER_PAGE - 1) / TARGETS_PER_PAGE);
        ushort pageCount = checked((ushort)pageCountValue);
        for (ushort pageIndex = 0; pageIndex < pageCount; pageIndex++)
        {
            List<ScannerResourceTarget> targets = result.Targets.Skip(pageIndex * TARGETS_PER_PAGE).Take(TARGETS_PER_PAGE).ToList();
            List<ScannerResourceSummary> summaries = pageIndex == 0 ? result.AvailableResources.ToList() : [];
            pages.Add(CreatePage(packet, result, pageIndex, pageCount, summaries, targets));
        }
        return pages;
    }

    private static ScannerRoomSnapshotPage CreatePage(
        ScannerRoomQuery packet,
        ScannerRoomQueryResult result,
        ushort pageIndex,
        ushort pageCount,
        List<ScannerResourceSummary> summaries,
        List<ScannerResourceTarget> targets) =>
        new(packet.MapRoomId, packet.RequestId, result.Status, result.EffectiveRange, result.SelectedTechType, result.Revision, pageIndex, pageCount, summaries, targets);

    private static Task ReplyWithStatus(AuthProcessorContext context, ScannerRoomQuery packet, ScannerRoomQueryStatus status, float effectiveRange) =>
        context.ReplyAsync(new ScannerRoomSnapshotPage(
            packet.MapRoomId,
            packet.RequestId,
            status,
            effectiveRange,
            ScannerRoomQueryParameters.NormalizeSelection(packet.SelectedTechType),
            0,
            0,
            1,
            [],
            []));
}
