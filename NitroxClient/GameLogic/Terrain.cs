using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Map;
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

        private bool cellsPendingSync;
        private float timeWhenCellsBecameOutOfSync;

        private List<AbsoluteEntityCell> added = new List<AbsoluteEntityCell>();
        private List<AbsoluteEntityCell> removed = new List<AbsoluteEntityCell>();

        public Terrain(IMultiplayerSession multiplayerSession, IPacketSender packetSender, VisibleCells visibleCells)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;
            this.visibleCells = visibleCells;
        }

        public void CellLoaded(Int3 batchId, Int3 cellId, int level)
        {
            LargeWorldStreamer.main.StartCoroutine(WaitAndAddCell(batchId, cellId, level));
            MarkCellsReadyForSync(0.5f);
        }

        private IEnumerator WaitAndAddCell(Int3 batchId, Int3 cellId, int level)
        {
            yield return new WaitForSeconds(0.5f);

            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId.ToDto(), cellId.ToDto(), level);

            if (!visibleCells.Contains(cell))
            {
                visibleCells.Add(cell);
                added.Add(cell);
            }
        }

        public void CellUnloaded(Int3 batchId, Int3 cellId, int level)
        {
            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId.ToDto(), cellId.ToDto(), level);

            if (visibleCells.Contains(cell))
            {
                visibleCells.Remove(cell);
                removed.Add(cell);
                MarkCellsReadyForSync(0);
            }
        }

        private void MarkCellsReadyForSync(float delay)
        {
            if (cellsPendingSync == false)
            {
                timeWhenCellsBecameOutOfSync = Time.time;
                LargeWorldStreamer.main.StartCoroutine(WaitAndSyncCells(delay));
                cellsPendingSync = true;
            }
        }

        private IEnumerator WaitAndSyncCells(float delay)
        {
            yield return new WaitForSeconds(delay);

            while (cellsPendingSync)
            {
                float currentTime = Time.time;
                float elapsed = currentTime - timeWhenCellsBecameOutOfSync;

                if (elapsed >= 0.1)
                {
                    CellVisibilityChanged cellsChanged = new CellVisibilityChanged(multiplayerSession.Reservation.PlayerId, added.ToArray(), removed.ToArray());
                    packetSender.Send(cellsChanged);

                    added.Clear();
                    removed.Clear();

                    cellsPendingSync = false;
                    yield break;
                }

                yield return new WaitForSeconds(0.05f);
            }
        }

        public IEnumerator WaitForWorldLoad()
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
