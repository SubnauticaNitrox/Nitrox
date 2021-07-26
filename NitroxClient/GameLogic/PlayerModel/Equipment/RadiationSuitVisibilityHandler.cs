using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Equipment
{
    public class RadiationSuitVisibilityHandler : EquipmentVisibilityHandler
    {
        public override void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            bool tankEquipped = currentEquipment.Contains(TechType.Tank) ||
                                currentEquipment.Contains(TechType.DoubleTank) ||
                                currentEquipment.Contains(TechType.HighCapacityTank) ||
                                currentEquipment.Contains(TechType.PlasteelTank);

            bool helmetVisible = currentEquipment.Contains(TechType.RadiationHelmet);
            bool glovesVisible = currentEquipment.Contains(TechType.RadiationGloves);
            bool bodyVisible = currentEquipment.Contains(TechType.RadiationSuit);
            bool vestVisible = bodyVisible || helmetVisible;
            bool tankVisible = tankEquipped && vestVisible;
            bool tubesVisible = tankVisible && helmetVisible;

            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_HEAD_GAME_OBJECT_NAME).gameObject.SetActive(helmetVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_HELMET_GAME_OBJECT_NAME).gameObject.SetActive(helmetVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME).gameObject.SetActive(helmetVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_GLOVES_GAME_OBJECT_NAME).gameObject.SetActive(glovesVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_SUIT_GAME_OBJECT_NAME).gameObject.SetActive(bodyVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_SUIT_VEST_GAME_OBJECT_NAME).gameObject.SetActive(vestVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_TANK_GAME_OBJECT_NAME).gameObject.SetActive(tankVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_TANK_TUBES_GAME_OBJECT_NAME).gameObject.SetActive(tubesVisible);

            CallSuccessor(playerModel, currentEquipment);
        }
    }
}
