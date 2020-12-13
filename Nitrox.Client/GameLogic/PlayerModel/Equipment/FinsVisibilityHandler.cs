using System.Collections.ObjectModel;
using Nitrox.Client.GameLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace Nitrox.Client.GameLogic.PlayerModel.Equipment
{
    public class FinsVisibilityHandler : EquipmentVisibilityHandler
    {
        public override void UpdateEquipmentVisibility(GameObject playerModel, ReadOnlyCollection<TechType> currentEquipment)
        {
            bool basicFinsVisible = currentEquipment.Contains(TechType.Fins);
            bool chargedFinsVisible = currentEquipment.Contains(TechType.SwimChargeFins);
            bool glideFinsVisible = currentEquipment.Contains(TechType.UltraGlideFins);

            playerModel.transform.Find(PlayerEquipmentConstants.FINS_ROOT_GAME_OBJECT_NAME).gameObject.SetActive(basicFinsVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.FINS_GAME_OBJECT_NAME).gameObject.SetActive(basicFinsVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_GAME_OBJECT_NAME).gameObject.SetActive(chargedFinsVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_ROOT_GAME_OBJECT_NAME).gameObject.SetActive(chargedFinsVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_GAME_OBJECT_NAME).gameObject.SetActive(glideFinsVisible);
            playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_ROOT_GAME_OBJECT_NAME).gameObject.SetActive(glideFinsVisible);

            CallSuccessor(playerModel, currentEquipment);
        }
    }
}
