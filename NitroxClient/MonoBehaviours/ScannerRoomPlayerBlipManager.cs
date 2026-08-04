using System.Collections.Generic;
using System.Linq;
using Nitrox.Model.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ScannerRooms;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Owns remote-player hologram blips for one Scanner Room. Vanilla camera-drone blips remain owned exclusively by
/// <see cref="MapRoomFunctionality"/>.
/// </summary>
public sealed class ScannerRoomPlayerBlipManager : MonoBehaviour
{
    private readonly Dictionary<SessionId, PlayerBlipInstance> blipsBySessionId = [];
    private MapRoomFunctionality mapRoom = null!;
    private bool blipPrefabIsInvalid;

    public static ScannerRoomPlayerBlipManager GetOrCreate(MapRoomFunctionality mapRoom)
    {
        if (!mapRoom.TryGetComponent(out ScannerRoomPlayerBlipManager manager))
        {
            manager = mapRoom.gameObject.AddComponent<ScannerRoomPlayerBlipManager>();
        }

        manager.mapRoom = mapRoom;
        return manager;
    }

    public void Refresh(IEnumerable<RemotePlayer> remotePlayers)
    {
        List<RemotePlayer> players = remotePlayers.ToList();
        HashSet<SessionId> connectedSessionIds = new(players.Select(player => player.SessionId));

        RemoveDisconnectedPlayers(connectedSessionIds);

        List<ScannerRoomPlayerLocation> playerLocations = players
                                                          .Where(player => player.Body != null)
                                                          .Select(player => new ScannerRoomPlayerLocation(player.SessionId, player.PlayerName, player.Body!.transform.position))
                                                          .ToList();

        IReadOnlyList<ScannerRoomPlayerBlip> projectedPlayers = ScannerRoomPlayerBlipProjector.Project(
            playerLocations,
            mapRoom.wireFrameWorld.position,
            mapRoom.cameraBlipRoot.transform.position,
            mapRoom.scanRange,
            mapRoom.mapScale);

        HashSet<SessionId> visibleSessionIds = new(projectedPlayers.Select(player => player.SessionId));
        foreach (PlayerBlipInstance blip in blipsBySessionId.Where(pair => !visibleSessionIds.Contains(pair.Key)).Select(pair => pair.Value))
        {
            if (blip.GameObject)
            {
                blip.GameObject.SetActive(false);
            }
        }

        foreach (ScannerRoomPlayerBlip projectedPlayer in projectedPlayers)
        {
            PlayerBlipInstance? blip = GetOrCreateBlip(projectedPlayer.SessionId);
            if (blip == null)
            {
                continue;
            }

            blip.GameObject.name = $"Nitrox player blip - {projectedPlayer.PlayerName}";
            blip.GameObject.transform.localPosition = projectedPlayer.LocalPosition;
            blip.CameraBlip.cameraName.text = projectedPlayer.PlayerName;
            blip.GameObject.SetActive(true);
        }
    }

    private PlayerBlipInstance? GetOrCreateBlip(SessionId sessionId)
    {
        if (blipsBySessionId.TryGetValue(sessionId, out PlayerBlipInstance existingBlip) && existingBlip.GameObject)
        {
            return existingBlip;
        }

        if (blipPrefabIsInvalid)
        {
            return null;
        }

        if (!mapRoom.cameraBlipPrefab || !mapRoom.cameraBlipRoot)
        {
            blipPrefabIsInvalid = true;
            Log.Error($"[{nameof(ScannerRoomPlayerBlipManager)}] {nameof(MapRoomFunctionality)} is missing its camera blip prefab or root");
            return null;
        }

        MapRoomCameraBlip cameraBlip = Instantiate(mapRoom.cameraBlipPrefab, mapRoom.cameraBlipRoot.transform, false);
        if (!cameraBlip || cameraBlip.cameraName == null)
        {
            if (cameraBlip)
            {
                Destroy(cameraBlip.gameObject);
            }
            blipPrefabIsInvalid = true;
            Log.Error($"[{nameof(ScannerRoomPlayerBlipManager)}] {nameof(MapRoomFunctionality)}.{nameof(MapRoomFunctionality.cameraBlipPrefab)} is missing a usable {nameof(MapRoomCameraBlip)}");
            return null;
        }
        GameObject blipObject = cameraBlip.gameObject;
        blipObject.transform.localRotation = Quaternion.identity;

        // Player blips are informational and must not be selectable as camera drones.
        foreach (Collider blipCollider in blipObject.GetComponentsInChildren<Collider>(true))
        {
            blipCollider.enabled = false;
        }

        PlayerBlipInstance newBlip = new(blipObject, cameraBlip);
        blipsBySessionId[sessionId] = newBlip;
        return newBlip;
    }

    private void RemoveDisconnectedPlayers(ISet<SessionId> connectedSessionIds)
    {
        foreach (SessionId disconnectedSessionId in blipsBySessionId.Keys.Where(sessionId => !connectedSessionIds.Contains(sessionId)).ToList())
        {
            PlayerBlipInstance disconnectedBlip = blipsBySessionId[disconnectedSessionId];
            if (disconnectedBlip.GameObject)
            {
                Destroy(disconnectedBlip.GameObject);
            }
            blipsBySessionId.Remove(disconnectedSessionId);
        }
    }

    private void OnDestroy()
    {
        foreach (PlayerBlipInstance blip in blipsBySessionId.Values)
        {
            if (blip.GameObject)
            {
                Destroy(blip.GameObject);
            }
        }
        blipsBySessionId.Clear();
    }

    private sealed record PlayerBlipInstance(GameObject GameObject, MapRoomCameraBlip CameraBlip);
}
