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

        public static void UpdateCenter(NitroxInt3 position, HashSet<NitroxInt3> currentlyVisibleCells, CellChanges cellChanges)
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

            public QualityLevel(int qualityLevel)
            {
                switch (qualityLevel)
                {
                    case 0:
                    case 1:
                    case 2:
                    default:
                        CreateLevelsFromQualityLevel(qualityLevel, 7, 7, 16);
                        break;
                }
            }

            public void UpdateCenter(NitroxInt3 position, HashSet<NitroxInt3> currentlyVisibleCells, CellChanges cellChanges)
            {
                for (int i = 0; i < Levels.Length; i++)
                {
                    Levels[i].UpdateCenter(position, currentlyVisibleCells, cellChanges);
                }
            }

            private void CreateLevelsFromQualityLevel(int qualityLevel, int chunksPerSIde, int chunksVertically, int cellSize)
            {
                Levels = new Level[5];
                for (int i = 0; i < 5; i++)
                {
                    int levelSize = cellSize << i;
                    Levels[i] = new Level(chunksPerSIde, chunksVertically, i, levelSize);
                }

            }
        }

        public class Level
        {
            NitroxInt3 centerCell;
            NitroxInt3 arraySize;
            public int Id { get; private set; }
            public int LevelSize { get; private set; }
            public bool entityLoading => Id >= 4;

            public Level(int chunksPerSide, int chunksVertically, int level, int levelSize)
            {
                Id = level;
                LevelSize = levelSize;
                arraySize = new NitroxInt3(chunksPerSide, chunksVertically, chunksPerSide);
                centerCell = new NitroxInt3(-1, -1, -1);
            }

            public void UpdateCenter(NitroxInt3 position, HashSet<NitroxInt3> currentlyVisibleCells, CellChanges cellChanges)
            {
                NitroxInt3 curCell = NitroxInt3.FloorDiv(position, LevelSize);

                if (curCell == centerCell || entityLoading)
                {
                    return;
                }
                centerCell = curCell;

                foreach (NitroxInt3 int3 in NitroxInt3.CenterSize(centerCell, arraySize))
                {
                    if (int3 == NitroxInt3.PositiveModulo(int3, arraySize))
                    {
                        HandleEntities(int3, cellChanges.Added);
                    }
                    else // Essentially "unload the cell if it is already visible load it otherwise"
                    {
                        if (currentlyVisibleCells.Contains(int3))
                        {
                            HandleEntities(int3, cellChanges.Removed);
                        }
                        else
                        {
                            HandleEntities(int3, cellChanges.Added);
                        }
                    }
                }
            }

            public void HandleEntities(NitroxInt3 cell, ICollection<AbsoluteEntityCell> newlyVisibleCells)
            {
                NitroxInt3.Bounds blockRange = NitroxInt3.Bounds.FinerBounds(cell, LevelSize);

                NitroxInt3 blocksPerBatch = new NitroxInt3(160, 160, 160);

                NitroxInt3.Bounds batchBounds = NitroxInt3.Bounds.OuterCoarserBounds(blockRange, blocksPerBatch);

                foreach (NitroxInt3 batchId in batchBounds)
                {
                    NitroxInt3 s = batchId * blocksPerBatch;
                    NitroxInt3.Bounds bsRange = (blockRange - s).Clamp(new NitroxInt3(0, 0, 0), blocksPerBatch - 1);

                    NitroxInt3 cellSize = GetCellSize(blocksPerBatch);

                    NitroxInt3.Bounds cellBounds = NitroxInt3.Bounds.OuterCoarserBounds(bsRange, cellSize);

                    foreach (NitroxInt3 cellId in cellBounds)
                    {
                        newlyVisibleCells.Add(new AbsoluteEntityCell(batchId, cellId, Id));
                    }
                }
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
