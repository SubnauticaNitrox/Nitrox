using System;
using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;

namespace NitroxClient.GameLogic.ScannerRooms;

internal readonly record struct ScannerRoomQueryTicket(uint RequestId, ulong KnownRevision);

internal enum ScannerRoomSnapshotApplyResult
{
    Ignored,
    WaitingForPages,
    Applied,
    NotModified,
    Failed
}

internal sealed record ScannerRoomSnapshot(
    NitroxId MapRoomId,
    float EffectiveRange,
    NitroxTechType? SelectedTechType,
    ulong Revision,
    IReadOnlyList<ScannerResourceSummary> AvailableResources,
    IReadOnlyList<ScannerResourceTarget> Targets);

internal sealed record ScannerRoomSnapshotPageData(
    NitroxId MapRoomId,
    uint RequestId,
    ScannerRoomQueryStatus Status,
    float EffectiveRange,
    NitroxTechType? SelectedTechType,
    ulong Revision,
    ushort PageIndex,
    ushort PageCount,
    IReadOnlyList<ScannerResourceSummary> AvailableResources,
    IReadOnlyList<ScannerResourceTarget> Targets);

internal sealed class ScannerRoomSnapshotStore
{
    private readonly LockObject storeLock = new();
    private readonly Dictionary<NitroxId, RoomState> rooms = [];

    public ScannerRoomQueryTicket BeginQuery(NitroxId mapRoomId, float range, NitroxTechType? selectedTechType)
    {
        lock (storeLock)
        {
            RoomState room = GetOrCreateRoom(mapRoomId);
            room.NextRequestId++;
            if (room.NextRequestId == 0)
            {
                room.NextRequestId = 1;
            }

            ulong knownRevision = room.Snapshot is { } snapshot &&
                                  snapshot.EffectiveRange.Equals(range) &&
                                  TechTypesEqual(snapshot.SelectedTechType, selectedTechType)
                                      ? snapshot.Revision
                                      : 0;

            room.Pending = new PendingSnapshot(room.NextRequestId, range, selectedTechType);
            return new ScannerRoomQueryTicket(room.NextRequestId, knownRevision);
        }
    }

    public ScannerRoomSnapshotApplyResult AcceptPage(ScannerRoomSnapshotPageData page)
    {
        lock (storeLock)
        {
            if (!rooms.TryGetValue(page.MapRoomId, out RoomState? room) || room.Pending is not { } pending || pending.RequestId != page.RequestId)
            {
                return ScannerRoomSnapshotApplyResult.Ignored;
            }
            if (!pending.Matches(page))
            {
                return ScannerRoomSnapshotApplyResult.Ignored;
            }

            if (page.Status == ScannerRoomQueryStatus.NotModified)
            {
                room.Pending = null;
                return room.Snapshot?.Revision == page.Revision
                           ? ScannerRoomSnapshotApplyResult.NotModified
                           : ScannerRoomSnapshotApplyResult.Ignored;
            }
            if (page.Status != ScannerRoomQueryStatus.Complete || page.PageCount == 0 || page.PageIndex >= page.PageCount)
            {
                room.Pending = null;
                return ScannerRoomSnapshotApplyResult.Failed;
            }

            if (!pending.TryAdd(page))
            {
                return ScannerRoomSnapshotApplyResult.Ignored;
            }
            if (!pending.IsComplete)
            {
                return ScannerRoomSnapshotApplyResult.WaitingForPages;
            }

            room.Snapshot = pending.BuildSnapshot(page.MapRoomId);
            room.Pending = null;
            return ScannerRoomSnapshotApplyResult.Applied;
        }
    }

    public bool TryGetSnapshot(NitroxId mapRoomId, out ScannerRoomSnapshot? snapshot)
    {
        lock (storeLock)
        {
            snapshot = rooms.TryGetValue(mapRoomId, out RoomState? room) ? room.Snapshot : null;
            return snapshot != null;
        }
    }

    public void RemoveRoom(NitroxId mapRoomId)
    {
        lock (storeLock)
        {
            rooms.Remove(mapRoomId);
        }
    }

    public void Clear()
    {
        lock (storeLock)
        {
            rooms.Clear();
        }
    }

    private RoomState GetOrCreateRoom(NitroxId mapRoomId)
    {
        if (!rooms.TryGetValue(mapRoomId, out RoomState? room))
        {
            room = rooms[mapRoomId] = new RoomState();
        }
        return room;
    }

    private static bool TechTypesEqual(NitroxTechType? left, NitroxTechType? right) =>
        ReferenceEquals(left, right) || left?.Equals(right) == true;

    private sealed class RoomState
    {
        public uint NextRequestId;
        public PendingSnapshot? Pending;
        public ScannerRoomSnapshot? Snapshot;
    }

    private sealed class PendingSnapshot(uint requestId, float requestedRange, NitroxTechType? requestedTechType)
    {
        private readonly Dictionary<ushort, ScannerRoomSnapshotPageData> pages = [];
        private ushort? pageCount;
        private ulong? revision;

        public uint RequestId { get; } = requestId;

        public bool IsComplete => pageCount is { } count && pages.Count == count;

        public bool Matches(ScannerRoomSnapshotPageData page) =>
            page.EffectiveRange.Equals(requestedRange) && TechTypesEqual(page.SelectedTechType, requestedTechType);

        public bool TryAdd(ScannerRoomSnapshotPageData page)
        {
            if (pageCount is { } expectedPageCount && expectedPageCount != page.PageCount ||
                revision is { } expectedRevision && expectedRevision != page.Revision)
            {
                return false;
            }
            if (pages.ContainsKey(page.PageIndex))
            {
                return false;
            }

            pageCount = page.PageCount;
            revision = page.Revision;
            pages.Add(page.PageIndex, page);
            return true;
        }

        public ScannerRoomSnapshot BuildSnapshot(NitroxId mapRoomId)
        {
            List<ScannerResourceSummary> summaries = pages.OrderBy(entry => entry.Key)
                                                             .SelectMany(entry => entry.Value.AvailableResources)
                                                             .GroupBy(summary => summary.TechType)
                                                             .Select(group => group.First())
                                                             .ToList();
            List<ScannerResourceTarget> targets = pages.OrderBy(entry => entry.Key)
                                                       .SelectMany(entry => entry.Value.Targets)
                                                       .GroupBy(target => (target.EntityId, target.TrackerIndex))
                                                       .Select(group => group.First())
                                                       .ToList();

            return new ScannerRoomSnapshot(mapRoomId, requestedRange, requestedTechType, revision!.Value, summaries, targets);
        }
    }
}
