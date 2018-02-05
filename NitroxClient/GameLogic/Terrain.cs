using NitroxClient.Communication;
using NitroxClient.Map;
using NitroxModel.DataStructures;
using NitroxModel.Logger;
using NitroxModel.Packets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.GameLogic
{
    public class Terrain
    {
        private IPacketSender packetSender;
        private VisibleCells visibleCells;
        private DeferringPacketReceiver packetReceiver;

        private bool cellsPendingSync = false;
        private float timeWhenCellsBecameOutOfSync;

        private List<AbsoluteEntityCell> added = new List<AbsoluteEntityCell>();
        private List<AbsoluteEntityCell> removed = new List<AbsoluteEntityCell>();

        public Terrain(IPacketSender packetSender, VisibleCells visibleCells, DeferringPacketReceiver packetReceiver)
        {
            this.packetSender = packetSender;
            this.visibleCells = visibleCells;
            this.packetReceiver = packetReceiver;
        }

        public void CellLoaded(Int3 batchId, Int3 cellId, int level)
        {
            LargeWorldStreamer.main.StartCoroutine(WaitAndAddCell(batchId, cellId, level));
            markCellsReadyForSync(0.5f);
        }
        
        private IEnumerator WaitAndAddCell(Int3 batchId, Int3 cellId, int level)
        {
            yield return new WaitForSeconds(0.5f);

            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId, cellId, level);

            if (!visibleCells.Contains(cell))
            {
                visibleCells.Add(cell);
                added.Add(cell);
                packetReceiver.CellLoaded(cell);
            }            
        }

        public void CellUnloaded(Int3 batchId, Int3 cellId, int level)
        {
            AbsoluteEntityCell cell = new AbsoluteEntityCell(batchId, cellId, level);

            if (visibleCells.Contains(cell))
            {
                visibleCells.Remove(cell);
                removed.Add(cell);
                markCellsReadyForSync(0);
            }     
        }

        private void markCellsReadyForSync(float delay)
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
                    CellVisibilityChanged cellsChanged = new CellVisibilityChanged(packetSender.PlayerId, added.ToArray(), removed.ToArray());
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
