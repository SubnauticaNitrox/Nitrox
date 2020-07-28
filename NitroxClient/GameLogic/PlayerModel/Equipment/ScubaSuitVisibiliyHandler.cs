using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel.Equipment
{
    internal class ScubaSuitVisibiliyHandler : EquipmentVisibilityHandler
    {
        public override void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            bool tankEquipped = currentEquipment.Contains(TechType.Tank) ||
                                currentEquipment.Contains(TechType.DoubleTank) ||
                                currentEquipment.Contains(TechType.HighCapacityTank) ||
                                currentEquipment.Contains(TechType.PlasteelTank);

            bool rebreatherVisible = currentEquipment.Contains(TechType.Rebreather);
            bool radiationHelmetVisible = currentEquipment.Contains(TechType.RadiationHelmet);
            bool tankVisible = tankEquipped && !currentEquipment.Contains(TechType.RadiationSuit);
            bool tubesVisible = (rebreatherVisible || radiationHelmetVisible) && tankVisible;
            bool rootVisible = rebreatherVisible || tankVisible;

            playerModel.transform.Find(PlayerEquipmentConstants.REBREATHER_GAME_OBJECT_NAME).gameObject.SetActive(rebreatherVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.SCUBA_TANK_GAME_OBJECT_NAME).gameObject.SetActive(tankVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.SCUBA_TANK_TUBES_GAME_OBJECT_NAME).gameObject.SetActive(tubesVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.SCUBA_ROOT_GAME_OBJECT_NAME).gameObject.SetActive(rootVisible);

            CallSuccessor(playerModel, currentEquipment);
        }
    }
}
