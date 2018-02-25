using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.Map
{
    public class VisibleCells
    {
        private readonly HashSet<AbsoluteEntityCell> cells = new HashSet<AbsoluteEntityCell>();

        public void Add(AbsoluteEntityCell cell)
        {
            lock (cells)
            {
                cells.Add(cell);
            }
        }

        public void Remove(AbsoluteEntityCell cell)
        {
            lock (cells)
            {
                cells.Remove(cell);
            }
        }

        public bool Contains(AbsoluteEntityCell cell)
        {
            lock (cells)
            {
                return cells.Contains(cell);
            }
        }
    }
}
