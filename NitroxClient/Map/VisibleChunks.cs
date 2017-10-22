using NitroxModel.DataStructures;
using System.Collections.Generic;

namespace NitroxClient.Map
{
    public class VisibleCells
    {
        private HashSet<VisibleCell> cells = new HashSet<VisibleCell>();

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
