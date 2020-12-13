using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    public class BasePieceSpawnPrioritizer
    {
        public readonly Dictionary<TechType, int> PiecesToPriority = new Dictionary<TechType, int>()
        {
            // Prioritize core foundations, basic rooms, and corridors
            [TechType.BaseFoundation] = 1,
            [TechType.BaseRoom] = 1,
            [TechType.BaseMapRoom] = 1,
            [TechType.BaseMoonpool] = 1,
            [TechType.BaseObservatory] = 1,
            [TechType.BaseCorridor] = 1,
            [TechType.BaseCorridorGlass] = 1,
            [TechType.BaseCorridorGlassI] = 1,
            [TechType.BaseCorridorGlassL] = 1,
            [TechType.BaseCorridorI] = 1,
            [TechType.BaseCorridorL] = 1,
            [TechType.BaseCorridorT] = 1,
            [TechType.BaseCorridorX] = 1,
            [TechType.BasePipeConnector] = 1,

            // Reinforce fortifications to ensure good hull integrity
            [TechType.BaseReinforcement] = 2,
            [TechType.BaseBulkhead] = 2,

            // upgrade consoles are a faced based piece, so allow them to be prioritized aside the two above.
            [TechType.BaseUpgradeConsole] = 2,

            // Water tanks are internal and need to go before hatches because hatches can be placed on them.
            [TechType.BaseWaterPark] = 3,

            // Everything that needs a hatch should come before here.
            [TechType.BaseHatch] = 4,

            [TechType.BaseWindow] = 5,
            [TechType.BaseLadder] = 5,

            // Place energy producing before consuming
            [TechType.SolarPanel] = 6,
            [TechType.BaseNuclearReactor] = 6,
            [TechType.NuclearReactor] = 6,
            [TechType.BaseBioReactor] = 6,
            [TechType.Bioreactor] = 6,
            [TechType.ThermalPlant] = 6,
            [TechType.PowerTransmitter] = 6,

            // Energy Consuming
            [TechType.BaseFiltrationMachine] = 7,
            [TechType.Fabricator] = 7,
            [TechType.BatteryCharger] = 7

            // Anything not here will default to the end below.

        };

        public List<BasePiece> OrderBasePiecesByPriority(List<BasePiece> inputPieces)
        {
            return inputPieces.OrderBy(piece => piece.ParentId.HasValue) // Ensure pieces without parents go first.
                              .ThenByDescending(piece => piece.ConstructionCompleted) // Ensure completed pieces are before pending pieces
                              .ThenBy(piece => piece.IsFurniture) // Ensure base building block go before furniture
                              .ThenBy(piece => ComputeBasePiecePriority(piece))    // Ensure remaining pieces are prioritized by above order. 
                              .ThenBy(piece => piece.BuildIndex)
                              .ToList();                              
        }

        private int ComputeBasePiecePriority(BasePiece basePiece)
        {
            TechType techType = basePiece.TechType.ToUnity();
            int position;

            if (PiecesToPriority.TryGetValue(techType, out position))
            {
                return position;
            }

            return int.MaxValue;
        }
    }
}
