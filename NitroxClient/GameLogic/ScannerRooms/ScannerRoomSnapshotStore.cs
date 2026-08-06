using System;
using System.Collections;
using System.Collections.Generic;
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
    ScannerRoomScanState ScanState,
    ulong Revision,
    IReadOnlyList<ScannerResourceSummary> AvailableResources,
    IReadOnlyList<ScannerResourceTarget> Targets)
{
    public NitroxTechType? SelectedTechType => ScanState.SelectedTechType;
}

internal sealed record ScannerRoomSnapshotPageData(
    NitroxId MapRoomId,
    uint RequestId,
    ScannerRoomQueryStatus Status,
    float EffectiveRange,
    ScannerRoomScanState ScanState,
    ulong Revision,
    ushort PageIndex,
    ushort PageCount,
    IReadOnlyList<ScannerResourceSummary> AvailableResources,
    IReadOnlyList<ScannerResourceTarget> Targets);

internal sealed class ScannerRoomSnapshotStore
{
    private readonly LockObject storeLock = new();
    private readonly Dictionary<NitroxId, RoomState> rooms = [];

    public ScannerRoomQueryTicket BeginQuery(NitroxId mapRoomId, float range, ScannerRoomScanState expectedScanState)
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
                                  ScanStatesEqual(snapshot.ScanState, expectedScanState)
                                      ? snapshot.Revision
                                      : 0;

