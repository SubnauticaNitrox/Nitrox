using System;
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

    public uint RequestSnapshot(NitroxId mapRoomId, float range, NitroxTechType? selectedTechType, NitroxVector3? observedOrigin)
    {
        range = ScannerRoomQueryParameters.NormalizeRange(range);
        selectedTechType = ScannerRoomQueryParameters.NormalizeSelection(selectedTechType);
        ScannerRoomQueryTicket ticket = snapshotStore.BeginQuery(mapRoomId, range, selectedTechType);
        packetSender.Send(new ScannerRoomQuery(mapRoomId, ticket.RequestId, range, selectedTechType, ticket.KnownRevision, observedOrigin));
        return ticket.RequestId;
    }

    public void ProcessPage(ScannerRoomSnapshotPage packet)
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

        if (result is ScannerRoomSnapshotApplyResult.Applied or ScannerRoomSnapshotApplyResult.NotModified or ScannerRoomSnapshotApplyResult.Failed)
        {
            SnapshotChanged?.Invoke(packet.MapRoomId, result);
        }
    }

    public bool TryGetSnapshot(NitroxId mapRoomId, out ScannerRoomSnapshot? snapshot) => snapshotStore.TryGetSnapshot(mapRoomId, out snapshot);

    public void RemoveRoom(NitroxId mapRoomId) => snapshotStore.RemoveRoom(mapRoomId);

    public void Clear()
    {
        snapshotStore.Clear();
        StateCleared?.Invoke();
    }

    public void Dispose()
    {
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
}
