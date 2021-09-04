using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Equipment
{
    public class DiveSuitVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject body;
        private readonly GameObject head;
        private readonly GameObject hands;

        public DiveSuitVisibilityHandler(GameObject playerModel)
        {
            body = playerModel.transform.Find(PlayerEquipmentConstants.DIVE_SUIT_GAME_OBJECT_NAME).gameObject;
#if SUBNAUTICA
            head = playerModel.transform.Find(PlayerEquipmentConstants.NORMAL_HEAD_GAME_OBJECT_NAME).gameObject;
            hands = playerModel.transform.Find(PlayerEquipmentConstants.NORMAL_HANDS_GAME_OBJECT_NAME).gameObject;
#endif
        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
#if SUBNAUTICA
            bool bodyVisible = !currentEquipment.Contains(TechType.RadiationSuit) &&
                               !currentEquipment.Contains(TechType.Stillsuit) &&
                               !currentEquipment.Contains(TechType.ReinforcedDiveSuit);
            bool headVisible = !currentEquipment.Contains(TechType.RadiationHelmet) && !currentEquipment.Contains(TechType.Rebreather);
            bool handsVisible = !currentEquipment.Contains(TechType.RadiationGloves) && !currentEquipment.Contains(TechType.ReinforcedGloves);
#elif BELOWZERO
            bool bodyVisible = !currentEquipment.Contains(TechType.ReinforcedDiveSuit);
#endif
            body.gameObject.SetActive(bodyVisible);
#if SUBNAUTICA
            head.SetActive(headVisible);
            hands.SetActive(handsVisible);
#endif
        }
    }
}
