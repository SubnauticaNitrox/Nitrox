using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.Packets;
using Nitrox.Model.Subnautica.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.Packets;
using UnityEngine;
using WorldStreaming;

namespace NitroxClient.GameLogic;

public class Terrain
{
    private readonly IMultiplayerSession multiplayerSession;
    private readonly IPacketSender packetSender;

    private readonly HashSet<AbsoluteEntityCell> visibleCells = [];
    private readonly List<AbsoluteEntityCell> addedCells = [];
    private readonly List<AbsoluteEntityCell> removedCells = [];
    // We use a tuple rather than AbsoluteEntityCell for lightweight fetching by IsCellFullySpawned
    private readonly HashSet<(Int3, Int3, int)> fullySpawnedCells = [];

    private bool cellsPendingSync;
    private float bufferedTime = 0f;
    private const float TIME_BUFFER = 0.05f;

    public Terrain(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
    {
        this.multiplayerSession = multiplayerSession;
        this.packetSender = packetSender;
    }

    public void CellLoaded(Int3 batchId, Int3 cellId, int level)
    {
        AbsoluteEntityCell cell = new(batchId.ToDto(), cellId.ToDto(), level);

        if (!visibleCells.Contains(cell))
        {
            removedCells.Remove(cell);
            visibleCells.Add(cell);
            addedCells.Add(cell);
            cellsPendingSync = true;
        }
    }

    public void CellUnloaded(Int3 batchId, Int3 cellId, int level)
    {
        AbsoluteEntityCell cell = new(batchId.ToDto(), cellId.ToDto(), level);

        if (visibleCells.Contains(cell))
        {
            visibleCells.Remove(cell);
            removedCells.Add(cell);
            cellsPendingSync = true;
            fullySpawnedCells.Remove((batchId, cellId, level));
        }
    }

    public void AddFullySpawnedCell(AbsoluteEntityCell cell)
    {
        if (visibleCells.Contains(cell))
        {
            fullySpawnedCells.Add((cell.BatchId.ToUnity(), cell.CellId.ToUnity(), cell.Level));
        }
    }

    public bool IsCellFullySpawned(Int3 batchId, Int3 cellId, int level)
    {
        return fullySpawnedCells.Contains((batchId, cellId, level));
    }

    public void UpdateVisibility()
    {
        bufferedTime += Time.deltaTime;
        if (bufferedTime >= TIME_BUFFER)
        {
            if (cellsPendingSync)
            {
                CellVisibilityChanged cellsChanged = new(multiplayerSession.Reservation.PlayerId, addedCells, removedCells);
                packetSender.Send(cellsChanged);

                addedCells.Clear();
                removedCells.Clear();

                cellsPendingSync = false;
            }

            bufferedTime = 0f;
        }
    }

    /// <summary>
    /// Forces world streamer's to load the terrain around the MainCamera and waits until it's done to unfreeze the player.
    /// </summary>
    public static IEnumerator WaitForWorldLoad()
    {
        // In WorldStreamer.CreateStreamers() three coroutines are created to constantly call UpdateCenter() on the streamers
        // We force these updates so that the world streamer gets busy instantly
        WorldStreamer streamerV2 = LargeWorldStreamer.main.streamerV2;
        streamerV2.UpdateStreamingCenter(MainCamera.camera.transform.position);
        streamerV2.octreesStreamer.UpdateCenter(streamerV2.streamingCenter);
        streamerV2.lowDetailOctreesStreamer.UpdateCenter(streamerV2.streamingCenter);
        streamerV2.clipmapStreamer.UpdateCenter(streamerV2.streamingCenter);

        yield return new WaitUntil(LargeWorldStreamer.main.IsWorldSettled);
        Player.main.cinematicModeActive = false;
    }

    public static IEnumerator SafeWaitForWorldLoad()
    {
        yield return WaitForWorldLoad().OnYieldError(e =>
        {
            Player.main.cinematicModeActive = false;
            Log.Warn($"Something wrong happened while waiting for the terrain to load.\n{e}");
        });
    }
}
