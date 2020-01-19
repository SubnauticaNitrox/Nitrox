using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.DataStructures
{
    public struct CellRoot
    {
        public Int3 CellId;
        public Int3 BatchId;
        public int Level;

        public override bool Equals(object obj)
        {
            CellRoot cellRoot = (CellRoot)obj;
            return CellId.Equals(cellRoot.CellId) && BatchId.Equals(cellRoot.BatchId) && Level.Equals(cellRoot.Level);
        }

        public override int GetHashCode()
        {
            var hashCode = -1821606989;
            hashCode = hashCode * -1521134295 + EqualityComparer<Int3>.Default.GetHashCode(CellId);
            hashCode = hashCode * -1521134295 + EqualityComparer<Int3>.Default.GetHashCode(BatchId);
            hashCode = hashCode * -1521134295 + Level.GetHashCode();
            return hashCode;
        }
    }
}
