using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;

namespace NitroxClient.GameLogic.ScannerRooms;

internal readonly record struct ScannerRoomRequestParameters(
    float Range,
    NitroxTechType? SelectedTechType,
    NitroxVector3? ObservedOrigin);

internal readonly record struct ScannerRoomDispatch(NitroxId MapRoomId, ScannerRoomRequestParameters Request);

internal readonly record struct ScannerRoomExpiredRequest(NitroxId MapRoomId, uint RequestId);

/// <summary>
/// Keeps one request in flight per room and coalesces overlapping refreshes to the latest requested parameters.
/// </summary>
internal sealed class ScannerRoomRequestCoordinator
{
    internal const double REQUEST_TIMEOUT_SECONDS = 60;

    private readonly LockObject coordinatorLock = new();
    private readonly Dictionary<NitroxId, RoomRequestState> rooms = [];

    public bool EnqueueOrReplace(
        NitroxId mapRoomId,
        ScannerRoomRequestParameters request,
        out ScannerRoomDispatch dispatch)
    {
        lock (coordinatorLock)
        {
            RoomRequestState room = GetOrCreateRoom(mapRoomId);
            if (room.IsInFlight)
            {
                room.QueuedRequest = request;
                dispatch = default;
                return false;
            }

            BeginDispatch(room, request);
            dispatch = new ScannerRoomDispatch(mapRoomId, request);
            return true;
        }
    }

    public bool ConfirmDispatch(NitroxId mapRoomId, uint requestId, double now)
    {
        lock (coordinatorLock)
        {
            if (!rooms.TryGetValue(mapRoomId, out RoomRequestState? room) ||
                !room.IsInFlight ||
                room.ActiveRequestId != null)
            {
                return false;
            }

            room.ActiveRequestId = requestId;
            room.Deadline = now + REQUEST_TIMEOUT_SECONDS;
            return true;
        }
    }

    public bool ObserveResponse(
        NitroxId mapRoomId,
        uint requestId,
        ScannerRoomSnapshotApplyResult result,
        double now,
        out ScannerRoomDispatch dispatch)
    {
        lock (coordinatorLock)
        {
            dispatch = default;
            if (!rooms.TryGetValue(mapRoomId, out RoomRequestState? room) ||
                room.ActiveRequestId != requestId)
            {
                return false;
            }

            if (result == ScannerRoomSnapshotApplyResult.WaitingForPages)
            {
                room.Deadline = now + REQUEST_TIMEOUT_SECONDS;
                return false;
            }
            if (!IsTerminal(result))
            {
                return false;
            }

            return CompleteAndPromote(mapRoomId, room, out dispatch);
        }
    }

    public bool AbortDispatch(NitroxId mapRoomId, uint requestId, out ScannerRoomDispatch dispatch)
    {
        lock (coordinatorLock)
        {
            dispatch = default;
            if (!rooms.TryGetValue(mapRoomId, out RoomRequestState? room) ||
                room.ActiveRequestId != requestId)
            {
                return false;
            }

            return CompleteAndPromote(mapRoomId, room, out dispatch);
        }
    }

    public bool TryExpire(
        NitroxId mapRoomId,
        double now,
        out ScannerRoomExpiredRequest expiredRequest,
        out ScannerRoomDispatch dispatch)
    {
        lock (coordinatorLock)
        {
            expiredRequest = default;
            dispatch = default;
            if (!rooms.TryGetValue(mapRoomId, out RoomRequestState? room) ||
                room.ActiveRequestId is not { } requestId ||
                now < room.Deadline)
            {
                return false;
            }

            ScannerRoomRequestParameters requestToRetry = room.QueuedRequest ?? room.ActiveRequest;
            room.QueuedRequest = null;
            BeginDispatch(room, requestToRetry);
            expiredRequest = new ScannerRoomExpiredRequest(mapRoomId, requestId);
            dispatch = new ScannerRoomDispatch(mapRoomId, requestToRetry);
            return true;
        }
    }

    public void RemoveRoom(NitroxId mapRoomId)
    {
        lock (coordinatorLock)
        {
            rooms.Remove(mapRoomId);
        }
    }

    public void Clear()
    {
        lock (coordinatorLock)
        {
            rooms.Clear();
        }
    }

    private bool CompleteAndPromote(NitroxId mapRoomId, RoomRequestState room, out ScannerRoomDispatch dispatch)
    {
        if (room.QueuedRequest is { } queuedRequest)
        {
            room.QueuedRequest = null;
            BeginDispatch(room, queuedRequest);
            dispatch = new ScannerRoomDispatch(mapRoomId, queuedRequest);
            return true;
        }

        rooms.Remove(mapRoomId);
        dispatch = default;
        return false;
    }

    private RoomRequestState GetOrCreateRoom(NitroxId mapRoomId)
    {
        if (!rooms.TryGetValue(mapRoomId, out RoomRequestState? room))
        {
            room = rooms[mapRoomId] = new RoomRequestState();
        }
        return room;
    }

    private static void BeginDispatch(RoomRequestState room, ScannerRoomRequestParameters request)
    {
        room.IsInFlight = true;
        room.ActiveRequest = request;
        room.ActiveRequestId = null;
        room.Deadline = double.PositiveInfinity;
    }

    private static bool IsTerminal(ScannerRoomSnapshotApplyResult result) =>
        result is ScannerRoomSnapshotApplyResult.Applied or
            ScannerRoomSnapshotApplyResult.NotModified or
            ScannerRoomSnapshotApplyResult.Failed;

    private sealed class RoomRequestState
    {
        public bool IsInFlight;
        public ScannerRoomRequestParameters ActiveRequest;
        public uint? ActiveRequestId;
        public double Deadline = double.PositiveInfinity;
        public ScannerRoomRequestParameters? QueuedRequest;
    }
}
