using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Configuration;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal sealed class ScannerRoomQueryService(
    EntityRegistry entityRegistry,
    ScannerResourceIndex resourceIndex,
    IScannerRoomResourceCatalog resourceCatalog,
    IScannerRoomBatchLoader batchLoader,
    ScannerRoomScanStateService scanStateService,
    ScannerRoomDiagnostics diagnostics,
    IOptions<SubnauticaServerOptions> options,
    ILogger<ScannerRoomQueryService> logger)
{
    private const float LEGACY_ORIGIN_REPAIR_DISTANCE = 25f;

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ScannerResourceIndex resourceIndex = resourceIndex;
    private readonly IScannerRoomResourceCatalog resourceCatalog = resourceCatalog;
    private readonly IScannerRoomBatchLoader batchLoader = batchLoader;
    private readonly ScannerRoomScanStateService scanStateService = scanStateService;
    private readonly ScannerRoomDiagnostics diagnostics = diagnostics;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<ScannerRoomQueryService> logger = logger;
    private readonly Lock originRepairLock = new();

    public async Task<ScannerRoomQueryResult> QueryAsync(
        Player player,
        NitroxId mapRoomId,
        float reportedRange,
        ulong expectedScanStateVersion,
        ulong knownRevision,
        NitroxVector3? observedOrigin,
        CancellationToken cancellationToken = default)
    {
        long queryStartedAt = diagnostics.QueryStarted();
        ScannerRoomQueryStatus finalStatus = ScannerRoomQueryStatus.Failed;
        try
        {
            float effectiveRange = ScannerRoomQueryParameters.NormalizeRange(reportedRange);
            if (!options.Value.EnableScannerRoomResourceSync)
            {
                finalStatus = ScannerRoomQueryStatus.Rejected;
                return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.Rejected, effectiveRange, scanStateService.GetStateOrEmpty(mapRoomId));
            }
            if (!entityRegistry.TryGetEntityById(mapRoomId, out MapRoomEntity? mapRoom))
            {
                finalStatus = ScannerRoomQueryStatus.InvalidRoom;
                return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.InvalidRoom, effectiveRange, ScannerRoomScanState.Empty);
            }

            if (!scanStateService.TryGetState(mapRoomId, out ScannerRoomScanState scanState) || scanState.Version != expectedScanStateVersion)
            {
                finalStatus = ScannerRoomQueryStatus.StateOutdated;
                return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.StateOutdated, effectiveRange, scanState);
            }

            NitroxVector3? origin = ResolveOrigin(player, mapRoom, observedOrigin);
            if (origin == null)
            {
                finalStatus = ScannerRoomQueryStatus.OriginUnavailable;
                return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.OriginUnavailable, effectiveRange, scanState);
            }

            float loadRadius = effectiveRange + resourceCatalog.MaximumRelativeOffset;
            IReadOnlyList<NitroxInt3> batchIds = ScannerBatchCoverage.EnumerateIntersectingBatches(origin.Value, loadRadius);
            diagnostics.BatchLoadStarted(batchIds.Count);
            try
            {
                await batchLoader.LoadAsync(batchIds, cancellationToken);
            }
            finally
            {
                diagnostics.BatchLoadCompleted();
            }

            if (!scanStateService.TryGetState(mapRoomId, out ScannerRoomScanState latestScanState) ||
                latestScanState.Version != scanState.Version ||
                !Equals(latestScanState.SelectedTechType, scanState.SelectedTechType))
            {
                finalStatus = ScannerRoomQueryStatus.StateOutdated;
                return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.StateOutdated, effectiveRange, latestScanState);
            }

            IReadOnlyList<ScannerResourceNode> nodes = resourceIndex.Query(batchIds, origin.Value, effectiveRange);
            List<ScannerResourceSummary> summaries = nodes.GroupBy(node => node.TechType)
                                                                 .OrderBy(group => group.Key.Name, StringComparer.Ordinal)
                                                                 .Select(group => new ScannerResourceSummary(group.Key, group.Count()))
                                                                 .ToList();

            List<ScannerResourceTarget> targets = scanState.SelectedTechType == null
                ? []
                : nodes.Where(node => node.TechType.Equals(scanState.SelectedTechType))
                       .OrderBy(node => SquaredDistance(node.Position, origin.Value))
                       .ThenBy(node => node.Key.EntityId)
                       .ThenBy(node => node.Key.TrackerIndex)
                       .Select(node => new ScannerResourceTarget(node.Key.EntityId, node.Key.TrackerIndex, node.TechType, node.Position))
                       .ToList();
            diagnostics.ResultMatched(summaries.Count, targets.Count);

            ulong revision = ScannerRoomSnapshotRevision.Compute(effectiveRange, scanState.SelectedTechType, summaries, targets);
            ScannerRoomQueryStatus status = knownRevision != 0 && knownRevision == revision
                                                ? ScannerRoomQueryStatus.NotModified
                                                : ScannerRoomQueryStatus.Complete;
            finalStatus = status;

            logger.ZLogDebug($"Scanner Room {mapRoomId} query for {player.Name} returned {summaries.Count:@ResourceTypes} resource types and {targets.Count:@Targets} selected targets across {batchIds.Count:@Batches} batches.");
            return status == ScannerRoomQueryStatus.NotModified
                       ? new(status, effectiveRange, scanState, revision, [], [])
                       : new(status, effectiveRange, scanState, revision, summaries, targets);
        }
        finally
        {
            diagnostics.QueryCompleted(queryStartedAt, finalStatus);
        }
    }

    private NitroxVector3? ResolveOrigin(Player player, MapRoomEntity mapRoom, NitroxVector3? observedOrigin)
    {
        lock (originRepairLock)
        {
            if (mapRoom.ScanOrigin != null)
            {
                return mapRoom.ScanOrigin;
            }

            if (observedOrigin is not { } candidate || !IsFinite(candidate) || NitroxVector3.Distance(player.Position, candidate) > LEGACY_ORIGIN_REPAIR_DISTANCE)
            {
                return null;
            }

            mapRoom.ScanOrigin = candidate;
            logger.ZLogInformation($"Repaired missing scan origin for legacy Scanner Room {mapRoom.Id} from player {player.Name}.");
            return candidate;
        }
    }

    private static bool IsFinite(NitroxVector3 value) => float.IsFinite(value.X) && float.IsFinite(value.Y) && float.IsFinite(value.Z);

    private static float SquaredDistance(NitroxVector3 left, NitroxVector3 right)
    {
        NitroxVector3 delta = left - right;
        return delta.X * delta.X + delta.Y * delta.Y + delta.Z * delta.Z;
    }
}
