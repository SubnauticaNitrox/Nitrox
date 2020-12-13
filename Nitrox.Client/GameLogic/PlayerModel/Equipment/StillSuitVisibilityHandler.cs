using System.Collections.ObjectModel;
using Nitrox.Client.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.Equipment
{
    internal class StillSuitVisibilityHandler : EquipmentVisibilityHandler
    {
        public override void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            bool bodyVisible = currentEquipment.Contains(TechType.Stillsuit);

            playerModel.transform.Find(PlayerEquipmentConstants.STILL_SUIT_GAME_OBJECT_NAME).gameObject.SetActive(bodyVisible);

            CallSuccessor(playerModel, currentEquipment);
        }
    }
}
