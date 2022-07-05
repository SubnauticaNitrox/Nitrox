using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class RadiationSuitVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject head;
        private readonly GameObject helmet;
        private readonly GameObject gloves;
        private readonly GameObject suit;
        private readonly GameObject suitNeck;
        private readonly GameObject suitVest;
        private readonly GameObject tank;
        private readonly GameObject tankTubes;

        public RadiationSuitVisibilityHandler(GameObject playerModel)
        {
            head = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_HEAD_GAME_OBJECT_NAME).gameObject;
            helmet = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_HELMET_GAME_OBJECT_NAME).gameObject;
            gloves = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_GLOVES_GAME_OBJECT_NAME).gameObject;
            suit = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_SUIT_GAME_OBJECT_NAME).gameObject;
            suitNeck = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME).gameObject;
            suitVest = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_SUIT_VEST_GAME_OBJECT_NAME).gameObject;
            tank = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_TANK_GAME_OBJECT_NAME).gameObject;
            tankTubes = playerModel.transform.Find(PlayerEquipmentConstants.RADIATION_TANK_TUBES_GAME_OBJECT_NAME).gameObject;

        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
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

            head.SetActive(helmetVisible);
            helmet.SetActive(helmetVisible);
            gloves.SetActive(glovesVisible);
            suit.SetActive(bodyVisible);
            suitNeck.SetActive(helmetVisible);
            suitVest.SetActive(vestVisible);
            tank.SetActive(tankVisible);
            tankTubes.SetActive(tubesVisible);
        }
    }
}
