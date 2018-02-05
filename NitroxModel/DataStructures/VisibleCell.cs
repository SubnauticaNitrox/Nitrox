using System;
using UnityEngine;

namespace NitroxModel.DataStructures
{
    [Serializable]
    public class VisibleCell
    {
        public AbsoluteEntityCell AbsoluteCellEntity { get; }

        public VisibleCell(Vector3 worldSpace, int level)
            : this(
                new AbsoluteEntityCell(worldSpace, level)
            )
        {
        }

        public VisibleCell(Int3 batchId, Int3 cellId, int level)
            : this(
                new AbsoluteEntityCell(batchId, cellId, level)
            )
        {
        }

        public VisibleCell(AbsoluteEntityCell absoluteCellEntity)
        {
            AbsoluteCellEntity = absoluteCellEntity;
        }

        public override string ToString()
        {
            return "[VisibleCell AbsoluteCellEntity: " + AbsoluteCellEntity + "]";
        }

        public override bool Equals(object obj)
        {
            // Check for null values and compare run-time types.
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            VisibleCell visibleCell = (VisibleCell)obj;

            return (visibleCell.AbsoluteCellEntity.Equals(AbsoluteCellEntity));
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 269;
                hash = hash * 23 + AbsoluteCellEntity.GetHashCode();
                return hash;
            }
        }
    }
}