            room.Pending = new PendingSnapshot(room.NextRequestId, range, expectedScanState);
            return new ScannerRoomQueryTicket(room.NextRequestId, knownRevision);
        }
    }

    public ScannerRoomSnapshotApplyResult AcceptPage(ScannerRoomSnapshotPageData page) => AcceptPage(page, out _);

    public ScannerRoomSnapshotApplyResult AcceptPage(ScannerRoomSnapshotPageData page, out ScannerRoomQueryStatus? acceptedStatus)
    {
        lock (storeLock)
        {
            acceptedStatus = null;
            if (!rooms.TryGetValue(page.MapRoomId, out RoomState? room) || room.Pending is not { } pending || pending.RequestId != page.RequestId)
            {
                return ScannerRoomSnapshotApplyResult.Ignored;
            }
            if (!pending.Matches(page))
            {
                return ScannerRoomSnapshotApplyResult.Ignored;
            }

            if (page.Status != ScannerRoomQueryStatus.Complete)
            {
                room.Pending = null;
                if (!IsValidStatusPage(page))
                {
                    return ScannerRoomSnapshotApplyResult.Failed;
                }

                acceptedStatus = page.Status;
                if (page.Status == ScannerRoomQueryStatus.NotModified)
                {
                    return room.Snapshot is { } snapshot &&
                           snapshot.Revision == page.Revision &&
                           snapshot.EffectiveRange.Equals(page.EffectiveRange) &&
                           ScanStatesEqual(snapshot.ScanState, page.ScanState)
                               ? ScannerRoomSnapshotApplyResult.NotModified
                               : ScannerRoomSnapshotApplyResult.Failed;
                }
                return ScannerRoomSnapshotApplyResult.Failed;
            }
            if (page.PageCount == 0 || page.PageIndex >= page.PageCount)
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
            acceptedStatus = ScannerRoomQueryStatus.Complete;
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

    public bool CancelQuery(NitroxId mapRoomId, uint requestId)
    {
        lock (storeLock)
        {
            if (!rooms.TryGetValue(mapRoomId, out RoomState? room) || room.Pending?.RequestId != requestId)
            {
                return false;
            }

            room.Pending = null;
            return true;
        }
    }

    public bool CancelPendingQuery(NitroxId mapRoomId)
    {
        lock (storeLock)
        {
            if (!rooms.TryGetValue(mapRoomId, out RoomState? room) || room.Pending == null)
            {
                return false;
            }

            room.Pending = null;
            return true;
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

    private static bool IsValidStatusPage(ScannerRoomSnapshotPageData page) =>
        page.PageIndex == 0 &&
        page.PageCount == 1 &&
        page.AvailableResources.Count == 0 &&
        page.Targets.Count == 0;

    private static bool ScanStatesEqual(ScannerRoomScanState left, ScannerRoomScanState right) =>
        left.Version == right.Version &&
        (ReferenceEquals(left.SelectedTechType, right.SelectedTechType) ||
         left.SelectedTechType?.Equals(right.SelectedTechType) == true ||
         left.SelectedTechType == null && right.SelectedTechType == null);

    private sealed class RoomState
    {
        public uint NextRequestId;
        public PendingSnapshot? Pending;
        public ScannerRoomSnapshot? Snapshot;
    }

    private sealed class PendingSnapshot(uint requestId, float requestedRange, ScannerRoomScanState expectedScanState)
    {
        private readonly HashSet<ushort> receivedPageIndexes = [];
        private readonly PagedAccumulator<NitroxTechType, ScannerResourceSummary> summaries = new(summary => summary.TechType);
        private readonly PagedAccumulator<(NitroxId EntityId, ushort TrackerIndex), ScannerResourceTarget> targets =
            new(target => (target.EntityId, target.TrackerIndex));
        private ushort? pageCount;
        private ulong? revision;
        private ScannerRoomScanState? scanState;

        public uint RequestId { get; } = requestId;

        public bool IsComplete => pageCount is { } count && receivedPageIndexes.Count == count;

        public bool Matches(ScannerRoomSnapshotPageData page) =>
            page.EffectiveRange.Equals(requestedRange) &&
            (page.Status is not (ScannerRoomQueryStatus.Complete or ScannerRoomQueryStatus.NotModified) ||
             ScanStatesEqual(page.ScanState, expectedScanState));

        public bool TryAdd(ScannerRoomSnapshotPageData page)
        {
            if (pageCount is { } expectedPageCount && expectedPageCount != page.PageCount ||
                revision is { } expectedRevision && expectedRevision != page.Revision)
            {
                return false;
            }
            if (scanState is { } expectedScanState && !ScanStatesEqual(expectedScanState, page.ScanState))
            {
                return false;
            }
            if (!receivedPageIndexes.Add(page.PageIndex))
            {
                return false;
            }

            pageCount = page.PageCount;
            revision = page.Revision;
            scanState = page.ScanState;
            summaries.AddPage(page.PageIndex, page.AvailableResources);
            targets.AddPage(page.PageIndex, page.Targets);
            return true;
        }

        public ScannerRoomSnapshot BuildSnapshot(NitroxId mapRoomId) =>
            new(mapRoomId, requestedRange, scanState!, revision!.Value, summaries.Build(), targets.Build());

    }

    /// <summary>
    /// Accumulates each page independently while retaining page order and first-occurrence de-duplication. This avoids
    /// re-walking every earlier target when the last page arrives, including when pages arrive out of order.
    /// </summary>
    private sealed class PagedAccumulator<TKey, TValue>(Func<TValue, TKey> getKey) where TKey : notnull
    {
        private readonly Dictionary<TKey, PagedItem<TValue>> includedItems = [];
        private readonly SortedDictionary<ushort, List<PagedItem<TValue>>> itemsByPage = [];
        private int includedCount;

        public void AddPage(ushort pageIndex, IReadOnlyList<TValue> values)
        {
            List<PagedItem<TValue>> pageItems = new(values.Count);
            foreach (TValue value in values)
            {
                TKey key = getKey(value);
                if (includedItems.TryGetValue(key, out PagedItem<TValue>? existingItem))
                {
                    if (existingItem.PageIndex <= pageIndex)
                    {
                        continue;
                    }

                    existingItem.Included = false;
                }
                else
                {
                    includedCount++;
                }

                PagedItem<TValue> item = new(pageIndex, value);
                includedItems[key] = item;
                pageItems.Add(item);
            }
            itemsByPage.Add(pageIndex, pageItems);
        }

        public IReadOnlyList<TValue> Build() => new PagedReadOnlyList<TValue>(itemsByPage, includedCount);
    }

    private sealed class PagedItem<TValue>(ushort pageIndex, TValue value)
    {
        public ushort PageIndex { get; } = pageIndex;
        public TValue Value { get; } = value;
        public bool Included { get; set; } = true;
    }

    private sealed class PagedReadOnlyList<TValue>(
        SortedDictionary<ushort, List<PagedItem<TValue>>> itemsByPage,
        int count) : IReadOnlyList<TValue>
    {
        public int Count { get; } = count;

        public TValue this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                foreach (List<PagedItem<TValue>> pageItems in itemsByPage.Values)
                {
                    foreach (PagedItem<TValue> item in pageItems)
                    {
                        if (!item.Included)
                        {
                            continue;
                        }

                        if (index-- == 0)
                        {
                            return item.Value;
                        }
                    }
                }

                throw new InvalidOperationException("Scanner Room snapshot item count did not match its accumulated pages");
            }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            foreach (List<PagedItem<TValue>> pageItems in itemsByPage.Values)
            {
                foreach (PagedItem<TValue> item in pageItems)
                {
                    if (item.Included)
                    {
                        yield return item.Value;
                    }
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
