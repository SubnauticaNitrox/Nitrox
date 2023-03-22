using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.Map;
using NitroxClient.Unity.Helper;
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
        private float bufferedTime = 0f;
        private float timeBuffer = 0.05f;

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
            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId.ToDto(), cellId.ToDto(), level);

            if (!visibleCells.Contains(cell))
            {
                visibleCells.Add(cell);
                added.Add(cell);
                cellsPendingSync = true;
            }
        }

        public void CellUnloaded(Int3 batchId, Int3 cellId, int level)
        {
            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId.ToDto(), cellId.ToDto(), level);

            if (visibleCells.Contains(cell))
            {
                visibleCells.Remove(cell);
                removed.Add(cell);
                cellsPendingSync = true;
            }
        }

        public void UpdateVisibility()
        {
            bufferedTime += Time.deltaTime;
            if (bufferedTime > timeBuffer)
            {
                if (cellsPendingSync)
                {
                    CellVisibilityChanged cellsChanged = new CellVisibilityChanged(multiplayerSession.Reservation.PlayerId, added.ToArray(), removed.ToArray());
                    packetSender.Send(cellsChanged);

                    added.Clear();
                    removed.Clear();

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

            yield return new WaitUntil(() => LargeWorldStreamer.main.IsWorldSettled());
            Player.main.cinematicModeActive = false;
        }
    }
}
