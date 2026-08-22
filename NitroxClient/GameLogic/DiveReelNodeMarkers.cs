using System.Collections;
using System.Collections.Generic;
using Nitrox.Model.DataStructures.Unity;
using NitroxClient.Extensions;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxClient.MonoBehaviours;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic;

/// <summary>
/// Owns every visual-only DiveReel (Pathfinder Tool) node marker spawned on this client to
/// represent OTHER players' trails -- never the local player's own nodes, which the base
/// game's own DiveReel already renders normally. Tracks spawned markers per player so a
/// DiveReelNodesReset for that player can clean them all up, and so a player disconnecting
/// doesn't leave orphaned markers behind (handled by subscribing to PlayerManager.OnRemove
/// below, the same pattern BulletManager uses for its own per-player stasis spheres).
/// </summary>
public class DiveReelNodeMarkers
{
    private readonly PlayerManager playerManager;
    private readonly Dictionary<ushort, List<GameObject>> markersByPlayer = new();

    public DiveReelNodeMarkers(PlayerManager playerManager)
    {
        this.playerManager = playerManager;

        // Mirrors BulletManager's playerManager.OnRemove subscription (BulletManager.cs) for cleaning
        // up per-player decorative GameObjects on disconnect. We subscribe here (rather than having
        // PlayerManager call ClearMarkers directly) because PlayerManager is already a constructor
        // dependency of this class -- the reverse dependency would be an unconstructable DI cycle.
        playerManager.OnRemove += (playerId, _) => ClearMarkers(playerId);
    }

    public void SpawnMarker(ushort playerId, NitroxVector3 position)
    {
        TaskResult<GameObject> result = new();
        CoroutineHost.StartCoroutine(SpawnMarkerAsync(playerId, position.ToUnity(), result));
    }

    private IEnumerator SpawnMarkerAsync(ushort playerId, Vector3 position, TaskResult<GameObject> prefabResult)
    {
        yield return DefaultWorldEntitySpawner.RequestPrefab(TechType.DiveReel, prefabResult);
        GameObject toolPrefab = prefabResult.Get();
        if (!toolPrefab || !toolPrefab.TryGetComponent(out DiveReel diveReel) || !diveReel.nodePrefab)
        {
            Log.Warn($"[DiveReelNodeMarkers] Could not resolve a DiveReelNode prefab to spawn a marker for player {playerId}.");
            yield break;
        }

        // The player's RemotePlayer object may not exist client-side YET even though they are
        // genuinely connected: DiveReelNodesInitialSync (a regular packet, processed through the
        // normal packet queue) can be processed before RemotePlayerInitialSyncProcessor -- a
        // separate step in the client's own InitialSync pipeline, and the thing that actually
        // constructs RemotePlayer objects for already-connected players -- has had a chance to run.
        // This is most likely during the burst of spawns right after a fresh join, when both
        // mechanisms are racing. A single check-and-bail (the original form of this guard) cannot
        // tell "genuinely disconnected" apart from "connected, but not created here yet" -- and
        // confirmed live 2026-08-19, every marker in an InitialSync burst was silently skipped this
        // way, making resync appear to do nothing even though the server correctly sent the data.
        //
        // Retry for a bounded number of frames instead. RemotePlayerInitialSyncProcessor typically
        // completes within a fraction of a second of InitialPlayerSync being received, so this is a
        // generous margin in the common (connected) case, not a real wait. If the RemotePlayer
        // genuinely never appears (actually disconnected, e.g. mid-spawn while we were waiting on
        // RequestPrefab, which can itself span multiple frames the first time TechType.DiveReel is
        // requested in a session) we still give up and must not add this marker to markersByPlayer,
        // or it leaks for the rest of the session -- ClearMarkers already ran and no-opped, since
        // nothing was registered for this in-flight spawn yet, and no further OnRemove will fire.
        const int maxWaitFrames = 90;
        int framesWaited = 0;
        while (!playerManager.TryFind(playerId, out _))
        {
            framesWaited++;
            if (framesWaited >= maxWaitFrames)
            {
                Log.Info($"[DiveReelNodeMarkers] Skipping marker spawn for player {playerId} at {position}: no RemotePlayer found after waiting {framesWaited} frames (disconnected, or never created client-side).");
                yield break;
            }
            yield return null;
        }

        GameObject marker = Object.Instantiate(diveReel.nodePrefab, position, Quaternion.identity);
        // Real nodes (DiveReel.CreateNewNode) and decorative markers are instantiated from the same
        // prefab with no parent, so DiveReelNode_Start_LocalTint_Patch (a Harmony postfix on
        // DiveReelNode.Start(), which fires for every instance regardless of origin) needs this tag
        // to tell them apart, or it clobbers this marker's own per-player tint below with the local
        // player's own color. Added before Start() can possibly run (synchronously, same as
        // TintMarker below -- Start() is always deferred to a later Unity lifecycle step).
        marker.AddComponent<NitroxDiveReelNodeMarkerTag>();
        TintMarker(marker, playerId);

        if (!markersByPlayer.TryGetValue(playerId, out List<GameObject> markers))
        {
            markers = new List<GameObject>();
            markersByPlayer[playerId] = markers;
        }
        markers.Add(marker);
        Log.Info($"[DiveReelNodeMarkers] Spawned marker for player {playerId} at {position} -> now {markers.Count} marker(s) for this player.");
    }

