#if BELOWZERO
using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class ColdProtectiveSuitVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject head;
        private readonly GameObject mask;
        private readonly GameObject suit;
        private readonly GameObject hands;

        public ColdProtectiveSuitVisibilityHandler(GameObject playerModel)
        {
            head = playerModel.transform.Find(PlayerEquipmentConstants.COLD_PROTECTIVE_HEAD_GAME_OBJECT_NAME).gameObject;
            mask = playerModel.transform.Find(PlayerEquipmentConstants.COLD_PROTECTIVE_MASK_GAME_OBJECT_NAME).gameObject;
            suit = playerModel.transform.Find(PlayerEquipmentConstants.COLD_PROTECTIVE_BODY_GAME_OBJECT_NAME).gameObject;
            hands = playerModel.transform.Find(PlayerEquipmentConstants.COLD_PROTECTIVE_HANDS_GAME_OBJECT_NAME).gameObject;
        }
        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool headVisible = currentEquipment.Contains(TechType.ColdSuitHelmet);
            bool maskVisible = currentEquipment.Contains(TechType.ColdSuitHelmet);
            bool bodyVisible = currentEquipment.Contains(TechType.ColdSuit);
            bool handsVisible = currentEquipment.Contains(TechType.ColdSuitGloves);

            head.SetActive(headVisible);
            mask.SetActive(maskVisible);
            suit.SetActive(bodyVisible);
            hands.SetActive(handsVisible);
        }
    }
}
#endif
