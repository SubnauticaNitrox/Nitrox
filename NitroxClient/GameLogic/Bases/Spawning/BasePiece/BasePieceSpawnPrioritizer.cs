using System.Collections.Generic;
using System.Linq;
using NitroxModel_Subnautica.DataStructures;

using BasePieceData = NitroxModel.DataStructures.GameLogic.BasePiece;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    public class BasePieceSpawnPrioritizer
    {
        private readonly Dictionary<TechType, int> piecesToPriority = new Dictionary<TechType, int>()
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
            [TechType.BaseConnector] = 1,

            // Reinforce fortifications to ensure good hull integrity
            [TechType.BaseReinforcement] = 2,
            [TechType.BaseBulkhead] = 2,

            // upgrade consoles are a faced based piece, so allow them to be prioritized aside the two above.
            [TechType.BaseUpgradeConsole] = 2,

            // Ladders should always come before water parks. Else, the ladders won't spawn
            [TechType.BaseLadder] = 3,
            
            // Water tanks are internal and need to go before hatches because hatches can be placed on them.
            [TechType.BaseWaterPark] = 4,

            // Everything that needs a hatch should come before here.
            [TechType.BaseHatch] = 5,
            [TechType.BaseWindow] = 5,

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

        public IEnumerable<BasePieceData> OrderBasePiecesByPriority(IEnumerable<BasePieceData> inputPieces)
        {
            return inputPieces.OrderBy(piece => piece.ParentId.HasValue) // Ensure pieces without parents go first.
                              .ThenByDescending(piece => piece.ConstructionCompleted) // Ensure completed pieces are before pending pieces
                              .ThenBy(piece => piece.IsFurniture) // Ensure base building block go before furniture
                              .ThenBy(ComputeBasePiecePriority) // Ensure remaining pieces are prioritized by above order. 
                              .ThenBy(piece => piece.BuildIndex);
        }

        private int ComputeBasePiecePriority(BasePieceData basePiece)
        {
            TechType techType = basePiece.TechType.ToUnity();

            return piecesToPriority.TryGetValue(techType, out int position) ? position : int.MaxValue;
        }
    }
}
