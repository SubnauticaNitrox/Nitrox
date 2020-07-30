using System;
using System.Collections.Generic;
using NitroxModel.Helper;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public sealed class AbsoluteEntityCell : IEquatable<AbsoluteEntityCell>, IEqualityComparer<AbsoluteEntityCell>
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

        public AbsoluteEntityCell(Int3 batchId, Int3 cellId, int level)
        {
            BatchId = batchId;
            CellId = cellId;
            Level = level;
        }

        public AbsoluteEntityCell(NitroxVector3 worldSpace, int level)
        {
            Level = level;
            CellId = null;

            NitroxVector3 localPosition = (worldSpace + Map.Main.BatchDimensionCenter) / Map.Main.BatchSize;
            BatchId = Int3.Floor(localPosition);

            NitroxVector3 cell = (localPosition - BatchId) * GetCellsPerBlock();
            CellId = Int3.Floor(new NitroxVector3(cell.X + 0.0001f, cell.Y + 0.0001f, cell.Z + 0.0001f));
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
                    throw new ArgumentOutOfRangeException($"Given level '{level}' does not have any defined cells.");
            }
        }

        public override string ToString()
        {
            return "[AbsoluteEntityCell Position: " + Position + " BatchId: " + BatchId + " CellId: " + CellId + " Level: " + Level + " ]";
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

        public bool Equals(AbsoluteEntityCell other)
        {
            return Equals(this, other);
        }

        public bool Equals(AbsoluteEntityCell x, AbsoluteEntityCell y)
        {
            return
            x.BatchId.Equals(y.BatchId) &&
            x.CellId.Equals(y.CellId) &&
            x.Level.Equals(y.Level);
        }

        public int GetHashCode(AbsoluteEntityCell obj)
        {
            int hashCode = 658330915;
            hashCode = hashCode * -1521134295 + EqualityComparer<Int3>.Default.GetHashCode(obj.BatchId);
            hashCode = hashCode * -1521134295 + EqualityComparer<Int3>.Default.GetHashCode(obj.CellId);
            hashCode = hashCode * -1521134295 + obj.Level.GetHashCode();
            return hashCode;
        }
    }
}
