using System.Collections.Generic;
using System.Linq;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.Spawning
{
    public class BasePieceSpawnPrioritizer
    {
        public readonly List<TechType> PieceOrder = new List<TechType>()
        {
            // Prioritize core foundation and basic rooms.
            TechType.BaseFoundation,
            TechType.BaseRoom,
            TechType.BaseMapRoom,
            TechType.BaseMoonpool,
            TechType.BaseObservatory,

            // Corridors can then be used to connect the varios base pieces
            TechType.BaseCorridor,
            TechType.BaseCorridorGlass,
            TechType.BaseCorridorGlassI,
            TechType.BaseCorridorGlassL,
            TechType.BaseCorridorI,
            TechType.BaseCorridorL,
            TechType.BaseCorridorT,
            TechType.BaseCorridorX,
            TechType.BasePipeConnector,
            
            TechType.BaseUpgradeConsole,

            // Reinforce fortifications to ensure good hull integrity
            TechType.BaseReinforcement,
            TechType.BaseBulkhead,

            // Water tanks are internal and need to go before hatches because hatches can be placed on them.
            TechType.BaseWaterPark,

            // Everything that needs a hatch should come before here.
            TechType.BaseHatch,

            TechType.BaseWindow,
            TechType.BaseLadder,
            
            // Place energy producing before consuming
            TechType.SolarPanel,
            TechType.BaseNuclearReactor,
            TechType.NuclearReactor,
            TechType.BaseBioReactor,
            TechType.Bioreactor,
            TechType.ThermalPlant,
            TechType.PowerTransmitter,

            // Energy Consuming
            TechType.BaseFiltrationMachine,
            TechType.Fabricator,
            TechType.BatteryCharger

            // Anything not here will default to the end below.

        };

        public List<BasePiece> OrderBasePiecesByPriority(List<BasePiece> inputPieces)
        {
            return inputPieces.OrderBy(piece => piece.ParentId.HasValue) // Ensure pieces without parents go first.
                              .ThenByDescending(piece => piece.ConstructionCompleted) // Ensure completed pieces are before pending pieces
                              .ThenBy(piece => piece.IsFurniture) // Ensure base building block go before furniture
                              .ThenBy(piece => ComputeBasePiecePriority(piece))    // Ensure remaining pieces are prioritized by above order. 
                              .ToList();                              
        }

        private int ComputeBasePiecePriority(BasePiece basePiece)
        {
            TechType techType = basePiece.TechType.ToUnity();
            int position = PieceOrder.IndexOf(techType);

            if (position == -1)
            {
                return int.MaxValue;
            }

            return position;
        }
    }
}
