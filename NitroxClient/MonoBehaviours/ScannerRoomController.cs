using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Helper;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.ScannerRooms;
using Nitrox.Model.Subnautica.Extensions;
using NitroxClient.Extensions;
using NitroxClient.GameLogic.ScannerRooms;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Binds a spawned Scanner Room to its multiplayer identity and translates vanilla Scanner Room interactions into
/// authoritative snapshot requests.
/// </summary>
[DisallowMultipleComponent]
public sealed class ScannerRoomController : MonoBehaviour
{
    private const int TARGET_PREPARATION_BUDGET_PER_FRAME = 256;
    private static readonly FieldInfo RESOURCE_NODES_FIELD =
        Reflect.Field((MapRoomFunctionality mapRoom) => mapRoom.resourceNodes);

    private readonly HashSet<TechType> authoritativeTechTypes = [];
    private readonly HashSet<Collider> localInteractionColliders = [];
    private readonly ScannerRoomRefreshScheduler refreshScheduler = new();
    private readonly ScannerRoomResourceAuthorityState resourceAuthority = new();
    private List<ResourceTrackerDatabase.ResourceInfo> authoritativeResourceNodes = [];
    private MapRoomFunctionality mapRoom = null!;
    private NitroxId mapRoomId = null!;
    private ScannerRoomRequestTrigger? requestTrigger;
    private ScannerRoomManager scannerRoomManager = null!;
    private ScannerRoomSnapshot? authoritativeSnapshot;
    private ScannerRoomTargetPreparation<ResourceTrackerDatabase.ResourceInfo>? pendingTargetPreparation;
    private ScannerRoomVirtualResourceCache<ResourceTrackerDatabase.ResourceInfo>? virtualResourceCache;
    private uGUI_ResourceTracker resourceTracker = null!;
    private volatile bool sessionJoined;
    private int reconnectRefreshPending;
    private int stateClearPending;

    public bool ShouldSuppressVanillaResources =>
        Volatile.Read(ref stateClearPending) != 0 || resourceAuthority.SuppressVanillaResources;

    public static ScannerRoomController Attach(MapRoomFunctionality mapRoom, NitroxId mapRoomId)
    {
        if (!mapRoom.TryGetComponent(out ScannerRoomController controller))
        {
            controller = mapRoom.gameObject.AddComponent<ScannerRoomController>();
        }

        controller.Initialize(mapRoom, mapRoomId);
        return controller;
    }

    public void RequestInitialSnapshot() => TryRequestSnapshot(SnapshotRequestMode.Initial);

    public void RequestImmediateSnapshot() => TryRequestSnapshot(SnapshotRequestMode.Immediate);

    public void RequestSnapshotIfStateChanged() => TryRequestSnapshot(SnapshotRequestMode.StateChanged);

    public void OnLocalInteractionEntered(Collider interactionCollider)
    {
        if (!interactionCollider || !localInteractionColliders.Add(interactionCollider) || localInteractionColliders.Count != 1)
        {
            return;
        }

        double now = Time.unscaledTime;
        UpdateRefreshActivity(now);
        if (!refreshScheduler.WasRefreshedRecently(now))
        {
            RequestImmediateSnapshot();
        }
    }

    public void OnLocalInteractionExited(Collider interactionCollider)
    {
        if (interactionCollider)
        {
            localInteractionColliders.Remove(interactionCollider);
        }
        UpdateRefreshActivity(Time.unscaledTime);
    }

