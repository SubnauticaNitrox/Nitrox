using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Equipment
{
    public class DiveSuitVisibilityHandler : EquipmentVisibilityHandler
    {
        public override void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            bool headVisible = !currentEquipment.Contains(TechType.RadiationHelmet) && !currentEquipment.Contains(TechType.Rebreather);
            bool bodyVisible = !currentEquipment.Contains(TechType.RadiationSuit) && 
                               !currentEquipment.Contains(TechType.Stillsuit) && 
                               !currentEquipment.Contains(TechType.ReinforcedDiveSuit);
            bool handsVisible = !currentEquipment.Contains(TechType.RadiationGloves) && !currentEquipment.Contains(TechType.ReinforcedGloves);

            playerModel.transform.Find(PlayerEquipmentConstants.NORMAL_HEAD_GAME_OBJECT_NAME).gameObject.SetActive(headVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.DIVE_SUIT_GAME_OBJECT_NAME).gameObject.SetActive(bodyVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.NORMAL_HANDS_GAME_OBJECT_NAME).gameObject.SetActive(handsVisible);

            CallSuccessor(playerModel, currentEquipment);
        }
    }
}
