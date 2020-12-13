using System.Collections.ObjectModel;
using Nitrox.Client.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.Equipment
{
    internal class ReinforcedSuitVisibilityHandler : EquipmentVisibilityHandler
    {
        public override void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            bool glovesVisible = currentEquipment.Contains(TechType.ReinforcedGloves);
            bool bodyVisible = currentEquipment.Contains(TechType.ReinforcedDiveSuit);

            playerModel.transform.Find(PlayerEquipmentConstants.REINFORCED_GLOVES_GAME_OBJECT_NAME).gameObject.SetActive(glovesVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.REINFORCED_SUIT_GAME_OBJECT_NAME).gameObject.SetActive(bodyVisible);

            CallSuccessor(playerModel, currentEquipment);
        }
    }
}
