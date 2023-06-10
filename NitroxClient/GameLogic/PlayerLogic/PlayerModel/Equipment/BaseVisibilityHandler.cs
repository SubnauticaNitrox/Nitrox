#if BELOWZERO
using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class BaseVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject gloves;
        private readonly GameObject mask;
        private readonly GameObject hands;
        private readonly GameObject head;
        private readonly GameObject body;

        public BaseVisibilityHandler(GameObject playerModel)
        {
            gloves = playerModel.transform.Find(PlayerEquipmentConstants.BASE_GLOVES_GAME_OBJECT_NAME).gameObject;
            mask = playerModel.transform.Find(PlayerEquipmentConstants.BASE_MASK_GAME_OBJECT_NAME).gameObject;
            hands = playerModel.transform.Find(PlayerEquipmentConstants.BASE_HANDS_GAME_OBJECT_NAME).gameObject;
            head = playerModel.transform.Find(PlayerEquipmentConstants.BASE_HEAD_GAME_OBJECT_NAME).gameObject;
            body = playerModel.transform.Find(PlayerEquipmentConstants.BASE_BODY_GAME_OBJECT_NAME).gameObject;
        }
        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool headVisible = !currentEquipment.Contains(TechType.ColdSuitHelmet);
            bool maskVisible = currentEquipment.Contains(TechType.Rebreather);
            bool bodyVisible = !currentEquipment.Contains(TechType.ColdSuit);
            bool handsVisible = !currentEquipment.Contains(TechType.ColdSuitGloves);

            gloves.SetActive(handsVisible);
            mask.SetActive(maskVisible);
            hands.SetActive(handsVisible);
            head.SetActive(headVisible);
            body.SetActive(bodyVisible);
        }
    }
}
#endif
