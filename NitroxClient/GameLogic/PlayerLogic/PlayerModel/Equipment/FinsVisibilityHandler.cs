using System.Collections.ObjectModel;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Equipment
{
    public class FinsVisibilityHandler : IEquipmentVisibilityHandler
    {
        private readonly GameObject fins;
        private readonly GameObject finsRoot;
        private readonly GameObject chargedFins;
        private readonly GameObject chargedFinsRoot;
        private readonly GameObject glideFins;
        private readonly GameObject glideFinsRoot;

        public FinsVisibilityHandler(GameObject playerModel)
        {
            fins = playerModel.transform.Find(PlayerEquipmentConstants.FINS_GAME_OBJECT_NAME).gameObject;
            finsRoot = playerModel.transform.Find(PlayerEquipmentConstants.FINS_ROOT_GAME_OBJECT_NAME).gameObject;
            chargedFins = playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_GAME_OBJECT_NAME).gameObject;
            chargedFinsRoot = playerModel.transform.Find(PlayerEquipmentConstants.CHARGED_FINS_ROOT_GAME_OBJECT_NAME).gameObject;
            glideFins = playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_GAME_OBJECT_NAME).gameObject;
            glideFinsRoot = playerModel.transform.Find(PlayerEquipmentConstants.GLIDE_FINS_ROOT_GAME_OBJECT_NAME).gameObject;
        }

        public void UpdateEquipmentVisibility(ReadOnlyCollection<TechType> currentEquipment)
        {
            bool basicFinsVisible = currentEquipment.Contains(TechType.Fins);
            bool chargedFinsVisible = currentEquipment.Contains(TechType.SwimChargeFins);
            bool glideFinsVisible = currentEquipment.Contains(TechType.UltraGlideFins);

            fins.SetActive(basicFinsVisible);
            finsRoot.SetActive(basicFinsVisible);
            chargedFins.SetActive(chargedFinsVisible);
            chargedFinsRoot.SetActive(chargedFinsVisible);
            glideFins.SetActive(glideFinsVisible);
            glideFinsRoot.SetActive(glideFinsVisible);
        }
    }
}
