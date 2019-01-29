using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerModel.Equipment;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel
{
    public class PlayerModelManager
    {
        public void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            IEquipmentVisibilityHandler handler = BuildVisibilityHandlerChain();
            handler.UpdateEquipmentVisibility(playerModel, currentEquipment);
        }

        private EquipmentVisibilityHandler BuildVisibilityHandlerChain()
        {
            return new DiveSuitVisibilityHandler()
                .WithPredecessorHandler(new ScubaSuitVisibiliyHandler())
                .WithPredecessorHandler(new FinsVisibilityHandler())
                .WithPredecessorHandler(new RadiationSuitVisibilityHandler())
                .WithPredecessorHandler(new ReinforcedSuitVisibilityHandler())
                .WithPredecessorHandler(new StillSuitVisibilityHandler());
        }
    }
}
