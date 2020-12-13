using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Entities;

namespace NitroxServer.GameLogic
{
    public static class CellManager
    {

        public static void UpdateCenter(NitroxInt3 position, HashSet<AbsoluteEntityCell> currentlyVisibleCells, CellChanges cellChanges)
        {
            for (int i = 0; i < QualityLevels.Length; i++)
            {
                QualityLevels[i].UpdateCenter(position, currentlyVisibleCells, cellChanges);
            }
        }


        public static QualityLevel[] QualityLevels { get; private set; } = new QualityLevel[]
        {
            new QualityLevel(0)
        };

        public class QualityLevel
        {
            public Level[] Levels { get; set; }

            private readonly Dictionary<int, NitroxInt3> chunksVisibleByLevel = new Dictionary<int, NitroxInt3>();

            public QualityLevel(int qualityLevel)
            {
                switch (qualityLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                    default:
                        chunksVisibleByLevel[0] = new NitroxInt3(7, 7, 7);
                        chunksVisibleByLevel[1] = new NitroxInt3(7, 7, 7);
                        chunksVisibleByLevel[2] = new NitroxInt3(7, 7, 7);
                        chunksVisibleByLevel[3] = new NitroxInt3(8, 4, 8);
                        chunksVisibleByLevel[4] = new NitroxInt3(1, 1, 1);

                        CreateLevels(16);
                        break;
                }
            }

            public void UpdateCenter(NitroxInt3 position, HashSet<AbsoluteEntityCell> currentlyVisibleCells, CellChanges cellChanges)
            {
                for (int i = 0; i < Levels.Length; i++)
                {
                    Levels[i].UpdateCenter(position, currentlyVisibleCells, cellChanges);
                }
            }

            private void CreateLevels(int cellSize)
            {
                Levels = new Level[5];
                for (int i = 0; i < 5; i++)
                {
                    NitroxInt3 chunkVisibility = chunksVisibleByLevel[i];
                    int levelSize = cellSize << i;
                    Levels[i] = new Level(chunkVisibility.X, chunkVisibility.Y, i, levelSize);
                }

            }
        }

        public class Level
        {
            NitroxInt3 centerCell;
            NitroxInt3 arraySize;
            public int Id { get; private set; }
            public int LevelSize { get; private set; }
            public bool entityLoading => Id <= 2;

            public Level(int chunksPerSide, int chunksVertically, int level, int levelSize)
            {
                Id = level;
                LevelSize = levelSize;
                arraySize = new NitroxInt3(chunksPerSide, chunksVertically, chunksPerSide);
                centerCell = new NitroxInt3(-1, -1, -1);
            }

            public void UpdateCenter(NitroxInt3 position, HashSet<AbsoluteEntityCell> currentlyVisibleCells, CellChanges cellChanges)
            {
                NitroxInt3 curCell = NitroxInt3.FloorDiv(position, LevelSize);

                if (curCell == centerCell || !entityLoading)
                {
                    return;
                }
                centerCell = curCell;

                foreach (NitroxInt3 int3 in NitroxInt3.CenterSize(centerCell, arraySize))
                {
                    HandleEntities(int3, cellChanges, currentlyVisibleCells);
                }
            }

            public void HandleEntities(NitroxInt3 cell, CellChanges cellChanges, HashSet<AbsoluteEntityCell> currentlyVisibleCells)
            {
                NitroxInt3 p = NitroxInt3.PositiveModulo(cell, arraySize);
                NitroxInt3 blocksPerBatch = new NitroxInt3(160, 160, 160);

                if (cell == p)
                {
                    NitroxInt3.Bounds blockRange = NitroxInt3.Bounds.FinerBounds(cell, LevelSize);
                    NitroxInt3.Bounds batchBounds = NitroxInt3.Bounds.OuterCoarserBounds(blockRange, blocksPerBatch);

                    foreach (NitroxInt3 batchId in batchBounds)
                    {
                        NitroxInt3.Bounds cellBounds = GetCellBounds(blockRange, batchId, blocksPerBatch);

                        foreach (NitroxInt3 cellId in cellBounds)
                        {
                            cellChanges.Add(new AbsoluteEntityCell(batchId, cellId, Id));
                        }
                    }
                }
                else
                {
                    NitroxInt3.Bounds blockRange = NitroxInt3.Bounds.FinerBounds(cell, LevelSize);
                    NitroxInt3.Bounds batchBounds = NitroxInt3.Bounds.OuterCoarserBounds(blockRange, blocksPerBatch);
                    foreach (NitroxInt3 batchId in batchBounds)
                    {
                        NitroxInt3.Bounds cellBounds = GetCellBounds(blockRange, batchId, blocksPerBatch);

                        foreach (NitroxInt3 cellId in cellBounds)
                        {
                            AbsoluteEntityCell newCell = new AbsoluteEntityCell(batchId, cellId, Id);
                            if (currentlyVisibleCells.Contains(newCell))
                            {
                                cellChanges.Remove(newCell);
                            }
                            else
                            {
                                cellChanges.Add(newCell);
                            }
                        }
                    }
                }    
            }

            public NitroxInt3.Bounds GetCellBounds(NitroxInt3.Bounds blockRange, NitroxInt3 batchId, NitroxInt3 blocksPerBatch)
            {
                NitroxInt3 s = batchId * blocksPerBatch;
                NitroxInt3.Bounds bsRange = (blockRange - s).Clamp(new NitroxInt3(0, 0, 0), blocksPerBatch - 1);

                NitroxInt3 cellSize = GetCellSize(blocksPerBatch);

                return NitroxInt3.Bounds.OuterCoarserBounds(bsRange, cellSize);
            }

            public NitroxInt3 GetCellSize(NitroxInt3 blocksPerBatch)
            {
                NitroxInt3 int3 = blocksPerBatch / 10;
                if (Id == 0)
                {
                    return int3;
                }

                return int3 << 1;
            }
        }
    }
}
