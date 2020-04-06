using System;
using NitroxModel.Helper;
using ProtoBufNet;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class AbsoluteEntityCell
    {
        [ProtoMember(1)]
        public Int3 BatchId { get; }

        [ProtoMember(2)]
        public Int3 CellId { get; }

        [ProtoMember(3)]
        public int Level { get; }

        private Int3 BatchPosition => BatchId * Map.Main.BatchSize - Map.Main.BatchDimensionCenter;
        public Int3 Position => BatchPosition + CellId * GetCellSize();

        public Int3 Center
        {
            get
            {
                Int3 cellSize = GetCellSize();
                return BatchPosition + CellId * cellSize + (cellSize >> 1);
            }
        }

        public AbsoluteEntityCell()
        {
            // For serialization 
        }

        public AbsoluteEntityCell(Int3 batchId, Int3 cellId, int level)
        {
            BatchId = batchId;
            CellId = cellId;
            Level = level;
        }

        public AbsoluteEntityCell(Vector3 worldSpace, int level)
        {
            Level = level;

            Vector3 localPosition = (worldSpace + Map.Main.BatchDimensionCenter.ToVector3()) / Map.Main.BatchSize;
            BatchId = Int3.Floor(localPosition);

            Vector3 cell = (localPosition - BatchId.ToVector3()) * GetCellsPerBlock();
            CellId = Int3.Floor(new Vector3(cell.x + 0.0001f, cell.y + 0.0001f, cell.z + 0.0001f));
        }

        public static bool operator ==(AbsoluteEntityCell left, AbsoluteEntityCell right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AbsoluteEntityCell left, AbsoluteEntityCell right)
        {
            return !Equals(left, right);
        }

        public static Int3 GetCellSize(int level, Int3 blocksPerBatch)
        {
            // Our own implementation for BatchCells.GetCellSize, that works on the server and client.
            return blocksPerBatch / GetCellsPerBlock(level);
        }

        public static int GetCellsPerBlock(int level)
        {
            switch (level)
            {
                case 0:
                    return 10;
                case 1:
                case 2:
                case 3:
                    return 5;
                default:
                    throw new Exception($"Given level '{level}' does not have any defined cells.");
            }
        }

        public override string ToString()
        {
            return "[AbsoluteEntityCell Position: " + Position + " BatchId: " + BatchId + " CellId: " + CellId + " Level: " + Level + " ]";
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != GetType())
            {
                return false;
            }
            return Equals((AbsoluteEntityCell)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = BatchId != null ? BatchId.GetHashCode() : 0;
                hash = (hash * 397) ^ (CellId != null ? CellId.GetHashCode() : 0);
                hash = (hash * 397) ^ Level;
                return hash;
            }
        }

        public Int3 GetCellSize()
        {
            return GetCellSize(Map.Main.BatchDimensions);
        }

        public Int3 GetCellSize(Int3 blocksPerBatch)
        {
            return GetCellSize(Level, blocksPerBatch);
        }

        public int GetCellsPerBlock()
        {
            return GetCellsPerBlock(Level);
        }

        protected bool Equals(AbsoluteEntityCell other)
        {
            return Equals(BatchId, other.BatchId) && Equals(CellId, other.CellId) && Level == other.Level;
        }
    }
}
