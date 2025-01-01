using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Map;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;
using WorldStreaming;

namespace NitroxClient.GameLogic;

public class Terrain
{
    private readonly IMultiplayerSession multiplayerSession;
    private readonly IPacketSender packetSender;
    private readonly VisibleCells visibleCells;
    private readonly VisibleBatches visibleBatches;

    private readonly List<AbsoluteEntityCell> addedCells = [];
    private readonly List<AbsoluteEntityCell> removedCells = [];

    private bool cellsPendingSync;
    private float bufferedTime = 0f;
    private const float TIME_BUFFER = 0.05f;

    public Terrain(IMultiplayerSession multiplayerSession, IPacketSender packetSender, VisibleCells visibleCells, VisibleBatches visibleBatches)
    {
        this.multiplayerSession = multiplayerSession;
        this.packetSender = packetSender;
        this.visibleCells = visibleCells;
        this.visibleBatches = visibleBatches;
    }

    public void CellLoaded(Int3 batchId, Int3 cellId, int level)
    {
        AbsoluteEntityCell cell = new(batchId.ToDto(), cellId.ToDto(), level);

        if (!visibleCells.Contains(cell))
        {
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
        }
    }

    public void UpdateVisibility()
    {
        bufferedTime += Time.deltaTime;
        if (bufferedTime >= TIME_BUFFER)
        {
            if (cellsPendingSync)
            {
                CellVisibilityChanged cellsChanged = new(multiplayerSession.Reservation.PlayerId, addedCells.ToArray(), removedCells.ToArray());
                packetSender.Send(cellsChanged);

                addedCells.Clear();
                removedCells.Clear();

                cellsPendingSync = false;
            }

            bufferedTime = 0f;
        }

    }

    public static void WaitForEntities()
    {
        // In case the player is spawned in the air, we need to hold them up while all the entities load around them
        if (Player.main && !Player.main.IsUnderwater() && !Player.main.groundMotor.grounded)
        {
            Player.main.cinematicModeActive = true;
            CoroutineHost.StartCoroutine(WaitForWorldLoad(Yielders.WaitFor2Seconds));
        }
    }

    /// <summary>
    /// Forces world streamer's to load the terrain around the MainCamera and waits until it's done to unfreeze the player.
    /// </summary>
    public static IEnumerator WaitForWorldLoad(WaitForSeconds initialWait = null)
    {
        Entities entities = NitroxServiceLocator.LocateService<Entities>();

        // In WorldStreamer.CreateStreamers() three coroutines are created to constantly call UpdateCenter() on the streamers
        // We force these updates so that the world streamer gets busy instantly
        WorldStreamer streamerV2 = LargeWorldStreamer.main.streamerV2;
        streamerV2.UpdateStreamingCenter(MainCamera.camera.transform.position);
        streamerV2.octreesStreamer.UpdateCenter(streamerV2.streamingCenter);
        streamerV2.lowDetailOctreesStreamer.UpdateCenter(streamerV2.streamingCenter);
        streamerV2.clipmapStreamer.UpdateCenter(streamerV2.streamingCenter);

        yield return initialWait;

        yield return new WaitUntil(() => LargeWorldStreamer.main.IsWorldSettled() && entities.EntitiesToSpawn.Count == 0);
        Player.main.cinematicModeActive = false;
    }
}
