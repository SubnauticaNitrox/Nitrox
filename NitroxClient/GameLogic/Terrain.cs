using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxClient.Communication.Abstract;
using UnityEngine;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic
{
    public class Terrain
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;
        private readonly VisibleCells visibleCells;

        private bool cellsPendingSync;
        private float timeWhenCellsBecameOutOfSync;

        private List<NitroxModel.DataStructures.NitroxInt3> added = new List<NitroxModel.DataStructures.NitroxInt3>();
        private List<NitroxModel.DataStructures.NitroxInt3> removed = new List<NitroxModel.DataStructures.NitroxInt3>();

        public Terrain(IMultiplayerSession multiplayerSession, IPacketSender packetSender, VisibleCells visibleCells)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;
            this.visibleCells = visibleCells;
        }

        public void CellLoaded(Int3 batchId)
        {
            LargeWorldStreamer.main.StartCoroutine(WaitAndAddCell(batchId));
            MarkCellsReadyForSync(0.5f);
        }

        private IEnumerator WaitAndAddCell(Int3 batchId)
        {
            yield return new WaitForSeconds(0.5f);

            if (!visibleCells.Contains(batchId.ToDto()))
            {
                visibleCells.Add(batchId.ToDto());
                added.Add(batchId.ToDto());
            }
        }

        public void CellUnloaded(Int3 batchId)
        {
            if (visibleCells.Contains(batchId.ToDto()))
            {
                visibleCells.Remove(batchId.ToDto());
                removed.Add(batchId.ToDto());
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
    }
}
