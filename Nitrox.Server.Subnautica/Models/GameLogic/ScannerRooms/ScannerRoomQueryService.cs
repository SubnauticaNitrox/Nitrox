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
    IOptions<SubnauticaServerOptions> options,
    ILogger<ScannerRoomQueryService> logger)
{
    internal const float MINIMUM_RANGE = 300f;
    internal const float MAXIMUM_RANGE = 500f;
    internal const float RANGE_INCREMENT = 50f;
    private const float LEGACY_ORIGIN_REPAIR_DISTANCE = 25f;

    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly ScannerResourceIndex resourceIndex = resourceIndex;
    private readonly IScannerRoomResourceCatalog resourceCatalog = resourceCatalog;
    private readonly IScannerRoomBatchLoader batchLoader = batchLoader;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<ScannerRoomQueryService> logger = logger;
    private readonly Lock originRepairLock = new();

    public async Task<ScannerRoomQueryResult> QueryAsync(
        Player player,
        NitroxId mapRoomId,
        float reportedRange,
        NitroxTechType? selectedTechType,
        ulong knownRevision,
        NitroxVector3? observedOrigin,
        CancellationToken cancellationToken = default)
    {
        float effectiveRange = NormalizeRange(reportedRange);
        if (!options.Value.EnableScannerRoomResourceSync)
        {
            return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.Rejected, effectiveRange, selectedTechType);
        }
        if (!entityRegistry.TryGetEntityById(mapRoomId, out MapRoomEntity? mapRoom))
        {
            return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.InvalidRoom, effectiveRange, selectedTechType);
        }

        NitroxVector3? origin = ResolveOrigin(player, mapRoom, observedOrigin);
        if (origin == null)
        {
            return ScannerRoomQueryResult.Error(ScannerRoomQueryStatus.OriginUnavailable, effectiveRange, selectedTechType);
        }

        if (selectedTechType?.Equals(NitroxTechType.None) == true)
        {
            selectedTechType = null;
        }

        float loadRadius = effectiveRange + resourceCatalog.MaximumRelativeOffset;
        IReadOnlyList<NitroxInt3> batchIds = ScannerBatchCoverage.EnumerateIntersectingBatches(origin.Value, loadRadius);
        await batchLoader.LoadAsync(batchIds, cancellationToken);

        IReadOnlyList<ScannerResourceNode> nodes = resourceIndex.Query(batchIds, origin.Value, effectiveRange);
        List<ScannerResourceSummary> summaries = nodes.GroupBy(node => node.TechType)
                                                             .OrderBy(group => group.Key.Name, StringComparer.Ordinal)
                                                             .Select(group => new ScannerResourceSummary(group.Key, group.Count()))
                                                             .ToList();

        List<ScannerResourceTarget> targets = selectedTechType == null
            ? []
            : nodes.Where(node => node.TechType.Equals(selectedTechType))
                   .OrderBy(node => SquaredDistance(node.Position, origin.Value))
                   .ThenBy(node => node.Key.EntityId)
                   .ThenBy(node => node.Key.TrackerIndex)
                   .Select(node => new ScannerResourceTarget(node.Key.EntityId, node.Key.TrackerIndex, node.TechType, node.Position))
                   .ToList();

        ulong revision = ScannerRoomSnapshotRevision.Compute(effectiveRange, selectedTechType, summaries, targets);
        ScannerRoomQueryStatus status = knownRevision != 0 && knownRevision == revision
                                            ? ScannerRoomQueryStatus.NotModified
                                            : ScannerRoomQueryStatus.Complete;

        logger.ZLogDebug($"Scanner Room {mapRoomId} query for {player.Name} returned {summaries.Count:@ResourceTypes} resource types and {targets.Count:@Targets} selected targets across {batchIds.Count:@Batches} batches.");
        return status == ScannerRoomQueryStatus.NotModified
                   ? new(status, effectiveRange, selectedTechType, revision, [], [])
                   : new(status, effectiveRange, selectedTechType, revision, summaries, targets);
    }

    internal static float NormalizeRange(float reportedRange)
    {
        if (!float.IsFinite(reportedRange))
        {
            return MINIMUM_RANGE;
        }

        float clamped = Math.Clamp(reportedRange, MINIMUM_RANGE, MAXIMUM_RANGE);
        return MINIMUM_RANGE + MathF.Floor((clamped - MINIMUM_RANGE) / RANGE_INCREMENT) * RANGE_INCREMENT;
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
