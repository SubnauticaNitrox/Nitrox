using System.Collections.Generic;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxClient.Map
{
    public class VisibleCells
    {
        private readonly HashSet<NitroxInt3> cells = new HashSet<NitroxInt3>();

        public void Add(NitroxInt3 cell)
        {
            lock (cells)
            {
                cells.Add(cell);
            }
        }

        public void Remove(NitroxInt3 cell)
        {
            lock (cells)
            {
                cells.Remove(cell);
            }
        }

        public bool Contains(NitroxInt3 cell)
        {
            lock (cells)
            {
                return cells.Contains(cell);
            }
        }
    }
}
