using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class DiveSuitVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject head;
        private readonly GameObject body;
        private readonly GameObject hands;

        public DiveSuitVisibilityHandler(GameObject playerModel)
        {
            head = playerModel.transform.Find(PlayerEquipmentConstants.NORMAL_HEAD_GAME_OBJECT_NAME).gameObject;
            body = playerModel.transform.Find(PlayerEquipmentConstants.DIVE_SUIT_GAME_OBJECT_NAME).gameObject;
            hands = playerModel.transform.Find(PlayerEquipmentConstants.NORMAL_HANDS_GAME_OBJECT_NAME).gameObject;
        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool headVisible = !currentEquipment.Contains(TechType.RadiationHelmet) && !currentEquipment.Contains(TechType.Rebreather);
            bool bodyVisible = !currentEquipment.Contains(TechType.RadiationSuit) &&
                               !currentEquipment.Contains(TechType.Stillsuit) &&
                               !currentEquipment.Contains(TechType.ReinforcedDiveSuit);
            bool handsVisible = !currentEquipment.Contains(TechType.RadiationGloves) && !currentEquipment.Contains(TechType.ReinforcedGloves);

            head.SetActive(headVisible);
            body.gameObject.SetActive(bodyVisible);
            hands.SetActive(handsVisible);
        }
    }
}
