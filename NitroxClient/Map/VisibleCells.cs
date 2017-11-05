using System.Collections.Generic;
using NitroxModel.DataStructures;

namespace NitroxClient.Map
{
    public class VisibleCells
    {
        private readonly HashSet<VisibleCell> cells = new HashSet<VisibleCell>();

        public void Add(VisibleCell cell)
        {
            lock (cells)
            {
                cells.Add(cell);
            }
        }

        public void Remove(VisibleCell cell)
        {
            lock (cells)
            {
                cells.Remove(cell);
            }
        }

        public bool Contains(VisibleCell cell)
        {
            lock (cells)
            {
                return cells.Contains(cell);
            }
        }

        public bool HasVisibleCell(VisibleCell cell)
        {
            lock (cells)
            {
                return cells.Contains(cell);
            }
        }
    }
}
