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
using NitroxClient.GameLogic;
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
    private readonly ScannerRoomLocalIntentTracker localIntentTracker = new();
    private readonly ScannerRoomRefreshScheduler refreshScheduler = new();
    private readonly ScannerRoomResourceAuthorityState resourceAuthority = new();
    private List<ResourceTrackerDatabase.ResourceInfo> authoritativeResourceNodes = [];
    private MapRoomFunctionality mapRoom = null!;
    private NitroxId mapRoomId = null!;
    private ScannerRoomRequestTrigger? requestTrigger;
    private ScannerRoomManager scannerRoomManager = null!;
    private ScannerRoomScanState canonicalScanState = ScannerRoomScanState.Empty;
    private ScannerRoomSnapshot? authoritativeSnapshot;
    private ScannerRoomTargetPreparation<ResourceTrackerDatabase.ResourceInfo>? pendingTargetPreparation;
    private ScannerRoomVirtualResourceCache<ResourceTrackerDatabase.ResourceInfo>? virtualResourceCache;
    private uGUI_ResourceTracker resourceTracker = null!;
    private volatile bool sessionJoined;
    private bool authorityProbePending;
    private bool applyingCanonicalScanState;
    private bool hasRollbackLocalIntent;
    private int rollbackLocalScanProgress;
    private TechType rollbackLocalSelection;
    private int reconnectRefreshPending;
    private int stateClearPending;

    public bool ShouldSuppressVanillaResources =>
        Volatile.Read(ref stateClearPending) != 0 || resourceAuthority.SuppressVanillaResources;

    public static ScannerRoomController Attach(MapRoomFunctionality mapRoom, NitroxId mapRoomId) =>
        Attach(mapRoom, mapRoomId, ScannerRoomScanState.Empty);

    public static ScannerRoomController Attach(
        MapRoomFunctionality mapRoom,
        NitroxId mapRoomId,
        ScannerRoomScanState persistedScanState)
    {
        if (!mapRoom.TryGetComponent(out ScannerRoomController controller))
        {
            controller = mapRoom.gameObject.AddComponent<ScannerRoomController>();
        }

        controller.Initialize(mapRoom, mapRoomId, persistedScanState ?? ScannerRoomScanState.Empty);
        return controller;
    }

    public void RequestInitialSnapshot() => TryRequestSnapshot(SnapshotRequestMode.Initial);

    public void RequestImmediateSnapshot() => TryRequestSnapshot(SnapshotRequestMode.Immediate);

    public void RequestSnapshotIfStateChanged() => TryRequestSnapshot(SnapshotRequestMode.StateChanged);

    /// <summary>
    /// Publishes the vanilla transition which just happened as an optimistic shared-selection intent. Both the
    /// scanner UI and MapRoomFunctionality hooks route here, so nested vanilla calls are de-duplicated.
    /// </summary>
    public void SubmitLocalScanStateIntent()
    {
        if (applyingCanonicalScanState || !mapRoom || scannerRoomManager == null)
        {
            return;
        }
        if (resourceAuthority.Mode == ScannerRoomResourceAuthorityMode.Rollback)
        {
            localIntentTracker.Clear();
            return;
        }

        NitroxTechType? desiredTechType = mapRoom.typeToScan == TechType.None ? null : mapRoom.typeToScan.ToDto();
        if (SelectionsEqual(canonicalScanState.SelectedTechType, desiredTechType))
        {
            localIntentTracker.Clear();
            RequestSnapshotIfStateChanged();
            return;
        }
        if (!localIntentTracker.TryBegin(desiredTechType))
        {
            return;
        }

        CaptureRollbackLocalState();
        DiscardTargetsForMismatchedSelection(mapRoom.typeToScan);

        if (!sessionJoined)
        {
            localIntentTracker.Clear();
            return;
        }

        if (!scannerRoomManager.RequestScanStateChange(mapRoomId, desiredTechType))
        {
            localIntentTracker.Clear();
            if (resourceAuthority.Mode != ScannerRoomResourceAuthorityMode.Rollback)
            {
                ApplyCanonicalScanState(canonicalScanState, requestSnapshot: true);
            }
        }
    }

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

    private void Initialize(MapRoomFunctionality mapRoom, NitroxId mapRoomId, ScannerRoomScanState persistedScanState)
    {
        if (requestTrigger != null)
        {
            scannerRoomManager.SeedScanState(mapRoomId, persistedScanState);
            return;
        }

        this.mapRoom = mapRoom;
        this.mapRoomId = mapRoomId;
        scannerRoomManager = this.Resolve<ScannerRoomManager>();
        ScannerRoomPlayerBlipManager.GetOrCreate(mapRoom, this.Resolve<PlayerManager>());
        scannerRoomManager.SeedScanState(mapRoomId, persistedScanState);
        virtualResourceCache = new ScannerRoomVirtualResourceCache<ResourceTrackerDatabase.ResourceInfo>(
            mapRoomId,
            uniqueId => new ResourceTrackerDatabase.ResourceInfo { uniqueId = uniqueId });
        scannerRoomManager.SnapshotChanged += OnSnapshotChanged;
        scannerRoomManager.ScanStateChanged += OnScanStateChanged;
        scannerRoomManager.SessionJoined += OnSessionJoined;
        scannerRoomManager.StateCleared += OnStateCleared;
        sessionJoined = scannerRoomManager.IsSessionJoined;
        requestTrigger = new ScannerRoomRequestTrigger(
            (range, expectedScanState, observedOrigin) => scannerRoomManager.RequestSnapshot(mapRoomId, range, expectedScanState, observedOrigin));

        if (scannerRoomManager.TryGetScanState(mapRoomId, out ScannerRoomScanState? scanState))
        {
            // The server's rollback flag is discovered through the first query response. Preserve the vanilla value
            // so a disabled server can reject authority without a persisted shared state permanently replacing it.
            CaptureRollbackLocalState();
            canonicalScanState = scanState!;
            authorityProbePending = mapRoom.typeToScan != (ParseSelectedTechType(canonicalScanState.SelectedTechType) ?? TechType.None);
            ApplyCanonicalScanState(canonicalScanState, requestSnapshot: false);
        }

        if (scannerRoomManager.TryGetSnapshot(mapRoomId, out ScannerRoomSnapshot? snapshot))
        {
            resourceAuthority.ObserveAcceptedResponse(ScannerRoomSnapshotApplyResult.Applied, ScannerRoomQueryStatus.Complete);
            hasRollbackLocalIntent = false;
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
        if (refreshScheduler.HasRefreshActivity || authorityProbePending)
        {
            RequestInitialSnapshot();
            if (sessionJoined)
            {
                authorityProbePending = false;
            }
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
        if (sessionJoined && authorityProbePending)
        {
            authorityProbePending = false;
            RequestInitialSnapshot();
        }
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
            !TryGetRequestParameters(out float range, out ScannerRoomScanState expectedScanState, out NitroxVector3? observedOrigin))
        {
            return;
        }

        double now = Time.unscaledTime;
        UpdateRefreshActivity(now);
        bool requestIssued = requestMode switch
        {
            SnapshotRequestMode.Initial => requestTrigger.TryRequestInitial(range, expectedScanState, observedOrigin),
            SnapshotRequestMode.StateChanged => requestTrigger.TryRequestIfChanged(range, expectedScanState, observedOrigin),
            SnapshotRequestMode.Immediate => RequestImmediately(requestTrigger, range, expectedScanState, observedOrigin),
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
        ScannerRoomScanState expectedScanState,
        NitroxVector3? observedOrigin)
    {
        trigger.RequestImmediate(range, expectedScanState, observedOrigin);
        return true;
    }

    private void UpdateRefreshActivity(double now) =>
        refreshScheduler.SetActivity(mapRoom && mapRoom.typeToScan != TechType.None, localInteractionColliders.Count > 0, now);

    private bool TryGetRequestParameters(out float range, out ScannerRoomScanState expectedScanState, out NitroxVector3? observedOrigin)
    {
        range = default;
        expectedScanState = ScannerRoomScanState.Empty;
        observedOrigin = null;

        if (!mapRoom)
        {
            return false;
        }

        range = mapRoom.GetScanRange();
        if (scannerRoomManager.TryGetScanState(mapRoomId, out ScannerRoomScanState? scanState))
        {
            expectedScanState = scanState!;
        }
        observedOrigin = mapRoom.wireFrameWorld ? mapRoom.wireFrameWorld.position.ToDto() : null;
        return true;
    }

    private void OnScanStateChanged(ScannerRoomScanStateUpdate update)
    {
        if (!mapRoomId.Equals(update.MapRoomId))
        {
            return;
        }

        canonicalScanState = update.ScanState;
        localIntentTracker.Clear();
        if (resourceAuthority.Mode != ScannerRoomResourceAuthorityMode.Rollback)
        {
            ApplyCanonicalScanState(update.ScanState, requestSnapshot: true);
        }
    }

    private void ApplyCanonicalScanState(ScannerRoomScanState scanState, bool requestSnapshot)
    {
        TechType desiredTechType = ParseSelectedTechType(scanState.SelectedTechType) ?? TechType.None;
        bool selectionChanged = mapRoom.typeToScan != desiredTechType;
        uGUI_MapRoomScanner scanner = mapRoom.GetComponentInChildren<uGUI_MapRoomScanner>(true);

        CancelSnapshotPreparation();
        if (authoritativeSnapshot == null || !ScanStatesEqual(authoritativeSnapshot.ScanState, scanState))
        {
            authoritativeSnapshot = null;
            authoritativeResourceNodes = [];
            ReplaceResourceNodes([]);
        }

        if (scanner && ShouldSuppressVanillaResources)
        {
            // Rebuild before applying the transition so OnStartScan sees the authoritative catalog. The transition
            // itself is what changes an already-open scanner from the resource list to its scanning presentation.
            TryApplyAuthoritativeResourceList(scanner);
        }

        if (selectionChanged)
        {
            ApplyVanillaScanSelection(desiredTechType, scanner);
        }

        NotifyResourceTrackerChanged();
        RefreshBlipsWithoutAdvancingScan();
        UpdateRefreshActivity(Time.unscaledTime);

        if (requestSnapshot && sessionJoined)
        {
            RequestImmediateSnapshot();
        }
    }

    private void ApplyVanillaScanSelection(TechType desiredTechType, uGUI_MapRoomScanner? scanner = null)
    {
        applyingCanonicalScanState = true;
        try
        {
            if (desiredTechType != TechType.None)
            {
                scanner = scanner ? scanner : mapRoom.GetComponentInChildren<uGUI_MapRoomScanner>(true);
                if (scanner)
                {
                    StartScanningThroughUi(scanner, desiredTechType);
                }
                else
                {
                    mapRoom.StartScanning(desiredTechType);
                }
                return;
            }

            scanner = scanner ? scanner : mapRoom.GetComponentInChildren<uGUI_MapRoomScanner>(true);
            if (scanner)
            {
                scanner.OnCancelScan();
            }
            // Keep the room canonical even if its UI is currently inactive and vanilla's cancel handler did nothing.
            mapRoom.typeToScan = TechType.None;
            ReplaceResourceNodes([]);
        }
        finally
        {
            applyingCanonicalScanState = false;
        }
    }

    private static void StartScanningThroughUi(uGUI_MapRoomScanner scanner, TechType desiredTechType)
    {
        // OnStartScan owns the list-to-scanning UI transition, but its integer is tied to the current page/list.
        // Temporarily make the canonical selection the unambiguous first entry. This also handles a state packet
        // arriving before the first authoritative resource catalog snapshot has populated the scanner list.
        List<TechType> sortedTechTypes = [.. scanner.sortedTechTypes];
        int currentPage = scanner.currentPage;
        try
        {
            scanner.sortedTechTypes.Clear();
            scanner.sortedTechTypes.Add(desiredTechType);
            scanner.currentPage = 0;
            scanner.OnStartScan(0);
        }
        finally
        {
            scanner.sortedTechTypes.Clear();
            scanner.sortedTechTypes.AddRange(sortedTechTypes);
            scanner.currentPage = currentPage;
        }
    }

    private void DiscardTargetsForMismatchedSelection(TechType selectedTechType)
    {
        CancelSnapshotPreparation();
        if (authoritativeSnapshot != null && SnapshotMatchesSelection(authoritativeSnapshot, selectedTechType))
        {
            return;
        }

        authoritativeSnapshot = null;
        authoritativeResourceNodes = [];
        ReplaceResourceNodes([]);
        NotifyResourceTrackerChanged();
        RefreshBlipsWithoutAdvancingScan();
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
            hasRollbackLocalIntent = false;
            if (previousMode == ScannerRoomResourceAuthorityMode.Rollback)
            {
                // A reconnect can move from a disabled server/session back to an authoritative one after an equal
                // state packet was deliberately ignored during rollback. Re-apply the accepted snapshot selection.
                canonicalScanState = snapshot!.ScanState;
                ApplyCanonicalScanState(canonicalScanState, requestSnapshot: false);
            }
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
        localIntentTracker.Clear();
        if (mapRoom)
        {
            CaptureRollbackLocalState();
        }
        Interlocked.Exchange(ref stateClearPending, 1);
        sessionJoined = false;
    }

    private void EnterPendingState()
    {
        if (!hasRollbackLocalIntent)
        {
            CaptureRollbackLocalState();
        }
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
        bool restoreScanProgress = hasRollbackLocalIntent;
        if (hasRollbackLocalIntent)
        {
            if (mapRoom.typeToScan != rollbackLocalSelection)
            {
                ApplyVanillaScanSelection(rollbackLocalSelection);
            }
            hasRollbackLocalIntent = false;
        }
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
        if (restoreScanProgress)
        {
            mapRoom.numNodesScanned = rollbackLocalScanProgress;
        }

        NotifyResourceTrackerChanged();
        RefreshBlipsWithoutAdvancingScan();
    }

    private void CaptureRollbackLocalState()
    {
        hasRollbackLocalIntent = true;
        rollbackLocalSelection = mapRoom.typeToScan;
        rollbackLocalScanProgress = mapRoom.numNodesScanned;
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

    private static bool ScanStatesEqual(ScannerRoomScanState left, ScannerRoomScanState right) =>
        left.Version == right.Version && SelectionsEqual(left.SelectedTechType, right.SelectedTechType);

    private static bool SelectionsEqual(NitroxTechType? left, NitroxTechType? right) =>
        ReferenceEquals(left, right) || left?.Equals(right) == true || left == null && right == null;

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
            scannerRoomManager.ScanStateChanged -= OnScanStateChanged;
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