    /// <summary>
    /// Replaces the scanner list with server-provided resource types and rebuilds the vanilla controls.
    /// </summary>
    /// <returns>Whether synchronized state owns the list and the vanilla local-database lookup should be skipped.</returns>
    public bool TryApplyAuthoritativeResourceList(uGUI_MapRoomScanner scanner)
    {
        if (!ShouldSuppressVanillaResources)
        {
            return false;
        }

        scanner.availableTechTypes.Clear();
        foreach (TechType techType in authoritativeTechTypes)
        {
            scanner.availableTechTypes.Add(techType);
        }

        scanner.sortedTechTypes.Clear();
        foreach (TechType techType in authoritativeTechTypes)
        {
            scanner.sortedTechTypes.Add(techType);
        }
        scanner.sortedTechTypes.Sort(uGUI_MapRoomScanner.CompareByName);
        scanner.currentPage = 0;
        scanner.RebuildResourceList();
        return true;
    }

    /// <summary>
    /// Replaces the room-local resource nodes for the requested vanilla scan type.
    /// </summary>
    /// <returns>Whether synchronized state owns the nodes and the vanilla local-database lookup should be skipped.</returns>
    public bool TryApplyAuthoritativeResourceNodes(TechType requestedTechType)
    {
        if (!ShouldSuppressVanillaResources)
        {
            return false;
        }

        List<ResourceTrackerDatabase.ResourceInfo> replacementNodes = [];
        if (authoritativeSnapshot != null &&
            SnapshotMatchesSelection(authoritativeSnapshot, requestedTechType))
        {
            replacementNodes = authoritativeResourceNodes;
        }

        ReplaceResourceNodes(replacementNodes);

        NotifyResourceTrackerChanged();
        return true;
    }