    private void TintMarker(GameObject marker, ushort playerId)
    {
        if (!playerManager.TryFind(playerId, out RemotePlayer remotePlayer))
        {
            return;
        }
        Color playerColor = remotePlayer.PlayerSettings.PlayerColor.ToUnity();

        // DiveReelNode (reference/decompiled/Assembly-CSharp/DiveReelNode.cs) has no public renderer or
        // material of its own to tint. It privately resolves a MeshRenderer via
        // GetComponentInChildren<MeshRenderer>() on whichever of its "arrow"/"firstNodeHolder" transforms
        // is in use, and tints it through the plain "_Color" shader property (DiveReelNode.cs:118-121) --
        // NOT "_GlowColor", which is only used by DiveReel.cs's own held-tool model (a different prefab
        // entirely, tinted via its SkinnedMeshRenderer at DiveReel.cs:93/181-182).
        //
        // We tint every MeshRenderer under the marker (including inactive ones -- firstNodeHolder vs.
        // standardNodeHolder aren't toggled active/inactive until DiveReelNode.Start() runs) with the same
        // "_Color" property, and do it synchronously right after Instantiate so DiveReelNode.Start() (which
        // runs on a later Unity lifecycle step, after this call returns) captures our tint as its own
        // "baseColor" field. That ordering makes our tint survive DiveReelNode.Update()'s per-frame Lerp
        // back toward baseColor instead of being fought/reverted a moment later.
        foreach (MeshRenderer meshRenderer in marker.GetComponentsInChildren<MeshRenderer>(true))
        {
            meshRenderer.material.SetColor(ShaderPropertyID._Color, playerColor);
        }

        // DiveReelNode.light (DiveReelNode.cs:23, [AssertNotNull] public Light) is a prefab-wired
        // serialized reference, not computed at runtime -- unlike arrowMat above, it's already valid
        // immediately after Instantiate, no Start()-timing concerns. Nothing in DiveReelNode ever sets
        // its color, so every marker's emitted light was always whatever the prefab's default is,
        // regardless of player color.
        if (marker.TryGetComponentInChildren(out DiveReelNode diveReelNode, true) && diveReelNode.light)
        {
            diveReelNode.light.color = playerColor;
        }
    }

    public void ClearMarkers(ushort playerId)
    {
        if (!markersByPlayer.TryGetValue(playerId, out List<GameObject> markers))
        {
            return;
        }
        foreach (GameObject marker in markers)
        {
            if (marker)
            {
                Object.Destroy(marker);
            }
        }
        markersByPlayer.Remove(playerId);
    }
}
