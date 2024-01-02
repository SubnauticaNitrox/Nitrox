using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using WorldStreaming;

namespace NitroxClient.GameLogic
{
    public class Terrain
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;
        private readonly VisibleCells visibleCells;
        private readonly VisibleBatches visibleBatches;

        private bool cellsPendingSync;
        private bool batchesPendingSync;
        private float bufferedTime = 0f;
        private float timeBuffer = 0.05f;

        private List<AbsoluteEntityCell> addedCells = new List<AbsoluteEntityCell>();
        private List<AbsoluteEntityCell> removedCells = new List<AbsoluteEntityCell>();
        private List<NitroxInt3> addedBatches = new List<NitroxInt3>();
        private List<NitroxInt3> removedBatches = new List<NitroxInt3>();

        public Terrain(IMultiplayerSession multiplayerSession, IPacketSender packetSender, VisibleCells visibleCells, VisibleBatches visibleBatches)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;
            this.visibleCells = visibleCells;
            this.visibleBatches = visibleBatches;
        }

        public void CellLoaded(Int3 batchId, Int3 cellId, int level)
        {
            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId.ToDto(), cellId.ToDto(), level);

            if (!visibleCells.Contains(cell))
            {
                visibleCells.Add(cell);
                addedCells.Add(cell);
                cellsPendingSync = true;
            }
        }

        public void CellUnloaded(Int3 batchId, Int3 cellId, int level)
        {
            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId.ToDto(), cellId.ToDto(), level);

            if (visibleCells.Contains(cell))
            {
                visibleCells.Remove(cell);
                removedCells.Add(cell);
                cellsPendingSync = true;
            }
        }
        public void BatchLoaded(Int3 batchId)
        {
            NitroxInt3 nitroxBatchId = batchId.ToDto();
            if (!visibleBatches.Contains(nitroxBatchId))
            {
                visibleBatches.Add(nitroxBatchId);
                addedBatches.Add(nitroxBatchId);
                batchesPendingSync = true;
            }
        }

        public void BatchUnloaded(Int3 batchId)
        {
            NitroxInt3 nitroxBatchId = batchId.ToDto();
            if (visibleBatches.Contains(nitroxBatchId))
            {
                visibleBatches.Remove(nitroxBatchId);
                removedBatches.Add(nitroxBatchId);
                batchesPendingSync = true;
            }
        }

        public void UpdateVisibility()
        {
            bufferedTime += Time.deltaTime;
            if (bufferedTime > timeBuffer)
            {
                if (cellsPendingSync)
                {
                    CellVisibilityChanged cellsChanged = new CellVisibilityChanged(multiplayerSession.Reservation.PlayerId, addedCells.ToArray(), removedCells.ToArray());
                    packetSender.Send(cellsChanged);

                    addedCells.Clear();
                    removedCells.Clear();

                    cellsPendingSync = false;
                }

                if (batchesPendingSync)
                {
                    BatchVisibilityChanged batchesChanged = new BatchVisibilityChanged(multiplayerSession.Reservation.PlayerId, addedBatches.ToArray(), removedBatches.ToArray());
                    packetSender.Send(batchesChanged);

                    addedBatches.Clear();
                    removedBatches.Clear();

                    batchesPendingSync = false;
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

            yield return new WaitUntil(() => LargeWorldStreamer.main.IsWorldSettled());
            Player.main.cinematicModeActive = false;
        }
    }
}