    private void Initialize(MapRoomFunctionality mapRoom, NitroxId mapRoomId)
    {
        if (requestTrigger != null)
        {
            return;
        }

        this.mapRoom = mapRoom;
        this.mapRoomId = mapRoomId;
        scannerRoomManager = this.Resolve<ScannerRoomManager>();
        virtualResourceCache = new ScannerRoomVirtualResourceCache<ResourceTrackerDatabase.ResourceInfo>(
            mapRoomId,
            uniqueId => new ResourceTrackerDatabase.ResourceInfo { uniqueId = uniqueId });
        scannerRoomManager.SnapshotChanged += OnSnapshotChanged;
        scannerRoomManager.SessionJoined += OnSessionJoined;
        scannerRoomManager.StateCleared += OnStateCleared;
        sessionJoined = scannerRoomManager.IsSessionJoined;
        requestTrigger = new ScannerRoomRequestTrigger(
            (range, selectedTechType, observedOrigin) => scannerRoomManager.RequestSnapshot(mapRoomId, range, selectedTechType, observedOrigin));

        if (scannerRoomManager.TryGetSnapshot(mapRoomId, out ScannerRoomSnapshot? snapshot))
        {
            resourceAuthority.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Applied, ScannerRoomQueryStatus.Complete);
            BeginSnapshotPreparation(snapshot!);
        }
        else
        {
            // Post-spawn attachment can happen after vanilla has already populated local nodes and blips.
            ClearSynchronizedResourceState();
        }
    }

    private void Start()
    {
        UpdateRefreshActivity(Time.unscaledTime);
        if (refreshScheduler.HasRefreshActivity)
        {
            RequestInitialSnapshot();
        }
    }

    private void Update()
    {
        if (requestTrigger == null || !mapRoom)
        {
            return;
        }

        localInteractionColliders.RemoveWhere(collider => !collider);
        double now = Time.unscaledTime;
        UpdateRefreshActivity(now);

        if (Interlocked.Exchange(ref stateClearPending, 0) != 0)
        {
            EnterPendingState();
        }
        AdvanceSnapshotPreparation();
        if (sessionJoined)
        {
            scannerRoomManager.PumpRequests(mapRoomId);
        }
        if (sessionJoined && Interlocked.Exchange(ref reconnectRefreshPending, 0) != 0)
        {
            if (refreshScheduler.HasRefreshActivity)
            {
                RequestImmediateSnapshot();
                return;
            }
        }
        if (sessionJoined && refreshScheduler.IsRefreshDue(now))
        {
            RequestImmediateSnapshot();
        }
    }

    private void TryRequestSnapshot(SnapshotRequestMode requestMode)
    {
        if (!sessionJoined || requestTrigger == null ||
            !TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin))
        {
            return;
        }

        double now = Time.unscaledTime;
        UpdateRefreshActivity(now);
        bool requestIssued = requestMode switch
        {
            SnapshotRequestMode.Initial => requestTrigger.TryRequestInitial(range, selectedTechType, observedOrigin),
            SnapshotRequestMode.StateChanged => requestTrigger.TryRequestIfChanged(range, selectedTechType, observedOrigin),
            SnapshotRequestMode.Immediate => RequestImmediately(requestTrigger, range, selectedTechType, observedOrigin),
            _ => false
        };
        if (requestIssued)
        {
            refreshScheduler.MarkRefreshed(now);
        }
    }

    private static bool RequestImmediately(
        ScannerRoomRequestTrigger trigger,
        float range,
        NitroxTechType? selectedTechType,
        NitroxVector3? observedOrigin)
    {
        trigger.RequestImmediate(range, selectedTechType, observedOrigin);
        return true;
    }

    private void UpdateRefreshActivity(double now) =>
        refreshScheduler.SetActivity(mapRoom && mapRoom.typeToScan != TechType.None, localInteractionColliders.Count > 0, now);

    private bool TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin)
    {
        range = default;
        selectedTechType = null;
        observedOrigin = null;

        if (!mapRoom)
        {
            return false;
        }

        range = mapRoom.GetScanRange();
        selectedTechType = mapRoom.typeToScan == TechType.None ? null : mapRoom.typeToScan.ToDto();
        observedOrigin = mapRoom.wireFrameWorld ? mapRoom.wireFrameWorld.position.ToDto() : null;
        return true;
    }

    private void OnSnapshotChanged(ScannerRoomSnapshotUpdate update)
    {
        if (!mapRoomId.Equals(update.MapRoomId))
        {
            return;
        }

        bool supersededPendingReset = false;
        if (ScannerRoomResourceAuthorityState.IsAuthorityDecision(update.Result, update.AcceptedStatus))
        {
            // A correlated response can arrive before this component's next Update after reconnect. It supersedes the
            // queued reset and fully replaces or restores resource state below.
            supersededPendingReset = Interlocked.Exchange(ref stateClearPending, 0) != 0;
        }
        ScannerRoomResourceAuthorityMode previousMode = resourceAuthority.Mode;
        resourceAuthority.ObserveAcceptedResponse(update.Result, update.AcceptedStatus);
        if (resourceAuthority.Mode == ScannerRoomResourceAuthorityMode.Rollback)
        {
            CancelSnapshotPreparation();
            if (previousMode != ScannerRoomResourceAuthorityMode.Rollback || supersededPendingReset)
            {
                RestoreVanillaResourceState();
            }
            return;
        }
        if (ScannerRoomResourceAuthorityState.RequiresFallbackClear(previousMode, resourceAuthority.Mode))
        {
            // Rollback may have populated both the vanilla scanner list and local resource nodes. Once the server
            // accepts authority again, hide that fallback immediately while the replacement is prepared in shadow.
            ClearSynchronizedResourceState();
        }

        if (resourceAuthority.Mode == ScannerRoomResourceAuthorityMode.Authoritative &&
            (update.Result is ScannerRoomSnapshotApplyResult.Applied or ScannerRoomSnapshotApplyResult.NotModified) &&
            scannerRoomManager.TryGetSnapshot(mapRoomId, out ScannerRoomSnapshot? snapshot))
        {
            BeginSnapshotPreparation(snapshot!);
        }
    }

    private void OnSessionJoined()
    {
        Interlocked.Exchange(ref stateClearPending, 1);
        sessionJoined = true;
        Interlocked.Exchange(ref reconnectRefreshPending, 1);
    }

    private void OnStateCleared()
    {
        Interlocked.Exchange(ref stateClearPending, 1);
        sessionJoined = false;
    }

    private void EnterPendingState()
    {
        CancelSnapshotPreparation();
        resourceAuthority.ResetToPending();
        ClearSynchronizedResourceState();
    }

    private void ClearSynchronizedResourceState()
    {
        authoritativeSnapshot = null;
        authoritativeResourceNodes = [];
        authoritativeTechTypes.Clear();
        ReplaceResourceNodes([]);

        NotifyResourceTrackerChanged();

        uGUI_MapRoomScanner scanner = mapRoom.GetComponentInChildren<uGUI_MapRoomScanner>(true);
        if (scanner)
        {
            scanner.availableTechTypes.Clear();
            scanner.sortedTechTypes.Clear();
            scanner.currentPage = 0;
            scanner.RebuildResourceList();
        }
        RefreshBlipsWithoutAdvancingScan();
    }

    private void RestoreVanillaResourceState()
    {
        authoritativeSnapshot = null;
        authoritativeResourceNodes = [];
        authoritativeTechTypes.Clear();
        ReplaceResourceNodes([]);

        uGUI_MapRoomScanner scanner = mapRoom.GetComponentInChildren<uGUI_MapRoomScanner>(true);
        if (scanner)
        {
            scanner.UpdateAvailableTechTypes();
        }
        if (mapRoom.typeToScan != TechType.None)
        {
            mapRoom.ObtainResourceNodes(mapRoom.typeToScan);
        }

        NotifyResourceTrackerChanged();
        RefreshBlipsWithoutAdvancingScan();
    }

    private void NotifyResourceTrackerChanged()
    {
        if (!resourceTracker)
        {
            resourceTracker = UnityEngine.Object.FindObjectOfType<uGUI_ResourceTracker>();
        }
        if (resourceTracker)
        {
            resourceTracker.gatherNextTick = true;
        }
    }

    /// <summary>
    /// Vanilla declares this list readonly. Replacing its backing reference through the cached field metadata keeps
    /// publication atomic: vanilla can observe either the old complete generation or the new complete generation,
    /// never the shadow list while it is being populated over multiple frames.
    /// </summary>
    private void ReplaceResourceNodes(List<ResourceTrackerDatabase.ResourceInfo> resourceNodes) =>
        RESOURCE_NODES_FIELD.SetValue(mapRoom, resourceNodes);

    private void BeginSnapshotPreparation(ScannerRoomSnapshot snapshot)
    {
        if (ReferenceEquals(authoritativeSnapshot, snapshot) ||
            pendingTargetPreparation != null && ReferenceEquals(pendingTargetPreparation.Snapshot, snapshot))
        {
            return;
        }

        CancelSnapshotPreparation();
        pendingTargetPreparation = new ScannerRoomTargetPreparation<ResourceTrackerDatabase.ResourceInfo>(snapshot);
    }

    private void AdvanceSnapshotPreparation()
    {
        ScannerRoomTargetPreparation<ResourceTrackerDatabase.ResourceInfo>? preparation = pendingTargetPreparation;
        if (preparation == null)
        {
            return;
        }

        try
        {
            TechType? selectedTechType = ParseSelectedTechType(preparation.Snapshot.SelectedTechType);
            preparation.Advance(
                TARGET_PREPARATION_BUDGET_PER_FRAME,
                target => PrepareResourceNode(target, selectedTechType));

            if (!preparation.TryTakeCompleted(out List<ResourceTrackerDatabase.ResourceInfo>? preparedResourceNodes))
            {
                return;
            }

            pendingTargetPreparation = null;
            CommitPreparedSnapshot(preparation.Snapshot, preparedResourceNodes!);
            preparation.Dispose();
        }
        catch (Exception ex)
        {
            if (ReferenceEquals(pendingTargetPreparation, preparation))
            {
                pendingTargetPreparation = null;
            }
            preparation.Dispose();
            Log.Error(ex, $"Failed to prepare Scanner Room snapshot {preparation.Snapshot.Revision} for room {mapRoomId}");
        }
    }

    private ResourceTrackerDatabase.ResourceInfo? PrepareResourceNode(ScannerResourceTarget target, TechType? selectedTechType)
    {
        if (selectedTechType is not { } techType || virtualResourceCache == null)
        {
            return null;
        }

        ScannerRoomVirtualResource<ResourceTrackerDatabase.ResourceInfo> virtualResource = virtualResourceCache.CreateFresh(target);
        virtualResource.Resource.techType = techType;
        virtualResource.Resource.position = target.Position.ToUnity();
        return virtualResource.Resource;
    }

    private void CommitPreparedSnapshot(
        ScannerRoomSnapshot snapshot,
        List<ResourceTrackerDatabase.ResourceInfo> preparedResourceNodes)
    {
        HashSet<TechType> nextTechTypes = GetAvailableTechTypes(snapshot.AvailableResources);
        bool catalogChanged = authoritativeSnapshot == null || !authoritativeTechTypes.SetEquals(nextTechTypes);

        authoritativeSnapshot = snapshot;
        authoritativeResourceNodes = preparedResourceNodes;
        authoritativeTechTypes.Clear();
        authoritativeTechTypes.UnionWith(nextTechTypes);

        if (catalogChanged)
        {
            uGUI_MapRoomScanner scanner = mapRoom.GetComponentInChildren<uGUI_MapRoomScanner>(true);
            if (scanner)
            {
                TryApplyAuthoritativeResourceList(scanner);
            }
        }

        if (TryApplyAuthoritativeResourceNodes(mapRoom.typeToScan))
        {
            RefreshBlipsWithoutAdvancingScan();
        }
    }

    private void CancelSnapshotPreparation()
    {
        pendingTargetPreparation?.Cancel();
        pendingTargetPreparation = null;
    }

    private void RefreshBlipsWithoutAdvancingScan()
    {
        int scanProgress = mapRoom.numNodesScanned;
        mapRoom.numNodesScanned = Math.Max(-1, scanProgress - 1);
        try
        {
            mapRoom.UpdateBlips();
        }
        finally
        {
            mapRoom.numNodesScanned = scanProgress;
        }
    }

    private static HashSet<TechType> GetAvailableTechTypes(IReadOnlyList<ScannerResourceSummary> summaries)
    {
        HashSet<TechType> techTypes = [];
        foreach (ScannerResourceSummary summary in summaries)
        {
            if (summary.Count > 0 && Enum.TryParse(summary.TechType.Name, out TechType techType) && techType != TechType.None)
            {
                techTypes.Add(techType);
            }
        }
        return techTypes;
    }

    private static bool SnapshotMatchesSelection(ScannerRoomSnapshot snapshot, TechType requestedTechType) =>
        requestedTechType == TechType.None
            ? snapshot.SelectedTechType == null
            : snapshot.SelectedTechType?.Equals(requestedTechType.ToDto()) == true;

    private static TechType? ParseSelectedTechType(NitroxTechType? selectedTechType) =>
        selectedTechType != null && Enum.TryParse(selectedTechType.Name, out TechType techType) && techType != TechType.None
            ? techType
            : null;

    private void OnDestroy()
    {
        CancelSnapshotPreparation();
        if (scannerRoomManager != null)
        {
            scannerRoomManager.SnapshotChanged -= OnSnapshotChanged;
            scannerRoomManager.SessionJoined -= OnSessionJoined;
            scannerRoomManager.StateCleared -= OnStateCleared;
            scannerRoomManager.RemoveRoom(mapRoomId);
        }
        localInteractionColliders.Clear();
    }

    private enum SnapshotRequestMode
    {
        Initial,
        StateChanged,
        Immediate
    }
}
