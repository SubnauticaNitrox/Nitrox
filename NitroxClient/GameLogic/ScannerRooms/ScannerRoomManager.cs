using System;
using System.Diagnostics;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.GameLogic.ScannerRooms;

internal sealed class ScannerRoomManager : IDisposable
{
    private readonly IPacketSender packetSender;
    private readonly ScannerRoomSnapshotStore snapshotStore;
    private readonly IMultiplayerSession multiplayerSession;
    private readonly ScannerRoomRequestCoordinator requestCoordinator = new();
    private readonly ScannerRoomSnapshotPageQueue snapshotPageQueue = new();

    public event Action<NitroxId, ScannerRoomSnapshotApplyResult>? SnapshotChanged;
    public event Action? SessionJoined;
    public event Action? StateCleared;

    public bool IsSessionJoined => multiplayerSession.CurrentState.CurrentStage == MultiplayerSessionConnectionStage.SESSION_JOINED;

    public ScannerRoomManager(IPacketSender packetSender, ScannerRoomSnapshotStore snapshotStore, IMultiplayerSession multiplayerSession)
    {
        this.packetSender = packetSender;
        this.snapshotStore = snapshotStore;
        this.multiplayerSession = multiplayerSession;

        multiplayerSession.ConnectionStateChanged += OnConnectionStateChanged;
        Multiplayer.OnBeforeMultiplayerStart += Clear;
        Multiplayer.OnAfterMultiplayerEnd += Clear;
    }

    public void RequestSnapshot(NitroxId mapRoomId, float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin)
    {
        range = ScannerRoomQueryParameters.NormalizeRange(range);
        selectedTechType = ScannerRoomQueryParameters.NormalizeSelection(selectedTechType);
        ScannerRoomRequestParameters request = new(range, selectedTechType, observedOrigin);
        if (requestCoordinator.EnqueueOrReplace(mapRoomId, request, out ScannerRoomDispatch dispatch))
        {
            Dispatch(dispatch);
        }
    }

    public void EnqueuePage(ScannerRoomSnapshotPage packet) => snapshotPageQueue.Enqueue(packet);

    public int ProcessQueuedPages(int pageBudget) => snapshotPageQueue.Process(pageBudget, ProcessQueuedPageSafely);

    private void ProcessQueuedPageSafely(ScannerRoomSnapshotPage packet)
    {
        try
        {
            ProcessQueuedPage(packet);
        }
        catch (Exception ex)
        {
            Log.Error(ex, $"Failed to process Scanner Room snapshot page {packet.PageIndex + 1}/{packet.PageCount} for room {packet.MapRoomId}");
        }
    }

    private void ProcessQueuedPage(ScannerRoomSnapshotPage packet)
    {
        ScannerRoomSnapshotApplyResult result = snapshotStore.AcceptPage(new ScannerRoomSnapshotPageData(
            packet.MapRoomId,
            packet.RequestId,
            packet.Status,
            packet.EffectiveRange,
            packet.SelectedTechType,
            packet.Revision,
            packet.PageIndex,
            packet.PageCount,
            packet.AvailableResources,
            packet.Targets));

        bool isTerminal = result is ScannerRoomSnapshotApplyResult.Applied or
            ScannerRoomSnapshotApplyResult.NotModified or
            ScannerRoomSnapshotApplyResult.Failed;
        if (isTerminal)
        {
            SnapshotChanged?.Invoke(packet.MapRoomId, result);
        }

        if (requestCoordinator.ObserveResponse(
                packet.MapRoomId,
                packet.RequestId,
                result,
                GetTimestampSeconds(),
                out ScannerRoomDispatch dispatch))
        {
            Dispatch(dispatch);
        }
    }

    public bool TryGetSnapshot(NitroxId mapRoomId, out ScannerRoomSnapshot? snapshot) => snapshotStore.TryGetSnapshot(mapRoomId, out snapshot);

    public void RemoveRoom(NitroxId mapRoomId)
    {
        snapshotPageQueue.RemoveRoom(mapRoomId);
        requestCoordinator.RemoveRoom(mapRoomId);
        snapshotStore.RemoveRoom(mapRoomId);
    }

    public void PumpRequests(NitroxId mapRoomId)
    {
        // Receiving a large response is not a missing-response timeout. Keep the active request alive while any of its
        // pages are waiting for their frame-budgeted turn.
        if (requestCoordinator.TryGetActiveRequestId(mapRoomId, out uint activeRequestId) &&
            snapshotPageQueue.HasQueuedPage(mapRoomId, activeRequestId))
        {
            return;
        }

        double now = GetTimestampSeconds();
        if (requestCoordinator.TryExpire(mapRoomId, now, out ScannerRoomExpiredRequest expiredRequest, out ScannerRoomDispatch dispatch))
        {
            snapshotStore.CancelQuery(expiredRequest.MapRoomId, expiredRequest.RequestId);
            SnapshotChanged?.Invoke(expiredRequest.MapRoomId, ScannerRoomSnapshotApplyResult.Failed);
            Dispatch(dispatch);
        }
    }

    public void Clear()
    {
        snapshotPageQueue.Clear();
        requestCoordinator.Clear();
        snapshotStore.Clear();
        StateCleared?.Invoke();
    }

    public void Dispose()
    {
        snapshotPageQueue.Clear();
        multiplayerSession.ConnectionStateChanged -= OnConnectionStateChanged;
        Multiplayer.OnBeforeMultiplayerStart -= Clear;
        Multiplayer.OnAfterMultiplayerEnd -= Clear;
    }

    private void OnConnectionStateChanged(IMultiplayerSessionConnectionState state)
    {
        if (state.CurrentStage == MultiplayerSessionConnectionStage.DISCONNECTED)
        {
            Clear();
        }
        else if (state.CurrentStage == MultiplayerSessionConnectionStage.SESSION_JOINED)
        {
            SessionJoined?.Invoke();
        }
    }

    private void Dispatch(ScannerRoomDispatch dispatch)
    {
        ScannerRoomRequestParameters request = dispatch.Request;
        ScannerRoomQueryTicket ticket = snapshotStore.BeginQuery(dispatch.MapRoomId, request.Range, request.SelectedTechType);
        double now = GetTimestampSeconds();
        if (!requestCoordinator.ConfirmDispatch(dispatch.MapRoomId, ticket.RequestId, now))
        {
            snapshotStore.CancelQuery(dispatch.MapRoomId, ticket.RequestId);
            return;
        }

        bool sent = packetSender.Send(new ScannerRoomQuery(
            dispatch.MapRoomId,
            ticket.RequestId,
            request.Range,
            request.SelectedTechType,
            ticket.KnownRevision,
            request.ObservedOrigin));
        if (!sent)
        {
            snapshotStore.CancelQuery(dispatch.MapRoomId, ticket.RequestId);
            if (requestCoordinator.AbortDispatch(dispatch.MapRoomId, ticket.RequestId, out ScannerRoomDispatch nextDispatch))
            {
                Dispatch(nextDispatch);
            }
        }
    }

    private static double GetTimestampSeconds() => Stopwatch.GetTimestamp() / (double)Stopwatch.Frequency;
}
