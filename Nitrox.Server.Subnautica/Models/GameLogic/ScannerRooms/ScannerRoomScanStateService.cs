using System.Collections.Concurrent;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Bases;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Server.Subnautica.Models.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.Resources.Parsers;

namespace Nitrox.Server.Subnautica.Models.GameLogic.ScannerRooms;

internal enum ScannerRoomScanStateChangeStatus
{
    Changed,
    Unchanged,
    Rejected,
    InvalidRoom
}

internal sealed record ScannerRoomScanStateChangeResult(
    ScannerRoomScanStateChangeStatus Status,
    ScannerRoomScanState State);

/// <summary>
/// Owns the persisted resource selection for every Scanner Room. A separate lock per room makes a state
/// replacement and its version increment one atomic operation while allowing independent rooms to change concurrently.
/// </summary>
internal sealed class ScannerRoomScanStateService(
    EntityRegistry entityRegistry,
    IScannerRoomResourceCatalog resourceCatalog,
    IOptions<SubnauticaServerOptions> options,
    ILogger<ScannerRoomScanStateService> logger)
{
    private readonly EntityRegistry entityRegistry = entityRegistry;
    private readonly IScannerRoomResourceCatalog resourceCatalog = resourceCatalog;
    private readonly IOptions<SubnauticaServerOptions> options = options;
    private readonly ILogger<ScannerRoomScanStateService> logger = logger;
    private readonly ConcurrentDictionary<NitroxId, Lock> mutationLocks = new();

    public ScannerRoomScanStateChangeResult Change(NitroxId mapRoomId, NitroxTechType? desiredTechType)
    {
        desiredTechType = ScannerRoomQueryParameters.NormalizeSelection(desiredTechType);
        if (!entityRegistry.TryGetEntityById(mapRoomId, out MapRoomEntity? _))
        {
            return new(ScannerRoomScanStateChangeStatus.InvalidRoom, ScannerRoomScanState.Empty);
        }

        Lock mutationLock = mutationLocks.GetOrAdd(mapRoomId, static _ => new Lock());
        lock (mutationLock)
        {
            if (!entityRegistry.TryGetEntityById(mapRoomId, out MapRoomEntity? mapRoom))
            {
                return new(ScannerRoomScanStateChangeStatus.InvalidRoom, ScannerRoomScanState.Empty);
            }

            ScannerRoomScanState current = mapRoom.ScanState ?? ScannerRoomScanState.Empty;
            if (!options.Value.EnableScannerRoomResourceSync ||
                desiredTechType != null && !resourceCatalog.IsKnownTechType(desiredTechType))
            {
                return new(ScannerRoomScanStateChangeStatus.Rejected, current);
            }

            if (Equals(current.SelectedTechType, desiredTechType))
            {
                return new(ScannerRoomScanStateChangeStatus.Unchanged, current);
            }

            if (current.Version == ulong.MaxValue)
            {
                logger.ZLogError($"Unable to change Scanner Room {mapRoomId} scan state because its version is exhausted.");
                return new(ScannerRoomScanStateChangeStatus.Rejected, current);
            }

            ScannerRoomScanState updated = new(desiredTechType, current.Version + 1);
            mapRoom.ScanState = updated;
            return new(ScannerRoomScanStateChangeStatus.Changed, updated);
        }
    }

    public bool TryGetState(NitroxId mapRoomId, out ScannerRoomScanState state)
    {
        if (!entityRegistry.TryGetEntityById(mapRoomId, out MapRoomEntity? _))
        {
            state = ScannerRoomScanState.Empty;
            return false;
        }

        Lock mutationLock = mutationLocks.GetOrAdd(mapRoomId, static _ => new Lock());
        lock (mutationLock)
        {
            if (entityRegistry.TryGetEntityById(mapRoomId, out MapRoomEntity? mapRoom))
            {
                state = mapRoom.ScanState ?? ScannerRoomScanState.Empty;
                return true;
            }

            state = ScannerRoomScanState.Empty;
            return false;
        }
    }

    public ScannerRoomScanState GetStateOrEmpty(NitroxId mapRoomId) =>
        TryGetState(mapRoomId, out ScannerRoomScanState state) ? state : ScannerRoomScanState.Empty;
}
