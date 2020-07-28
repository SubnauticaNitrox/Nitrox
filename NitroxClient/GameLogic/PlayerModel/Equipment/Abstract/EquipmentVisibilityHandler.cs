using System.Collections.ObjectModel;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Equipment.Abstract
{
    public abstract class EquipmentVisibilityHandler : IEquipmentVisibilityHandler
    {
        private EquipmentVisibilityHandler successor;

        public abstract void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment);

        public EquipmentVisibilityHandler WithPredecessorHandler(EquipmentVisibilityHandler predecessorHandler)
        {
            predecessorHandler.successor = this;
            return predecessorHandler;
        }

        protected void CallSuccessor(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            successor?.UpdateEquipmentVisibility(playerModel, currentEquipment);
        }
    }
}