using NitroxModel.DataStructures;
using NitroxModel.Packets.Processors.Abstract;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxServer
{
    public class Player : IProcessorContext
    {
        public String Id { get; }
        public Vector3 Position { get; set; }

        private HashSet<VisibleCell> visibleCells;

        public Player(String id)
        {
            this.Id = id;
            this.visibleCells = new HashSet<VisibleCell>();
        }

        public void AddCells(IEnumerable<VisibleCell> cells)
        {
            lock (visibleCells)
            {
                foreach (VisibleCell cell in cells)
                {
                    visibleCells.Add(cell);
                }
            }
        }

        public void RemoveCells(IEnumerable<VisibleCell> cells)
        {
            lock (visibleCells)
            {
                foreach (VisibleCell cell in cells)
                {
                    visibleCells.Remove(cell);
                }
            }
        }

        public bool HasCellLoaded(VisibleCell cell)
        {
            lock (visibleCells)
            {
                return visibleCells.Contains(cell);
            }
        }
    }
}
