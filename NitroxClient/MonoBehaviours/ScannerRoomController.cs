using System;
using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
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
    private readonly HashSet<TechType> authoritativeTechTypes = [];
    private MapRoomFunctionality mapRoom = null!;
    private NitroxId mapRoomId = null!;
    private ScannerRoomRequestTrigger? requestTrigger;
    private ScannerRoomManager scannerRoomManager = null!;
    private ScannerRoomSnapshot? authoritativeSnapshot;
    private ScannerRoomVirtualResourceCache<ResourceTrackerDatabase.ResourceInfo>? virtualResourceCache;
    private uGUI_ResourceTracker resourceTracker = null!;

    public bool HasAuthoritativeSnapshot => authoritativeSnapshot != null;

    public static ScannerRoomController Attach(MapRoomFunctionality mapRoom, NitroxId mapRoomId)
    {
        if (!mapRoom.TryGetComponent(out ScannerRoomController controller))
        {
            controller = mapRoom.gameObject.AddComponent<ScannerRoomController>();
        }

        controller.Initialize(mapRoom, mapRoomId);
        return controller;
    }

    public void RequestInitialSnapshot()
    {
        if (requestTrigger == null || !TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin))
        {
            return;
        }

        requestTrigger.TryRequestInitial(range, selectedTechType, observedOrigin);
    }

    public void RequestImmediateSnapshot()
    {
        if (requestTrigger == null || !TryGetRequestParameters(out float range, out NitroxTechType? selectedTechType, out NitroxVector3? observedOrigin))
        {
            return;
        }

        requestTrigger.RequestImmediate(range, selectedTechType, observedOrigin);
    }

    /// <summary>
    /// Replaces the scanner list with server-provided resource types and rebuilds the vanilla controls.
    /// </summary>
    /// <returns>Whether authoritative state was available and the vanilla local-database lookup should be skipped.</returns>
    public bool TryApplyAuthoritativeResourceList(uGUI_MapRoomScanner scanner)
    {
        if (authoritativeSnapshot == null)
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
    /// <returns>Whether authoritative state was available and the vanilla local-database lookup should be skipped.</returns>
    public bool TryApplyAuthoritativeResourceNodes(TechType requestedTechType)
    {
        if (authoritativeSnapshot == null || virtualResourceCache == null)
        {
            return false;
        }

        List<ResourceTrackerDatabase.ResourceInfo> replacementNodes = [];
        if (SnapshotMatchesSelection(authoritativeSnapshot, requestedTechType))
        {
            IReadOnlyList<ScannerRoomVirtualResource<ResourceTrackerDatabase.ResourceInfo>> virtualResources =
                virtualResourceCache.Resolve(authoritativeSnapshot.Targets);
            replacementNodes.Capacity = virtualResources.Count;

            foreach (ScannerRoomVirtualResource<ResourceTrackerDatabase.ResourceInfo> virtualResource in virtualResources)
            {
                virtualResource.Resource.techType = requestedTechType;
                virtualResource.Resource.position = virtualResource.Target.Position.ToUnity();
                replacementNodes.Add(virtualResource.Resource);
            }
        }

        mapRoom.resourceNodes.Clear();
        foreach (ResourceTrackerDatabase.ResourceInfo resourceNode in replacementNodes)
        {
            mapRoom.resourceNodes.Add(resourceNode);
        }

        if (!resourceTracker)
        {
            resourceTracker = UnityEngine.Object.FindObjectOfType<uGUI_ResourceTracker>();
        }
        if (resourceTracker)
        {
            resourceTracker.gatherNextTick = true;
        }
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
        requestTrigger = new ScannerRoomRequestTrigger(
            (range, selectedTechType, observedOrigin) => scannerRoomManager.RequestSnapshot(mapRoomId, range, selectedTechType, observedOrigin));

        if (scannerRoomManager.TryGetSnapshot(mapRoomId, out ScannerRoomSnapshot? snapshot))
        {
            ApplySnapshot(snapshot!);
        }
    }

    private void Start() => RequestInitialSnapshot();

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

    private void OnSnapshotChanged(NitroxId changedMapRoomId, ScannerRoomSnapshotApplyResult result)
    {
        if (!mapRoomId.Equals(changedMapRoomId) || result != ScannerRoomSnapshotApplyResult.Applied)
        {
            return;
        }

        if (scannerRoomManager.TryGetSnapshot(mapRoomId, out ScannerRoomSnapshot? snapshot))
        {
            ApplySnapshot(snapshot!);
        }
    }

    private void ApplySnapshot(ScannerRoomSnapshot snapshot)
    {
        HashSet<TechType> nextTechTypes = GetAvailableTechTypes(snapshot.AvailableResources);
        bool catalogChanged = authoritativeSnapshot == null || !authoritativeTechTypes.SetEquals(nextTechTypes);

        authoritativeSnapshot = snapshot;
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

    private void OnDestroy()
    {
        if (scannerRoomManager != null)
        {
            scannerRoomManager.SnapshotChanged -= OnSnapshotChanged;
        }
    }
}
